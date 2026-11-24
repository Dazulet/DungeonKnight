using UnityEngine;

namespace DungeonKnight
{
    public enum ShooterState { Watch, Engage, Aim, Recover, Search }

    public class ShooterAI : EnemyBrain<ShooterState>
    {
        [SerializeField] EnemyVision vision;
        [SerializeField] AttackTelegraph telegraph;
        [SerializeField] ShooterStats stats;
        [SerializeField] EnemyBolt boltPrefab;
        [SerializeField] Vector2 muzzleOffset = Vector2.zero;
        [SerializeField] Vector2 postFacing = Vector2.right;
        [SerializeField] LayerMask obstacleMask = 1 << GameLayers.Obstacle;

        Vector2 postPosition;
        Vector2 aimDirection = Vector2.right;
        Vector2 lastKnownTargetPosition;
        float nextShotTime;
        float sidestepSign = 1f;
        float sweepClock;
        float retreatStartTime = -1f;
        Rigidbody2D targetBody;

        protected override ShooterState InitialState => ShooterState.Watch;
        protected override SightedEnemyStats SightStats => stats;

        Vector2 Muzzle => Position + muzzleOffset;
        float BoltRadius => boltPrefab != null ? boltPrefab.Radius : 0f;

        public override void Configure(PlayerHealth player, EnemySpawn spawn)
        {
            base.Configure(player, spawn);
            if (spawn.facing.sqrMagnitude > 0.0001f) postFacing = spawn.facing.normalized;
        }

        protected override void Awake()
        {
            base.Awake();
            RefCheck.Valid(this, stats, nameof(stats));
            RefCheck.Valid(this, vision, nameof(vision));
            RefCheck.Valid(this, telegraph, nameof(telegraph));
            RefCheck.Valid(this, boltPrefab, nameof(boltPrefab));
        }

        void Start()
        {
            postPosition = Position;
            if (postFacing.sqrMagnitude < 0.0001f) postFacing = Vector2.right;
            motor.Face(postFacing);
        }

        protected override void Tick()
        {
            if (vision == null || telegraph == null || boltPrefab == null) return;
            switch (State)
            {
                case ShooterState.Watch: TickWatch(); break;
                case ShooterState.Engage: TickEngage(); break;
                case ShooterState.Aim: TickAim(); break;
                case ShooterState.Recover: TickRecover(); break;
                case ShooterState.Search: TickSearch(); break;
            }
        }

        void TickWatch()
        {
            if (CanDetectTarget())
            {
                ChangeState(ShooterState.Engage);
                return;
            }
            if (!motor.MoveTo(postPosition, stats.returnSpeed)) return;
            sweepClock += Time.deltaTime;
            float sweep = Mathf.Sin(sweepClock * Mathf.PI * 2f / stats.watchSweepPeriod) * stats.watchSweepAngle;
            motor.Face(Rotate(postFacing, sweep));
        }

        void TickEngage()
        {
            if (!CanTrackTarget())
            {
                ChangeState(HasLivingTarget ? ShooterState.Search : ShooterState.Watch);
                return;
            }

            lastKnownTargetPosition = TargetPosition;
            Vector2 toTarget = lastKnownTargetPosition - Position;
            float distance = toTarget.magnitude;
            bool clearShot = HasClearShot(lastKnownTargetPosition);
            bool tooClose = distance < stats.minDistance;
            if (!tooClose) retreatStartTime = -1f;
            else if (retreatStartTime < 0f) retreatStartTime = Time.time;
            bool cornered = tooClose && Time.time - retreatStartTime >= stats.maxRetreatTime;
            bool goodRange = (!tooClose || cornered) && distance >= stats.minShootDistance && distance <= stats.maxDistance;

            if (Time.time >= nextShotTime && clearShot && goodRange && CanStartAttack)
            {
                ChangeState(ShooterState.Aim);
                return;
            }

            if (tooClose) motor.Move(-toTarget, stats.retreatSpeed);
            else if (distance > stats.maxDistance) motor.Move(toTarget, stats.approachSpeed);
            else if (!clearShot) motor.Move(new Vector2(-toTarget.y, toTarget.x) * sidestepSign, stats.sidestepSpeed);
            else motor.Stop();
            motor.Face(toTarget);
        }

        void TickAim()
        {
            motor.Stop();
            if (!HasLivingTarget)
            {
                ChangeState(ShooterState.Watch);
                return;
            }

            bool tracking = TimeInState < stats.aimDuration - stats.lockBeforeFire;
            if (tracking)
            {
                if (!vision.HasLineOfSight(TargetPosition))
                {
                    ChangeState(ShooterState.Search);
                    return;
                }
                Vector2 toTarget = PredictedTargetPosition() - Muzzle;
                if (toTarget.sqrMagnitude > 0.0001f) aimDirection = toTarget.normalized;
            }
            else
            {
                telegraph.SetLocked(true);
            }

            motor.Face(aimDirection);
            telegraph.UpdateLine(Muzzle, aimDirection, ShotLength(aimDirection));
            telegraph.SetProgress(TimeInState / stats.aimDuration);
            if (TimeInState >= stats.aimDuration) ChangeState(ShooterState.Recover);
        }

