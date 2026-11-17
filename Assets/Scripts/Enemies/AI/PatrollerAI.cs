using UnityEngine;

namespace DungeonKnight
{
    public enum PatrollerState { Patrol, Chase, WindUp, Recover, Search }

    public class PatrollerAI : EnemyBrain<PatrollerState>
    {
        [SerializeField] EnemyVision vision;
        [SerializeField] AttackTelegraph telegraph;
        [SerializeField] PatrollerStats stats;
        [SerializeField] GameObject strikeEffect;
        [SerializeField] Vector2[] route;

        [Header("Navigation")]
        [SerializeField, Min(0.1f)] float stuckCheckInterval = 2.5f;
        [SerializeField, Min(0f)] float stuckDistance = 0.3f;
        [SerializeField] LayerMask obstacleMask = 1 << GameLayers.Obstacle;

        int waypointIndex;
        float pausedUntil;
        float nextAttackTime;
        Vector2 lastKnownTargetPosition;
        Vector2 strikeCenter;
        Vector2 progressAnchor;
        float nextProgressCheck;
        bool lookingAround;
        float lookAroundEnd;
        float nextLookTurn;

        protected override PatrollerState InitialState => PatrollerState.Patrol;
        protected override SightedEnemyStats SightStats => stats;

        public override void Configure(PlayerHealth player, EnemySpawn spawn)
        {
            base.Configure(player, spawn);
            route = spawn.patrolRoute != null ? (Vector2[])spawn.patrolRoute.Clone() : new Vector2[0];
        }

        protected override void Awake()
        {
            base.Awake();
            RefCheck.Valid(this, stats, nameof(stats));
            RefCheck.Valid(this, vision, nameof(vision));
            RefCheck.Valid(this, telegraph, nameof(telegraph));
        }

        void Start()
        {
            waypointIndex = NearestWaypointIndex();
            ResetProgressCheck();
        }

        protected override void Tick()
        {
            if (vision == null || telegraph == null) return;
            switch (State)
            {
                case PatrollerState.Patrol: TickPatrol(); break;
                case PatrollerState.Chase: TickChase(); break;
                case PatrollerState.WindUp: TickWindUp(); break;
                case PatrollerState.Recover: TickRecover(); break;
                case PatrollerState.Search: TickSearch(); break;
            }
        }

        void TickPatrol()
        {
            if (CanDetectTarget())
            {
                ChangeState(PatrollerState.Chase);
                return;
            }
            if (route == null || route.Length == 0 || Time.time < pausedUntil)
            {
                motor.Stop();
                return;
            }

            bool arrived = motor.MoveTo(route[waypointIndex], stats.patrolSpeed);
            if (!arrived && !IsStuck()) return;

            waypointIndex = (waypointIndex + 1) % route.Length;
            pausedUntil = Time.time + stats.waypointPause;
            ResetProgressCheck();
            motor.Face(route[waypointIndex] - Position);
        }

        void TickChase()
        {
            if (!CanTrackTarget())
            {
                ChangeState(HasLivingTarget ? PatrollerState.Search : PatrollerState.Patrol);
                return;
            }

            lastKnownTargetPosition = TargetPosition;
            Vector2 toTarget = lastKnownTargetPosition - Position;
            float distance = toTarget.magnitude;
            if (distance <= stats.attackRange && Time.time >= nextAttackTime && CanStartAttack)
            {
                ChangeState(PatrollerState.WindUp);
                return;
            }

            if (distance <= stats.holdDistance)
            {
                motor.Stop();
                motor.Face(toTarget);
            }
            else
            {
                motor.Move(toTarget, stats.chaseSpeed);
            }
        }

        void TickWindUp()
        {
            motor.Stop();
            if (!HasLivingTarget)
            {
                ChangeState(PatrollerState.Patrol);
                return;
            }
            telegraph.SetProgress(TimeInState / stats.windUpDuration);
            if (TimeInState >= stats.windUpDuration) ChangeState(PatrollerState.Recover);
        }

        void TickRecover()
        {
            motor.Stop();
            if (TimeInState < stats.recoveryDuration) return;
            if (CanTrackTarget()) ChangeState(PatrollerState.Chase);
            else ChangeState(HasLivingTarget ? PatrollerState.Search : PatrollerState.Patrol);
        }

        void TickSearch()
        {
            if (CanDetectTarget())
            {
                ChangeState(PatrollerState.Chase);
                return;
            }

            if (!lookingAround)
            {
                bool arrived = motor.MoveTo(lastKnownTargetPosition, stats.searchSpeed);
                if (arrived || IsStuck()) StartLookingAround();
                return;
            }

            motor.Stop();
            if (Time.time >= nextLookTurn)
            {
                motor.Face(Quaternion.Euler(0f, 0f, stats.lookTurnAngle) * motor.Facing);
                nextLookTurn = Time.time + stats.lookTurnInterval;
            }
            if (Time.time >= lookAroundEnd) ChangeState(PatrollerState.Patrol);
        }

