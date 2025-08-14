using UnityEngine;

public class VisualRootController : MonoBehaviour
{
    private Renderer[] _renderers;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
    }

    public void SetVisible(bool visible)
    {
        if (_renderers == null) return;

        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].enabled = visible;
        }
    }
}
