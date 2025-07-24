using Fusion;
using System;
using UnityEngine;

interface IDamageable
{
    void TakeDamage(int damage);

}
public class DummyTarget : NetworkBehaviour,IDamageable
{
    [SerializeField] private float _reviveTime = 3f;
    [Networked] private TickTimer _reviveCooldown { get; set; }

    [SerializeField] private bool _useLagCompensation;
    [Networked,OnChangedRender(nameof(OnchangedHealth))] private int health{ get; set; }
    public static event Action<int> onDamaged;
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
                transform.localScale = Vector3.one;
            }
            else if (_reviveCooldown.IsRunning == false)
            {
                _reviveCooldown = TickTimer.CreateFromSeconds(Runner, _reviveTime);
                transform.localScale = Vector3.zero;
            }
        }
    }


    public void TakeDamage(int damage)
    {
        health -= damage;
        onDamaged?.Invoke(damage);
    }

    void OnchangedHealth()
    {
        Debug.Log($"[{gameObject.name}] hp:{health}");
    }
}