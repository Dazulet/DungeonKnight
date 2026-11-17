using UnityEngine;

namespace DungeonKnight
{
    public class CoinPickup : MonoBehaviour
    {
        [SerializeField] CoinData data;
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

        void Awake() => RefCheck.Valid(this, data, nameof(data));

        void Start() => popVelocity = Random.insideUnitCircle.normalized * 2.5f;

        void Update()
        {
            if (data == null) return;
            age += Time.deltaTime;
            Vector3 pos = transform.position;

            pos += (Vector3)(popVelocity * Time.deltaTime);
            popVelocity = Vector2.MoveTowards(popVelocity, Vector2.zero, 8f * Time.deltaTime);

            if (player != null && player.gameObject.activeInHierarchy)
            {
                Vector2 toPlayer = player.position - pos;
                if (toPlayer.sqrMagnitude < data.magnetRange * data.magnetRange)
                    pos = Vector3.MoveTowards(pos, player.position, data.magnetSpeed * Time.deltaTime);
            }
            transform.position = pos;

            if (body != null && age > data.lifetime - 3f)
                body.enabled = Mathf.Repeat(age, 0.2f) > 0.1f;
            if (age > data.lifetime) Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (collected || other.gameObject.layer != GameLayers.Player) return;
            collected = true;
            if (score != null) score.AddCoin(data != null ? data.points : 0);
            if (collectEffect != null) Instantiate(collectEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
