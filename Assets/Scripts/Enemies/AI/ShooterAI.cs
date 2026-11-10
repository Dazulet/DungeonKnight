using UnityEngine;

namespace DungeonKnight
{
    public enum ShooterState { Watch, Engage, Aim, Recover, Search }

    public class ShooterAI : EnemyBrain<ShooterState>
    {
        [SerializeField] EnemyVision vision;
        [SerializeField] AttackTelegraph telegraph;
        [SerializeField] EnemyBolt boltPrefab;
        [SerializeField] Vector2 muzzleOffset = Vector2.zero;
        [SerializeField] Vector2 postFacing = Vector2.right;
        [SerializeField] LayerMask obstacleMask = 1 << GameLayers.Obstacle;

        [Header("Watch")]
        [SerializeField, Min(0f)] float returnSpeed = 1.5f;
        [SerializeField, Range(0f, 90f)] float watchSweepAngle = 40f;
        [SerializeField, Min(0.1f)] float watchSweepPeriod = 3.2f;

        [Header("Spacing")]
        [SerializeField, Min(0f)] float minDistance = 2.5f;
        [SerializeField, Min(0f)] float maxDistance = 7f;
        [SerializeField, Min(0f)] float retreatSpeed = 1.7f;
        [SerializeField, Min(0f)] float approachSpeed = 1.8f;
        [SerializeField, Min(0f)] float sidestepSpeed = 1.6f;
        [SerializeField, Min(0f)] float minShootDistance = 2f;
        [SerializeField, Min(0f)] float maxRetreatTime = 1.2f;

        [Header("Attack")]
        [SerializeField, Range(0.3f, 0.8f)] float aimDuration = 0.7f;
        [SerializeField, Min(0f)] float lockBeforeFire = 0.25f;
        [SerializeField, Range(0f, 1f)] float aimLeadFactor = 0.6f;
        [SerializeField, Min(0f)] float recoveryDuration = 0.35f;
        [SerializeField, Min(0f)] float shotCooldown = 1.8f;
        [SerializeField, Min(0f)] float firstShotDelay = 0.4f;
        [SerializeField, Min(0f)] float projectileSpeed = 6.5f;
        [SerializeField, Min(0f)] float projectileRange = 9f;
        [SerializeField, Min(1)] int attackDamage = 1;

        [Header("Search")]
        [SerializeField, Min(0f)] float searchDuration = 2.5f;
        [SerializeField, Range(0f, 90f)] float searchSweepAngle = 60f;
        [SerializeField, Min(0.1f)] float searchSweepPeriod = 1.6f;

        Vector2 postPosition;
        Vector2 aimDirection = Vector2.right;
        Vector2 lastKnownTargetPosition;
        float nextShotTime;
        float sidestepSign = 1f;
        float sweepClock;
        float retreatStartTime = -1f;
        Rigidbody2D targetBody;

        protected override ShooterState InitialState => ShooterState.Watch;

        Vector2 Muzzle => Position + muzzleOffset;
        float BoltRadius => boltPrefab != null ? boltPrefab.Radius : 0f;

        protected override void Awake()
        {
            base.Awake();
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
            if (!motor.MoveTo(postPosition, returnSpeed)) return;
            sweepClock += Time.deltaTime;
            float sweep = Mathf.Sin(sweepClock * Mathf.PI * 2f / watchSweepPeriod) * watchSweepAngle;
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
            bool tooClose = distance < minDistance;
            if (!tooClose) retreatStartTime = -1f;
            else if (retreatStartTime < 0f) retreatStartTime = Time.time;
            bool cornered = tooClose && Time.time - retreatStartTime >= maxRetreatTime;
            bool goodRange = (!tooClose || cornered) && distance >= minShootDistance && distance <= maxDistance;

            if (Time.time >= nextShotTime && clearShot && goodRange)
            {
                ChangeState(ShooterState.Aim);
                return;
            }

            if (tooClose) motor.Move(-toTarget, retreatSpeed);
            else if (distance > maxDistance) motor.Move(toTarget, approachSpeed);
            else if (!clearShot) motor.Move(new Vector2(-toTarget.y, toTarget.x) * sidestepSign, sidestepSpeed);
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

            bool tracking = TimeInState < aimDuration - lockBeforeFire;
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
            telegraph.SetProgress(TimeInState / aimDuration);
            if (TimeInState >= aimDuration) ChangeState(ShooterState.Recover);
        }

        void TickRecover()
        {
            motor.Stop();
            if (TimeInState < recoveryDuration) return;
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
            float sweep = Mathf.Sin(TimeInState * Mathf.PI * 2f / searchSweepPeriod) * searchSweepAngle;
            motor.Face(Rotate(toLastKnown, sweep));
            if (TimeInState >= searchDuration) ChangeState(ShooterState.Watch);
        }

        protected override void OnStateChanged(ShooterState previous, ShooterState next)
        {
            if (previous == ShooterState.Aim && next != ShooterState.Recover) telegraph.Hide();

            switch (next)
            {
                case ShooterState.Watch:
                    sweepClock = 0f;
                    break;
                case ShooterState.Engage:
                    retreatStartTime = -1f;
                    if (previous == ShooterState.Watch)
                        nextShotTime = Mathf.Max(nextShotTime, Time.time + firstShotDelay);
                    sidestepSign = Random.value < 0.5f ? -1f : 1f;
                    break;
                case ShooterState.Aim:
                    BeginAim();
                    break;
                case ShooterState.Recover:
                    Fire();
                    break;
            }
        }

        Vector2 PredictedTargetPosition()
        {
            if (targetBody == null) targetBody = target.GetComponent<Rigidbody2D>();
            if (targetBody == null || projectileSpeed <= 0f) return TargetPosition;
            float distance = Vector2.Distance(TargetPosition, Muzzle);
            float remainingAim = Mathf.Max(0f, aimDuration - TimeInState);
            float leadTime = remainingAim + distance / projectileSpeed;
            return TargetPosition + targetBody.GetVelocity() * leadTime * aimLeadFactor;
        }

        void BeginAim()
        {
            Vector2 toTarget = PredictedTargetPosition() - Muzzle;
            aimDirection = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : motor.Facing;
            motor.Face(aimDirection);
            telegraph.ShowLine(Muzzle, aimDirection, ShotLength(aimDirection));
        }

        void Fire()
        {
            nextShotTime = Time.time + shotCooldown;
            telegraph.Flash();
            EnemyBolt bolt = Instantiate(boltPrefab, Muzzle, Quaternion.identity);
            bolt.Launch(aimDirection, projectileSpeed, attackDamage, projectileRange);
        }

        float ShotLength(Vector2 direction)
        {
            RaycastHit2D hit = Physics2D.CircleCast(Muzzle, BoltRadius, direction, projectileRange, obstacleMask);
            return hit.collider != null ? hit.distance : projectileRange;
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
            Gizmos.color = new Color(1f, 0.4f, 0.8f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, minDistance);
            Gizmos.DrawWireSphere(transform.position, maxDistance);
            Gizmos.color = new Color(0.4f, 0.9f, 1f, 0.9f);
            Gizmos.DrawWireCube(center, Vector3.one * 0.3f);
        }
    }
}
