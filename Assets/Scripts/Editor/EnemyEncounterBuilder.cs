using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonKnight.EditorTools
{
    public static class EnemyEncounterBuilder
    {
        const string RootName = "Encounter";

        static readonly Vector2[][] PatrollerRoutes =
        {
            new[] { new Vector2(-5f, 4f), new Vector2(5f, 4f) },
            new[] { new Vector2(5f, -4f), new Vector2(-5f, -4f) },
        };

        static readonly (Vector2 position, Vector2 facing)[] ShooterPosts =
        {
            (new Vector2(-8f, 0f), Vector2.right),
            (new Vector2(8f, 0f), Vector2.left),
        };

        static readonly (Vector2 position, Vector2 facing)[] ChargerHomes =
        {
            (new Vector2(0f, -3f), Vector2.down),
        };

        [MenuItem("Dungeon Knight/Rebuild Enemies", priority = 10)]
        public static void RebuildEnemies()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EnemyPrefabBuilder.BuildAll(BuilderUtil.LoadPrefab("DeathPuff"), BuilderUtil.LoadPrefab("Coin"),
                BuilderUtil.LoadPrefab("HitEffect"));

            Scene scene = EditorSceneManager.OpenScene($"{BuilderUtil.SceneRoot}/{SceneNames.Dungeon}.unity");
            GameObject existing = GameObject.Find(RootName);
            if (existing != null) Object.DestroyImmediate(existing);

            var player = Object.FindAnyObjectByType<PlayerHealth>();
            var score = Object.FindAnyObjectByType<ScoreManager>();
            if (player == null || score == null)
            {
                Debug.LogError("Dungeon Knight: the Dungeon scene has no player or score manager. Run 'Build Project' first.");
                return;
            }

            Build(player, score);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Dungeon Knight: enemy prefabs and encounter rebuilt.");
        }

        public static void Build(PlayerHealth player, ScoreManager score)
        {
            var root = new GameObject(RootName);
            Transform enemies = BuilderUtil.Child(root, "Enemies").transform;
            Transform routes = BuilderUtil.Child(root, "Patrol Routes").transform;

            GameObject patroller = BuilderUtil.LoadPrefab("Patroller");
            for (int i = 0; i < PatrollerRoutes.Length; i++)
                PlacePatroller(patroller, enemies, routes, $"Patroller {(char)('A' + i)}", PatrollerRoutes[i], player, score);

            GameObject shooter = BuilderUtil.LoadPrefab("Shooter");
            for (int i = 0; i < ShooterPosts.Length; i++)
                PlaceShooter(shooter, enemies, $"Shooter {(char)('A' + i)}", ShooterPosts[i].position, ShooterPosts[i].facing, player, score);

            GameObject charger = BuilderUtil.LoadPrefab("Charger");
            for (int i = 0; i < ChargerHomes.Length; i++)
                PlaceCharger(charger, enemies, $"Charger {(char)('A' + i)}", ChargerHomes[i].position, ChargerHomes[i].facing, player, score);
        }

        static void PlacePatroller(GameObject prefab, Transform enemies, Transform routes, string name, Vector2[] route,
            PlayerHealth player, ScoreManager score)
        {
            GameObject routeRoot = BuilderUtil.Child(routes.gameObject, $"{name} Route");
            var points = new Transform[route.Length];
            for (int i = 0; i < route.Length; i++)
            {
                GameObject point = BuilderUtil.Child(routeRoot, $"Point {i + 1}");
                point.transform.position = route[i];
                points[i] = point.transform;
            }

            GameObject enemy = PlaceEnemy(prefab, enemies, name, route[0], score, player);
            var ai = enemy.GetComponent<PatrollerAI>();
            BuilderUtil.Set(ai, "target", player);
            BuilderUtil.Set(ai, "waypoints", points);
        }

        static void PlaceShooter(GameObject prefab, Transform enemies, string name, Vector2 position, Vector2 facing,
            PlayerHealth player, ScoreManager score)
        {
            GameObject enemy = PlaceEnemy(prefab, enemies, name, position, score, player);
            var ai = enemy.GetComponent<ShooterAI>();
            BuilderUtil.Set(ai, "target", player);
            BuilderUtil.Set(ai, "postFacing", facing);
        }

        static void PlaceCharger(GameObject prefab, Transform enemies, string name, Vector2 position, Vector2 facing,
            PlayerHealth player, ScoreManager score)
        {
            GameObject enemy = PlaceEnemy(prefab, enemies, name, position, score, player);
            var ai = enemy.GetComponent<ChargerAI>();
            BuilderUtil.Set(ai, "target", player);
            BuilderUtil.Set(ai, "homeFacing", facing);
        }

        static GameObject PlaceEnemy(GameObject prefab, Transform parent, string name, Vector2 position,
            ScoreManager score, PlayerHealth player)
        {
            var enemy = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            enemy.name = name;
            enemy.transform.position = position;
            var health = enemy.GetComponent<EnemyHealth>();
            BuilderUtil.Set(health, "score", score);
            BuilderUtil.Set(health, "player", player.transform);
            return enemy;
        }
    }
}
