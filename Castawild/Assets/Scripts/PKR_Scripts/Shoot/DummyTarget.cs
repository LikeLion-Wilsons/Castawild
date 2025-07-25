using Fusion;
using System;
using UnityEngine;

interface IDamageable
{
    void TakeDamage(PlayerRef player, int damage);

}
public class DummyTarget : NetworkBehaviour,IDamageable
{
    [SerializeField] private float _reviveTime = 3f;
    [Networked] private TickTimer _reviveCooldown { get; set; }

    [SerializeField] private bool _useLagCompensation;
    [Networked, OnChangedRender(nameof(OnchangedHealth))] private int health { get; set; } = 100;
    public static event Action<PlayerRef,int> onDamaged;
    private HitboxRoot _hitboxRoot;

    private Collider _collider;
    protected void Awake()
    {
        _hitboxRoot = GetComponent<HitboxRoot>();
        _collider = GetComponentInChildren<Collider>();
    }

    public override void Spawned()
    {
        _collider.enabled = _useLagCompensation == false;
        _hitboxRoot.HitboxRootActive = _useLagCompensation;
    }

    bool IsAlive()
    {
        return health > 0;
    }
    public override void FixedUpdateNetwork()
    {
        if (_useLagCompensation == true)
        {
            _hitboxRoot.HitboxRootActive = IsAlive();
        }
        else
        {
            _collider.enabled = IsAlive();
        }

        if (IsAlive() == false)
        {
            if (_reviveCooldown.Expired(Runner) == true)
            {
                health = 100;
                _reviveCooldown = default;
                
            }
            else if (_reviveCooldown.IsRunning == false)
            {
                _reviveCooldown = TickTimer.CreateFromSeconds(Runner, _reviveTime);
                transform.localScale = Vector3.zero;
            }
        }
    }
    public override void Render()
    {
        transform.localScale = IsAlive() ? Vector3.one : Vector3.zero;
    }

    public void TakeDamage(PlayerRef player, int damage)
    {
        health -= damage;
        RPC_Request(player, damage);
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_Request(PlayerRef player, int damage)
    {
        RPC_Broadcast(player, damage);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_Broadcast(PlayerRef player, int message)
    {
        if (Runner.LocalPlayer != player) return;
        onDamaged?.Invoke(player, message);
    }

    void OnchangedHealth()
    {
        Debug.Log($"[{gameObject.name}] hp:{health}");
    }
}