using UnityEngine;
using UnityEngine.Rendering;

namespace DungeonKnight
{
    public class JuiceView : MonoBehaviour
    {
        [SerializeField] Volume volume;
        [SerializeField] SpriteRenderer legacyVignette;

        void Awake() => RefCheck.Valid(this, volume, nameof(volume));

        void OnEnable()
        {
            SaveManager.SettingsChanged += Apply;
            Apply();
        }

        void OnDisable() => SaveManager.SettingsChanged -= Apply;

        public void Apply()
        {
            bool on = JuiceFx.Enabled;
            if (volume != null) volume.enabled = on;
            if (legacyVignette != null) legacyVignette.enabled = !on;
        }
    }
}
