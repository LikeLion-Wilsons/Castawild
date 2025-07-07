using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    [HideInInspector] public bool isAnimationFinished = false;
    public void AnimationFinishTrigger() => isAnimationFinished = true;
}
