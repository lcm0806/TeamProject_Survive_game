using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineableResource : MonoBehaviour
{
    [Header("광물 설정")]
    [SerializeField]public int maxHealth;
    private float currentHealth;        //float 로! (연속 데미지용)

    [Header("드롭 설정")]
    public GameObject lootPrefab;
    public float lootLaunchForce = 5f;

    [Header("파괴 연출")]
    public float shrinkDuration = 2f;

    private bool isBeingMined = false;
    private int lastWholeHp;            //체력의 '정수 부분'을 저장

    /* -------------------- Unity -------------------- */
    private void Start()
    {
        currentHealth = maxHealth;
        lastWholeHp = maxHealth;      // 처음엔 둘이 같음
    }

    //private void Update()
    //{
    //    if (!isBeingMined || currentHealth <= 0f) return;

    //    //초당 1씩 데미지
    //    currentHealth -= 1f * Time.deltaTime;
    //    currentHealth = Mathf.Max(currentHealth, 0f);   // 음수 방지

    //    //체력이 1 깎였는지 확인
    //    int currentWholeHp = Mathf.FloorToInt(currentHealth);
    //    if (currentWholeHp < lastWholeHp)        // 한 칸 내려갔으면
    //    {
    //        SpawnLoot();                         // 드롭 1개
    //        lastWholeHp = currentWholeHp;        // 기준 갱신
    //    }

    //    //0이 되었으면 파괴 연출
    //    if (currentHealth <= 0f)
    //        StartCoroutine(FadeOutAndDestroy());
    //}
    public void TakeMiningDamage(float miningDamage)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= miningDamage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        UpdateEmissionColor();

        Debug.Log($"{gameObject.name}이 {miningDamage}만큼 데미지를 받았습니다");
        int currentWholeHp = Mathf.FloorToInt(currentHealth);
        if (currentWholeHp < lastWholeHp) // 한 칸 내려갔으면
        {
            SpawnLoot(); // 드롭 1개
            lastWholeHp = currentWholeHp; // 기준 갱신
        }

        if (currentHealth <= 0f)
        {
            Debug.Log($"{gameObject.name} 채굴 완료!");
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

    /* -------------------- Fade & Destroy -------------------- */
    //private IEnumerator FadeOutAndDestroy()
    //{
    //    isBeingMined = false;

    //    MeshRenderer renderer = GetComponent<MeshRenderer>();
    //    if (renderer == null)
    //    {
    //        Debug.LogWarning("MeshRenderer가 없습니다.");
    //        Destroy(gameObject);
    //        yield break;
    //    }

    //    Material[] materials = renderer.materials; // 메테리얼 인스턴스 배열

    //    // 각 메테리얼의 초기 색상 저장
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
                mat.EnableKeyword("_EMISSION"); // Emission 활성화 필요!
            }
        }
    }
}