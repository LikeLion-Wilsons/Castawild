using System;
using UnityEngine;

public class FootstepCurveReceiver : MonoBehaviour
{
    private Player player;
    [SerializeField] Transform footTransform;

    private int index;
    public float step;
    float prevStep;
    float threshold = 0.5f;

    void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    void Update()
    {
        Sound[] walkSounds = { Sound.Player_Walk1, Sound.Player_Walk2, Sound.Player_Walk3, Sound.Player_Walk4, Sound.Player_Walk5, Sound.Player_Walk6, Sound.Player_Walk7 };
        if (step >= threshold && prevStep < threshold)
            SoundManager.Instance.PlayGlobal3D(walkSounds[index], player.soundPosition.position);
        index = (index + 1) % walkSounds.Length;

        prevStep = step;
    }
}
