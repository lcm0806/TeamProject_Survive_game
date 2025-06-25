using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineableResource : MonoBehaviour
{
    [Header("광물 설정")]
    [SerializeField] public int maxHealth = 5;
    private float currentHealth;

    [Header("드롭 아이템 설정")]
    [Tooltip("파괴 시 튀어나올 아이템 프리팹")]
    public GameObject lootPrefab;
    [Tooltip("아이템 튀어나가는 힘(Impulse)")]
    public float lootLaunchForce = 5f;
    [Tooltip("여러 개를 떨어뜨리고 싶다면 개수 지정")]
    public int lootCount = 1;

    [Header("서서히 사라짐")]
    public float shrinkDuration = 2f;

    private bool isBeingMined = false;
    private bool lootSpawned = false;   // 중복 드롭 방지

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (isBeingMined && currentHealth > 0)
        {
            const float miningDamagePerSecond = 1f;
            currentHealth -= miningDamagePerSecond * Time.deltaTime;
            if (currentHealth <= 0)
            {
                // ① 드롭 아이템 만들기
                SpawnLoot();
                // ② 사라지기 연출
                StartCoroutine(FadeOutAndDestroy());
            }
        }
    }

    public void StartMining() => isBeingMined = true;
    public void StopMining() => isBeingMined = false;

    /* -------------------------- NEW: 아이템 드롭 -------------------------- */
    private void SpawnLoot()
    {
        if (lootSpawned || lootPrefab == null) return;
        lootSpawned = true;

        for (int i = 0; i < lootCount; i++)
        {
            // 살짝 위쪽에 생성
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            GameObject loot = Instantiate(lootPrefab, spawnPos, Quaternion.identity);

            // Rigidbody가 있으면 랜덤 방향 + 위쪽으로 힘을 가함
            if (loot.TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 dir = (Random.insideUnitSphere + Vector3.up).normalized;
                rb.AddForce(dir * lootLaunchForce, ForceMode.Impulse);
            }
        }
    }
    /* -------------------------------------------------------------------- */

    private IEnumerator FadeOutAndDestroy()
    {
        isBeingMined = false;

        Vector3 originalScale = transform.localScale;
        float timer = 0f;

        while (timer < shrinkDuration)
        {
            float t = timer / shrinkDuration;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
