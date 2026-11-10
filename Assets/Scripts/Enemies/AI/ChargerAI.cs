using UnityEngine;

namespace DungeonKnight
{
    public enum ChargerState { Prowl, Track, WindUp, Charge, Recover }

    public class ChargerAI : EnemyBrain<ChargerState>
    {
        [SerializeField] EnemyVision vision;
        [SerializeField] AttackTelegraph telegraph;
        [SerializeField] SpriteRenderer body;
        [SerializeField] Vector2 homeFacing = Vector2.down;
        [SerializeField] LayerMask obstacleMask = 1 << GameLayers.Obstacle;

        [Header("Prowl")]
        [SerializeField, Min(0f)] float prowlSpeed = 1.2f;
        [SerializeField, Min(0f)] float prowlRadius = 2.2f;
        [SerializeField, Min(0f)] float prowlPause = 1f;
        [SerializeField, Min(1)] int prowlPointAttempts = 8;
        [SerializeField, Min(0f)] float prowlClearance = 0.1f;
        [SerializeField, Min(0.1f)] float stuckCheckInterval = 2f;
        [SerializeField, Min(0f)] float stuckDistance = 0.3f;

        [Header("Track")]
        [SerializeField, Min(0f)] float trackSpeed = 1.5f;
        [SerializeField, Min(0f)] float chargeTriggerRange = 5f;
        [SerializeField, Min(0f)] float stalkDistance = 4f;

        [Header("Charge")]
        [SerializeField, Range(0.3f, 0.8f)] float windUpDuration = 0.65f;
        [SerializeField, Min(0f)] float windUpShake = 0.05f;
        [SerializeField, Min(0f)] float chargeSpeed = 11f;
        [SerializeField, Min(0f)] float chargeDistance = 6.5f;
        [SerializeField, Min(0f)] float chargeTimeMargin = 0.25f;
        [SerializeField, Min(0.1f)] float laneWidth = 0.9f;
        [SerializeField, Min(0f)] float hitRadius = 0.6f;
        [SerializeField, Min(1)] int attackDamage = 1;
        [SerializeField, Min(0f)] float wallCheckDistance = 0.25f;
        [SerializeField, Range(0.1f, 1f)] float wallCheckRadiusScale = 0.9f;

        [Header("Recover")]
        [SerializeField, Min(0f)] float recoveryDuration = 0.8f;
        [SerializeField, Min(0f)] float wallStunDuration = 1f;
        [SerializeField, Min(0f)] float chargeCooldown = 2f;
        [SerializeField] Color stunnedTint = new Color(0.6f, 0.65f, 1f, 1f);
        [SerializeField, Min(0f)] float hitAlertLookDuration = 1.2f;
        [SerializeField, Range(1f, 180f)] float prowlTurnStep = 135f;

        CircleCollider2D bodyCollider;
        Vector2 homePosition;
        Vector2 prowlTarget;
        bool hasProwlTarget;
        float prowlPauseUntil;
        Vector2 progressAnchor;
        float nextProgressCheck;
        Vector2 chargeDirection = Vector2.right;
        Vector2 chargeStart;
        Vector2 previousChargePosition;
        bool hitThisCharge;
        bool hitWall;
        float currentRecovery;
        float nextChargeTime;
        int prowlStep;
        Vector3 baseBodyPosition;

        protected override ChargerState InitialState => ChargerState.Prowl;

        public bool HitWallLastCharge => hitWall;

        float BodyRadius => bodyCollider != null ? bodyCollider.radius : 0.35f;
        Vector2 BodyCenter => Position + (bodyCollider != null ? bodyCollider.offset : Vector2.zero);

        protected override void Awake()
        {
            base.Awake();
            RefCheck.Valid(this, vision, nameof(vision));
            RefCheck.Valid(this, telegraph, nameof(telegraph));
            RefCheck.Valid(this, body, nameof(body));
            bodyCollider = GetComponent<CircleCollider2D>();
            if (body != null)
            {
                baseBodyPosition = body.transform.localPosition;
            }
        }

