using UnityEngine;

namespace DungeonKnight
{
    public class CoinPickup : MonoBehaviour
    {
        [SerializeField, Min(0f)] float magnetRange = 2.2f;
        [SerializeField, Min(0f)] float magnetSpeed = 9f;
        [SerializeField, Min(1f)] float lifetime = 25f;
        [SerializeField] SpriteRenderer body;
        [SerializeField] GameObject collectEffect;

        ScoreManager score;
        Transform player;
        Vector2 popVelocity;
        float age;
        bool collected;

        public void Init(ScoreManager scoreManager, Transform playerTransform)
        {
            score = scoreManager;
            player = playerTransform;
        }

        void Start() => popVelocity = Random.insideUnitCircle.normalized * 2.5f;

        void Update()
        {
            age += Time.deltaTime;
            Vector3 pos = transform.position;

            pos += (Vector3)(popVelocity * Time.deltaTime);
            popVelocity = Vector2.MoveTowards(popVelocity, Vector2.zero, 8f * Time.deltaTime);

            if (player != null && player.gameObject.activeInHierarchy)
            {
                Vector2 toPlayer = player.position - pos;
                if (toPlayer.sqrMagnitude < magnetRange * magnetRange)
                    pos = Vector3.MoveTowards(pos, player.position, magnetSpeed * Time.deltaTime);
            }
            transform.position = pos;

            if (body != null && age > lifetime - 3f)
                body.enabled = Mathf.Repeat(age, 0.2f) > 0.1f;
            if (age > lifetime) Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (collected || other.gameObject.layer != GameLayers.Player) return;
            collected = true;
            if (score != null) score.AddCoin();
            if (collectEffect != null) Instantiate(collectEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
