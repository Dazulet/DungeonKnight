using UnityEngine;

namespace DungeonKnight
{
    public enum PatrollerState { Patrol, Chase, WindUp, Recover, Search }

    public class PatrollerAI : EnemyBrain<PatrollerState>
    {
        [SerializeField] EnemyVision vision;
        [SerializeField] AttackTelegraph telegraph;
        [SerializeField] GameObject strikeEffect;
        [SerializeField] Transform[] waypoints;

        [Header("Movement")]
        [SerializeField, Min(0f)] float patrolSpeed = 1.4f;
        [SerializeField, Min(0f)] float chaseSpeed = 2.6f;
        [SerializeField, Min(0f)] float waypointPause = 0.6f;
        [SerializeField, Min(0.1f)] float stuckCheckInterval = 2.5f;
        [SerializeField, Min(0f)] float stuckDistance = 0.3f;

        [Header("Attack")]
        [SerializeField, Min(0f)] float attackRange = 1.3f;
        [SerializeField, Min(0f)] float holdDistance = 0.8f;
        [SerializeField, Min(0f)] float strikeReach = 0.7f;
        [SerializeField, Min(0.1f)] float strikeRadius = 0.85f;
        [SerializeField, Min(1)] int attackDamage = 1;
        [SerializeField, Range(0.3f, 0.8f)] float windUpDuration = 0.55f;
        [SerializeField, Min(0f)] float recoveryDuration = 0.45f;
        [SerializeField, Min(0f)] float attackCooldown = 1.1f;

        [Header("Search")]
        [SerializeField, Min(0f)] float searchSpeed = 1.8f;
        [SerializeField, Min(0f)] float lookAroundDuration = 2.2f;
        [SerializeField, Min(0.05f)] float lookTurnInterval = 0.55f;
        [SerializeField, Range(0f, 180f)] float lookTurnAngle = 90f;

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

        protected override void Awake()
        {
            base.Awake();
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
            if (waypoints == null || waypoints.Length == 0 || Time.time < pausedUntil)
            {
                motor.Stop();
                return;
            }

            Transform waypoint = waypoints[waypointIndex];
            bool arrived = waypoint == null || motor.MoveTo(waypoint.position, patrolSpeed);
            if (!arrived && !IsStuck()) return;

            waypointIndex = (waypointIndex + 1) % waypoints.Length;
            pausedUntil = Time.time + waypointPause;
            ResetProgressCheck();
            Transform next = waypoints[waypointIndex];
            if (next != null) motor.Face((Vector2)next.position - Position);
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
            if (distance <= attackRange && Time.time >= nextAttackTime && CanStartAttack)
            {
                ChangeState(PatrollerState.WindUp);
                return;
            }

            if (distance <= holdDistance)
            {
                motor.Stop();
                motor.Face(toTarget);
            }
            else
            {
                motor.Move(toTarget, chaseSpeed);
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
            telegraph.SetProgress(TimeInState / windUpDuration);
            if (TimeInState >= windUpDuration) ChangeState(PatrollerState.Recover);
        }

        void TickRecover()
        {
            motor.Stop();
            if (TimeInState < recoveryDuration) return;
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
                bool arrived = motor.MoveTo(lastKnownTargetPosition, searchSpeed);
                if (arrived || IsStuck()) StartLookingAround();
                return;
            }

            motor.Stop();
            if (Time.time >= nextLookTurn)
            {
                motor.Face(Quaternion.Euler(0f, 0f, lookTurnAngle) * motor.Facing);
                nextLookTurn = Time.time + lookTurnInterval;
            }
            if (Time.time >= lookAroundEnd) ChangeState(PatrollerState.Patrol);
        }

        protected override void OnStateChanged(PatrollerState previous, PatrollerState next)
        {
            if (previous == PatrollerState.WindUp && next != PatrollerState.Recover) telegraph.Hide();

            switch (next)
            {
                case PatrollerState.Patrol:
                    waypointIndex = NearestWaypointIndex();
                    pausedUntil = 0f;
                    ResetProgressCheck();
                    break;
                case PatrollerState.WindUp:
                    BeginWindUp();
                    break;
                case PatrollerState.Recover:
                    Strike();
                    break;
                case PatrollerState.Search:
                    lookingAround = false;
                    ResetProgressCheck();
                    break;
            }
        }

        void BeginWindUp()
        {
            Vector2 direction = TargetPosition - Position;
            if (direction.sqrMagnitude < 0.0001f) direction = motor.Facing;
            motor.Stop();
            motor.Face(direction);
            strikeCenter = Position + direction.normalized * strikeReach;
            telegraph.ShowCircle(strikeCenter, strikeRadius);
        }

        void Strike()
        {
            nextAttackTime = Time.time + attackCooldown;
            telegraph.Flash();
            if (strikeEffect != null) Instantiate(strikeEffect, strikeCenter, Quaternion.identity);
            if (!HasLivingTarget) return;
            if ((TargetPosition - strikeCenter).sqrMagnitude > strikeRadius * strikeRadius) return;
            if (!vision.HasLineOfSight(TargetPosition)) return;
            target.TakeDamage(attackDamage);
        }

        void StartLookingAround()
        {
            lookingAround = true;
            lookAroundEnd = Time.time + lookAroundDuration;
            nextLookTurn = Time.time + lookTurnInterval;
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
            if (waypoints == null || waypoints.Length == 0) return 0;
            int best = 0;
            float bestSqr = float.MaxValue;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                float sqr = ((Vector2)waypoints[i].position - Position).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = i;
                }
            }
            return best;
        }

        void OnValidate()
        {
            holdDistance = Mathf.Min(holdDistance, attackRange);
        }

        void OnDrawGizmosSelected()
        {
            if (waypoints != null && waypoints.Length > 0)
            {
                Gizmos.color = new Color(0.4f, 0.9f, 1f, 0.9f);
                for (int i = 0; i < waypoints.Length; i++)
                {
                    Transform a = waypoints[i];
                    Transform b = waypoints[(i + 1) % waypoints.Length];
                    if (a == null || b == null) continue;
                    Gizmos.DrawWireSphere(a.position, 0.15f);
                    Gizmos.DrawLine(a.position, b.position);
                }
            }
            Gizmos.color = new Color(1f, 0.3f, 0.2f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
