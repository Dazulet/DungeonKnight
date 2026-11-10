using UnityEditor;
using UnityEngine;

namespace DungeonKnight.EditorTools
{
    public static class EnemyEncounterBuilder
    {
        static readonly Vector2[][] PatrollerRoutes =
        {
            new[] { new Vector2(-5f, 4f), new Vector2(5f, 4f) },
            new[] { new Vector2(5f, -4f), new Vector2(-5f, -4f) },
        };

        public static void Build(PlayerHealth player, ScoreManager score)
        {
            var root = new GameObject("Encounter");
            Transform enemies = BuilderUtil.Child(root, "Enemies").transform;
            Transform routes = BuilderUtil.Child(root, "Patrol Routes").transform;

            GameObject patroller = BuilderUtil.LoadPrefab("Patroller");
            for (int i = 0; i < PatrollerRoutes.Length; i++)
                PlacePatroller(patroller, enemies, routes, $"Patroller {(char)('A' + i)}", PatrollerRoutes[i], player, score);
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
