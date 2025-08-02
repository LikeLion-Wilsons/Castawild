using Fusion;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public enum ViewType { None, FirstPerson, ThirdPerson }

public class PlayerCameraManager : MonoBehaviour
{
    #region Components
    private PlayerController playerController;
    private PlayerInputManager inputManager;
    private MovementStateManager movementManager;
    private CinemachineOrbitalFollow orbital;
    private CinemachineInputAxisController inputAxisController;
    private ToolStateManager toolManager;
    private Player player;
    #endregion

    [HideInInspector] public bool isAiming = false;
    [HideInInspector] public ViewType currentView = ViewType.FirstPerson;

    #region First Person
    [Header("1인칭")]
    [SerializeField] private GameObject[] playerMeshes;
    [SerializeField] private GameObject playerHead;

    public CinemachineCamera firstPersonCam;
    [SerializeField] private Transform firstPersonTarget;

    [Header("마우스")]
    public float sensitivity = 1.5f;

    private float pitch = 0f;
    private float yaw = 0f;

    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float minYaw = -90f;
    [SerializeField] private float maxYaw = 90f;
    #endregion

    #region Third Person
    [Header("3인칭")]
    public CinemachineCamera thirdPersonCam;
    [SerializeField] private Transform thirdPersonTarget;
    [SerializeField] private Transform thirdPerson_AimTargetPos;
    [SerializeField] private float thirdPerson_AimFov;

    private Vector3 thirdPerson_DefaultTargetPos;
    private float thirdPerson_DefaultFov;
    private Coroutine moveCameraCoroutine;
    #endregion

    #region Third Person Camera Zoom
    [Header("3인칭 Zoom")]
    [SerializeField] private float thirdPerson_aimZoomDuration = 0.3f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomLerpSpeed = 10f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 15f;

    private float targetZoom;
    private float currentZoom;
    #endregion

    public CinemachineCamera CurrenCam
    {
        get
        {
            if (currentView == ViewType.FirstPerson)
                return firstPersonCam;
            else
                return thirdPersonCam;
        }
    }

    private void Awake()
    {
        InitComponents();
        InitVariables();
        SubscribeEvents();
    }

    private void Start()
    {
        ViewChange(ViewType.FirstPerson);
    }

    private void InitComponents()
    {
        player = GetComponentInParent<Player>();
        playerController = GetComponentInParent<PlayerController>();
        inputManager = GetComponentInParent<PlayerInputManager>();
        movementManager = GetComponentInParent<MovementStateManager>();
        toolManager = GetComponentInParent<ToolStateManager>();
        orbital = thirdPersonCam.GetComponent<CinemachineOrbitalFollow>();
        inputAxisController = thirdPersonCam.GetComponent<CinemachineInputAxisController>();
        Camera.main.GetComponent<CinemachineBrain>().DefaultBlend = new(CinemachineBlendDefinition.Styles.Cut, 0f);
    }

    private void SubscribeEvents()
    {
        inputManager.cursorLocked += ActivateCameraInput;
        inputManager.cursorUnLocked = InactivateCameraInput;
    }

    private void InitVariables()
    {
        thirdPerson_DefaultTargetPos = thirdPersonTarget.localPosition;
        thirdPerson_DefaultFov = thirdPersonCam.Lens.FieldOfView;
        targetZoom = currentZoom = orbital.Radius;
    }

    public void ActivateCameraInput() => inputAxisController.enabled = true;
    public void InactivateCameraInput() => inputAxisController.enabled = false;

    private void Update()
    {
        if (!player.HasInputAuthority || !player || !player.isSpawned)
            return;

        HandleViewChange();
        UpdateCameraPitch();
        //ZoomCamera();
    }

    private void HandleViewChange()
    {
        if (inputManager.viewChangeAction.WasPressedThisFrame()
            && (toolManager.currentState == toolManager.idleState || toolManager.currentState == toolManager.carryState))
        {
            ViewChange(currentView == ViewType.FirstPerson ? ViewType.ThirdPerson : ViewType.FirstPerson);
        }
    }

    public void SetNetworkCamera()
    {
        firstPersonCam.Priority = 1;
        thirdPersonCam.Priority = 0;
    }

