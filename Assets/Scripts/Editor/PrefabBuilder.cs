using UnityEngine;

namespace DungeonKnight.EditorTools
{
    public static class PrefabBuilder
    {
        const int EffectOrder = 2600;

        public static void BuildAll()
        {
            GameObject hit = BuildEffect("HitEffect", BuilderUtil.Frames("Effects/hit", 3), 18f, Color.white, 1f);
            GameObject puff = BuildEffect("DeathPuff", BuilderUtil.Frames("Effects/puff", 4), 14f, Color.white, 1.2f);
            BuildEffect("SpawnMarker", BuilderUtil.Frames("Effects/puff", 4), 10f, new Color(1f, 0.45f, 0.55f, 0.9f), 1f);
            GameObject sparkle = BuildEffect("CoinSparkle", BuilderUtil.Frames("Effects/hit", 3), 20f, new Color(1f, 0.9f, 0.35f), 0.7f);

            BuildProjectile(hit);
            GameObject coin = BuildCoin(sparkle);
            BuildEnemy("Slime", BuilderUtil.Frames("Enemies/slime", 2), 4f, health: 3, speed: 1.7f, score: 100,
                coinChance: 0.45f, wobble: 0f, radius: 0.36f, bodyHeight: 0f, puff, coin);
            BuildEnemy("Bat", BuilderUtil.Frames("Enemies/bat", 2), 10f, health: 2, speed: 3.0f, score: 150,
                coinChance: 0.3f, wobble: 1.2f, radius: 0.3f, bodyHeight: 0.3f, puff, coin);
            EnemyPrefabBuilder.BuildAll(puff, coin, hit);
            BuildKnight(puff);
        }

        static GameObject BuildEffect(string name, Sprite[] frames, float fps, Color tint, float scale)
        {
            var go = new GameObject(name);
            go.transform.localScale = Vector3.one * scale;
            BuilderUtil.AddSprite(go, frames[0], EffectOrder, tint);
            BuilderUtil.AddFlipbook(go, frames, fps, loop: false, destroyOnFinish: true);
            return BuilderUtil.SavePrefab(go, name);
        }

        internal static GameObject Shadow(GameObject parent, float y, float scale)
        {
            GameObject shadow = BuilderUtil.Child(parent, "Shadow", new Vector3(0f, y, 0f));
            shadow.transform.localScale = new Vector3(scale, scale, 1f);
            BuilderUtil.AddSprite(shadow, BuilderUtil.Sprite("Environment/shadow"), SortOrder.Shadow, new Color(1f, 1f, 1f, 0.6f));
            return shadow;
        }

        internal static Rigidbody2D AddBody(GameObject go, RigidbodyType2D type)
        {
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = type;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            return rb;
        }

        static void BuildProjectile(GameObject hitEffect)
        {
            var go = new GameObject("Projectile") { layer = GameLayers.Projectile };
            Sprite[] frames = BuilderUtil.Frames("Effects/bolt", 2);
            BuilderUtil.AddSprite(go, frames[0], SortOrder.Projectile);
            BuilderUtil.AddFlipbook(go, frames, 12f);

            GameObject glow = BuilderUtil.Child(go, "Glow");
            glow.transform.localScale = Vector3.one * 0.35f;
            BuilderUtil.AddSprite(glow, BuilderUtil.Sprite("Environment/glow"), SortOrder.Projectile - 1, new Color(0.45f, 0.85f, 1f, 0.55f));

            Rigidbody2D rb = AddBody(go, RigidbodyType2D.Dynamic);
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.18f;

            var projectile = go.AddComponent<Projectile>();
            BuilderUtil.Set(projectile, "hitEffect", hitEffect);
            BuilderUtil.SavePrefab(go, "Projectile");
        }

        static GameObject BuildCoin(GameObject sparkle)
        {
            var go = new GameObject("Coin") { layer = GameLayers.Pickup };
            Shadow(go, -0.32f, 0.5f);
            GameObject body = BuilderUtil.Child(go, "Body");
            Sprite[] frames = BuilderUtil.Frames("Environment/coin", 4);
            SpriteRenderer sr = BuilderUtil.AddSprite(body, frames[0], 0);
            BuilderUtil.AddFlipbook(body, frames, 8f, randomizeStart: true);
            BuilderUtil.Set(body.AddComponent<YSorter>(), "feetOffset", -0.3f);

            AddBody(go, RigidbodyType2D.Kinematic);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.32f;

            var coin = go.AddComponent<CoinPickup>();
            BuilderUtil.Set(coin, "body", sr);
            BuilderUtil.Set(coin, "collectEffect", sparkle);
            return BuilderUtil.SavePrefab(go, "Coin");
        }

