using UnityEngine;

namespace DungeonKnight.EditorTools
{
    public static class EnemyPrefabBuilder
    {
        public static void BuildAll(GameObject deathEffect, GameObject coin, GameObject hitEffect)
        {
            BuildPatroller(deathEffect, coin, hitEffect);
            EnemyBolt bolt = BuildEnemyBolt(hitEffect);
            BuildShooter(deathEffect, coin, bolt);
        }

        static void BuildPatroller(GameObject deathEffect, GameObject coin, GameObject hitEffect)
        {
            GameObject go = CreateShell("Patroller", BuilderUtil.Frames("Enemies/patroller", 2), 5f,
                health: 4, score: 200, coinChance: 0.6f, radius: 0.34f, deathEffect, coin,
                out EnemyHealth health, out EnemyMotor motor);

            var vision = go.AddComponent<EnemyVision>();
            BuilderUtil.Set(vision, "motor", motor);
            AttackTelegraph telegraph = AddTelegraph(go);

            var ai = go.AddComponent<PatrollerAI>();
            BuilderUtil.Set(ai, "health", health);
            BuilderUtil.Set(ai, "motor", motor);
            BuilderUtil.Set(ai, "vision", vision);
            BuilderUtil.Set(ai, "telegraph", telegraph);
            BuilderUtil.Set(ai, "strikeEffect", hitEffect);

            BuilderUtil.SavePrefab(go, "Patroller");
        }

        static EnemyBolt BuildEnemyBolt(GameObject hitEffect)
        {
            var go = new GameObject("EnemyBolt");
            Sprite[] frames = BuilderUtil.Frames("Effects/enemy_bolt", 2);
            BuilderUtil.AddSprite(go, frames[0], SortOrder.Projectile);
            BuilderUtil.AddFlipbook(go, frames, 12f);

            GameObject glow = BuilderUtil.Child(go, "Glow");
            glow.transform.localScale = Vector3.one * 0.45f;
            BuilderUtil.AddSprite(glow, BuilderUtil.Sprite("Environment/glow"), SortOrder.Projectile - 1, new Color(1f, 0.3f, 0.8f, 0.6f));

            var bolt = go.AddComponent<EnemyBolt>();
            BuilderUtil.Set(bolt, "hitEffect", hitEffect);
            return BuilderUtil.SavePrefab(go, "EnemyBolt").GetComponent<EnemyBolt>();
        }

        static void BuildShooter(GameObject deathEffect, GameObject coin, EnemyBolt bolt)
        {
            GameObject go = CreateShell("Shooter", BuilderUtil.Frames("Enemies/shooter", 2), 3f,
                health: 3, score: 250, coinChance: 0.6f, radius: 0.32f, deathEffect, coin,
                out EnemyHealth health, out EnemyMotor motor);

            var vision = go.AddComponent<EnemyVision>();
            BuilderUtil.Set(vision, "motor", motor);
            BuilderUtil.Set(vision, "viewDistance", 7.5f);
            BuilderUtil.Set(vision, "fieldOfView", 120f);
            AttackTelegraph telegraph = AddTelegraph(go);

            var ai = go.AddComponent<ShooterAI>();
            BuilderUtil.Set(ai, "health", health);
            BuilderUtil.Set(ai, "motor", motor);
            BuilderUtil.Set(ai, "vision", vision);
            BuilderUtil.Set(ai, "telegraph", telegraph);
            BuilderUtil.Set(ai, "boltPrefab", bolt);

            BuilderUtil.SavePrefab(go, "Shooter");
        }

        static GameObject CreateShell(string name, Sprite[] frames, float fps, int health, int score, float coinChance,
            float radius, GameObject deathEffect, GameObject coin, out EnemyHealth enemyHealth, out EnemyMotor motor)
        {
            var go = new GameObject(name) { layer = GameLayers.Enemy };
            PrefabBuilder.Shadow(go, -0.45f, 0.9f);
            GameObject body = BuilderUtil.Child(go, "Body");
            SpriteRenderer sr = BuilderUtil.AddSprite(body, frames[0], 0);
            BuilderUtil.AddFlipbook(body, frames, fps, randomizeStart: true);
            BuilderUtil.Set(body.AddComponent<YSorter>(), "feetOffset", -0.45f);

            Rigidbody2D rb = PrefabBuilder.AddBody(go, RigidbodyType2D.Dynamic);
            rb.mass = 1f;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = radius;
            col.offset = new Vector2(0f, -0.15f);

            var flash = go.AddComponent<HitFlash>();
            BuilderUtil.Set(flash, "target", sr);

            enemyHealth = go.AddComponent<EnemyHealth>();
            BuilderUtil.Set(enemyHealth, "maxHealth", health);
            BuilderUtil.Set(enemyHealth, "scoreValue", score);
            BuilderUtil.Set(enemyHealth, "coinDropChance", coinChance);
            BuilderUtil.Set(enemyHealth, "hitFlash", flash);
            BuilderUtil.Set(enemyHealth, "deathEffect", deathEffect);
            BuilderUtil.Set(enemyHealth, "coinPrefab", coin.GetComponent<CoinPickup>());

            motor = go.AddComponent<EnemyMotor>();
            BuilderUtil.Set(motor, "body", sr);
            return go;
        }

        static AttackTelegraph AddTelegraph(GameObject owner)
        {
            GameObject root = BuilderUtil.Child(owner, "Telegraph");
            SpriteRenderer fill = BuilderUtil.AddSprite(BuilderUtil.Child(root, "Fill"),
                BuilderUtil.Sprite("Effects/telegraph_fill"), SortOrder.Telegraph);
            SpriteRenderer outline = BuilderUtil.AddSprite(BuilderUtil.Child(root, "Outline"),
                BuilderUtil.Sprite("Effects/telegraph_ring"), SortOrder.Telegraph + 1);
            fill.enabled = false;
            outline.enabled = false;

            var telegraph = root.AddComponent<AttackTelegraph>();
            BuilderUtil.Set(telegraph, "outline", outline);
            BuilderUtil.Set(telegraph, "fill", fill);
            BuilderUtil.Set(telegraph, "circleOutlineSprite", outline.sprite);
            BuilderUtil.Set(telegraph, "circleFillSprite", fill.sprite);
            BuilderUtil.Set(telegraph, "lineSprite", BuilderUtil.Sprite("Effects/telegraph_line"));
            return telegraph;
        }
    }
}
