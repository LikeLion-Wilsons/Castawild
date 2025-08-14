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
        if (_renderers == null || _renderers.Length == 0)
            _renderers = GetComponentsInChildren<Renderer>(true); // 아직 초기화 안 된 경우 다시 가져오기

        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].enabled = visible;
        }
    }
}