        void Start()
        {
            homePosition = Position;
            motor.Face(homeFacing);
            prowlPauseUntil = Time.time + prowlPause;
            ResetProgressCheck();
        }

        protected override void Tick()
        {
            if (vision == null || telegraph == null || body == null) return;
            switch (State)
            {
                case ChargerState.Prowl: TickProwl(); break;
                case ChargerState.Track: TickTrack(); break;
                case ChargerState.WindUp: TickWindUp(); break;
                case ChargerState.Charge: TickCharge(); break;
                case ChargerState.Recover: TickRecover(); break;
            }
        }

        void TickProwl()
        {
            if (CanDetectTarget())
            {
                ChangeState(ChargerState.Track);
                return;
            }
            if (Time.time < prowlPauseUntil)
            {
                motor.Stop();
                return;
            }
            if (!hasProwlTarget)
            {
                prowlTarget = PickProwlTarget();
                hasProwlTarget = true;
                ResetProgressCheck();
            }
            if (motor.MoveTo(prowlTarget, prowlSpeed) || IsStuck())
            {
                hasProwlTarget = false;
                prowlPauseUntil = Time.time + prowlPause;
            }
        }

        void TickTrack()
        {
            if (!CanTrackTarget())
            {
                ChangeState(ChargerState.Prowl);
                return;
            }

            Vector2 toTarget = TargetPosition - Position;
            float distance = toTarget.magnitude;
            if (Time.time >= nextChargeTime && distance <= chargeTriggerRange && IsLaneClear(toTarget, distance) && CanStartAttack)
            {
                ChangeState(ChargerState.WindUp);
                return;
            }

            if (distance > stalkDistance) motor.Move(toTarget, trackSpeed);
            else motor.Stop();
            motor.Face(toTarget);
        }

        void TickWindUp()
        {
            motor.Stop();
            if (!HasLivingTarget)
            {
                ChangeState(ChargerState.Prowl);
                return;
            }
            body.transform.localPosition = baseBodyPosition + (Vector3)(Random.insideUnitCircle * windUpShake);
            telegraph.SetProgress(TimeInState / windUpDuration);
            if (TimeInState >= windUpDuration) ChangeState(ChargerState.Charge);
        }

        void TickCharge()
        {
            motor.Dash(chargeDirection, chargeSpeed);
            Vector2 current = Position;
            if (!hitThisCharge && HasLivingTarget && SweptDistance(previousChargePosition, current, TargetPosition) <= hitRadius)
            {
                hitThisCharge = true;
                target.TakeDamage(attackDamage);
            }
            previousChargePosition = current;

            if (IsWallAhead())
            {
                hitWall = true;
                ChangeState(ChargerState.Recover);
                return;
            }
            float travelled = (current - chargeStart).magnitude;
            float maxTime = chargeSpeed > 0f ? chargeDistance / chargeSpeed + chargeTimeMargin : 0f;
            if (travelled >= chargeDistance || TimeInState >= maxTime) ChangeState(ChargerState.Recover);
        }

        void TickRecover()
        {
            motor.Stop();
            if (TimeInState < currentRecovery) return;
            ChangeState(CanTrackTarget() ? ChargerState.Track : ChargerState.Prowl);
        }

        protected override void OnStateChanged(ChargerState previous, ChargerState next)
        {
            if (previous == ChargerState.WindUp)
            {
                ClearBodyTint();
                body.transform.localPosition = baseBodyPosition;
                if (next != ChargerState.Charge) telegraph.Hide();
            }
            if (previous == ChargerState.Recover) ClearBodyTint();
            if (previous == ChargerState.Prowl && next == ChargerState.Track) IconAlert();

            switch (next)
            {
                case ChargerState.Prowl:
                    hasProwlTarget = false;
                    prowlPauseUntil = Time.time + prowlPause;
                    break;
                case ChargerState.WindUp:
                    BeginWindUp();
                    ShowWindUpTint();
                    break;
                case ChargerState.Charge:
                    BeginCharge();
                    break;
                case ChargerState.Recover:
                    BeginRecover();
                    break;
            }
        }

