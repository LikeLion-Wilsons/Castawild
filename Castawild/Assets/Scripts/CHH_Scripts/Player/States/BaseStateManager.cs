using Unity.Cinemachine;
using UnityEngine;

public class BaseStateManager : MonoBehaviour
{
    [HideInInspector] public Animator anim;
    [HideInInspector] public PlayerInputManager inputManager;
    [HideInInspector] public CinemachineCamera cineCam;
    [HideInInspector] public CwPlayer player;

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        inputManager = GetComponent<PlayerInputManager>();
        cineCam = GetComponentInChildren<CinemachineCamera>();
        player = GetComponent<CwPlayer>();
    }
}