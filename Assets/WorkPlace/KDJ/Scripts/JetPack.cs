using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetPack : MonoBehaviour
{
    [SerializeField] GameObject _jetPackObject;
    [SerializeField] Material _noJetPack;
    [SerializeField] Material _jetPack;

    private float _airUsage = 0;

    private void Awake()
    {
        PlayerManager.Instance.IsUpgraded[0] = true;

        if (PlayerManager.Instance.IsUpgraded[0])
        {
            _jetPackObject.GetComponent<Renderer>().material = _jetPack;
        }
        else
        {
            _jetPackObject.GetComponent<Renderer>().material = _noJetPack;
        }
    }

    /// <summary>
    /// 입력으로 카메라의 정면 방향을 받아 해당 방향으로 제트팩을 사용합니다.
    /// </summary>
    /// <param name="camForward">카메라 정면 방향</param>
    public Vector3 UseUpgrade(Vector3 camForward)
    {
        if (!PlayerManager.Instance.IsUpgraded[0] || PlayerManager.Instance.AirGauge.Value <= 0) return Vector3.zero;

        // 제트팩 사용 시 0.5초당 플레이어의 산소 1 감소
        
        _airUsage += 2f * Time.deltaTime;

        if (_airUsage >= 1f)
        {
            PlayerManager.Instance.AirGauge.Value -= 1f;
            _airUsage = 0f;
        }

        return camForward * 5f;
    }
     
}