    public void ViewChange(ViewType viewType)
    {
        if (viewType == ViewType.FirstPerson)
        {
            if (playerController.HasInputAuthority)
            {
                currentView = ViewType.FirstPerson;
                foreach (var mesh in playerMeshes)
                {
                    mesh.SetActive(false);
                }
                firstPersonCam.Priority = 10;
                thirdPersonCam.Priority = 0;
            }
        }

        else if (viewType == ViewType.ThirdPerson)
        {
            if (playerController.HasInputAuthority)
            {
                currentView = ViewType.ThirdPerson;

                SettingThirdPersonCam();

                foreach (var mesh in playerMeshes)
                {
                    mesh.SetActive(true);
                }
                firstPersonCam.Priority = 0;
                thirdPersonCam.Priority = 10;
            }
        }
    }

    private void SettingThirdPersonCam()
    {
        var transposer = thirdPersonCam.GetComponent<CinemachineOrbitalFollow>();

        if (transposer != null)
        {
            // 플레이어가 바라보는 방향의 각도를 카메라 회전값으로 설정
            float targetYaw = player.transform.eulerAngles.y;
            transposer.HorizontalAxis.Value = targetYaw;
        }

        thirdPersonCam.GetComponent<CinemachineOrbitalFollow>().VerticalAxis.Value = 22f;
    }

    // 카메라 상하각도 조절
    private void UpdateCameraPitch()
    {
        if (currentView == ViewType.ThirdPerson || !player.IsCursorLocked)
            return;

        pitch -= inputManager.lookInput.y * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // 자고있을 때 좌우회전 추가
        if (movementManager.currentState == movementManager.sleepState)
        {
            yaw += inputManager.lookInput.x * sensitivity;
            yaw = Mathf.Clamp(yaw, minYaw, maxYaw);
        }
        else if (movementManager.currentState != movementManager.sleepState && yaw != 0f)
            yaw = 0f;

        firstPersonCam.transform.localEulerAngles = new Vector3(pitch, yaw, 0f);
    }

    private void ZoomCamera()
    {
        if (inputManager.zoomInput.y != 0 && inputAxisController.enabled)
        {
            if (orbital != null)
            {
                targetZoom = Mathf.Clamp(orbital.Radius - inputManager.zoomInput.y * zoomSpeed, minDistance, maxDistance);
                inputManager.zoomInput = Vector2.zero;
            }
        }

        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomLerpSpeed);
        orbital.Radius = currentZoom;
    }

    /// <summary>
    /// 3인칭 조준할 때 카메라 움직이는 함수
    /// </summary>
    public void MoveCamera(bool _isAiming)
    {
        if (isAiming == _isAiming)
            return;
        isAiming = _isAiming;

        if (moveCameraCoroutine != null)
        {
            StopCoroutine(moveCameraCoroutine);
            moveCameraCoroutine = null;
        }

        if (_isAiming)
            moveCameraCoroutine = StartCoroutine(MoveCameraCoroutine(thirdPerson_AimTargetPos.localPosition, thirdPerson_AimFov));
        else
            moveCameraCoroutine = StartCoroutine(MoveCameraCoroutine(thirdPerson_DefaultTargetPos, thirdPerson_DefaultFov));
    }

    private IEnumerator MoveCameraCoroutine(Vector3 targetPos, float targetFov)
    {
        Vector3 startPosition = thirdPersonTarget.localPosition;
        float startFov = thirdPersonCam.Lens.FieldOfView;

        float elapsed = 0f;

        while (elapsed < thirdPerson_aimZoomDuration)
        {
            thirdPersonTarget.localPosition = Vector3.Lerp(startPosition, targetPos, elapsed / thirdPerson_aimZoomDuration);
            thirdPersonCam.Lens.FieldOfView = Mathf.Lerp(startFov, targetFov, elapsed / thirdPerson_aimZoomDuration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        thirdPersonTarget.localPosition = targetPos;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_ApplySleepCameraView(bool isSleep)
    {
        if (isSleep)
        {
            firstPersonCam.Follow = playerHead.transform;
            firstPersonCam.LookAt = playerHead.transform;
        }
        else
        {
            firstPersonCam.Follow = firstPersonTarget;
            firstPersonCam.LookAt = firstPersonTarget;
        }
    }
}
