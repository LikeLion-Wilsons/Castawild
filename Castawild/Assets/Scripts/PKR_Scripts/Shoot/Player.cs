using Fusion;
using UnityEngine;

namespace Test.Shoot
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private Transform _cameraPivot;
        private Transform _mainCamTransform;
        [SerializeField] private MeshRenderer[] _thirdPersonRenderers;
        [SerializeField] private NetworkCharacterController _cc;
        [SerializeField] private NicknameUI nicknameUI;
        [SerializeField] private Weapon_Linear _weaponLinear;
        [SerializeField] private Weapon_Parabola _weaponParabola;
        [Networked, OnChangedRender(nameof(OnChangedNickname))] private string nickname { get; set; }

        private NetworkButtons _prevInputButtons;

        public void Init()
        {
            //spawned 되기전에 초기화작업.
        }

        public override void Spawned()
        {
            _mainCamTransform = Camera.main.transform;
            
            //내 닉네임은 서버로 RPC.
            if (HasInputAuthority)
            {
                RPC_SetNickname(PlayerTempData.nickname);
            }

            //다른플레이어 닉네임 refresh.
            OnChangedNickname();
            
            
            
            //내캐릭은 숨기고, 상대캐릭은 보이게.
            for (int i = 0; i < _thirdPersonRenderers.Length; i++)
            {
                _thirdPersonRenderers[i].enabled = Object.HasInputAuthority == false;
            }
            
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

                if (input.Buttons.WasPressed(_prevInputButtons, NetworkInputData.BUTTON_FIRE))
                {
                    Debug.Log("Fire1()");
                    _weaponLinear.Fire();
                }
                
                if (input.Buttons.WasPressed(_prevInputButtons, NetworkInputData.BUTTON_FIRE2))
                {
                    Debug.Log("Fire2()");
                    _weaponParabola.Fire();
                }

                

                _prevInputButtons = input.Buttons;
            }
        }

        void LateUpdate()
        {
            if (HasInputAuthority == false) return;

            if (_mainCamTransform != null)
            {
                _mainCamTransform.position = _cameraPivot.position;
                _mainCamTransform.rotation = _cameraPivot.rotation;
            }
        }


        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetNickname(string nickname)
        {
            this.nickname = nickname;
        }

        void OnChangedNickname()
        {
            nicknameUI.SetNickname(nickname);
        }
    }
}