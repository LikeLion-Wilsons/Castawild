using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

namespace Test
{
    public class PlayerInput : NetworkBehaviour
    {
        public Vector2 LookRotation => _input.LookRotation;
        private NetworkInputData _input;
        public override void Spawned()
        {
            if (HasInputAuthority == false) return;
                
            var networkEvents = Runner.GetComponent<NetworkEvents>();
            networkEvents.OnInput.AddListener(OnInput);
        }
        
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (runner == null) return;

            var networkEvents = runner.GetComponent<NetworkEvents>();
            if (networkEvents != null)
            {
                networkEvents.OnInput.RemoveListener(OnInput);
            }
        }
        private void Update()
        {
            if (HasInputAuthority == false) return;
                

            var lookRotationDelta = new Vector2(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"));
            _input.LookRotation = ClampLookRotation(_input.LookRotation + lookRotationDelta);

            var moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            _input.MoveDirection = moveDirection.normalized;

            _input.Buttons.Set(NetworkInputData.JUMP_BUTTON, Input.GetButton("Jump"));
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            input.Set(_input);
        }

        private Vector2 ClampLookRotation(Vector2 lookRotation)
        {
            lookRotation.x = Mathf.Clamp(lookRotation.x, -30f, 70f);
            return lookRotation;
        }
    }
}