        void BeginWindUp()
        {
            motor.Halt();
            Vector2 toTarget = TargetPosition - Position;
            chargeDirection = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : motor.Facing;
            motor.Face(chargeDirection);
            telegraph.ShowLane(BodyCenter, chargeDirection, LaneLength(chargeDirection), laneWidth);
        }

        void BeginCharge()
        {
            chargeStart = Position;
            previousChargePosition = chargeStart;
            hitThisCharge = false;
            hitWall = false;
            telegraph.Flash();
            motor.Dash(chargeDirection, chargeSpeed);
        }

        void BeginRecover()
        {
            motor.Halt();
            nextChargeTime = Time.time + chargeCooldown;
            currentRecovery = hitWall ? wallStunDuration : recoveryDuration;
            TintBody(stunnedTint);
        }

        protected override void OnHit(Vector2 towardShooter)
        {
            vision.Alert();
            if (State != ChargerState.Prowl) return;
            motor.Stop();
            motor.Face(towardShooter);
            hasProwlTarget = false;
            prowlPauseUntil = Time.time + hitAlertLookDuration;
            IconSuspicious(hitAlertLookDuration);
        }

        Vector2 PickProwlTarget()
        {
            for (int i = 0; i < prowlPointAttempts; i++)
            {
                Vector2 direction = Quaternion.Euler(0f, 0f, prowlStep * prowlTurnStep) * Vector2.right;
                prowlStep++;
                Vector2 candidate = homePosition + direction * prowlRadius;
                if (Physics2D.OverlapCircle(candidate, BodyRadius + prowlClearance, obstacleMask) != null) continue;
                Vector2 toCandidate = candidate - BodyCenter;
                float distance = toCandidate.magnitude;
                if (distance < 0.0001f) continue;
                if (Physics2D.CircleCast(BodyCenter, BodyRadius, toCandidate / distance, distance, obstacleMask).collider != null) continue;
                return candidate;
            }
            return homePosition;
        }

        float LaneLength(Vector2 direction)
        {
            RaycastHit2D hit = Physics2D.CircleCast(BodyCenter, BodyRadius, direction, chargeDistance, obstacleMask);
            return (hit.collider != null ? hit.distance : chargeDistance) + BodyRadius;
        }

        bool IsLaneClear(Vector2 toTarget, float distance)
        {
            if (distance < 0.0001f) return true;
            return Physics2D.CircleCast(BodyCenter, BodyRadius, toTarget / distance, distance, obstacleMask).collider == null;
        }

        bool IsWallAhead()
        {
            return Physics2D.CircleCast(BodyCenter, BodyRadius * wallCheckRadiusScale, chargeDirection, wallCheckDistance, obstacleMask).collider != null;
        }

        static float SweptDistance(Vector2 from, Vector2 to, Vector2 point)
        {
            Vector2 segment = to - from;
            float lengthSqr = segment.sqrMagnitude;
            if (lengthSqr < 0.000001f) return Vector2.Distance(from, point);
            float t = Mathf.Clamp01(Vector2.Dot(point - from, segment) / lengthSqr);
            return Vector2.Distance(from + segment * t, point);
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

        void OnValidate()
        {
            stalkDistance = Mathf.Min(stalkDistance, chargeTriggerRange);
        }

        void OnDrawGizmosSelected()
        {
            Vector3 home = Application.isPlaying ? (Vector3)homePosition : transform.position;
            Gizmos.color = new Color(0.4f, 0.9f, 1f, 0.8f);
            Gizmos.DrawWireSphere(home, prowlRadius);
            Gizmos.color = new Color(1f, 0.55f, 0.1f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, chargeTriggerRange);
        }
    }
}
