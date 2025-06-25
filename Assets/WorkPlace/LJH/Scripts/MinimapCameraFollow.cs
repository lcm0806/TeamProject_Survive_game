using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    [Header("필수 설정")]
    [Tooltip("추적할 대상(플레이어)")]
    public Transform target;

    [Header("위치 오프셋")]
    [Tooltip("플레이어 기준 오프셋 (기본: 위로 20, 정면 0)")]
    public Vector3 offset = new Vector3(0f, 20f, 0f);

    [Header("카메라 회전 옵션")]
    [Tooltip("✔️ 체크하면 플레이어 방향에 맞춰 미니맵도 회전")]
    public bool rotateWithTarget = false;

    private void LateUpdate()
    {
        if (target == null) return;

        // 1) 즉시 위치 이동 ─────────────────────────────
        transform.position = target.position + offset;

        // 2) 회전 처리 ────────────────────────────────
        if (rotateWithTarget)
        {
            transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}