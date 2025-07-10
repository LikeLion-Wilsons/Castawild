using UnityEngine;
using Fusion;

namespace Test
{
    public class Player : NetworkBehaviour
    {
        private NetworkCharacterController _cc;
        [SerializeField] private PlayerInput Input;
        [SerializeField] private Transform CameraPivot;
        [SerializeField] private Transform CameraHandle;

        private void Awake()
        {
            _cc = GetComponent<NetworkCharacterController>();
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData data))
            {
                data.MoveDirection.Normalize();
                _cc.Move(5 * new Vector3(data.MoveDirection.x, 0, data.MoveDirection.y) * Runner.DeltaTime);
            }
        }

        private void LateUpdate()
        {
            if (HasInputAuthority == false) return;

            CameraPivot.rotation = Quaternion.Euler(Input.LookRotation);
            Camera.main.transform.SetPositionAndRotation(CameraHandle.position, CameraHandle.rotation);
        }
    }
}