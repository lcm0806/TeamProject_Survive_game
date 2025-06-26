using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("드롭 프리팹")]
    public GameObject batteryPrefab;
    public GameObject oxygenPrefab;
    public float lootLaunchForce = 5f;

    [Header("파괴(페이드) 연출")]
    public float fadeDuration = 2f;     // 2초 동안 투명화

    private bool _used = false;         // 이미 상호작용 했는지

    /* ------------------- 인터랙트 ------------------- */
    public void Interact()
    {
        if (_used) return;              // 중복 방지
        _used = true;

        float roll = Random.value;

        if (roll < 0.3f) SpawnLoot(batteryPrefab);
        else if (roll < 0.6f) SpawnLoot(oxygenPrefab);
        else ShowMessage("비어있는 보급 상자입니다...");

        StartCoroutine(FadeOutAndDestroy());
    }

    /* ------------------- 드롭 ------------------- */
    private void SpawnLoot(GameObject prefab)
    {
        if (prefab == null) return;

        Vector3 pos = transform.position + Vector3.up * 1.2f;
        GameObject loot = Instantiate(prefab, pos, Quaternion.identity);

        // 상자와 충돌 무시
        if (TryGetComponent<Collider>(out var boxCol) &&
            loot.TryGetComponent<Collider>(out var lootCol))
        {
            Physics.IgnoreCollision(boxCol, lootCol);
        }

        if (loot.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 dir = (Vector3.up + Random.insideUnitSphere * 0.3f).normalized;
            dir.y = Mathf.Max(dir.y, 0.5f);
            rb.AddForce(dir * lootLaunchForce, ForceMode.Impulse);
        }
    }

    /* ------------------- 페이드 & 파괴 ------------------- */
    private System.Collections.IEnumerator FadeOutAndDestroy()
    {
        //보급상자 모든 MeshRenderer 수집
        var renderers = GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length == 0) { Destroy(gameObject); yield break; }

        //각 머티리얼 인스턴스·초기색 저장
        var mats = new System.Collections.Generic.List<Material[]>();
        var startCols = new System.Collections.Generic.List<Color[]>();

        foreach (var r in renderers)
        {
            Material[] arr = r.materials;          // 인스턴스 배열
            mats.Add(arr);

            Color[] cols = new Color[arr.Length];
            for (int i = 0; i < arr.Length; ++i) cols[i] = arr[i].color;
            startCols.Add(cols);
        }

        //페이드 루프
        float t = 0f;
        while (t < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);

            for (int m = 0; m < mats.Count; ++m)
            {
                for (int i = 0; i < mats[m].Length; ++i)
                {
                    Color c = startCols[m][i];
                    c.a = alpha;
                    mats[m][i].color = c;
                }
            }

            t += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    /* ------------------- UI 메시지 ------------------- */
    private void ShowMessage(string msg)
    {
        Debug.Log(msg); // UIManager.Instance.ShowPopup(msg);
    }
}
