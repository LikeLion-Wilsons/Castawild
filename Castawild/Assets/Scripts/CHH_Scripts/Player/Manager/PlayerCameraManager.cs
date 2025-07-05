using Unity.Cinemachine;
using UnityEngine;


public class PlayerCameraManager : MonoBehaviour
{
    [SerializeField] private PlayerInputManager inputManager;
    private CinemachineCamera cam;
    private CinemachineOrbitalFollow orbital;
    private CinemachineInputAxisController inputAxisController;

    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomLerpSpeed = 10f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 15f;

    private float targetZoom;
    private float currentZoom;

    private void Awake()
    {
        cam = GetComponent<CinemachineCamera>();
        orbital = cam.GetComponent<CinemachineOrbitalFollow>();
        inputAxisController = cam.GetComponent<CinemachineInputAxisController>();

        inputManager.cursorLocked += LockRotate;
        inputManager.cursorUnLocked = UnLockRotate;

        targetZoom = currentZoom = orbital.Radius;
    }

    private void LockRotate() => inputAxisController.enabled = true;
    private void UnLockRotate() => inputAxisController.enabled = false;

    private void Update()
    {
        inputManager.HandleCameraInput();
        ZoomCamera();
    }

    private void ZoomCamera()
    {
        if (inputManager.zoomInput.y != 0)
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
}
