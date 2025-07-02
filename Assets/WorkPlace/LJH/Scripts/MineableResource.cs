using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class MineableResource : MonoBehaviour
{
    [Header("���� ����")]
    [SerializeField]public int maxHealth;
    private float currentHealth;        //float ��! (���� ��������)

    [Header("��� ����")]
    public GameObject lootPrefab;
    public float lootLaunchForce = 5f;
    public float dropCheckCooldown = 1f; // 1�ʿ� �� ���� ��� üũ
    private float dropTimer = 0f;

    [Header("�ı� ����")]
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

            dropTimer = dropCheckCooldown; // ��Ÿ�� �ʱ�ȭ
        }
        if (currentHealth <= 0f)
        {
            UpdateEmissionColor();
            Debug.Log($"{gameObject.name} ä�� �Ϸ�!");
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

            //�÷��̾� ���� ��� with null check
            Vector3 playerPos = transform.position; // �⺻ fallback

            if (PlayerManager.Instance != null && PlayerManager.Instance.Player != null)
            {
                playerPos = PlayerManager.Instance.Player.transform.position;
            }

            Vector3 toPlayer = (playerPos - center).normalized;
            toPlayer.y = 0f;
            toPlayer.Normalize();

            // �÷��̾� ������ ������
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

        // ������ ��Ƽ���� �̸��� (��Ȯ�� ��ġ�ϴ� �̸�)
        string[] excludeMaterialNames = { "rockTrack (Instance)", "rock (Instance)" };

        for (int i = 0; i < materials.Length; i++)
        {
            Material mat = materials[i];
            string matName = mat.name;

            // ��Ȯ�� ��ġ�ϴ� �̸��̸� ����
            bool isExcluded = System.Array.Exists(excludeMaterialNames, name => name == matName);

            if (!isExcluded && mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", emitColor);
                mat.EnableKeyword("_EMISSION");
            }
        }
    }

}