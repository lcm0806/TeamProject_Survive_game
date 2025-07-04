using UnityEngine;
using UnityEngine.UI;

public class ShelterMarker : MonoBehaviour
{
    [SerializeField] private Image _shelterMarker;

    private Vector2 _shelterPos;

    private void Awake()
    {
        // 쉘터 입구 위치 저장. UI상에서 업데이트 할 것이기에 좌표 2개만 필요
        _shelterPos = new Vector2(257.4693f, -124.8684f);
    }

    private void Update()
    {
        // 마커의 위치를 플레이어의 위치에 맞춰 업데이트
        SetShelterMarker();
    }

    private void SetShelterMarker()
    {
        // 마커의 위치를 옮기는 로직
        // 먼저 플레이어의 x,z를 받아 vector2로 변환
        Vector2 playerPos = new Vector2(PlayerManager.Instance.Player.transform.position.x, PlayerManager.Instance.Player.transform.position.z);
        // 플레이어에서 쉘터 위치의 방향 계산
        Vector2 direction = _shelterPos - playerPos;
        // 플레이어에서 쉘터까지 거리 계산, 카메라가 20만큼의 넓이를 담을 수 있으므로 쉘터의 최대 거리는 20으로 제한되어야함
        float distance = Mathf.Clamp(direction.magnitude, 0, 20);

        Debug.Log("플레이어 좌표: " + playerPos);
        Debug.Log("쉘터 좌표: " + _shelterPos);

        direction.Normalize();

        // 구한 값을 기준으로 ui 마커의 위치를 업데이트
        // 미니맵의 반지름은 128이므로 해당 값을 20에 맞춰 비율을 조정해야됨
        _shelterMarker.rectTransform.anchoredPosition = direction * (distance / 20f) * 128f;
    }
}
