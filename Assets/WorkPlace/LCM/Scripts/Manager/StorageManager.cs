using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorageManager : MonoBehaviour
{
    public GameObject inventorySlotPrefab; // 인벤토리 슬롯 프리팹
    public Transform contentParent;         // Scroll Rect의 Content 오브젝트

    public int numberOfSlotsToCreate = 50; // 생성할 슬롯의 개수 (예시)

    public GameObject StorageUIPanel;


    [Header("UI Management")]
    [SerializeField] private Canvas _gameCanvas;
    private const string MAIN_CANVAS_TAG = "MainUICanvas";


    private void Awake()
    {
        if (_gameCanvas == null)
        {
            GameObject canvasGO = GameObject.FindWithTag(MAIN_CANVAS_TAG);
            if (canvasGO != null)
            {
                _gameCanvas = canvasGO.GetComponent<Canvas>();
            }

            if (_gameCanvas == null)
            {
                Debug.LogError($"'{MAIN_CANVAS_TAG}' 태그를 가진 Canvas를 찾을 수 없습니다. 씬에 Canvas가 있는지, 태그가 올바른지 확인해주세요.");
                return;
            }
        }

        if (_gameCanvas.gameObject.scene.buildIndex != -1)
        {
            DontDestroyOnLoad(_gameCanvas.gameObject);
            Debug.Log($"Inventory: '{MAIN_CANVAS_TAG}' 태그를 가진 _gameCanvas를 DontDestroyOnLoad로 설정했습니다.");
        }
        else
        {
            Debug.Log($"Inventory: '{MAIN_CANVAS_TAG}' 태그를 가진 _gameCanvas가 이미 DontDestroyOnLoad 씬에 있습니다.");

        }

    }
    // Start is called before the first frame update
    void Start()
    {
        GenerateInventorySlots(numberOfSlotsToCreate);
        CloseStorageUI();
        if (StorageUIPanel != null && StorageUIPanel.transform.parent != _gameCanvas.transform)
        {
            StorageUIPanel.transform.SetParent(_gameCanvas.transform, false);
            RectTransform rectTransform = StorageUIPanel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(StorageUIPanel.GetComponent<RectTransform>());

            Debug.Log("Storage 로직 오브젝트가 Canvas 아래로 이동되었습니다.");
        }
        else if (StorageUIPanel != null && StorageUIPanel.transform.parent == _gameCanvas.transform)
        {
            Debug.Log("StorageUIPanel은 이미 Canvas 아래에 있습니다. 다시 설정할 필요가 없습니다.");
        }
        else if (StorageUIPanel == null)
        {
            Debug.LogError("StorageUIPanel이 할당되지 않았습니다. Inspector에서 할당해주세요!");
        }
    }

    public void OpenStorageUI()
    {
        if (StorageUIPanel != null)
        {
            StorageUIPanel.SetActive(true);
            Debug.Log("창고 UI를 열었습니다.");
            // 슬롯 재생성이 필요하다면 여기서 호출 (새로운 아이템이 들어왔을 때 등)
            // GenerateInventorySlots(numberOfSlotsToCreate);
        }
    }

    public void CloseStorageUI()
    {
        if (StorageUIPanel != null)
        {
            StorageUIPanel.SetActive(false);
            Debug.Log("창고 UI를 닫았습니다.");
        }
    }

    public void GenerateInventorySlots(int count)
    {
        // 기존 슬롯 제거 (옵션)
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 새로운 슬롯 생성
        for (int i = 0; i < count; i++)
        {
            GameObject slot = Instantiate(inventorySlotPrefab, contentParent);
            // 여기에 필요하다면 슬롯에 데이터(예: 아이템 정보)를 설정하는 로직 추가
            // 예: slot.GetComponent<InventorySlotUI>().SetItem(inventoryItems[i]);
        }

        // ContentSizeFitter가 즉시 업데이트되지 않을 수 있으므로 RebuildLayoutGroup 호출
        // LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());
    }
}
