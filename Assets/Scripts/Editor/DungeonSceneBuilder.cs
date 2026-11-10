using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonKnight.EditorTools
{
    public static class DungeonSceneBuilder
    {
        static readonly string[] Layout =
        {
            "####T#####T#####T####",
            "#S........S........S#",
            "#...................#",
            "#..PP...........PP..#",
            "#..P...C.....C...P..#",
            "#...................#",
            "#.....B...@...B.....#",
            "#...................#",
            "#..P...C.....C...P..#",
            "#..PP...........PP..#",
            "#...................#",
            "#S...o....S........S#",
            "####T#####T#####T####",
        };

        static readonly Color Background = new Color(0.06f, 0.05f, 0.08f);
        static readonly Color TorchGlow = new Color(1f, 0.6f, 0.25f, 0.35f);

        static int Cols => Layout[0].Length;
        static int Rows => Layout.Length;

        static Vector2 TileToWorld(int col, int row) => new Vector2(col - Cols / 2, Rows / 2 - row);

        static bool IsSolid(char tile) => tile == '#' || tile == 'T' || tile == 'P' || tile == 'C' || tile == 'B';

        public static void Build()
        {
            foreach (string row in Layout)
                if (row.Length != Cols) throw new System.InvalidOperationException("Dungeon layout rows must all have the same length.");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera cam = BuildCamera();
            Vector2 playerStart = BuildArena(out List<Transform> spawnPoints);

            var knight = (GameObject)PrefabUtility.InstantiatePrefab(BuilderUtil.LoadPrefab("Knight"), scene);
            knight.transform.position = playerStart;
            var player = knight.GetComponent<PlayerHealth>();

            var game = new GameObject("Game");
            var gameManager = game.AddComponent<GameManager>();
            var score = game.AddComponent<ScoreManager>();
            BuilderUtil.Set(gameManager, "player", player);

            BuildSpawner(player, score, spawnPoints);
            EnemyEncounterBuilder.Build(player, score);
            BuilderUtil.Set(cam.GetComponent<CameraShake>(), "player", player);

            HudBuilder.Result hud = HudBuilder.Build(gameManager, score, player);
            var input = knight.GetComponent<PlayerInputReader>();
            BuilderUtil.Set(input, "touchMove", hud.Joystick);
            BuilderUtil.Set(input, "touchShoot", hud.ShootButton);

            UiFactory.EventSystem();

            BuilderUtil.EnsureFolder(BuilderUtil.SceneRoot);
            EditorSceneManager.SaveScene(scene, $"{BuilderUtil.SceneRoot}/{SceneNames.Dungeon}.unity");
        }

        static Camera BuildCamera()
        {
            Camera cam = UiFactory.Camera(Background, 6.9f);
            var framer = cam.gameObject.AddComponent<CameraFramer>();
            cam.gameObject.AddComponent<CameraShake>();

            GameObject vignette = BuilderUtil.Child(cam.gameObject, "Vignette", new Vector3(0f, 0f, 5f));
            SpriteRenderer overlay = BuilderUtil.AddSprite(vignette, BuilderUtil.Sprite("Environment/vignette"),
                SortOrder.Overlay, new Color(1f, 1f, 1f, 0.7f));

            BuilderUtil.Set(framer, "minHalfHeight", 6.9f);
            BuilderUtil.Set(framer, "minHalfWidth", 10.8f);
            BuilderUtil.Set(framer, "screenOverlay", overlay);
            return cam;
        }

        static Vector2 BuildArena(out List<Transform> spawnPoints)
        {
            var random = new System.Random(42);
            var arena = new GameObject("Arena");
            Transform floor = BuilderUtil.Child(arena, "Floor").transform;
            Transform walls = BuilderUtil.Child(arena, "Walls").transform;
            Transform props = BuilderUtil.Child(arena, "Props").transform;
            Transform spawnRoot = BuilderUtil.Child(arena, "Spawn Points").transform;

            Sprite[] floorSprites = BuilderUtil.Frames("Environment/floor", 4);
            Sprite[] wallSprites = BuilderUtil.Frames("Environment/wall", 2);
            Sprite[] torchFrames = BuilderUtil.Frames("Environment/torch", 2);
            Sprite glow = BuilderUtil.Sprite("Environment/glow");
            var propSprites = new Dictionary<char, Sprite>
            {
                { 'P', BuilderUtil.Sprite("Environment/pillar") },
                { 'C', BuilderUtil.Sprite("Environment/crate") },
                { 'B', BuilderUtil.Sprite("Environment/barrel") },
            };

            spawnPoints = new List<Transform>();
            Vector2 playerStart = Vector2.zero;

            for (int row = 0; row < Rows; row++)
            for (int col = 0; col < Cols; col++)
            {
                char tile = Layout[row][col];
                Vector2 pos = TileToWorld(col, row);
                int uprightOrder = SortOrder.ForY(pos.y - 0.5f);

                if (tile == '#' || tile == 'T')
                {
                    GameObject wall = Tile(walls, $"Wall {col},{row}", pos);
                    BuilderUtil.AddSprite(wall, wallSprites[random.NextDouble() < 0.8 ? 0 : 1], uprightOrder);
                    if (tile == 'T') AddTorch(wall, torchFrames, glow, uprightOrder);
                    continue;
                }

                double roll = random.NextDouble();
                Sprite floorSprite = roll < 0.7 ? floorSprites[0] : floorSprites[1 + (int)((roll - 0.7) / 0.1) % 3];
                BuilderUtil.AddSprite(Tile(floor, $"Floor {col},{row}", pos), floorSprite, SortOrder.Floor);

                switch (tile)
                {
                    case 'P':
                    case 'C':
                    case 'B':
                        BuilderUtil.AddSprite(Tile(props, $"{propSprites[tile].name} {col},{row}", pos), propSprites[tile], uprightOrder);
                        break;
                    case 'o':
                        BuilderUtil.AddSprite(Tile(props, $"Bones {col},{row}", pos), BuilderUtil.Sprite("Environment/bones"), SortOrder.FloorDecor);
                        break;
                    case 'S':
                        spawnPoints.Add(Tile(spawnRoot, $"Spawn {spawnPoints.Count + 1}", pos).transform);
                        break;
                    case '@':
                        playerStart = pos;
                        break;
                }
            }

            BuildColliders(arena);
            return playerStart;
        }

        static GameObject Tile(Transform parent, string name, Vector2 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            return go;
        }

        static void AddTorch(GameObject wall, Sprite[] frames, Sprite glow, int wallOrder)
        {
            GameObject torch = BuilderUtil.Child(wall, "Torch");
            BuilderUtil.AddSprite(torch, frames[0], wallOrder + 1);
            BuilderUtil.AddFlipbook(torch, frames, 6f, randomizeStart: true);

            GameObject light = BuilderUtil.Child(wall, "Glow", new Vector3(0f, -0.4f, 0f));
            light.transform.localScale = Vector3.one * 0.9f;
            BuilderUtil.AddSprite(light, glow, SortOrder.Glow, TorchGlow);
            light.AddComponent<TorchFlicker>();
        }

        static void BuildColliders(GameObject arena)
        {
            GameObject root = BuilderUtil.Child(arena, "Colliders");
            root.layer = GameLayers.Obstacle;
            for (int row = 0; row < Rows; row++)
            {
                int col = 0;
                while (col < Cols)
                {
                    if (!IsSolid(Layout[row][col])) { col++; continue; }
                    int start = col;
                    while (col < Cols && IsSolid(Layout[row][col])) col++;
                    int length = col - start;

                    var box = root.AddComponent<BoxCollider2D>();
                    Vector2 left = TileToWorld(start, row);
                    box.offset = new Vector2(left.x + (length - 1) * 0.5f, left.y);
                    box.size = new Vector2(length, 1f);
                }
            }
        }

        static void BuildSpawner(PlayerHealth player, ScoreManager score, List<Transform> spawnPoints)
        {
            var go = new GameObject("Enemy Spawner");
            var spawner = go.AddComponent<EnemySpawner>();
            BuilderUtil.Set(spawner, "player", player);
            BuilderUtil.Set(spawner, "score", score);
            BuilderUtil.Set(spawner, "spawnPoints", spawnPoints);
            BuilderUtil.Set(spawner, "spawnEffect", BuilderUtil.LoadPrefab("SpawnMarker"));

            var so = new SerializedObject(spawner);
            SerializedProperty types = so.FindProperty("enemyTypes");
            types.arraySize = 2;
            SetEnemyType(types.GetArrayElementAtIndex(0), BuilderUtil.LoadPrefab("Slime"), weight: 3f, unlockTime: 0f);
            SetEnemyType(types.GetArrayElementAtIndex(1), BuilderUtil.LoadPrefab("Bat"), weight: 2f, unlockTime: 20f);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetEnemyType(SerializedProperty element, GameObject prefab, float weight, float unlockTime)
        {
            element.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            element.FindPropertyRelative("weight").floatValue = weight;
            element.FindPropertyRelative("unlockTime").floatValue = unlockTime;
        }
    }
}
