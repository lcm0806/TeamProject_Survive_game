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

    /* -------------------- Unity -------------------- */
    private void Start()
    {
        // HpCount 컴포넌트 참조
        HpCount hpCount = GetComponent<HpCount>();

        if (hpCount != null)
        {
            maxHealth = hpCount.InitialHp;       // 최대 체력 설정
            currentHealth = maxHealth;           // 현재 체력 초기화
            UpdateEmissionColor();
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
        
        SpawnLoot();
        UpdateEmissionColor();
        
        if (currentHealth <= 0f)
        {
            UpdateEmissionColor();
            Debug.Log($"{gameObject.name} 채굴 완료!");
        }
    }

    /* -------------------- Loot Spawn -------------------- */
    private void SpawnLoot()
    {
        if (lootPrefab == null) return;

        // 10 % 확률
        if (Random.value > 0.1f) return;

        /* 1) 드롭 위치 계산 */
        Vector3 spawnPos;
        Vector3 launchDir;

        // 팀원이 저장해둔 RaycastHit 사용
        RaycastHit hit = PlayerManager.Instance.HitInfo;

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            // 충돌 지점에서 법선 방향으로 0.3 m 밀어서 생성
            spawnPos = hit.point + hit.normal * 0.3f;
            launchDir = (hit.normal + Vector3.up * 0.5f).normalized; // 표면 바깥+위쪽
        }
        else
        {
            // Fallback: 기존 방식
            Vector3 spread = Random.insideUnitSphere * 1.0f;
            spread.y = Mathf.Abs(spread.y);
            spawnPos = transform.position + Vector3.up * 1.2f + spread;
            launchDir = (Vector3.up * 0.8f + Random.insideUnitSphere * 0.2f).normalized;
        }

        /* 2) Instantiate & 충돌 무시 */
        GameObject loot = Instantiate(lootPrefab, spawnPos, Quaternion.identity);

        if (TryGetComponent<Collider>(out var parentCol) &&
            loot.TryGetComponent<Collider>(out var lootCol))
        {
            Physics.IgnoreCollision(parentCol, lootCol);
        }

        /* 3) 힘 적용 */
        if (loot.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(launchDir * lootLaunchForce, ForceMode.Impulse);
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
        string[] excludeMaterialNames = { "rockTrack (Instance)", "rock (Instance)", "Gold Bady (Instance)" };

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