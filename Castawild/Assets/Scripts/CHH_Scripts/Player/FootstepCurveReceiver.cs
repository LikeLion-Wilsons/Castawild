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
        int randomNumber = UnityEngine.Random.Range(0, 7);
        Sound[] walkSounds = { Sound.Player_Sleep3, Sound.Player_Sleep3 };

        if (step >= threshold && prevStep < threshold)
            SoundManager.Instance.PlayGlobalSound3D(walkSounds[randomNumber], transform.position);

        prevStep = step;
    }
}