        static void BuildEnemy(string name, Sprite[] frames, float fps, int health, float speed, int score,
            float coinChance, float wobble, float radius, float bodyHeight, GameObject deathEffect, GameObject coin)
        {
            var go = new GameObject(name) { layer = GameLayers.Enemy };
            Shadow(go, -0.45f, bodyHeight > 0f ? 0.7f : 0.9f);
            GameObject body = BuilderUtil.Child(go, "Body", new Vector3(0f, bodyHeight, 0f));
            SpriteRenderer sr = BuilderUtil.AddSprite(body, frames[0], 0);
            BuilderUtil.AddFlipbook(body, frames, fps, randomizeStart: true);
            BuilderUtil.Set(body.AddComponent<YSorter>(), "feetOffset", -0.45f - bodyHeight);

            Rigidbody2D rb = AddBody(go, RigidbodyType2D.Dynamic);
            rb.mass = 1f;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = radius;
            col.offset = new Vector2(0f, -0.15f);

            var flash = go.AddComponent<HitFlash>();
            BuilderUtil.Set(flash, "target", sr);

            var hp = go.AddComponent<EnemyHealth>();
            BuilderUtil.Set(hp, "maxHealth", health);
            BuilderUtil.Set(hp, "scoreValue", score);
            BuilderUtil.Set(hp, "coinDropChance", coinChance);
            BuilderUtil.Set(hp, "hitFlash", flash);
            BuilderUtil.Set(hp, "deathEffect", deathEffect);
            BuilderUtil.Set(hp, "coinPrefab", coin.GetComponent<CoinPickup>());

            var ai = go.AddComponent<EnemyController>();
            BuilderUtil.Set(ai, "moveSpeed", speed);
            BuilderUtil.Set(ai, "wobble", wobble);
            BuilderUtil.Set(ai, "body", sr);

            BuilderUtil.SavePrefab(go, name);
        }

        static void BuildKnight(GameObject deathEffect)
        {
            var go = new GameObject("Knight") { layer = GameLayers.Player };
            Shadow(go, -0.45f, 0.9f);
            GameObject body = BuilderUtil.Child(go, "Body");
            Sprite[] idle = BuilderUtil.Frames("Characters/knight_idle", 2);
            Sprite[] walk = BuilderUtil.Frames("Characters/knight_walk", 4);
            SpriteRenderer sr = BuilderUtil.AddSprite(body, idle[0], 0);
            SpriteFlipbook flipbook = BuilderUtil.AddFlipbook(body, idle, 3f);
            BuilderUtil.Set(body.AddComponent<YSorter>(), "feetOffset", -0.4f);
            GameObject muzzle = BuilderUtil.Child(go, "Muzzle", new Vector3(0f, -0.05f, 0f));

            AddBody(go, RigidbodyType2D.Dynamic);
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;
            col.offset = new Vector2(0f, -0.2f);

            var flash = go.AddComponent<HitFlash>();
            BuilderUtil.Set(flash, "target", sr);

            var input = go.AddComponent<PlayerInputReader>();

            var health = go.AddComponent<PlayerHealth>();
            BuilderUtil.Set(health, "body", sr);
            BuilderUtil.Set(health, "hitFlash", flash);
            BuilderUtil.Set(health, "deathEffect", deathEffect);

            var controller = go.AddComponent<PlayerController>();
            BuilderUtil.Set(controller, "input", input);
            BuilderUtil.Set(controller, "health", health);
            BuilderUtil.Set(controller, "body", sr);
            BuilderUtil.Set(controller, "animator", flipbook);
            BuilderUtil.Set(controller, "idleFrames", idle);
            BuilderUtil.Set(controller, "walkFrames", walk);

            var combat = go.AddComponent<PlayerCombat>();
            BuilderUtil.Set(combat, "input", input);
            BuilderUtil.Set(combat, "controller", controller);
            BuilderUtil.Set(combat, "health", health);
            BuilderUtil.Set(combat, "projectilePrefab", BuilderUtil.LoadPrefab<Projectile>("Projectile"));
            BuilderUtil.Set(combat, "muzzle", muzzle.transform);

            BuilderUtil.SavePrefab(go, "Knight");
        }
    }
}
