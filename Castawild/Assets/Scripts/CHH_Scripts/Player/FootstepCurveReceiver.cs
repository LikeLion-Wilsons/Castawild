using UnityEngine;

public class FootstepCurveReceiver : MonoBehaviour
{
    private Animator anim;
    private Player player;
    private AudioSource audioSource;
    [SerializeField] Transform footTransform;

    public float step;
    float prevStep;
    float threshold = 0.5f;

    void Awake()
    {
        anim = GetComponent<Animator>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (step >= threshold && prevStep < threshold)
            SoundManager.Instance.PlayLocalSound3D(player.Object.InputAuthority, Sound.Player_Walk, footTransform.position);

        prevStep = step;
    }
}
