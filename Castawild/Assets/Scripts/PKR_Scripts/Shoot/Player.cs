using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;
using UnityEngine.Rendering;

namespace Test.Shoot
{
    public class Player : NetworkBehaviour
    {
        public SimpleKCC kcc;
        public Transform CameraPivot;
        public Transform CameraHandle;

        [SerializeField] private NicknameUI nicknameUI;
        [SerializeField] private Weapon_Linear _weaponLinear;
        [SerializeField] private Weapon_Parabola _weaponParabola;
        [SerializeField] private Transform _visual;

        [Header("Movement")] public float MoveSpeed = 10.0f;
        public float JumpImpulse = 10.0f;
        public float Gravity = -20.0f;
        public float GroundAcceleration = 55.0f;
        public float GroundDeceleration = 25.0f;
        public float AirAcceleration = 25.0f;
        public float AirDeceleration = 1.3f;

        [Networked, OnChangedRender(nameof(OnChangedNickname))]
        private string nickname { get; set; }

        [Networked] private Vector3 _moveVelocity { get; set; }
        private NetworkButtons _prevInputButtons;

        public void Init()
        {
            //spawned 되기전에 초기화작업.
        }

        public override void Spawned()
        {
            //내 닉네임은 서버로 RPC.
            if (HasInputAuthority)
            {
                RPC_SetNickname(PlayerTempData.nickname);
            }

            //다른플레이어 닉네임 refresh.
            OnChangedNickname();


            kcc.SetGravity(Gravity);

            // Disable visual for local player
            var renderers = _visual.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].shadowCastingMode =
                    HasInputAuthority ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
            }
        }


        public override void FixedUpdateNetwork()
        {
            if (GetInput<NetworkInputData>(out var input))
            {
                kcc.AddLookRotation(input.mouseDelta);

                var dir = default(Vector3);

                if (input.IsDown(NetworkInputData.BUTTON_RIGHT)) dir += Vector3.right;
                else if (input.IsDown(NetworkInputData.BUTTON_LEFT)) dir += Vector3.left;

                if (input.IsDown(NetworkInputData.BUTTON_FORWARD)) dir += Vector3.forward;
                else if (input.IsDown(NetworkInputData.BUTTON_BACKWARD)) dir += Vector3.back;

                if (input.Buttons.WasPressed(_prevInputButtons, NetworkInputData.BUTTON_FIRE))
                {
                    Debug.Log("Fire1()");
                    var rot = kcc.GetLookRotation();
                    var q = Quaternion.Euler(rot);
                    // var q2 = input.camerPivotRotation;
                    // Debug.Log($"q: {q.eulerAngles}, q2: {q2.eulerAngles}");
                    //q==q2 동일함.
                    _weaponLinear.Fire(q * Vector3.forward);
                }

                if (input.Buttons.WasPressed(_prevInputButtons, NetworkInputData.BUTTON_FIRE2))
                {
                    Debug.Log("Fire2()");
                    var rot = kcc.GetLookRotation();
                    var q = Quaternion.Euler(rot);
                    _weaponParabola.Fire(q * Vector3.forward);
                }


                //시선기준 입력방향.
                Vector3 inputDir = kcc.TransformRotation * dir;

                //입력방향에 따른 속도.
                Vector3 desiredMoveVelocity = inputDir * MoveSpeed;

                //경사를 고려한 속도.
                if (kcc.ProjectOnGround(desiredMoveVelocity, out Vector3 projectedDesiredMoveVelocity))
                {
                    desiredMoveVelocity = Vector3.Normalize(projectedDesiredMoveVelocity) * MoveSpeed;
                }

                //가속, 감속.
                float acceleration = 0f;
                if (desiredMoveVelocity == Vector3.zero)
                {
                    acceleration = kcc.IsGrounded == true ? GroundDeceleration : AirDeceleration;
                }
                else
                {
                    acceleration = kcc.IsGrounded == true ? GroundAcceleration : AirAcceleration;
                }

                //현재속도에서 목표속도 보간.
                _moveVelocity = Vector3.Lerp(_moveVelocity, desiredMoveVelocity, acceleration * Runner.DeltaTime);


                //점프키 입력.
                float jumpImpulse = 0f;
                if (input.WasPressed(_prevInputButtons, NetworkInputData.BUTTON_JUMP) && kcc.IsGrounded)
                {
                    jumpImpulse = JumpImpulse;
                }

                //속도에 따라 이동.
                kcc.Move(_moveVelocity, jumpImpulse);


                _prevInputButtons = input.Buttons;
            }
        }

        private void LateUpdate()
        {
            //본인캐릭에만 적용.
            if (HasInputAuthority == false) return;

            //상하회전값만 가져옴.
            Vector2 pitchRotation = kcc.GetLookRotation(true, false);
            CameraPivot.localRotation = Quaternion.Euler(pitchRotation);

            //카메라 업데이트.
            Camera.main.transform.SetPositionAndRotation(CameraHandle.position, CameraHandle.rotation);
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