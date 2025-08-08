using Fusion;
using UnityEngine;
using Test;
using System.Collections;

public class YSB_Player : NetworkBehaviour
{
    [SerializeField] private NetworkCharacterController _cc;
    [SerializeField] private NicknameUI nicknameUI;
    [Networked] private TickTimer interactTimer { get; set; }
    [Networked, OnChangedRender(nameof(OnChangedNickname))] private string nickname { get; set; }

    private NetworkButtons _prevInputButtons;
    private float _interactRadius = 1f;
    Collider[] _interactResult = new Collider[5];
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
            if (NetworkObjectVisibilityManager.Instance != null)
            {
                NetworkObjectVisibilityManager.Instance.SetPlayerTransform(Object.InputAuthority, transform);
            }
            else
            {
                Debug.LogWarning("[Spawned] VisibilityManager instance is null! Delaying registration...");

                // 예: 코루틴이나 타이머를 써서 나중에 다시 등록 시도
                StartCoroutine(RegisterPlayerTransformDelayed());
            }
        }

        //다른플레이어 닉네임 refresh.
        OnChangedNickname();
    }

    private IEnumerator RegisterPlayerTransformDelayed()
    {
        while (NetworkObjectVisibilityManager.Instance == null)
            yield return null;

        NetworkObjectVisibilityManager.Instance.SetPlayerTransform(Object.InputAuthority, transform);
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
        int layerMask = 1 << LayerMask.NameToLayer("Interactable");

        var hits = Runner.GetPhysicsScene()
            .OverlapSphere(pos, _interactRadius, _interactResult, layerMask, QueryTriggerInteraction.UseGlobal);

        if (hits > 0)
        {
            for (int i = 0; i < hits && i < _interactResult.Length; i++)
            {
                if (_interactResult[i].TryGetComponent<YSB_Scripts.IInteractable>(out var interactable))
                {
                    if (interactable.CanInteract())
                    {
                        interactable.Interact(Object.InputAuthority, 10); // 임시로 10 데미지
                        interactTimer = TickTimer.CreateFromSeconds(Runner, 1f);
                        break;
                    }
                }
            }
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