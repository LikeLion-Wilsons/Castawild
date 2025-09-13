using UnityEngine;

public class VisualRootController : MonoBehaviour
{
    private Renderer[] _renderers;
    private bool _isVisible = true; // 현재 상태 캐싱

    private void Awake()
    {
        CacheRenderers();
    }

    private void CacheRenderers()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
    }

    public void SetVisible(bool visible)
    {
        if (_renderers == null || _renderers.Length == 0)
            CacheRenderers();

        if (_isVisible == visible) return; 
        _isVisible = visible;

        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null)
                _renderers[i].enabled = visible;
        }
    }

    public bool IsVisible => _isVisible;
}
