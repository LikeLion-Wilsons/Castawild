using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;

public class OptionUI : UIPart
{
    [Header("Component")]
    [SerializeField] private PlayerCameraManager cameraManager;
    [SerializeField] private PlayerInteractUI interactUI;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI sessionName;

    [Header("Slider")]
    [SerializeField] private Slider sensivitySlider;
    [SerializeField] private Slider fovSlider;

    [Header("Button")]
    [SerializeField] private TextMeshProUGUI cameraShakeButtonText;
    [SerializeField] private Button cameraShakeButton;
    [SerializeField] private TextMeshProUGUI crossHairButtonText;
    [SerializeField] private Button crossHairButton;

    [Header("Color")]
    [SerializeField] private Color onColor;  // On 상태 색
    [SerializeField] private Color offColor;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Toggle();
    }

    public void ChangeValue(string button)
    {
        if (button == "CameraShake")
            UpdateButtonUI(cameraShakeButton, cameraShakeButtonText, cameraManager.MovingCamera);
        else
            UpdateButtonUI(crossHairButton, crossHairButtonText, interactUI.showCrosshair);
    }

    private void UpdateButtonUI(Button button, TextMeshProUGUI buttonText, bool target)
    {
        target = !target;

        ColorBlock colors = button.colors;
        colors.normalColor = target ? onColor : offColor;

        buttonText.text = target ? "On" : "Off";
    }

    public void ChangeSliderValue(string name)
    {
        if (name == "Sensivity")
            cameraManager.sensivity = sensivitySlider.value;
        else
            cameraManager.ChangeFOV(sensivitySlider.value);
    }
}