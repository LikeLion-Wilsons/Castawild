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
        private bool isPressedFire = false;
        private bool isPressedFire2 = false;
        private bool isPressedJump = false;

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
            myInput.Buttons.Set(NetworkInputData.BUTTON_FIRE, isPressedFire);
            myInput.Buttons.Set(NetworkInputData.BUTTON_FIRE2, isPressedFire2);
            myInput.Buttons.Set(NetworkInputData.BUTTON_JUMP, isPressedJump);
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
                isPressedFire = false;
                isPressedFire2 = false;
                isPressedJump = false;
            }

            _mouseDelta += new Vector2(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"));
            isPressedFire = isPressedFire | Input.GetMouseButtonDown(0);
            isPressedFire2 = isPressedFire2 | Input.GetMouseButtonDown(1);
            isPressedJump = isPressedJump | Input.GetKeyDown(KeyCode.Space);
        }
    }
}