using UnityEngine;

public class MineableResource : MonoBehaviour
{
    [Header("ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½")]
    [SerializeField]public int maxHealth;
    private float currentHealth;        //float ï¿½ï¿½! (ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½)

    [Header("ï¿½ï¿½ï¿? ï¿½ï¿½ï¿½ï¿½")]
    public GameObject lootPrefab;
    public float lootLaunchForce = 5f;

    [Header("ï¿½Ä±ï¿½ ï¿½ï¿½ï¿½ï¿½")]
    public float shrinkDuration = 2f;

    private bool isBeingMined = false;
    private int lastWholeHp;            //Ã¼ï¿½ï¿½ï¿½ï¿½ 'ï¿½ï¿½ï¿½ï¿½ ï¿½Îºï¿½'ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½

    /* -------------------- Unity -------------------- */
    private void Start()
    {
        currentHealth = maxHealth;
        lastWholeHp = maxHealth;      // Ã³ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
    }

    //private void Update()
    //{
    //    if (!isBeingMined || currentHealth <= 0f) return;

    //    //ï¿½Ê´ï¿½ 1ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½
    //    currentHealth -= 1f * Time.deltaTime;
    //    currentHealth = Mathf.Max(currentHealth, 0f);   // ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½

    //    //Ã¼ï¿½ï¿½ï¿½ï¿½ 1 ï¿½ð¿´´ï¿½ï¿½ï¿½ È®ï¿½ï¿½
    //    int currentWholeHp = Mathf.FloorToInt(currentHealth);
    //    if (currentWholeHp < lastWholeHp)        // ï¿½ï¿½ Ä­ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½
    //    {
    //        SpawnLoot();                         // ï¿½ï¿½ï¿? 1ï¿½ï¿½
    //        lastWholeHp = currentWholeHp;        // ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
    //    }

    //    //0ï¿½ï¿½ ï¿½Ç¾ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½Ä±ï¿½ ï¿½ï¿½ï¿½ï¿½
    //    if (currentHealth <= 0f)
    //        StartCoroutine(FadeOutAndDestroy());
    //}
    public void TakeMiningDamage(float miningDamage)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= miningDamage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        UpdateEmissionColor();

        int currentWholeHp = Mathf.FloorToInt(currentHealth);
        if (currentWholeHp < lastWholeHp) // ï¿½ï¿½ Ä­ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½
        {
            SpawnLoot(); // ï¿½ï¿½ï¿? 1ï¿½ï¿½
            lastWholeHp = currentWholeHp; // ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
        }

        if (currentHealth <= 0f)
        {
            Debug.Log($"{gameObject.name} Ã¤ï¿½ï¿½ ï¿½Ï·ï¿½!");
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

        float chance = Random.value; // 0.0 ~ 1.0

        if (chance > 0.1f) return; // 10% È®·ü¸¸ Åë°ú

        // 1) »ý¼º À§Ä¡: À§·Î 1.2m + ¼öÆò ¡¾0.5m ·£´ý
        Vector3 spread = Random.insideUnitSphere * 1.0f;
        spread.y = Mathf.Abs(spread.y);          // ï¿½Æ·ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½Ê°ï¿½
        Vector3 spawnPos = transform.position + Vector3.up * 1.2f + spread;

        GameObject loot = Instantiate(lootPrefab, spawnPos, Quaternion.identity);

        // 2) ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½Ã¼ï¿½ï¿½ ï¿½æµ¹ ï¿½ï¿½ï¿½ï¿½
        if (TryGetComponent<Collider>(out var parentCol) &&
            loot.TryGetComponent<Collider>(out var lootCol))
        {
            Physics.IgnoreCollision(parentCol, lootCol);
        }

        // 3) ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½Ä¡ ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
        if (loot.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 dir = (Vector3.up * 0.8f + Random.insideUnitSphere * 0.2f).normalized;
            dir.y = Mathf.Max(dir.y, 0.5f);            // ï¿½Ö¼ï¿½ ï¿½ï¿½ï¿½ï¿½ 0.5 ï¿½Ì»ï¿½
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
    //        Debug.LogWarning("MeshRendererï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½Ï´ï¿½.");
    //        Destroy(gameObject);
    //        yield break;
    //    }

    //    Material[] materials = renderer.materials; // ï¿½ï¿½ï¿½×¸ï¿½ï¿½ï¿½ ï¿½Î½ï¿½ï¿½Ï½ï¿½ ï¿½è¿­

    //    // ï¿½ï¿½ ï¿½ï¿½ï¿½×¸ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½Ê±ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
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
                mat.EnableKeyword("_EMISSION"); // Emission È°ï¿½ï¿½È­ ï¿½Ê¿ï¿½!
            }
        }
    }
}