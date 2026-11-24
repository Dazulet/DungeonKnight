using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DungeonKnight.EditorTools
{
    public static class JuiceBuilder
    {
        public const string SettingsRoot = "Assets/Settings";
        public const string PipelinePath = SettingsRoot + "/DungeonKnightURP.asset";
        public const string RendererPath = SettingsRoot + "/DungeonKnight2DRenderer.asset";
        public const string ProfilePath = SettingsRoot + "/JuiceVolumeProfile.asset";
        public const string ShaderPath = "Assets/Art/Shaders/SpriteHitFlash.shadergraph";
        public const string MaterialRoot = "Assets/Art/Materials";
        public const string FlashMaterialPath = MaterialRoot + "/SpriteHitFlash.mat";
        public const string SpriteMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";
        public const string ParticleMaterialPath = MaterialRoot + "/PixelParticle.mat";
        const string ParticleShader = "Universal Render Pipeline/2D/Sprite-Unlit-Default";
        public const int FxOrder = SortOrder.Projectile - 50;
        const float Pixel = 1f / 16f;

        public readonly struct Particles
        {
            public readonly GameObject EnemyHit;
            public readonly GameObject EnemyDeath;
            public readonly GameObject PlayerDamage;
            public readonly GameObject CoinPickup;
            public readonly GameObject Victory;

            public Particles(GameObject enemyHit, GameObject enemyDeath, GameObject playerDamage, GameObject coinPickup, GameObject victory)
            {
                EnemyHit = enemyHit;
                EnemyDeath = enemyDeath;
                PlayerDamage = playerDamage;
                CoinPickup = coinPickup;
                Victory = victory;
            }
        }

        public static void ConfigureRendering()
        {
            BuilderUtil.EnsureFolder(SettingsRoot);
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                ScriptableRendererData renderer = CreateRendererData();
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }

            pipeline.supportsHDR = false;
            pipeline.msaaSampleCount = 1;
            pipeline.renderScale = 1f;
            pipeline.supportsCameraDepthTexture = false;
            pipeline.supportsCameraOpaqueTexture = false;
            pipeline.useSRPBatcher = true;
            pipeline.colorGradingMode = ColorGradingMode.LowDynamicRange;
            EditorUtility.SetDirty(pipeline);

            GraphicsSettings.defaultRenderPipeline = pipeline;
            for (int level = 0; level < QualitySettings.names.Length; level++)
            {
                RenderPipelineAsset assigned = QualitySettings.GetRenderPipelineAssetAt(level);
                if (assigned != null && assigned != pipeline)
                    Debug.LogWarning($"Quality level {QualitySettings.names[level]} overrides the pipeline with {assigned.name}.");
            }
            AssetDatabase.SaveAssets();
        }

        static ScriptableRendererData CreateRendererData()
        {
            Type menus = Type.GetType("UnityEditor.Rendering.Universal.Renderer2DMenus, Unity.RenderPipelines.Universal.Editor");
            MethodInfo create = menus?.GetMethod("CreateRendererAsset", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (create == null) throw new InvalidOperationException("URP 2D Renderer factory was not found. Check the URP package version.");
            return (ScriptableRendererData)create.Invoke(null, new object[] { RendererPath, RendererType._2DRenderer, false, "Renderer" });
        }

        public static Material SpriteMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(SpriteMaterialPath);
            if (material == null) Debug.LogError($"URP sprite material not found at {SpriteMaterialPath}.");
            return material;
        }

        public static Material FlashMaterial()
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
            if (shader == null)
            {
                Debug.LogError($"Shader Graph not found at {ShaderPath}.");
                return SpriteMaterial();
            }
            Material material = LoadOrCreateMaterial(FlashMaterialPath, shader);
            material.SetFloat("_FlashAmount", 0f);
            material.SetColor("_FlashColor", Color.white);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material ParticleMaterial()
        {
            Shader shader = Shader.Find(ParticleShader);
            if (shader == null) throw new InvalidOperationException($"Shader '{ParticleShader}' was not found.");
            return LoadOrCreateMaterial(ParticleMaterialPath, shader);
        }

        static Material LoadOrCreateMaterial(string path, Shader shader)
        {
            BuilderUtil.EnsureFolder(MaterialRoot);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }
            return material;
        }

        public static VolumeProfile Profile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                BuilderUtil.EnsureFolder(SettingsRoot);
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            Bloom bloom = Component<Bloom>(profile);
            bloom.threshold.Override(0.82f);
            bloom.intensity.Override(0.55f);
            bloom.scatter.Override(0.5f);
            bloom.clamp.Override(4f);
            bloom.highQualityFiltering.Override(false);
            bloom.downscale.Override(BloomDownscaleMode.Quarter);
            bloom.maxIterations.Override(4);

            ColorAdjustments color = Component<ColorAdjustments>(profile);
            color.postExposure.Override(0f);
            color.contrast.Override(12f);
            color.saturation.Override(10f);

            Vignette vignette = Component<Vignette>(profile);
            vignette.color.Override(new Color(0.04f, 0.02f, 0.08f));
            vignette.intensity.Override(0.3f);
            vignette.smoothness.Override(0.45f);
            vignette.rounded.Override(false);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        static T Component<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet(out T component)) return component;
            component = profile.Add<T>();
            component.name = typeof(T).Name;
            component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(component, profile);
            EditorUtility.SetDirty(component);
            return component;
        }

        public static Volume AddPostProcessing(Camera cam, SpriteRenderer legacyVignette)
        {
            UniversalAdditionalCameraData data = cam.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.None;
            data.dithering = false;

            var go = new GameObject("Global Volume");
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 0f;
            volume.weight = 1f;
            volume.sharedProfile = Profile();

            var view = cam.gameObject.AddComponent<JuiceView>();
            BuilderUtil.Set(view, "volume", volume);
            BuilderUtil.Set(view, "legacyVignette", legacyVignette);
            return volume;
        }

        public static Particles BuildParticles()
        {
            Material material = ParticleMaterial();
            var hit = Build("FxEnemyHit", material, new FxSpec
            {
                Duration = 0.3f, Burst = 6, Lifetime = new Vector2(0.12f, 0.22f), Speed = new Vector2(2.5f, 4.5f),
                Size = new Vector2(2f, 2f), StartA = new Color(1f, 1f, 0.92f), StartB = new Color(1f, 0.85f, 0.45f),
                EndColor = new Color(1f, 0.6f, 0.25f), Radius = 0.08f, Max = 12,
            });
            var death = Build("FxEnemyDeath", material, new FxSpec
            {
                Duration = 0.5f, Burst = 14, Lifetime = new Vector2(0.25f, 0.45f), Speed = new Vector2(1.5f, 4f),
                Size = new Vector2(2f, 3f), StartA = new Color(0.95f, 0.9f, 1f), StartB = new Color(0.7f, 0.55f, 0.95f),
                EndColor = new Color(0.35f, 0.3f, 0.45f), Radius = 0.2f, Max = 20, Drag = 0.25f,
            });
            var damage = Build("FxPlayerDamage", material, new FxSpec
            {
                Duration = 0.4f, Burst = 10, Lifetime = new Vector2(0.2f, 0.35f), Speed = new Vector2(2f, 3.5f),
                Size = new Vector2(2f, 2f), StartA = new Color(1f, 0.35f, 0.35f), StartB = new Color(1f, 0.7f, 0.55f),
                EndColor = new Color(0.55f, 0.05f, 0.1f), Radius = 0.15f, Max = 14,
            });
            var coin = Build("FxCoinPickup", material, new FxSpec
            {
                Duration = 0.5f, Burst = 8, Lifetime = new Vector2(0.3f, 0.45f), Speed = new Vector2(1.2f, 2.4f),
                Size = new Vector2(1f, 2f), StartA = new Color(1f, 0.85f, 0.3f), StartB = new Color(1f, 1f, 0.7f),
                EndColor = new Color(1f, 0.7f, 0.15f), Radius = 0.1f, Max = 12, ConeUp = 35f, Gravity = 0.5f,
            });
            var victory = Build("FxVictory", material, new FxSpec
            {
                Duration = 1.6f, Burst = 26, SecondBurst = 18, Lifetime = new Vector2(0.9f, 1.4f), Speed = new Vector2(3f, 6f),
                Size = new Vector2(2f, 3f), Rainbow = true, Radius = 0.3f, Max = 60, ConeUp = 50f, Gravity = 0.8f,
                Unscaled = true,
            });
            return new Particles(hit, death, damage, coin, victory);
        }

        struct FxSpec
        {
            public float Duration;
            public int Burst;
            public int SecondBurst;
            public Vector2 Lifetime;
            public Vector2 Speed;
            public Vector2 Size;
            public Color StartA;
            public Color StartB;
            public Color EndColor;
            public bool Rainbow;
            public float Radius;
            public int Max;
            public float ConeUp;
            public float Gravity;
            public float Drag;
            public bool Unscaled;
        }

        static GameObject Build(string name, Material material, FxSpec spec)
        {
            var go = new GameObject(name);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = ps.main;
            main.duration = spec.Duration;
            main.loop = false;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(spec.Lifetime.x, spec.Lifetime.y);
            main.startSpeed = new ParticleSystem.MinMaxCurve(spec.Speed.x, spec.Speed.y);
            main.startSize = new ParticleSystem.MinMaxCurve(spec.Size.x * Pixel, spec.Size.y * Pixel);
            main.startRotation = 0f;
            main.gravityModifier = spec.Gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = spec.Max;
            main.stopAction = ParticleSystemStopAction.Destroy;
            main.useUnscaledTime = spec.Unscaled;
            main.startColor = spec.Rainbow
                ? new ParticleSystem.MinMaxGradient(RainbowGradient()) { mode = ParticleSystemGradientMode.RandomColor }
                : new ParticleSystem.MinMaxGradient(spec.StartA, spec.StartB);

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.rateOverDistance = 0f;
            emission.SetBursts(spec.SecondBurst > 0
                ? new[] { new ParticleSystem.Burst(0f, (short)spec.Burst), new ParticleSystem.Burst(0.25f, (short)spec.SecondBurst) }
                : new[] { new ParticleSystem.Burst(0f, (short)spec.Burst) });

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            if (spec.ConeUp > 0f)
            {
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = spec.ConeUp;
                shape.radius = spec.Radius;
                shape.rotation = new Vector3(-90f, 0f, 0f);
            }
            else
            {
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = spec.Radius;
                shape.radiusThickness = 1f;
                shape.arc = 360f;
            }

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var fade = new Gradient();
            Color end = spec.Rainbow ? Color.white : spec.EndColor;
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(spec.Rainbow ? Color.white : Tint(end), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = fade;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.5f));

            if (spec.Drag > 0f)
            {
                ParticleSystem.LimitVelocityOverLifetimeModule limit = ps.limitVelocityOverLifetime;
                limit.enabled = true;
                limit.limit = 10f;
                limit.drag = spec.Drag * 10f;
            }

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = FxOrder;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.maxParticleSize = 0.5f;

            return BuilderUtil.SavePrefab(go, name);
        }

        static Color Tint(Color end) => new Color(end.r, end.g, end.b, 1f);

        static Gradient RainbowGradient()
        {
            var g = new Gradient { mode = GradientMode.Fixed };
            g.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.85f, 0.3f), 0.25f),
                    new GradientColorKey(new Color(1f, 1f, 1f), 0.45f),
                    new GradientColorKey(new Color(0.45f, 0.85f, 1f), 0.7f),
                    new GradientColorKey(new Color(1f, 0.5f, 0.7f), 1f),
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return g;
        }
    }
}
