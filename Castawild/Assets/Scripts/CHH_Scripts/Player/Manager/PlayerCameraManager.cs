using System.Collections;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;

public enum ViewType { None, FirstPerson, ThirdPerson }

public class PlayerCameraManager : MonoBehaviour
{
    #region Components
    [SerializeField] private CinemachineImpulseSource impulseSource;
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

    #region Mouse Settings
    [Header("마우스")]
    public float sensivity = 1.5f;
    private float pitch = 0f;
    private float yaw = 0f;

    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float minYaw = -90f;
    [SerializeField] private float maxYaw = 90f;
    #endregion



    #region First Person
    [Header("1인칭")]
    [SerializeField] private GameObject[] playerMeshes;

    public CinemachineCamera firstPersonCam;
    [SerializeField] private Transform firstPersonTarget;
    [SerializeField] private float firstPerson_AimFovDelta = 5f;

    [Header("CameraMove")]
    private Vector3 originfirstPersonTargetPos;
    [SerializeField] private Transform sleepCameraTarget;
    [SerializeField] private float sleepTransitionTime;
    [SerializeField] private Transform deadCameraTarget;
    [SerializeField] private float deadTransitionTime;
    #endregion

    #region Third Person
    [Header("3인칭")]
    public CinemachineCamera thirdPersonCam;
    [SerializeField] private Transform thirdPersonTarget;
    [SerializeField] private Transform thirdPerson_AimTargetPos;
    [SerializeField] private float thirdPerson_AimFovDelta = 20f;

    private Vector3 thirdPerson_DefaultTargetPos;

    private float defaultFov;
    private float currentFOV;
    #endregion


    #region Third Person Camera Zoom
    [Header("3인칭 Zoom")]
    [SerializeField] public float aimZoomDuration = 0.3f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomLerpSpeed = 10f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 15f;
    #endregion

    [Header("Move Camera")]
    [HideInInspector] public bool MovingCamera = true;
    [HideInInspector] public bool walk = false;
    [HideInInspector] public bool run = false;
    [Header("Walk")]
    [SerializeField] private float walkAmplitude = 0.2f; // 세기
    [SerializeField] private float walkFrequency = 8.5f; // 속도
    [Header("Run")]
    [SerializeField] private float sprintAmplitude = 0.2f;
    [SerializeField] private float sprintFrequency = 15f;
    [SerializeField] private float transitionSpeed = 10f;

    private float targetAmplitude = 0f;
    private float targetFrequency = 0f;

    private float currentAmplitude;
    private float currentFrequency;

    private Vector3 startPos;
    private float bobTimer = 0f;

