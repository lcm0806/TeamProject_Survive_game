using System.Collections;
using System.Collections.Generic;
// using UnityEditor.VersionControl;
using UnityEngine;

public class PickupItem : Structure
{
    
    public GameObject batteryPrefab;
    public GameObject oxygenPrefab;
    public GameObject AluminumPrefab;
    public GameObject LithiumnPrefab;
    public GameObject IronPrefab;
    public GameObject QuartzPrefab;
    public GameObject TitaniumPrefab;
    public GameObject GoldPrefab;
    public GameObject GrappleGunPrefab;
    public float lootLaunchForce = 5f;

    
    public float fadeDuration = 2f;     

    private bool _used = false;


    public override void Interact()
    {
        if (_used) return;
        _used = true;
        float roll = Random.value;
        Debug.LogWarning($"{roll}");
        if (roll < 0.3f) SpawnLoot(batteryPrefab);
        else if (roll < 0.6f) SpawnLoot(oxygenPrefab);
        else if (roll < 0.65f) SpawnLoot(AluminumPrefab);
        else if (roll < 0.7f) SpawnLoot(LithiumnPrefab);
        else if (roll < 0.75f) SpawnLoot(IronPrefab);
        else if (roll < 0.8f) SpawnLoot(QuartzPrefab);
        else if (roll < 0.85f) SpawnLoot(TitaniumPrefab);
        else if (roll < 0.95f) SpawnLoot(GoldPrefab);
        else SpawnLoot(GrappleGunPrefab);


        // TODO: 오류나서 임시 주석(LJH폴더에 있는 디졸브 스크립트는 지우고 Import 안에 있는 디졸브 스크립트를 이용해야 합니다.)
        var dissolve = GetComponentInChildren<DissolveExample.DissolveChilds>();
        if (dissolve != null)
        {
            dissolve.StartDissolve(2f);
        }

        Destroy(gameObject, 2.5f);
    }


    private void SpawnLoot(GameObject prefab)
    {
        if (prefab == null) return;

        Vector3 pos = transform.position + Vector3.up * 1.2f;
        GameObject loot = Instantiate(prefab, pos, Quaternion.identity);

        // ���ڿ� �浹 ����
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

    ///* ------------------- ���̵� & �ı� ------------------- */
    //private System.Collections.IEnumerator FadeOutAndDestroy()
    //{
    //    //���޻��� ��� MeshRenderer ����
    //    var renderers = GetComponentsInChildren<MeshRenderer>();
    //    if (renderers.Length == 0) { Destroy(gameObject); yield break; }

        
    //    var mats = new System.Collections.Generic.List<Material[]>();
    //    var startCols = new System.Collections.Generic.List<Color[]>();

    //    foreach (var r in renderers)
    //    {
    //        Material[] arr = r.materials;          // �ν��Ͻ� �迭
    //        mats.Add(arr);

    //        Color[] cols = new Color[arr.Length];
    //        for (int i = 0; i < arr.Length; ++i) cols[i] = arr[i].color;
    //        startCols.Add(cols);
    //    }

    //    //���̵� ����
    //    float t = 0f;
    //    while (t < fadeDuration)
    //    {
    //        float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);

    //        for (int m = 0; m < mats.Count; ++m)
    //        {
    //            for (int i = 0; i < mats[m].Length; ++i)
    //            {
    //                Color c = startCols[m][i];
    //                c.a = alpha;
    //                mats[m][i].color = c;
    //            }
    //        }

    //        t += Time.deltaTime;
    //        yield return null;
    //    }

    //    Destroy(gameObject);
    //}

    /* ------------------- UI �޽��� ------------------- */
    private void ShowMessage(string message)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPopup(message);
        }
        else
        {
            Debug.LogWarning("UIManager가 씬에 없습니다.");
        }
    }

    
}
