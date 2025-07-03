using UnityEngine;

public class MineableResource : MonoBehaviour
{
    [Header("���� ����")]
    [SerializeField]public int maxHealth;
    private float currentHealth;        //float ��! (���� ��������)

    [Header("��� ����")]
    public GameObject lootPrefab;
    public float lootLaunchForce = 5f;

    [Header("�ı� ����")]
    public float shrinkDuration = 2f;

    private bool isBeingMined = false;
    private int lastWholeHp;            //ü���� '���� �κ�'�� ����

    /* -------------------- Unity -------------------- */
    private void Start()
    {
        currentHealth = maxHealth;
        lastWholeHp = maxHealth;      // ó���� ���� ����
    }

    //private void Update()
    //{
    //    if (!isBeingMined || currentHealth <= 0f) return;

    //    //�ʴ� 1�� ������
    //    currentHealth -= 1f * Time.deltaTime;
    //    currentHealth = Mathf.Max(currentHealth, 0f);   // ���� ����

    //    //ü���� 1 �𿴴��� Ȯ��
    //    int currentWholeHp = Mathf.FloorToInt(currentHealth);
    //    if (currentWholeHp < lastWholeHp)        // �� ĭ ����������
    //    {
    //        SpawnLoot();                         // ��� 1��
    //        lastWholeHp = currentWholeHp;        // ���� ����
    //    }

    //    //0�� �Ǿ����� �ı� ����
    //    if (currentHealth <= 0f)
    //        StartCoroutine(FadeOutAndDestroy());
    //}
    public void TakeMiningDamage(float miningDamage)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= miningDamage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        UpdateEmissionColor();

        Debug.Log($"{gameObject.name}�� {miningDamage}��ŭ �������� �޾ҽ��ϴ�");
        int currentWholeHp = Mathf.FloorToInt(currentHealth);
        if (currentWholeHp < lastWholeHp) // �� ĭ ����������
        {
            SpawnLoot(); // ��� 1��
            lastWholeHp = currentWholeHp; // ���� ����
        }

        if (currentHealth <= 0f)
        {
            Debug.Log($"{gameObject.name} ä�� �Ϸ�!");
            //StartCoroutine(FadeOutAndDestroy());
        }
    }


    /* -------------------- Mining Control -------------------- */
    public void StartMining() => isBeingMined = true;
    public void StopMining() => isBeingMined = false;

    /* -------------------- Loot Spawn -------------------- */
    private void SpawnLoot()
    {
        if (lootPrefab == null) return;

        // 1) ���� ��ġ: ���� 1.2m + ���� ��0.5m ����
        Vector3 spread = Random.insideUnitSphere * 1.0f;
        spread.y = Mathf.Abs(spread.y);          // �Ʒ��� �������� �ʰ�
        Vector3 spawnPos = transform.position + Vector3.up * 1.2f + spread;

        GameObject loot = Instantiate(lootPrefab, spawnPos, Quaternion.identity);

        // 2) ���� ��ü�� �浹 ����
        if (TryGetComponent<Collider>(out var parentCol) &&
            loot.TryGetComponent<Collider>(out var lootCol))
        {
            Physics.IgnoreCollision(parentCol, lootCol);
        }

        // 3) ���� ����ġ �� ����
        if (loot.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 dir = (Vector3.up * 0.8f + Random.insideUnitSphere * 0.2f).normalized;
            dir.y = Mathf.Max(dir.y, 0.5f);            // �ּ� ���� 0.5 �̻�
            rb.AddForce(dir * lootLaunchForce, ForceMode.Impulse);
        }
    }

    /* -------------------- Fade & Destroy -------------------- */
    //private IEnumerator FadeOutAndDestroy()
    //{
    //    isBeingMined = false;

    //    MeshRenderer renderer = GetComponent<MeshRenderer>();
    //    if (renderer == null)
    //    {
    //        Debug.LogWarning("MeshRenderer�� �����ϴ�.");
    //        Destroy(gameObject);
    //        yield break;
    //    }

    //    Material[] materials = renderer.materials; // ���׸��� �ν��Ͻ� �迭

    //    // �� ���׸����� �ʱ� ���� ����
    //    Color[] startColors = new Color[materials.Length];
    //    for (int i = 0; i < materials.Length; i++)
    //    {
    //        startColors[i] = materials[i].color;
    //    }

    //    float timer = 0f;

    //    while (timer < shrinkDuration)
    //    {
    //        float t = timer / shrinkDuration;
    //        float fade = Mathf.SmoothStep(1f, 0f, t);

    //        for (int i = 0; i < materials.Length; i++)
    //        {
    //            Color newColor = startColors[i];
    //            newColor.a = fade;
    //            materials[i].color = newColor;
    //        }

    //        timer += Time.deltaTime;
    //        yield return null;
    //    }

    //    Destroy(gameObject);
    //}

    private void UpdateEmissionColor()
    {
        HpCount hpCount = GetComponent<HpCount>();
        if (hpCount == null) return;

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null) return;

        Material[] materials = renderer.materials;
        Color emitColor = hpCount.GetEmitColor(Mathf.RoundToInt(currentHealth));

        foreach (var mat in materials)
        {
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", emitColor);
                mat.EnableKeyword("_EMISSION"); // Emission Ȱ��ȭ �ʿ�!
            }
        }
    }
}