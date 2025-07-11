using Fusion;
using UnityEngine;

namespace Test
{
    public class NetworkPlayerInteract : NetworkBehaviour
    {
        [SerializeField] private float _interactRadius = 1.25f;
        private NetworkButtons _prevInput;

        private Collider[] _interactQueryResult = new Collider[5];

        public override void FixedUpdateNetwork()
        {
            if (GetInput<NetworkInputData2>(out var input))
            {
                //서버기준, 지난틱과 비교해서 이번틱에 눌렀는지 판단.
                if (input.WasPressed(_prevInput, NetworkInputData2.BUTTON_INTERACT))
                {
                    var hits = Runner.GetPhysicsScene().OverlapSphere(transform.position + transform.forward * 1.5f, _interactRadius, _interactQueryResult, 1,
                        QueryTriggerInteraction.UseGlobal);
                    if (hits > 0)
                    {
                        for (int i = 0; i < hits && i < _interactQueryResult.Length; i++)
                        {
                            var t = _interactQueryResult[i].GetComponentInParent<IInteractable>();
                            if (t != null)
                            {
                                t.Interact(Object.InputAuthority);
                                break;
                            }
                        }
                    }
                }

                _prevInput = input.Buttons;
            }
        }
    }
}