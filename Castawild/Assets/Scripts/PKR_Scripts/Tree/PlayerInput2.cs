using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace Test
{
    public class PlayerInput2 : NetworkBehaviour
    {
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
            var myInput = new NetworkInputData2();

            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = Input.GetAxisRaw("Vertical");

            myInput.Buttons.Set(NetworkInputData2.BUTTON_FORWARD, vertical > 0);
            myInput.Buttons.Set(NetworkInputData2.BUTTON_BACKWARD, vertical < 0);
            myInput.Buttons.Set(NetworkInputData2.BUTTON_RIGHT, horizontal > 0);
            myInput.Buttons.Set(NetworkInputData2.BUTTON_LEFT, horizontal < 0);
            myInput.Buttons.Set(NetworkInputData2.BUTTON_INTERACT, Input.GetKey(KeyCode.E));

            input.Set(myInput);
        }

    }
}