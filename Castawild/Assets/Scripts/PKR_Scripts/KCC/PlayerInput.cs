using UnityEngine;
using Fusion;

namespace Test.KCC
{
    public sealed class PlayerInput : NetworkBehaviour, IBeforeUpdate
    {
        private Vector2 _mouseDelta = Vector2.zero;
        private bool _resetInput = false;
        
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

        void IBeforeUpdate.BeforeUpdate()
        {
            if (HasInputAuthority == false) return;

            // Accumulate input only if the cursor is locked.
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                _mouseDelta = default;
                return;
            }

            if (_resetInput)
            {
                _resetInput = false;
                _mouseDelta = default;
            }

            _mouseDelta += new Vector2(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"));
        }

        private void OnInput(NetworkRunner runner, NetworkInput networkInput)
        {
            var myInput = new NetworkInputData();

            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = Input.GetAxisRaw("Vertical");

            myInput.Buttons.Set(NetworkInputData.BUTTON_FORWARD, vertical > 0);
            myInput.Buttons.Set(NetworkInputData.BUTTON_BACKWARD, vertical < 0);
            myInput.Buttons.Set(NetworkInputData.BUTTON_RIGHT, horizontal > 0);
            myInput.Buttons.Set(NetworkInputData.BUTTON_LEFT, horizontal < 0);
            myInput.Buttons.Set(NetworkInputData.BUTTON_JUMP, Input.GetKey(KeyCode.Space));
            myInput.mouseDelta = _mouseDelta;
            
            networkInput.Set(myInput);
            _resetInput = true;
        }
    }
}