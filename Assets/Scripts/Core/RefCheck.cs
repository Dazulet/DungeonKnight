using UnityEngine;

namespace DungeonKnight
{
    public static class RefCheck
    {
        public static bool Valid(Object owner, Object reference, string fieldName)
        {
            if (reference != null) return true;
            Debug.LogError($"{owner.GetType().Name} on '{owner.name}' is missing the reference '{fieldName}'.", owner);
            return false;
        }
    }
}
