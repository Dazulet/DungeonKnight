using UnityEngine;

namespace DungeonKnight
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyMotor : MonoBehaviour
    {
        [SerializeField, Min(0f)] float acceleration = 14f;
        [SerializeField, Min(0f)] float arriveDistance = 0.12f;
        [SerializeField, Min(0f)] float avoidanceLookAhead = 0.6f;
        [SerializeField, Range(0.1f, 1f)] float avoidanceRadiusScale = 0.85f;
        [SerializeField, Range(0f, 1f)] float avoidancePushAway = 0.15f;
        [SerializeField] LayerMask obstacleMask = 1 << GameLayers.Obstacle;
        [SerializeField] SpriteRenderer body;

        Rigidbody2D rb;
        CircleCollider2D circle;
        Vector2 desiredVelocity;
        float preferredSide = 1f;

        public Vector2 Facing { get; private set; } = Vector2.right;
        public Vector2 Position => rb.position;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            circle = GetComponent<CircleCollider2D>();
        }

        public bool MoveTo(Vector2 point, float speed)
        {
            Vector2 toPoint = point - rb.position;
            if (toPoint.sqrMagnitude <= arriveDistance * arriveDistance)
            {
                Stop();
                return true;
            }
            Move(toPoint, speed);
            return false;
        }

        public void Move(Vector2 direction, float speed)
        {
            if (direction.sqrMagnitude < 0.0001f)
            {
                Stop();
                return;
            }
            Face(direction);
            desiredVelocity = AvoidObstacles(direction.normalized) * speed;
        }

        public void Stop() => desiredVelocity = Vector2.zero;

        public void Dash(Vector2 direction, float speed)
        {
            if (direction.sqrMagnitude < 0.0001f) return;
            Face(direction);
            desiredVelocity = direction.normalized * speed;
            rb.SetVelocity(desiredVelocity);
        }

        public void Halt()
        {
            desiredVelocity = Vector2.zero;
            rb.SetVelocity(Vector2.zero);
        }

        public void Face(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f) return;
            Facing = direction.normalized;
            if (body != null && Mathf.Abs(direction.x) > 0.05f) body.flipX = direction.x < 0f;
        }

        void FixedUpdate()
        {
            Vector2 next = Vector2.MoveTowards(rb.GetVelocity(), desiredVelocity, acceleration * Time.fixedDeltaTime);
            rb.SetVelocity(next);
        }

        Vector2 AvoidObstacles(Vector2 direction)
        {
            if (avoidanceLookAhead <= 0f || circle == null) return direction;
            Vector2 origin = rb.position + circle.offset;
            float radius = circle.radius * avoidanceRadiusScale;
            RaycastHit2D hit = Physics2D.CircleCast(origin, radius, direction, avoidanceLookAhead, obstacleMask);
            if (hit.collider == null) return direction;

            Vector2 tangent = new Vector2(-hit.normal.y, hit.normal.x);
            float alignment = Vector2.Dot(tangent, direction);
            if (Mathf.Abs(alignment) > 0.15f) preferredSide = Mathf.Sign(alignment);
            return (tangent * preferredSide + hit.normal * avoidancePushAway).normalized;
        }
    }
}
