using Unity.Cinemachine;
using UnityEngine;

public class BaseStateManager : MonoBehaviour
{
    [HideInInspector] public Animator anim;
    [HideInInspector] public PlayerInputManager inputManager;
    [HideInInspector] public CinemachineCamera cam;
    [HideInInspector] public CwPlayer player;

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        inputManager = GetComponent<PlayerInputManager>();
        cam = GetComponentInChildren<CinemachineCamera>();
        player = GetComponent<CwPlayer>();
    }
}