using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarehouseUpgradeMonitor : MonoBehaviour
{
    [System.Serializable] // 유니티 인스펙터에서 리스트/배열을 보기 좋게 만듭니다.
    public class UpgradeItemMapping
    {
        public PlayerUpgradeType upgradeType; // Item.cs에 정의된 Enum
        public Item itemData;                 // 인스펙터에서 드래그앤드롭으로 연결할 Item ScriptableObject
        // public int playerManagerUpgradeIndex; // PlayerUpgradeType이 이미 인덱스를 가지고 있다면 필요 없음
    }

    [SerializeField] private List<UpgradeItemMapping> _upgradeMappings;

    private Dictionary<PlayerUpgradeType, Item> _upgradeTypeToItemMap;

    private void Awake()
    {
        _upgradeTypeToItemMap = new Dictionary<PlayerUpgradeType, Item>();
        foreach (var mapping in _upgradeMappings)
        {
            if (mapping.itemData != null)
            {
                _upgradeTypeToItemMap[mapping.upgradeType] = mapping.itemData;
            }
            else
            {
                Debug.LogWarning($"WarehouseUpgradeMonitor: '{mapping.upgradeType}'에 대한 Item 데이터가 할당되지 않았습니다.");
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (Storage.Instance != null)
        {
            // Storage의 OnStorageSlotItemUpdated 이벤트 구독
            Storage.Instance.OnStorageSlotItemUpdated += OnStorageItemUpdated;
            // 초기 상태 업데이트
            UpdateAllUpgradeStatuses();
        }
        else
        {
            Debug.LogError("WarehouseUpgradeMonitor: Storage.Instance를 찾을 수 없습니다. Storage가 씬에 있는지 확인하세요.");
        }
    }

    private void OnDestroy()
    {
        // 스크립트가 파괴될 때 이벤트 구독 해제 (메모리 누수 방지)
        if (Storage.Instance != null)
        {
            Storage.Instance.OnStorageSlotItemUpdated -= OnStorageItemUpdated;
        }
    }

    private void OnStorageItemUpdated(int slotIndex, Item changedItem, int newQuantity)
    {
        // 모든 강화 아이템의 상태를 다시 확인하여 업데이트
        // 특정 아이템만 변경되었더라도, 모든 강화 상태를 다시 검사하는 것이 가장 안전하고 단순합니다.
        UpdateAllUpgradeStatuses();
    }

    private void UpdateAllUpgradeStatuses()
    {
        if (PlayerManager.Instance == null) return;
        if (Storage.Instance == null) return;

        foreach (PlayerUpgradeType type in Enum.GetValues(typeof(PlayerUpgradeType)))
        {
            if (type == PlayerUpgradeType.None) continue; // None은 건너뛰기

            // 해당 강화 타입에 매핑된 실제 Item 데이터를 가져옵니다.
            if (!_upgradeTypeToItemMap.TryGetValue(type, out Item targetItem))
            {
                // 매핑되지 않은 강화 타입은 건너뜁니다.
                // Debug.LogWarning($"WarehouseUpgradeMonitor: '{type}'에 대한 Item 매핑이 없습니다. Inspector를 확인하세요.");
                continue;
            }

            // Storage에 해당 아이템이 하나라도 있는지 확인
            bool hasUpgradeItem = Storage.Instance.GetItemCount(targetItem) > 0;

            // PlayerManager의 강화 상태 업데이트 함수 호출
            //PlayerManager.Instance.SetPlayerUpgradeState(type, hasUpgradeItem);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