        void TickRecover()
        {
            motor.Stop();
            if (TimeInState < stats.recoveryDuration) return;
            if (CanTrackTarget()) ChangeState(ShooterState.Engage);
            else ChangeState(HasLivingTarget ? ShooterState.Search : ShooterState.Watch);
        }

        void TickSearch()
        {
            if (CanDetectTarget())
            {
                ChangeState(ShooterState.Engage);
                return;
            }
            motor.Stop();
            Vector2 toLastKnown = lastKnownTargetPosition - Position;
            if (toLastKnown.sqrMagnitude < 0.0001f) toLastKnown = motor.Facing;
            float sweep = Mathf.Sin(TimeInState * Mathf.PI * 2f / stats.searchSweepPeriod) * stats.searchSweepAngle;
            motor.Face(Rotate(toLastKnown, sweep));
            if (TimeInState >= stats.searchDuration) ChangeState(ShooterState.Watch);
        }

        protected override void OnStateChanged(ShooterState previous, ShooterState next)
        {
            if (previous == ShooterState.Aim)
            {
                ClearBodyTint();
                if (next != ShooterState.Recover)
                {
                    telegraph.Hide();
                    StopWarningSound();
                }
            }
            if (next == ShooterState.Engage && (previous == ShooterState.Watch || previous == ShooterState.Search)) IconAlert();
            if (next == ShooterState.Search) IconSuspicious(float.PositiveInfinity);
            if (previous == ShooterState.Search && next == ShooterState.Watch) IconClear();

            switch (next)
            {
                case ShooterState.Watch:
                    sweepClock = 0f;
                    break;
                case ShooterState.Engage:
                    retreatStartTime = -1f;
                    if (previous == ShooterState.Watch)
                        nextShotTime = Mathf.Max(nextShotTime, Time.time + stats.firstShotDelay);
                    sidestepSign = -sidestepSign;
                    break;
                case ShooterState.Aim:
                    BeginAim();
                    ShowWindUpTint();
                    break;
                case ShooterState.Recover:
                    Fire();
                    break;
            }
        }

        protected override void OnHit(Vector2 towardShooter)
        {
            vision.Alert();
            if (State != ShooterState.Watch && State != ShooterState.Search) return;
            lastKnownTargetPosition = Position + towardShooter * stats.hitAlertDistance;
            motor.Face(towardShooter);
            if (State == ShooterState.Watch)
            {
                ChangeState(ShooterState.Search);
                return;
            }
            RestartState();
            IconSuspicious(float.PositiveInfinity);
        }

        Vector2 PredictedTargetPosition()
        {
            if (targetBody == null) targetBody = target.GetComponent<Rigidbody2D>();
            if (targetBody == null || stats.projectileSpeed <= 0f) return TargetPosition;
            float distance = Vector2.Distance(TargetPosition, Muzzle);
            float remainingAim = Mathf.Max(0f, stats.aimDuration - TimeInState);
            float leadTime = remainingAim + distance / stats.projectileSpeed;
            return TargetPosition + targetBody.GetVelocity() * leadTime * stats.aimLeadFactor;
        }

        void BeginAim()
        {
            Vector2 toTarget = PredictedTargetPosition() - Muzzle;
            aimDirection = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : motor.Facing;
            motor.Face(aimDirection);
            telegraph.ShowLine(Muzzle, aimDirection, ShotLength(aimDirection));
            PlayWarningSound(SoundId.ShooterAim);
        }

        void Fire()
        {
            nextShotTime = Time.time + stats.shotCooldown;
            telegraph.Flash();
            PlayAttackSound(SoundId.ShooterFire, Muzzle);
            EnemyBolt bolt = Instantiate(boltPrefab, Muzzle, Quaternion.identity);
            bolt.Launch(aimDirection, stats.projectileSpeed, stats.attackDamage, stats.projectileRange);
        }

        float ShotLength(Vector2 direction)
        {
            RaycastHit2D hit = Physics2D.CircleCast(Muzzle, BoltRadius, direction, stats.projectileRange, obstacleMask);
            return hit.collider != null ? hit.distance : stats.projectileRange;
        }

        bool HasClearShot(Vector2 point)
        {
            Vector2 toPoint = point - Muzzle;
            float distance = toPoint.magnitude;
            if (distance < 0.0001f) return true;
            return Physics2D.CircleCast(Muzzle, BoltRadius, toPoint / distance, distance, obstacleMask).collider == null;
        }

        bool CanDetectTarget() => HasLivingTarget && vision.CanSee(TargetPosition);

        bool CanTrackTarget() => HasLivingTarget && vision.IsInRange(TargetPosition) && vision.HasLineOfSight(TargetPosition);

        static Vector2 Rotate(Vector2 v, float degrees) => Quaternion.Euler(0f, 0f, degrees) * v;


        void OnDrawGizmosSelected()
        {
            Vector3 center = Application.isPlaying ? (Vector3)postPosition : transform.position;
            Gizmos.color = new Color(0.4f, 0.9f, 1f, 0.9f);
            Gizmos.DrawWireCube(center, Vector3.one * 0.3f);
            if (stats == null) return;
            Gizmos.color = new Color(1f, 0.4f, 0.8f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, stats.minDistance);
            Gizmos.DrawWireSphere(transform.position, stats.maxDistance);
        }
    }
}
