using Fusion;
using System.Collections;
using Test.Shoot;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractUI : MonoBehaviour
{
    private PlayerController playerController;
    private PlayerInputManager inputManager;
    private MovementStateManager movementStateManager;

    [Header("Interact")]
    public CanvasGroup interactableUI;
    [SerializeField] private CanvasGroup placeableUI;
    public Image crosshairImage;

    private string originalText;
    public TextMeshProUGUI interactableText;
    [SerializeField] private Sprite originImage;
    [SerializeField] private Sprite axeImage;
    [SerializeField] private Sprite pickaxeImage;

    [Header("DeathUI")]
    [SerializeField] private float reviveTime = 1f;
    [SerializeField] private float autoReviveTime = 5f;
    [SerializeField] private GameObject deathUI;
    [SerializeField] private Image deathBackground;
    [SerializeField] private Image revivedBar;
    [SerializeField] private CanvasGroup deathText;
    private Animator deathAnim;

    [Header("Aim & Hit")]
    [SerializeField] private CanvasGroup aimCrosshairGroup;
    [SerializeField] public float aimZoomDuration = 0.3f;
    private Coroutine aimCrosshairCoroutine;
    [SerializeField] private UIHitNumbers _hitNumber;

    private bool canRevived = false;
    private float pressedRevivedElapsed;
    private float autoRevivedElapsed;

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
        inputManager = GetComponentInParent<PlayerInputManager>();
        movementStateManager = GetComponentInParent<MovementStateManager>();
        deathAnim = GetComponent<Animator>();
        originalText = interactableText.text;
    }

    void Start()
    {
        playerController.Hit += OnTargetDamaged;
        UIPart.openUI += Client_TurnOffInteractiveUI;
    }

    void OnDestroy()
    {
        playerController.Hit -= OnTargetDamaged;
        UIPart.openUI += Client_TurnOffInteractiveUI;
    }

    private void OnTargetDamaged(int damage) => _hitNumber.OnHit(damage);

    private void Update()
    {
        if (!canRevived || !movementStateManager.HasInputAuthority)
            return;

        autoRevivedElapsed += Time.deltaTime;

        if (inputManager.interactAction.IsPressed())
        {
            pressedRevivedElapsed += Time.deltaTime;
            revivedBar.fillAmount = pressedRevivedElapsed / reviveTime;
        }
        else
        {
            revivedBar.fillAmount = 0f;
            pressedRevivedElapsed = 0f;
            revivedBar.fillAmount = autoRevivedElapsed / autoReviveTime;
        }

        if (autoRevivedElapsed >= autoReviveTime || pressedRevivedElapsed >= reviveTime)
        {
            canRevived = false;
            revivedBar.fillAmount = 0f;
            autoRevivedElapsed = 0f;
            pressedRevivedElapsed = 0f;

            movementStateManager.RPC_RequestRevived();
            ActiveDeathUI(false);
        }
    }

    /// <summary>
    /// Interactable UI 변경
    /// </summary>
    public void InteractUI(InteractableType interactableType = InteractableType.None)
    {
        if (interactableType == InteractableType.Bed ||
            interactableType == InteractableType.Box ||
            interactableType == InteractableType.Campfire ||
            interactableType == InteractableType.WaterPurifier)
        {
            interactableUI.alpha = 1f;
            placeableUI.alpha = 1f;
        }

        else if (interactableType == InteractableType.Tree || interactableType == InteractableType.Stone)
        {
            interactableUI.alpha = 0f;
            placeableUI.alpha = 0f;
        }

        else if (interactableType == InteractableType.Item || interactableType == InteractableType.Gatherable)
        {
            interactableUI.alpha = 1f;
            placeableUI.alpha = 0f;
        }

        else if (interactableType == InteractableType.None)
        {
            interactableUI.alpha = 0f;
            placeableUI.alpha = 0f;
        }

        ChangeCrosshairUI(interactableType);
    }

    private void ChangeCrosshairUI(InteractableType type = InteractableType.None)
    {
        switch (type)
        {
            case InteractableType.Tree:
                crosshairImage.GetComponent<RectTransform>().sizeDelta = new Vector2(70f, 70f);
                crosshairImage.sprite = axeImage;
                break;
            case InteractableType.Stone:
                crosshairImage.GetComponent<RectTransform>().sizeDelta = new Vector2(70f, 70f);
                crosshairImage.sprite = pickaxeImage;
                break;
            case InteractableType.None:
            default:
                crosshairImage.GetComponent<RectTransform>().sizeDelta = new Vector2(10f, 10f);
                crosshairImage.sprite = originImage;
                break;
        }
    }

    /// <summary>
    /// 상호작용가능한 UI 끄기
    /// </summary>
    public void Client_TurnOffInteractiveUI(bool turnOff = true)
    {
        if (turnOff)
        {
            interactableUI.alpha = 0f;
            placeableUI.alpha = 0f;
        }
    }

    /// <summary>
    /// WakeUp UI 
    /// </summary>
    public void SetWakeUpUI()
    {
        if (interactableUI.alpha == 1f)
            return;

        interactableUI.alpha = 1f;
        placeableUI.alpha = 0f;
        SetInteractText("Wake Up");
    }

    /// <summary>
    /// 사망 UI
    /// </summary>
    public void ActiveDeathUI(bool active)
    {
        if (active)
        {
            deathUI.SetActive(true);
            deathAnim.SetBool("Death", true);
        }

        Color backgroundColor = deathBackground.color;
        backgroundColor.a = 0f;
        deathBackground.color = backgroundColor;

        deathText.alpha = 0f;

        if (!active)
        {
            deathUI.SetActive(false);
            deathAnim.SetBool("Death", false);
        }
    }

    /// <summary>
    /// 에임 Crosshair UI 설정
    /// </summary>
    public void SetAimCrosshair(bool isAiming)
    {
        if (aimCrosshairCoroutine != null)
            StopCoroutine(aimCrosshairCoroutine);

        if (isAiming)
            aimCrosshairCoroutine = StartCoroutine(ShowAimCrosshairCoroutine(aimCrosshairGroup.alpha, 1f));
        else
            aimCrosshairCoroutine = StartCoroutine(ShowAimCrosshairCoroutine(aimCrosshairGroup.alpha, 0f));
    }

    private IEnumerator ShowAimCrosshairCoroutine(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;

        while (elapsedTime < aimZoomDuration)
        {
            elapsedTime += Time.deltaTime;
            aimCrosshairGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / aimZoomDuration);
            yield return null;
        }

        aimCrosshairGroup.alpha = endAlpha;
    }

    /// <summary>
    /// 사망UI 알파값 유지 - 애니메이션 트리거용 
    /// </summary>
    public void ShowDeathUI()
    {
        Color backgroundColor = deathBackground.color;
        backgroundColor.a = 1f;
        deathBackground.color = backgroundColor;

        deathText.alpha = 1f;
        canRevived = true;
    }

    public void SetInteractText(string text) => interactableText.text = originalText + text;
}