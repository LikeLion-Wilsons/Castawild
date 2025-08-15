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
        int randomNumber = Random.Range(0, 7);
        Sound[] walkSounds = { Sound.Player_Walk1, Sound.Player_Walk2, Sound.Player_Walk3, Sound.Player_Walk4, Sound.Player_Walk5, Sound.Player_Walk6, Sound.Player_Walk7 };

        if (step >= threshold && prevStep < threshold)
            SoundManager.Instance.PlayGlobalSound3D(walkSounds[randomNumber], transform.position);

        prevStep = step;
    }
}
