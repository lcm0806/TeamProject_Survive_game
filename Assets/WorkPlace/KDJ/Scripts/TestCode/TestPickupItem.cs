using UnityEngine;

public class TestPickupItem : Structure
{
    [Header("��� ������")]
    public GameObject batteryPrefab;
    public GameObject oxygenPrefab;
    public float lootLaunchForce = 5f;

    [Header("�ı�(���̵�) ����")]
    public float fadeDuration = 2f;     // 2�� ���� ����ȭ

    private bool _used = false;         // �̹� ��ȣ�ۿ� �ߴ���

    /* ------------------- ���ͷ�Ʈ ------------------- */
    public override void Interact()
    {
        if (_used) return;              // �ߺ� ����
        _used = true;

        float roll = Random.value;

        if (roll < 0.3f) SpawnLoot(batteryPrefab);
        else if (roll < 0.6f) SpawnLoot(oxygenPrefab);
        else ShowMessage("����ִ� ���� �����Դϴ�...");

        StartCoroutine(FadeOutAndDestroy());
    }

    /* ------------------- ��� ------------------- */
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

    /* ------------------- ���̵� & �ı� ------------------- */
    private System.Collections.IEnumerator FadeOutAndDestroy()
    {
        //���޻��� ��� MeshRenderer ����
        var renderers = GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length == 0) { Destroy(gameObject); yield break; }

        //�� ��Ƽ���� �ν��Ͻ����ʱ�� ����
        var mats = new System.Collections.Generic.List<Material[]>();
        var startCols = new System.Collections.Generic.List<Color[]>();

        foreach (var r in renderers)
        {
            Material[] arr = r.materials;          // �ν��Ͻ� �迭
            mats.Add(arr);

            Color[] cols = new Color[arr.Length];
            for (int i = 0; i < arr.Length; ++i) cols[i] = arr[i].color;
            startCols.Add(cols);
        }

        //���̵� ����
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

    /* ------------------- UI �޽��� ------------------- */
    private void ShowMessage(string message)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPopup(message);
        }
        else
        {
            Debug.LogWarning("UIManager�� ���� �����ϴ�.");
        }
    }
}
