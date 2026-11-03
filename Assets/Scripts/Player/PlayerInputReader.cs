using UnityEngine;

namespace DungeonKnight
{
 
    public class PlayerInputReader : MonoBehaviour
    {
        [Tooltip("Component that implements IMoveSource, normally the on-screen joystick.")]
        [SerializeField] MonoBehaviour touchMove;
        [Tooltip("Component that implements IShootSource, normally the on-screen shoot button.")]
        [SerializeField] MonoBehaviour touchShoot;

        IMoveSource moveSource;
        IShootSource shootSource;

        public Vector2 Move { get; private set; }
        public bool ShootHeld { get; private set; }

        void Awake()
        {
            moveSource = touchMove as IMoveSource;
            shootSource = touchShoot as IShootSource;
            if (touchMove != null && moveSource == null)
                Debug.LogError($"'{touchMove.name}' does not implement IMoveSource.", this);
            if (touchShoot != null && shootSource == null)
                Debug.LogError($"'{touchShoot.name}' does not implement IShootSource.", this);
        }

        void Update()
        {
            Vector2 keys = KeyboardInput.Move;
            Vector2 touch = moveSource != null ? moveSource.Move : Vector2.zero;
            Move = touch.sqrMagnitude > keys.sqrMagnitude ? touch : keys;
            ShootHeld = KeyboardInput.ShootHeld || (shootSource != null && shootSource.IsShooting);
        }
    }
}
