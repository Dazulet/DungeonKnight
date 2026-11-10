using UnityEngine;

namespace DungeonKnight
{
    public class EnemyVision : MonoBehaviour
    {
        [SerializeField] EnemyMotor motor;
        [SerializeField, Min(0f)] float viewDistance = 5.5f;
        [SerializeField, Range(1f, 360f)] float fieldOfView = 110f;
        [SerializeField] Vector2 eyeOffset = Vector2.zero;
        [SerializeField] LayerMask obstacleMask = 1 << GameLayers.Obstacle;
        [SerializeField, Range(0f, 0.3f)] float senseInterval = 0.1f;
        [SerializeField, Min(0f)] float senseCacheDistance = 0.1f;
        [SerializeField] bool drawSightRay = true;
        [SerializeField, Min(1f)] float alertRangeMultiplier = 1.6f;
        [SerializeField, Min(0f)] float alertDuration = 3f;

        float alertUntil = float.NegativeInfinity;

        Vector2 cachedPoint;
        Vector2 cachedEye;
        bool cachedLineOfSight;
        float nextSenseTime = float.NegativeInfinity;

        public float ViewDistance => viewDistance;
        public float FieldOfView => fieldOfView;
        public bool IsAlerted => Time.time < alertUntil;
        float CurrentViewDistance => IsAlerted ? viewDistance * alertRangeMultiplier : viewDistance;

        public void Alert() => alertUntil = Time.time + alertDuration;

        Vector2 Eye => (Vector2)transform.position + eyeOffset;
        Vector2 Facing => motor != null ? motor.Facing : Vector2.right;

        void Awake() => RefCheck.Valid(this, motor, nameof(motor));

        public bool CanSee(Vector2 point) => IsInRange(point) && IsInFieldOfView(point) && HasLineOfSight(point);

        public bool IsInRange(Vector2 point) => (point - Eye).sqrMagnitude <= CurrentViewDistance * CurrentViewDistance;

        public bool IsInFieldOfView(Vector2 point)
        {
            Vector2 toPoint = point - Eye;
            if (toPoint.sqrMagnitude < 0.0001f) return true;
            return Vector2.Angle(Facing, toPoint) <= fieldOfView * 0.5f;
        }

        public bool HasLineOfSight(Vector2 point)
        {
            Vector2 eye = Eye;
            float maxShift = senseCacheDistance * senseCacheDistance;
            bool fresh = Time.time < nextSenseTime && (point - cachedPoint).sqrMagnitude <= maxShift
                && (eye - cachedEye).sqrMagnitude <= maxShift;
            if (fresh) return cachedLineOfSight;
            cachedPoint = point;
            cachedEye = eye;
            cachedLineOfSight = HasLineOfSightNow(point);
            nextSenseTime = Time.time + senseInterval;
            return cachedLineOfSight;
        }

        public bool HasLineOfSightNow(Vector2 point)
        {
            Vector2 eye = Eye;
            Vector2 toPoint = point - eye;
            float distance = toPoint.magnitude;
            if (distance < 0.0001f) return true;
            bool clear = Physics2D.Raycast(eye, toPoint / distance, distance, obstacleMask).collider == null;
            if (drawSightRay) Debug.DrawLine(eye, point, clear ? Color.green : Color.red, senseInterval);
            return clear;
        }

        void OnDrawGizmosSelected()
        {
            Vector3 eye = Eye;
            Vector2 facing = Application.isPlaying ? Facing : Vector2.right;
            float half = fieldOfView * 0.5f;
            const int segments = 24;
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.8f);
            Vector3 previous = eye + (Vector3)(Rotate(facing, -half) * viewDistance);
            Gizmos.DrawLine(eye, previous);
            for (int i = 1; i <= segments; i++)
            {
                float angle = Mathf.Lerp(-half, half, i / (float)segments);
                Vector3 point = eye + (Vector3)(Rotate(facing, angle) * viewDistance);
                Gizmos.DrawLine(previous, point);
                previous = point;
            }
            Gizmos.DrawLine(eye, previous);
        }

        static Vector2 Rotate(Vector2 v, float degrees) => Quaternion.Euler(0f, 0f, degrees) * v;
    }
}
