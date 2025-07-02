using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class MineableResource : MonoBehaviour
{
    [Header("광물 설정")]
    [SerializeField]public int maxHealth;
    private float currentHealth;        //float 로! (연속 데미지용)

    [Header("드롭 설정")]
    public GameObject lootPrefab;
    public float lootLaunchForce = 5f;
    public float dropCheckCooldown = 1f; // 1초에 한 번만 드롭 체크
    private float dropTimer = 0f;

    [Header("파괴 연출")]
    public float shrinkDuration = 2f;

    private void Start()
    {
        currentHealth = maxHealth;        
    }
    public void TakeMiningDamage(float miningDamage)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= miningDamage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        dropTimer -= Time.deltaTime;
        if (dropTimer <= 0f)
        {
            float r = Random.value;
            if (r < 0.1f)
            {
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
        if (lootPrefab == null) return;

        Vector3 spawnPos = transform.position + Vector3.up * 1f;

        if (TryGetComponent<Collider>(out var col))
        {
            float topY = col.bounds.max.y;
            Vector3 center = col.bounds.center;

            //플레이어 방향 계산 with null check
            Vector3 playerPos = transform.position; // 기본 fallback

            if (PlayerManager.Instance != null && PlayerManager.Instance.Player != null)
            {
                playerPos = PlayerManager.Instance.Player.transform.position;
            }

            Vector3 toPlayer = (playerPos - center).normalized;
            toPlayer.y = 0f;
            toPlayer.Normalize();

            // 플레이어 쪽으로 오프셋
            float offsetDistance = 2.5f;
            spawnPos = new Vector3(center.x, topY + 0.2f, center.z) + toPlayer * offsetDistance;
        }

        GameObject loot = Instantiate(lootPrefab, spawnPos, Quaternion.identity);

        if (loot.TryGetComponent<Collider>(out var lootCol) &&
            TryGetComponent<Collider>(out var parentCol))
        {
            Physics.IgnoreCollision(parentCol, lootCol);
        }

        if (loot.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 dir = (Vector3.up * 0.6f + Random.insideUnitSphere * 0.2f).normalized;
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