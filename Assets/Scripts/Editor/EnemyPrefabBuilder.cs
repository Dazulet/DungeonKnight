using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DungeonKnight.EditorTools
{
    public static class EnemyPrefabBuilder
    {
        [MenuItem("Dungeon Knight/Rebuild Enemies", priority = 10)]
        public static void RebuildEnemies()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildAll(BuilderUtil.LoadPrefab("DeathPuff"), BuilderUtil.LoadPrefab("Coin"), BuilderUtil.LoadPrefab("HitEffect"));
            AssetDatabase.SaveAssets();
            Debug.Log("Dungeon Knight: enemy prefabs rebuilt. Levels spawn them from Assets/Data/Levels.");
        }

        public static void BuildAll(GameObject deathEffect, GameObject coin, GameObject hitEffect)
        {
            BuildPatroller(deathEffect, coin, hitEffect);
            EnemyBolt bolt = BuildEnemyBolt(hitEffect);
            BuildShooter(deathEffect, coin, bolt);
            BuildCharger(deathEffect, coin);
        }

        static void BuildPatroller(GameObject deathEffect, GameObject coin, GameObject hitEffect)
        {
            var stats = BuilderUtil.Data<PatrollerStats>("Enemies/PatrollerStats");
            GameObject go = CreateShell("Patroller", BuilderUtil.Frames("Enemies/patroller", 2), 5f,
                stats, radius: 0.34f, deathEffect, coin, out EnemyHealth health, out EnemyMotor motor);

            EnemyVision vision = AddVision(go, motor, stats);
            AttackTelegraph telegraph = AddTelegraph(go);

            var ai = go.AddComponent<PatrollerAI>();
            BuilderUtil.Set(ai, "stats", stats);
            BuilderUtil.Set(ai, "health", health);
            BuilderUtil.Set(ai, "motor", motor);
            BuilderUtil.Set(ai, "vision", vision);
            BuilderUtil.Set(ai, "telegraph", telegraph);
            BuilderUtil.Set(ai, "strikeEffect", hitEffect);
            AddReadability(go, ai);

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
            var stats = BuilderUtil.Data<ShooterStats>("Enemies/ShooterStats");
            GameObject go = CreateShell("Shooter", BuilderUtil.Frames("Enemies/shooter", 2), 3f,
                stats, radius: 0.32f, deathEffect, coin, out EnemyHealth health, out EnemyMotor motor);

            EnemyVision vision = AddVision(go, motor, stats);
            AttackTelegraph telegraph = AddTelegraph(go);

            var ai = go.AddComponent<ShooterAI>();
            BuilderUtil.Set(ai, "stats", stats);
            BuilderUtil.Set(ai, "health", health);
            BuilderUtil.Set(ai, "motor", motor);
            BuilderUtil.Set(ai, "vision", vision);
            BuilderUtil.Set(ai, "telegraph", telegraph);
            BuilderUtil.Set(ai, "boltPrefab", bolt);
            AddReadability(go, ai);

            BuilderUtil.SavePrefab(go, "Shooter");
        }

        static void BuildCharger(GameObject deathEffect, GameObject coin)
        {
            var stats = BuilderUtil.Data<ChargerStats>("Enemies/ChargerStats");
            GameObject go = CreateShell("Charger", BuilderUtil.Frames("Enemies/charger", 2), 6f,
                stats, radius: 0.4f, deathEffect, coin, out EnemyHealth health, out EnemyMotor motor);

            EnemyVision vision = AddVision(go, motor, stats);
            AttackTelegraph telegraph = AddTelegraph(go);

            var ai = go.AddComponent<ChargerAI>();
            BuilderUtil.Set(ai, "stats", stats);
            BuilderUtil.Set(ai, "health", health);
            BuilderUtil.Set(ai, "motor", motor);
            BuilderUtil.Set(ai, "vision", vision);
            BuilderUtil.Set(ai, "telegraph", telegraph);
            BuilderUtil.Set(ai, "body", go.transform.Find("Body").GetComponent<SpriteRenderer>());
            AddReadability(go, ai);

            BuilderUtil.SavePrefab(go, "Charger");
        }

        static GameObject CreateShell(string name, Sprite[] frames, float fps, EnemyStats stats, float radius,
            GameObject deathEffect, GameObject coin, out EnemyHealth enemyHealth, out EnemyMotor motor)
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
            BuilderUtil.Set(enemyHealth, "stats", stats);
            BuilderUtil.Set(enemyHealth, "hitFlash", flash);
            BuilderUtil.Set(enemyHealth, "deathEffect", deathEffect);
            BuilderUtil.Set(enemyHealth, "coinPrefab", coin.GetComponent<CoinPickup>());

            motor = go.AddComponent<EnemyMotor>();
            BuilderUtil.Set(motor, "body", sr);
            return go;
        }

        static EnemyVision AddVision(GameObject owner, EnemyMotor motor, SightedEnemyStats stats)
        {
            var vision = owner.AddComponent<EnemyVision>();
            BuilderUtil.Set(vision, "motor", motor);
            BuilderUtil.Set(vision, "stats", stats);
            return vision;
        }

        static void AddReadability(GameObject owner, MonoBehaviour brain)
        {
            GameObject iconObject = BuilderUtil.Child(owner, "Alert Icon", new Vector3(0f, 0.95f, 0f));
            SpriteRenderer iconRenderer = BuilderUtil.AddSprite(iconObject, BuilderUtil.Sprite("Effects/alert_exclaim"), SortOrder.EnemyAlert);
            iconRenderer.enabled = false;
            var icon = iconObject.AddComponent<EnemyAlertIcon>();
            BuilderUtil.Set(icon, "icon", iconRenderer);
            BuilderUtil.Set(icon, "alertSprite", BuilderUtil.Sprite("Effects/alert_exclaim"));
            BuilderUtil.Set(icon, "suspiciousSprite", BuilderUtil.Sprite("Effects/alert_question"));

            GameObject labelObject = BuilderUtil.Child(owner, "State Label", new Vector3(0f, -0.85f, 0f));
            var text = labelObject.AddComponent<TextMesh>();
            Font font = BuilderUtil.PixelFont();
            text.font = font;
            text.fontSize = 48;
            text.characterSize = 0.05f;
            text.anchor = TextAnchor.UpperCenter;
            text.alignment = TextAlignment.Center;
            text.color = new Color(1f, 1f, 1f, 0.85f);
            text.text = "State";
            var meshRenderer = labelObject.GetComponent<MeshRenderer>();
            if (font != null) meshRenderer.sharedMaterial = font.material;
            meshRenderer.sortingOrder = SortOrder.EnemyAlert + 1;
            var label = labelObject.AddComponent<EnemyStateLabel>();
            BuilderUtil.Set(label, "label", text);

            BuilderUtil.Set(brain, "hitFlash", owner.GetComponent<HitFlash>());
            BuilderUtil.Set(brain, "alertIcon", icon);
            BuilderUtil.Set(brain, "stateLabel", label);
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
