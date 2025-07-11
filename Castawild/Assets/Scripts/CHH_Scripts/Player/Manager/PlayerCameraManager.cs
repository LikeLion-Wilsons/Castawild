using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerCameraManager : MonoBehaviour
{
    #region Components
    [SerializeField] private PlayerInputManager inputManager;
    private CinemachineCamera cam;
    private CinemachineOrbitalFollow orbital;
    private CinemachineInputAxisController inputAxisController;
    #endregion

    public bool isAiming = false;
    #region Third Person Aim
    [Header("3인칭 Aim")]
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

    private void Awake()
    {
        InitComponents();
        InitVariables();
        SubscribeEvents();
    }

    private void InitComponents()
    {
        cam = GetComponent<CinemachineCamera>();
        orbital = cam.GetComponent<CinemachineOrbitalFollow>();
        inputAxisController = cam.GetComponent<CinemachineInputAxisController>();
    }

    private void SubscribeEvents()
    {
        inputManager.cursorLocked += ActivateCameraInput;
        inputManager.cursorUnLocked = InactivateCameraInput;
    }

    private void InitVariables()
    {
        thirdPerson_DefaultTargetPos = thirdPersonTarget.localPosition;
        thirdPerson_DefaultFov = cam.Lens.FieldOfView;
        targetZoom = currentZoom = orbital.Radius;
    }

    public void ActivateCameraInput() => inputAxisController.enabled = true;
    public void InactivateCameraInput() => inputAxisController.enabled = false;

    private void Update()
    {
        inputManager.HandleCameraInput();
        //ZoomCamera();
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
        float startFov = cam.Lens.FieldOfView;

        float elapsed = 0f;

        while (elapsed < thirdPerson_aimZoomDuration)
        {
            thirdPersonTarget.localPosition = Vector3.Lerp(startPosition, targetPos, elapsed / thirdPerson_aimZoomDuration);
            cam.Lens.FieldOfView = Mathf.Lerp(startFov, targetFov, elapsed / thirdPerson_aimZoomDuration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        thirdPersonTarget.localPosition = targetPos;
    }
}
