using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : UIPart
{
    [Header("Component")]
    [SerializeField] private PlayerCameraManager cameraManager;
    [SerializeField] private PlayerInteractUI interactUI;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI sessionName;

    [Header("Slider")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider fovSlider;

    [Header("Button")]
    [SerializeField] private TextMeshProUGUI cameraShakeButtonText;
    [SerializeField] private Image cameraShakeButton;
    [SerializeField] private TextMeshProUGUI crossHairButtonText;
    [SerializeField] private Image crossHairButton;

    [Header("Color")]
    [SerializeField] private Color onColor;
    [SerializeField] private Color offColor;

    public static Action ReturnToMainMenu;

    public void ChangeValue(string button)
    {
        if (button == "CameraShake")
        {
            cameraManager.MovingCamera = !cameraManager.MovingCamera;
            UpdateButtonUI(cameraShakeButton, cameraShakeButtonText, cameraManager.MovingCamera);
        }
        else
        {
            interactUI.showCrosshair = !interactUI.showCrosshair;
            interactUI.ActiveCrosshair(interactUI.showCrosshair);

            UpdateButtonUI(crossHairButton, crossHairButtonText, interactUI.showCrosshair);
        }
    }

    private void UpdateButtonUI(Image button, TextMeshProUGUI buttonText, bool on)
    {
        Color color = button.color;
        color = on ? onColor : offColor;
        button.color = color;

        buttonText.text = on ? "On" : "Off";
    }

    public void ChangeSliderValue(string name)
    {
        if (name == "Sensivity")
            cameraManager.sensitivity = sensitivitySlider.value;
        else
            cameraManager.ChangeFOV(fovSlider.value);
    }

    public void Return()
    {
        ReturnToMainMenu?.Invoke();
        Close();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // 에디터 플레이모드 종료
#else
    Application.Quit(); // 빌드에서 종료
#endif
    }
}