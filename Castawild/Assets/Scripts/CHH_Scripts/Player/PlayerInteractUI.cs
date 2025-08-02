using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractUI : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup interactableUI;
    public CanvasGroup placeableUI;
    public Image crosshairImage;

    public TextMeshProUGUI interactableText;
    [SerializeField] private Sprite originImage;
    [SerializeField] private Sprite axeImage;
    [SerializeField] private Sprite pickaxeImage;

    public void ChangeCrosshairUI(InteractableType type = InteractableType.None)
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

        else if (interactableType == InteractableType.Item)
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

    public void TurnOffUI()
    {
        interactableUI.alpha = 0f;
        placeableUI.alpha = 0f;
    }

    public void SetWakeUpUI()
    {
        interactableUI.alpha = 1f;
        placeableUI.alpha = 0f;
        interactableText.text = "Wake Up";
    }
}