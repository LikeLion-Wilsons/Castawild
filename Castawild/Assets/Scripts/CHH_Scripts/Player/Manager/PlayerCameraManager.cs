using Fusion;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;


public class PlayerCameraManager : MonoBehaviour
{
    #region Components
    private PlayerInputManager inputManager;
    private CinemachineOrbitalFollow orbital;
    private CinemachineInputAxisController inputAxisController;
    private PlayerNetworkManager networkManager;
    private CwPlayer player;
    #endregion

    public bool isAiming = false;
    public ViewType CurrentView { get; private set; }

    #region Third Person Aim
    [Header("1인칭")]
    public CinemachineCamera firstPersonCam;
    [SerializeField] private Transform firstPersonTarget;

    [SerializeField] private GameObject playerMesh;

    public float sensitivity = 1.5f;
    [SerializeField] private float maxXRotation = 80f;
    [SerializeField] private float minXRotation = -80f;
    private float xRotation = 0f;

    #endregion

    #region Third Person Aim
    [Header("3인칭")]
    public CinemachineCamera thirdPersonCam;
    [SerializeField] private Transform thirdPersonTarget;
    [SerializeField] private Transform thirdPerson_AimTargetPos;
    [SerializeField] private float thirdPerson_AimFov;

    private Vector3 thirdPerson_DefaultTargetPos;
    private float thirdPerson_DefaultFov;
    private Coroutine moveCameraCoroutine;

    [SerializeField] private float thirdPerson_aimZoomDuration = 0.3f;
    #endregion

    #region Third Person Camera Zoom
    [Header("3인칭 Zoom")]
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
            if (CurrentView == ViewType.FirstPerson)
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
        ViewChange(ViewType.FirstPerson);
    }

    private void Start()
    {
        ViewChange(ViewType.FirstPerson);
    }

    private void InitComponents()
    {
        player = GetComponentInParent<CwPlayer>();
        inputManager = GetComponentInParent<PlayerInputManager>();
        orbital = thirdPersonCam.GetComponent<CinemachineOrbitalFollow>();
        inputAxisController = thirdPersonCam.GetComponent<CinemachineInputAxisController>();
        networkManager = GetComponentInParent<PlayerNetworkManager>();
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
        HandleViewChange();
        ZoomCamera();
    }

    private void HandleViewChange()
    {
        if (inputManager.viewChangeAction.WasPressedThisFrame())
        {
            if (CurrentView == ViewType.FirstPerson)
                ViewChange(ViewType.ThirdPerson);
            else if (CurrentView == ViewType.ThirdPerson)
                ViewChange(ViewType.FirstPerson);
        }
    }

    public void SetNetworkCamera()
    {
        CurrentView = ViewType.FirstPerson;
        playerMesh.SetActive(true);
        firstPersonCam.Priority = 1;
        thirdPersonCam.Priority = 0;
    }

    public void ViewChange(ViewType viewType)
    {
        if (viewType == ViewType.FirstPerson)
        {
            if (networkManager.HasInputAuthority)
            {
                CurrentView = ViewType.FirstPerson;
                playerMesh.SetActive(false);
                firstPersonCam.Priority = 10;
                thirdPersonCam.Priority = 0;
            }
        }

        else if (viewType == ViewType.ThirdPerson)
        {
            if (networkManager.HasInputAuthority)
            {
                CurrentView = ViewType.ThirdPerson;
                playerMesh.SetActive(true);
                firstPersonCam.Priority = 0;
                thirdPersonCam.Priority = 10;
            }
        }
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
}
