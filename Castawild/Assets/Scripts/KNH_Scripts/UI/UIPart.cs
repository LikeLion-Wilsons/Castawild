using Fusion;
using System;
using UnityEngine;

public class UIPart : NetworkBehaviour
{
    public static event Action<bool> openUI;
    public bool isActive => gameObject.activeSelf;
    // 수정한 부분
    public virtual void Open()
    {
        openUI?.Invoke(true);
        gameObject.SetActive(true);
    }

    // 수정한 부분
    public virtual void Close()
    {
        openUI?.Invoke(false);
        if (isActive == false)
        {
            //Debug.LogWarning("Not Active this UI");
            return;
        }
        gameObject.SetActive(false);
    }

    // 수정한 부분
    public virtual void Toggle()
    {
        if (isActive) Close();
        else Open();
    }

    public virtual bool IsOpen()
    {
        // 수정한 부분
        return gameObject.activeSelf;
    }
}
