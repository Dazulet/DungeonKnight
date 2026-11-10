using UnityEngine;

namespace DungeonKnight
{
    public class EnemyStateLabel : MonoBehaviour
    {
        [SerializeField] TextMesh label;
        [SerializeField] bool showInReleaseBuilds;

        public bool IsShown => label != null && label.gameObject.activeSelf;

        void Awake()
        {
            RefCheck.Valid(this, label, nameof(label));
            if (label != null) label.gameObject.SetActive(Application.isEditor || Debug.isDebugBuild || showInReleaseBuilds);
        }

        public void SetState(string stateName)
        {
            if (label != null) label.text = stateName;
        }
    }
}
