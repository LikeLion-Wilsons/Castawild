using UnityEngine;

public class UIPart : MonoBehaviour
{
    public bool isActive => gameObject.activeSelf;
    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        if (isActive == false)
        {
            Debug.LogWarning("Not Active this UI");
            return;
        }
        gameObject.SetActive(false);
    }

    public virtual void Toggle()
    {
        if (isActive) Close();
        else Open();
    }
}
