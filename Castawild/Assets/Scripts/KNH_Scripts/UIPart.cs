using UnityEngine;

public class UIPart : MonoBehaviour
{
    public bool isActive => gameObject.activeSelf;
    // 수정한 부분
    public virtual void Open(PlayerInputManager inputManager = null)
    {
        inputManager?.UnlockCursor();
        gameObject.SetActive(true);
    }

    // 수정한 부분
    public virtual void Close(PlayerInputManager inputManager = null)
    {
        if (isActive == false)
        {
            Debug.LogWarning("Not Active this UI");
            return;
        }
        inputManager?.LockCursor();
        gameObject.SetActive(false);
    }

    // 수정한 부분
    public virtual void Toggle(PlayerInputManager inputManager = null)
    {
        if (isActive) Close(inputManager);
        else Open(inputManager);
    }

    public virtual bool IsOpen()
    {
        // 수정한 부분
        return gameObject.activeSelf;
    }
}
