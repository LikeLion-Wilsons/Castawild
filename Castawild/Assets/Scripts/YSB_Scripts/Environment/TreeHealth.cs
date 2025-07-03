using UnityEngine;
using System;

public class TreeHealth : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private float maxHealth = 100f;
    public float CurrentHealth { get; private set; }
    public event Action<float, float> OnHealthChanged;
    public event Action OnTreeDied;

    private Action<GameObject> onDeathCallback;

    private bool isInitialized = false;

    public void Init(Action<GameObject> onDeath)
    {
        onDeathCallback = onDeath;
        CurrentHealth = maxHealth;
        isInitialized = true;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (!isInitialized) return;

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(CurrentHealth, 0);
        // Debug.Log($"나무가 {damage}의 피해를 입었습니다. 현재 체력: {CurrentHealth}");

        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (!isInitialized) return;
        isInitialized = false; // 중복 실행 방지

        // Debug.Log("나무가 파괴되었습니다.");

        OnTreeDied?.Invoke();
        onDeathCallback?.Invoke(gameObject);
    }
}