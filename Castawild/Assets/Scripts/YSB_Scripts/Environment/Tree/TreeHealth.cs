using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
public class TreeHealth : MonoBehaviour
{
    [Header("Tree Data")]
    [SerializeField] private TreeDefinition definition;
    [SerializeField] private string treeID; // 네트워크 식별 or 저장용

    public float CurrentHealth { get; private set; }

    public event Action<float, float> OnHealthChanged;
    public event Action<TreeHealth> OnTreeDied;

    private bool isInitialized = false;

    public void Init(string id, TreeDefinition def, Action<TreeHealth> onDeathCallback)
    {
        treeID = id;
        definition = def;
        CurrentHealth = definition.maxHealth;
        isInitialized = true;

        OnTreeDied = onDeathCallback;
        OnHealthChanged?.Invoke(CurrentHealth, definition.maxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (!isInitialized) return;

        // (선택) 포톤 마스터 클라이언트만 처리하도록 분기 가능
        // if (!PhotonNetwork.IsMasterClient) return;

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(CurrentHealth, 0);

        OnHealthChanged?.Invoke(CurrentHealth, definition.maxHealth);

        if (CurrentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        if (!isInitialized) return;
        isInitialized = false;

        if (!string.IsNullOrEmpty(definition.dropItemID))
        {
            Debug.Log($"[Tree] {treeID}, 드랍: {definition.dropItemID} x {definition.dropAmount}");
            //나중에 인벤토리에 추가.
        }

        if (!string.IsNullOrEmpty(definition.destroySFX))
        {
            //SoundManager.Instance?.PlaySound(definition.destroySFX, transform.position);
        }

        if (!string.IsNullOrEmpty(definition.destroyVFX))
        {
            //EffectManager.Instance?.PlayEffect(definition.destroyVFX, transform.position);
        }

        OnTreeDied?.Invoke(this);
    }

    public void ResetState()
    {
        isInitialized = false;
        OnTreeDied = null;
        OnHealthChanged = null;
    }
}
