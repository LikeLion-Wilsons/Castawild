using Fusion;
using UnityEngine;

namespace Test
{
    public class Player : NetworkBehaviour
    {
        private NetworkCharacterController _cc;

        private NetworkButtons _prevInputButtons;
        private float _interactRadius = 1f;
        Collider[] _interactResult = new Collider[5];
        [Networked] private TickTimer interactTimer { get; set; }
        [Networked] private Color color { get; set; }
        [SerializeField] private Renderer render;

        void Awake()
        {
            _cc = GetComponent<NetworkCharacterController>();
        }

        public void Init(Color color)
        {
            this.color = color;
        }

        public override void Spawned()
        {
            render.material.color = color;
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority == false) return;
            if (GetInput<NetworkInputData>(out var input))
            {
                var dir = default(Vector3);

                if (input.IsDown(NetworkInputData.BUTTON_RIGHT)) dir += Vector3.right;
                else if (input.IsDown(NetworkInputData.BUTTON_LEFT)) dir += Vector3.left;

                if (input.IsDown(NetworkInputData.BUTTON_FORWARD)) dir += Vector3.forward;
                else if (input.IsDown(NetworkInputData.BUTTON_BACKWARD)) dir += Vector3.back;

                _cc.Move(dir.normalized);

                if (input.WasPressed(_prevInputButtons, NetworkInputData.BUTTON_INTERACT))
                {
                    //임시로 쿨타임 1초.
                    if (interactTimer.ExpiredOrNotRunning(Runner))
                    {
                        TryInteract();
                    }
                }

                if (input.WasPressed(_prevInputButtons, NetworkInputData.BUTTON_INVENTORY))
                {
                    GetComponent<PlayerInventory>().ShowLog();
                }


                _prevInputButtons = input.Buttons;
            }
        }

        void TryInteract()
        {
            Vector3 pos = transform.position + transform.forward * 1.5f;
            var hits = Runner.GetPhysicsScene()
                .OverlapSphere(pos, _interactRadius, _interactResult, 1, QueryTriggerInteraction.UseGlobal);
            if (hits > 0)
            {
                for (int i = 0; i < hits && i < _interactResult.Length; i++)
                {
                    if (_interactResult[i].TryGetComponent<IInteractable>(out var interactable))
                    {
                        if (interactable.CanInteract())
                        {
                            interactable.Interact(Object.InputAuthority);
                            interactTimer = TickTimer.CreateFromSeconds(Runner, 1f);
                            break;
                        }
                    }
                }
            }
        }
    }
}