    private Coroutine moveCameraCoroutine;

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
        originfirstPersonTargetPos = firstPersonTarget.localPosition;
    }

    private void Start()
    {
        ViewChange(ViewType.FirstPerson);
        startPos = firstPersonCam.transform.localPosition;
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
        defaultFov = firstPersonCam.Lens.FieldOfView;
    }

    public void ActivateCameraInput() => inputAxisController.enabled = true;
    public void InactivateCameraInput() => inputAxisController.enabled = false;

    private void Update()
    {
        if (!player.HasInputAuthority || !player || !player.isSpawned)
            return;

        HandleViewChange();
        UpdateCameraPitch();

        if (MovingCamera)
            MoveUpDownCamera();
    }


    private void MoveUpDownCamera()
    {
        if (walk)
        {
            targetAmplitude = walkAmplitude;
            targetFrequency = walkFrequency;
        }
        else if (run)
        {
            targetAmplitude = sprintAmplitude;
            targetFrequency = sprintFrequency;
        }
        if (!walk && !run)
        {
            targetAmplitude = 0f;
            targetFrequency = 0f;
        }

        currentAmplitude = Mathf.Lerp(currentAmplitude, targetAmplitude, Time.deltaTime * transitionSpeed);
        currentFrequency = Mathf.Lerp(currentFrequency, targetFrequency, Time.deltaTime * transitionSpeed);

        if (walk || run)
        {
            bobTimer += Time.deltaTime * currentFrequency;
            float newY = startPos.y + Mathf.Sin(bobTimer) * currentAmplitude;

            firstPersonTarget.transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
        }
        else
        {
            bobTimer = 0f;
            firstPersonTarget.transform.localPosition = Vector3.Lerp(firstPersonTarget.transform.localPosition, startPos, Time.deltaTime * transitionSpeed);
        }
    }


    private void HandleViewChange()
    {
        if (inputManager.viewChangeAction.WasPressedThisFrame()
            && (toolManager.CurrentToolState == ToolState.Idle || toolManager.CurrentToolState == ToolState.Carry))
        {
            ViewChange(currentView == ViewType.FirstPerson ? ViewType.ThirdPerson : ViewType.FirstPerson);
        }
    }

    /// <summary>
    /// 다른 플레이어 카메라 우선순위 설정
    /// </summary>
    public void SetNetworkCamera()
    {
        firstPersonCam.Priority = 1;
        thirdPersonCam.Priority = 0;
    }

    private void ViewChange(ViewType viewType)
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
                player.Client_AttachToCamera(true);
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
                player.Client_AttachToCamera(false);
            }
        }
    }

    // 플레이어가 바라보는 방향의 각도를 카메라 회전값으로 설정
    private void SettingThirdPersonCam()
    {
        float targetYaw = player.transform.eulerAngles.y;
        orbital.HorizontalAxis.Value = targetYaw;

        orbital.VerticalAxis.Value = 22f;
    }

    // 1인칭 카메라 상하각도 조절
    private void UpdateCameraPitch()
    {
        if (currentView == ViewType.ThirdPerson || !player.IsCursorLocked || !player.All_CanMoving())
            return;

        pitch -= inputManager.lookInput.y * sensivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        firstPersonCam.transform.localEulerAngles = new Vector3(pitch, yaw, 0f);
    }

    /// <summary>
    /// 조준할 때 카메라 움직이는 함수
    /// </summary>
    public void MoveAimCamera(bool _isAiming)
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
        {
            if (currentView == ViewType.FirstPerson)
                moveCameraCoroutine = StartCoroutine(MoveCameraCoroutine(currentFOV + firstPerson_AimFovDelta));
            else
                moveCameraCoroutine = StartCoroutine(MoveCameraCoroutine(currentFOV + thirdPerson_AimFovDelta, thirdPerson_AimTargetPos.localPosition));
        }
        else
        {
            if (currentView == ViewType.FirstPerson)
                moveCameraCoroutine = StartCoroutine(MoveCameraCoroutine(currentFOV));
            else
                moveCameraCoroutine = StartCoroutine(MoveCameraCoroutine(currentFOV, thirdPerson_DefaultTargetPos));
        }
    }

    private IEnumerator MoveCameraCoroutine(float targetFov, Vector3 targetPos = default)
    {
        float startFov = firstPersonCam.Lens.FieldOfView;

        if (currentView == ViewType.ThirdPerson)
            startFov = thirdPersonCam.Lens.FieldOfView;

        Vector3 startPosition = thirdPersonTarget.localPosition;

        float elapsed = 0f;

        while (elapsed < aimZoomDuration)
        {
            if (currentView == ViewType.ThirdPerson)
            {
                thirdPersonCam.Lens.FieldOfView = Mathf.Lerp(startFov, targetFov, elapsed / aimZoomDuration);
                thirdPersonTarget.localPosition = Vector3.Lerp(startPosition, targetPos, elapsed / aimZoomDuration);
            }
            else
                firstPersonCam.Lens.FieldOfView = Mathf.Lerp(startFov, targetFov, elapsed / aimZoomDuration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (currentView == ViewType.ThirdPerson)
            thirdPersonTarget.localPosition = targetPos;
    }

    /// <summary>
    /// 죽거나 잘 때 카메라 위치
    /// </summary>
    public void SleepDeadCameraTarget(bool moveCamera, bool isSleep)
    {
        if (moveCamera)
        {
            if (moveCameraCoroutine != null)
            {
                StopCoroutine(moveCameraCoroutine);
                moveCameraCoroutine = null;
            }
            if (isSleep)
                firstPersonTarget.localPosition = sleepCameraTarget.localPosition;
            else
                firstPersonTarget.localPosition = deadCameraTarget.localPosition;
        }

        else
        {
            if (moveCameraCoroutine != null)
            {
                StopCoroutine(moveCameraCoroutine);
                moveCameraCoroutine = null;
            }
            else
                firstPersonTarget.localPosition = originfirstPersonTargetPos;
        }
    }

    /// <summary>
    /// 카메라 쉐이크
    /// </summary>
    public void ShakeCamera(Vector3 direction, float force)
    {
        if (MovingCamera)
            impulseSource.GenerateImpulse(direction.normalized * force);
    }

    public void ChangeFOV(float value)
    {
        float changeAmount = value - defaultFov;
        thirdPersonCam.Lens.FieldOfView = value;
        currentFOV = value;
    }
}
