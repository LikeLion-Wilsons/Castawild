using UnityEngine;
using System;

public class TreeHealth : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private float maxHealth = 100f;
    public float CurrentHealth { get; private set; } // 현재 체력을 외부에서 읽을 수 있도록 public 속성으로 변경

    // 체력 변경 시 호출될 이벤트 (현재 체력, 최대 체력)
    public event Action<float, float> OnHealthChanged;
    // 나무 사망 시 호출될 이벤트
    public event Action OnTreeDied;

    private string spawnPointId;
    private Action<string> onDeathCallback;

    private bool isInitialized = false;

    void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void Init(string id, Action<string> onDeath)
    {
        spawnPointId = id;
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
        Debug.Log($"나무 {spawnPointId}가 {damage}의 피해를 입었습니다. 현재 체력: {CurrentHealth}");

        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (!isInitialized) return;

        Debug.Log($"나무 {spawnPointId}가 파괴되었습니다.");

        OnTreeDied?.Invoke();
        onDeathCallback?.Invoke(spawnPointId);

        //효과(사운드, 파티클 등)를 추가 가능
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(25);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            CurrentHealth = maxHealth;
            Debug.Log($"나무 {spawnPointId}의 체력이 초기화되었습니다. 현재 체력: {CurrentHealth}");
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }
}