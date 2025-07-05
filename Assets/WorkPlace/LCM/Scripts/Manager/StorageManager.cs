using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DesignPattern;

public class StorageManager : Singleton<StorageManager>
{

    public GameObject inventorySlotPrefab; // �κ��丮 ���� ������
    public Transform contentParent;         // Scroll Rect�� Content ������Ʈ

    public int numberOfSlotsToCreate = 50; // ������ ������ ���� (����)

    public GameObject StorageUIPanel;


    [Header("UI Management")]
    [SerializeField] private Canvas _gameCanvas;
    private const string MAIN_CANVAS_TAG = "StorageUICanvas";

    private List<InventorySlot> generatedStorageSlots = new List<InventorySlot>();


    private void Awake()
    {
        SingletonInit();


        if (_gameCanvas == null)
        {
            GameObject canvasGO = GameObject.FindWithTag(MAIN_CANVAS_TAG);
            if (canvasGO != null)
            {
                _gameCanvas = canvasGO.GetComponent<Canvas>();
            }

            if (_gameCanvas == null)
            {
                Debug.LogError($"'{MAIN_CANVAS_TAG}' �±׸� ���� Canvas�� ã�� �� �����ϴ�. ���� Canvas�� �ִ���, �±װ� �ùٸ��� Ȯ�����ּ���.");
                return;
            }
        }

        if (_gameCanvas.gameObject.scene.buildIndex != -1)
        {
            DontDestroyOnLoad(_gameCanvas.gameObject);
            Debug.Log($"Inventory: '{MAIN_CANVAS_TAG}' �±׸� ���� _gameCanvas�� DontDestroyOnLoad�� �����߽��ϴ�.");
        }
        else
        {
            Debug.Log($"Inventory: '{MAIN_CANVAS_TAG}' �±׸� ���� _gameCanvas�� �̹� DontDestroyOnLoad ���� �ֽ��ϴ�.");

        }

    }
    // Start is called before the first frame update
    void Start()
    {
        GenerateInventorySlots(numberOfSlotsToCreate);
        if (Storage.Instance != null)
        {
            Storage.Instance.SetStorageSlots(generatedStorageSlots.ToArray());
        }
        else
        {
            Debug.LogError("Storage �ν��Ͻ��� ã�� �� �����ϴ�. Storage ��ũ��Ʈ�� ���� �ִ��� Ȯ���ϼ���.");
        }
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

            Debug.Log("Storage ���� ������Ʈ�� Canvas �Ʒ��� �̵��Ǿ����ϴ�.");
        }
        else if (StorageUIPanel != null && StorageUIPanel.transform.parent == _gameCanvas.transform)
        {
            Debug.Log("StorageUIPanel�� �̹� Canvas �Ʒ��� �ֽ��ϴ�. �ٽ� ������ �ʿ䰡 �����ϴ�.");
        }
        else if (StorageUIPanel == null)
        {
            Debug.LogError("StorageUIPanel�� �Ҵ���� �ʾҽ��ϴ�. Inspector���� �Ҵ����ּ���!");
        }
    }

    public void OpenStorageUI()
    {
        if (StorageUIPanel != null)
        {
            StorageUIPanel.SetActive(true);
            Debug.Log("â�� UI�� �������ϴ�.");
            // ���� ������� �ʿ��ϴٸ� ���⼭ ȣ�� (���ο� �������� ������ �� ��)
            // GenerateInventorySlots(numberOfSlotsToCreate);
        }
    }

    public void CloseStorageUI()
    {
        if (StorageUIPanel != null)
        {
            StorageUIPanel.SetActive(false);
            Debug.Log("â�� UI�� �ݾҽ��ϴ�.");
        }
    }

    public void GenerateInventorySlots(int count)
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        generatedStorageSlots.Clear(); // ����Ʈ�� ����ݴϴ�.

        // ���ο� ���� ����
        for (int i = 0; i < count; i++)
        {
            GameObject slotGO = Instantiate(inventorySlotPrefab, contentParent);
            InventorySlot slotComponent = slotGO.GetComponent<InventorySlot>();
            if (slotComponent != null)
            {
                generatedStorageSlots.Add(slotComponent); // ������ InventorySlot ������Ʈ ����
            }
            else
            {
                Debug.LogError("�ν��Ͻ�ȭ�� ���� �����տ� InventorySlot ������Ʈ�� �����ϴ�!");
            }
        }
    }
}
