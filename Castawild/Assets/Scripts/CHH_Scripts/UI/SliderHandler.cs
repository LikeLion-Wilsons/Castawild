using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SliderHandler : MonoBehaviour, IPointerUpHandler
{
    [SerializeField] private OptionUI optionUI;
    [SerializeField] private string slideName;

    public void OnPointerUp(PointerEventData eventData)
    {
        optionUI.ChangeSliderValue(slideName);
    }
}
