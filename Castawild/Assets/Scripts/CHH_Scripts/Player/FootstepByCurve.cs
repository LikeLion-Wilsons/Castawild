using Fusion;
using UnityEngine;

public class FootstepByCurve : NetworkBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] Transform leftFoot, rightFoot;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float rayLen = 0.6f; // 레이 최대 거리

    int kL, kR;
    float prevL, prevR;
    const float TH = 0.5f; // 임계값 

    void Awake()
    {
        kL = Animator.StringToHash("Footstep (0)");
        kR = Animator.StringToHash("Footstep (1)");
    }

    void Update()
    {
        // 소리 중복 재생 방지
        float l = anim.GetFloat(kL);
        if (l >= TH && prevL < TH)
            Step(leftFoot);

        float r = anim.GetFloat(kR);
        if (r >= TH && prevR < TH)
            Step(rightFoot);

        prevL = l; prevR = r;
    }

    void Step(Transform foot)
    {
        Vector3 position = foot.position + Vector3.up * 0.1f;
        if (Physics.Raycast(position, Vector3.down, out var hit, rayLen, groundMask))
            SoundManager.Instance.PlayLocalSound3D(Object.InputAuthority, Sound.Player_Walk, hit.point);
        else
            SoundManager.Instance.PlayLocalSound3D(Object.InputAuthority, Sound.Player_Walk, foot.position);
    }
}
