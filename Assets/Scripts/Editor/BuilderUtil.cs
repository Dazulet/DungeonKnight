using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DungeonKnight.EditorTools
{
    public static class BuilderUtil
    {
        public const string ArtRoot = "Assets/Art";
        public const string PrefabRoot = "Assets/Prefabs";
        public const string SceneRoot = "Assets/Scenes";
        public const string FontPath = "Assets/Fonts/PixelFont.ttf";

        public static Sprite Sprite(string path)
        {
            string full = $"{ArtRoot}/{path}.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(full);
            if (sprite == null) Debug.LogError($"Sprite not found at {full}. Run 'Dungeon Knight/Build Project' again after the import finishes.");
            return sprite;
        }

        public static Sprite[] Frames(string pathPrefix, int count)
        {
            var frames = new Sprite[count];
            for (int i = 0; i < count; i++) frames[i] = Sprite($"{pathPrefix}_{i}");
            return frames;
        }

        public static Font PixelFont()
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (font == null) Debug.LogError($"Font not found at {FontPath}.");
            return font;
        }

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        public static void Set(Object target, string field, object value)
        {
            var so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(field);
            if (property == null)
            {
                Debug.LogError($"{target.GetType().Name} has no serialized field '{field}'.", target);
                return;
            }
            Assign(property, value);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void Assign(SerializedProperty property, object value)
        {
            switch (value)
            {
                case null:
                    property.objectReferenceValue = null;
                    break;
                case Object obj:
                    property.objectReferenceValue = obj;
                    break;
                case float f:
                    property.floatValue = f;
                    break;
                case int i when property.propertyType == SerializedPropertyType.Float:
                    property.floatValue = i;
                    break;
                case int i:
                    property.intValue = i;
                    break;
                case bool b:
                    property.boolValue = b;
                    break;
                case Color c:
                    property.colorValue = c;
                    break;
                case Vector2 v:
                    property.vector2Value = v;
                    break;
                case string s:
                    property.stringValue = s;
                    break;
                case IList list:
                    property.arraySize = list.Count;
                    for (int n = 0; n < list.Count; n++) Assign(property.GetArrayElementAtIndex(n), list[n]);
                    break;
                default:
                    Debug.LogError($"BuilderUtil.Set cannot assign a {value.GetType().Name} to '{property.propertyPath}'.");
                    break;
            }
        }

        public static GameObject Child(GameObject parent, string name, Vector3 localPosition = default)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localPosition;
            go.layer = parent.layer;
            return go;
        }

        public static SpriteRenderer AddSprite(GameObject go, Sprite sprite, int order, Color? color = null)
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            if (color.HasValue) sr.color = color.Value;
            return sr;
        }

        public static SpriteFlipbook AddFlipbook(GameObject go, Sprite[] frames, float fps, bool loop = true,
            bool destroyOnFinish = false, bool randomizeStart = false, bool unscaledTime = false)
        {
            var flipbook = go.AddComponent<SpriteFlipbook>();
            Set(flipbook, "frames", frames);
            Set(flipbook, "fps", fps);
            Set(flipbook, "loop", loop);
            Set(flipbook, "destroyOnFinish", destroyOnFinish);
            Set(flipbook, "randomizeStart", randomizeStart);
            Set(flipbook, "unscaledTime", unscaledTime);
            return flipbook;
        }

        public static GameObject SavePrefab(GameObject go, string name)
        {
            EnsureFolder(PrefabRoot);
            string path = $"{PrefabRoot}/{name}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        public static T LoadPrefab<T>(string name) where T : Component
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/{name}.prefab");
            if (go == null)
            {
                Debug.LogError($"Prefab '{name}' not found in {PrefabRoot}.");
                return null;
            }
            return go.GetComponent<T>();
        }

        public static GameObject LoadPrefab(string name)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/{name}.prefab");
            if (go == null) Debug.LogError($"Prefab '{name}' not found in {PrefabRoot}.");
            return go;
        }
    }
}
