using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace Test.Shoot
{
    public class PlayerInput : NetworkBehaviour, IBeforeUpdate
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

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var myInput = new NetworkInputData();

            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = Input.GetAxisRaw("Vertical");

            myInput.Buttons.Set(NetworkInputData.BUTTON_FORWARD, vertical > 0);
            myInput.Buttons.Set(NetworkInputData.BUTTON_BACKWARD, vertical < 0);
            myInput.Buttons.Set(NetworkInputData.BUTTON_RIGHT, horizontal > 0);
            myInput.Buttons.Set(NetworkInputData.BUTTON_LEFT, horizontal < 0);
            myInput.Buttons.Set(NetworkInputData.BUTTON_FIRE, Input.GetMouseButton(0));//좌클
            myInput.Buttons.Set(NetworkInputData.BUTTON_FIRE2, Input.GetMouseButton(1));//우클
            myInput.Buttons.Set(NetworkInputData.BUTTON_JUMP, Input.GetKey(KeyCode.Space));
            myInput.mouseDelta = _mouseDelta;
            
            input.Set(myInput);
            _resetInput = true;
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
    }
}