        protected override void OnStateChanged(PatrollerState previous, PatrollerState next)
        {
            if (previous == PatrollerState.WindUp)
            {
                ClearBodyTint();
                if (next != PatrollerState.Recover) telegraph.Hide();
            }
            if (next == PatrollerState.Chase && (previous == PatrollerState.Patrol || previous == PatrollerState.Search)) IconAlert();
            if (previous == PatrollerState.Search && next == PatrollerState.Patrol) IconClear();

            switch (next)
            {
                case PatrollerState.Patrol:
                    waypointIndex = NearestWaypointIndex();
                    pausedUntil = 0f;
                    ResetProgressCheck();
                    break;
                case PatrollerState.WindUp:
                    BeginWindUp();
                    ShowWindUpTint();
                    break;
                case PatrollerState.Recover:
                    Strike();
                    break;
                case PatrollerState.Search:
                    lookingAround = false;
                    ResetProgressCheck();
                    IconSuspicious(float.PositiveInfinity);
                    break;
            }
        }

        protected override void OnHit(Vector2 towardShooter)
        {
            vision.Alert();
            if (State != PatrollerState.Patrol && State != PatrollerState.Search) return;
            RaycastHit2D wall = Physics2D.Raycast(Position, towardShooter, stats.hitInvestigateDistance, obstacleMask);
            float distance = wall.collider != null ? Mathf.Max(0f, wall.distance - 0.5f) : stats.hitInvestigateDistance;
            lastKnownTargetPosition = Position + towardShooter * distance;
            motor.Face(towardShooter);
            if (State == PatrollerState.Patrol)
            {
                ChangeState(PatrollerState.Search);
                return;
            }
            lookingAround = false;
            ResetProgressCheck();
            RestartState();
            IconSuspicious(float.PositiveInfinity);
        }

        void BeginWindUp()
        {
            Vector2 direction = TargetPosition - Position;
            if (direction.sqrMagnitude < 0.0001f) direction = motor.Facing;
            motor.Stop();
            motor.Face(direction);
            strikeCenter = Position + direction.normalized * stats.strikeReach;
            telegraph.ShowCircle(strikeCenter, stats.strikeRadius);
        }

        void Strike()
        {
            nextAttackTime = Time.time + stats.attackCooldown;
            telegraph.Flash();
            if (strikeEffect != null) Instantiate(strikeEffect, strikeCenter, Quaternion.identity);
            if (!HasLivingTarget) return;
            if ((TargetPosition - strikeCenter).sqrMagnitude > stats.strikeRadius * stats.strikeRadius) return;
            if (!vision.HasLineOfSightNow(TargetPosition)) return;
            target.TakeDamage(stats.attackDamage);
        }

        void StartLookingAround()
        {
            lookingAround = true;
            lookAroundEnd = Time.time + stats.lookAroundDuration;
            nextLookTurn = Time.time + stats.lookTurnInterval;
        }

        bool CanDetectTarget() => HasLivingTarget && vision.CanSee(TargetPosition);

        bool CanTrackTarget() => HasLivingTarget && vision.IsInRange(TargetPosition) && vision.HasLineOfSight(TargetPosition);

        bool IsStuck()
        {
            if (Time.time < nextProgressCheck) return false;
            bool stuck = (Position - progressAnchor).sqrMagnitude < stuckDistance * stuckDistance;
            ResetProgressCheck();
            return stuck;
        }

        void ResetProgressCheck()
        {
            progressAnchor = Position;
            nextProgressCheck = Time.time + stuckCheckInterval;
        }

        int NearestWaypointIndex()
        {
            if (route == null || route.Length == 0) return 0;
            int best = 0;
            float bestSqr = float.MaxValue;
            for (int i = 0; i < route.Length; i++)
            {
                float sqr = (route[i] - Position).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = i;
                }
            }
            return best;
        }


        void OnDrawGizmosSelected()
        {
            if (route != null && route.Length > 0)
            {
                Gizmos.color = new Color(0.4f, 0.9f, 1f, 0.9f);
                for (int i = 0; i < route.Length; i++)
                {
                    Gizmos.DrawWireSphere(route[i], 0.15f);
                    Gizmos.DrawLine(route[i], route[(i + 1) % route.Length]);
                }
            }
            if (stats == null) return;
            Gizmos.color = new Color(1f, 0.3f, 0.2f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, stats.attackRange);
        }
    }
}
