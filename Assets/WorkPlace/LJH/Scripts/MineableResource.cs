using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineableResource : MonoBehaviour
{
    [Header("광물 설정")]
    public int maxHealth;
    private float currentHealth;        //float 로! (연속 데미지용)

    [Header("드롭 설정")]
    public GameObject lootPrefab;
    public float lootLaunchForce = 5f;

    [Header("파괴 연출")]
    public float shrinkDuration = 2f;

    public float dropCheckCooldown = 1f; // 1초에 한 번만 드롭 체크
    private float dropTimer = 0f;

    /* -------------------- Unity -------------------- */
    private void Start()
    {
        // HpCount 컴포넌트 참조
        HpCount hpCount = GetComponent<HpCount>();

        if (hpCount != null)
        {
            maxHealth = hpCount.InitialHp;       // 최대 체력 설정
            currentHealth = maxHealth;           // 현재 체력 초기화
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}에 HpCount 컴포넌트가 없습니다. 디폴트 maxHealth({maxHealth}) 사용.");
            currentHealth = maxHealth;
        }
    }

    public void TakeMiningDamage(float miningDamage)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= miningDamage;
        currentHealth = Mathf.Max(currentHealth, 0f);
        //Debug.Log($"{gameObject.name}이 {miningDamage}만큼 데미지를 받았습니다");
        //Debug.Log($"{gameObject.name}의 현재 체력은 {currentHealth}입니다");

        dropTimer -= Time.deltaTime;

        if (dropTimer <= 0f)
        {
            float r = Random.value;
            Debug.Log($"Random.value = {r}");
            Debug.Log($"렌덤값 {r}");
            if (r < 0.1f)
            {
                Debug.Log(">> 드롭 발생!");
                SpawnLoot();
                UpdateEmissionColor();
            }

            dropTimer = dropCheckCooldown; // 쿨타임 초기화
        }

        if (currentHealth <= 0f)
        {
            UpdateEmissionColor();
            Debug.Log($"{gameObject.name} 채굴 완료!");
        }
    }

    /* -------------------- Loot Spawn -------------------- */
    private void SpawnLoot()
    {
        //if (lootPrefab == null) return;

        //float chance = Random.value; // 0.0 ~ 1.0

        //if (chance > 0.1f) return; // 10% 확률만 통과

        // 1) 생성 위치: 위로 1.2m + 수평 ±0.5m 랜덤
        Vector3 spread = Random.insideUnitSphere * 1.0f;
        spread.y = Mathf.Abs(spread.y);          // 아래로 떨어지지 않게
        Vector3 spawnPos = transform.position + Vector3.up * 1.2f + spread;

        GameObject loot = Instantiate(lootPrefab, spawnPos, Quaternion.identity);

        // 2) 광물 본체와 충돌 무시
        if (TryGetComponent<Collider>(out var parentCol) &&
            loot.TryGetComponent<Collider>(out var lootCol))
        {
            Physics.IgnoreCollision(parentCol, lootCol);
        }

        // 3) 위쪽 가중치 힘 적용
        if (loot.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 dir = (Vector3.up * 0.8f + Random.insideUnitSphere * 0.2f).normalized;
            dir.y = Mathf.Max(dir.y, 0.5f);            // 최소 위로 0.5 이상
            rb.AddForce(dir * lootLaunchForce, ForceMode.Impulse);
        }
    }

    private void UpdateEmissionColor()
    {
        HpCount hpCount = GetComponent<HpCount>();
        if (hpCount == null) return;

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null) return;

        Material[] materials = renderer.materials;
        Color emitColor = hpCount.GetEmitColor(Mathf.RoundToInt(currentHealth));

        // 제외할 머티리얼 이름들 (정확히 일치하는 이름)
        string[] excludeMaterialNames = { "rockTrack (Instance)", "rock (Instance)" };

        for (int i = 0; i < materials.Length; i++)
        {
            Material mat = materials[i];
            string matName = mat.name;

            // 정확히 일치하는 이름이면 제외
            bool isExcluded = System.Array.Exists(excludeMaterialNames, name => name == matName);

            if (!isExcluded && mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", emitColor);
                mat.EnableKeyword("_EMISSION");             
            }
        }
    }
}