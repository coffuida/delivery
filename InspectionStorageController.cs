using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 검수기 보관대 조작 컨트롤러 (모드 3)
/// 
/// 키 입력:
///   Tab   → 검수대 모드로 전환
///   W/S   → 슬롯 142536 순환
///   A/D   → 슬롯 123456 순환
///   F     → 검수기 꺼내기/반납
/// </summary>
public class InspectionStorageController : MonoBehaviour
{
    // ══════════════════════════════════════════
    // Inspector 설정
    // ══════════════════════════════════════════

    [Header("보관대 슬롯")]
    public List<StorageSlot> slots = new List<StorageSlot>();

    [Header("초기 검수기 프리팹 (6개)")]
    public List<GameObject> initialTools = new List<GameObject>();

    [Header("UI 참조")]
    public InspectionStorageUI storageUI;

    [Header("참조")]
    public InspectionDeskController deskController;
    public InspectionModeManager modeManager;

    [Header("디버그")]
    public bool showDebugLog = true;

    // ══════════════════════════════════════════
    // 내부 변수
    // ══════════════════════════════════════════

    private int currentSelectedIndex = 0;

    // ══════════════════════════════════════════
    // Start / Enable / Disable
    // ══════════════════════════════════════════

    void Start()
    {
        // 슬롯 초기화 (6개)
        slots.Clear();
        for (int i = 0; i < 6; i++)
            slots.Add(new StorageSlot());

        // 초기 검수기 배치
        PlaceInitialTools();
    }

    void OnEnable()
    {
        // UI 초기화
        if (storageUI != null)
        {
            storageUI.Initialize(this);
            storageUI.UpdateAllSlots();
            storageUI.SelectSlot(0);
        }

        currentSelectedIndex = 0;

        if (showDebugLog)
            Debug.Log("[보관대 조작] 모드 활성화");
    }

    void OnDisable()
    {
        if (showDebugLog)
            Debug.Log("[보관대 조작] 모드 비활성화");
    }

    // ══════════════════════════════════════════
    // 초기 검수기 배치
    // ══════════════════════════════════════════

    void PlaceInitialTools()
    {
        for (int i = 0; i < initialTools.Count && i < 6; i++)
        {
            if (initialTools[i] == null) continue;

            GameObject tool = Instantiate(initialTools[i]);
            tool.SetActive(false);

            Item itemScript = tool.GetComponent<Item>();
            if (itemScript != null)
            {
                slots[i].itemName = itemScript.itemName;
                slots[i].itemPrefab = tool; // ⭐ itemPrefab 사용
                slots[i].isEmpty = false;

                if (showDebugLog)
                    Debug.Log($"[보관대 조작] 슬롯 {i + 1} ← {itemScript.itemName}");
            }
        }
    }

    // ══════════════════════════════════════════
    // Update
    // ══════════════════════════════════════════

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Tab → 검수대 모드로
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (modeManager != null)
                modeManager.SwitchToDeskMode();
            return;
        }

        // W/S → 142536 순환
        if (Input.GetKeyDown(KeyCode.W))
            MoveSlot_WS(-1);
        else if (Input.GetKeyDown(KeyCode.S))
            MoveSlot_WS(1);

        // A/D → 123456 순환
        if (Input.GetKeyDown(KeyCode.A))
            MoveSlot_AD(-1);
        else if (Input.GetKeyDown(KeyCode.D))
            MoveSlot_AD(1);

        // F → 검수기 꺼내기/반납
        if (Input.GetKeyDown(KeyCode.F))
            ToggleTool();
    }

    // ══════════════════════════════════════════
    // 슬롯 이동 (W/S - 142536 순환)
    // ══════════════════════════════════════════

    void MoveSlot_WS(int direction)
    {
        int[] order = { 0, 3, 1, 4, 2, 5 }; // 142536 (index 0~5)
        int currentOrderIndex = System.Array.IndexOf(order, currentSelectedIndex);

        if (currentOrderIndex < 0) currentOrderIndex = 0;

        currentOrderIndex += direction;

        if (currentOrderIndex < 0) currentOrderIndex = 5;
        if (currentOrderIndex > 5) currentOrderIndex = 0;

        currentSelectedIndex = order[currentOrderIndex];

        if (storageUI != null)
            storageUI.SelectSlot(currentSelectedIndex);

        if (showDebugLog)
            Debug.Log($"[보관대 조작] W/S → 슬롯 {currentSelectedIndex + 1}");
    }

    // ══════════════════════════════════════════
    // 슬롯 이동 (A/D - 123456 순환)
    // ══════════════════════════════════════════

    void MoveSlot_AD(int direction)
    {
        currentSelectedIndex += direction;

        if (currentSelectedIndex < 0) currentSelectedIndex = 5;
        if (currentSelectedIndex > 5) currentSelectedIndex = 0;

        if (storageUI != null)
            storageUI.SelectSlot(currentSelectedIndex);

        if (showDebugLog)
            Debug.Log($"[보관대 조작] A/D → 슬롯 {currentSelectedIndex + 1}");
    }

    // ══════════════════════════════════════════
    // 검수기 꺼내기/반납 (F키)
    // ══════════════════════════════════════════

    void ToggleTool()
    {
        if (deskController == null)
        {
            Debug.LogError("[보관대 조작] deskController가 null!");
            return;
        }

        var slot = slots[currentSelectedIndex];

        // 보관대에 검수기 없음 → 반납 시도
        if (slot.isEmpty)
        {
            ReturnToolFromDesk();
        }
        // 보관대에 검수기 있음 → 꺼내기
        else
        {
            TakeToolToDesk();
        }
    }

    /// <summary>
    /// 보관대 → 검수대에 설치
    /// </summary>
    void TakeToolToDesk()
    {
        var slot = slots[currentSelectedIndex];
        if (slot.isEmpty || slot.itemPrefab == null) return;

        GameObject tool = slot.itemPrefab;

        // 검수대에 설치
        deskController.PlaceTool(currentSelectedIndex, tool);

        // 슬롯 비우기
        slot.Clear();
        storageUI?.UpdateSlot(currentSelectedIndex);

        if (showDebugLog)
            Debug.Log($"[보관대 조작] 검수기 {currentSelectedIndex} → 검수대");
    }

    /// <summary>
    /// 검수대 → 보관대로 반납
    /// </summary>
    void ReturnToolFromDesk()
    {
        var slot = slots[currentSelectedIndex];
        if (!slot.isEmpty)
        {
            if (showDebugLog)
                Debug.Log("[보관대 조작] 슬롯이 이미 차있음");
            return;
        }

        // 검수대에서 제거
        GameObject tool = deskController.RemoveTool(currentSelectedIndex);
        if (tool == null)
        {
            if (showDebugLog)
                Debug.Log("[보관대 조작] 검수대에 해당 검수기 없음");
            return;
        }

        // 보관대에 저장
        Item itemScript = tool.GetComponent<Item>();
        if (itemScript != null)
        {
            slot.itemName = itemScript.itemName;
            slot.itemPrefab = tool; // ⭐ itemPrefab 사용
            slot.isEmpty = false;
            tool.SetActive(false);
        }

        storageUI?.UpdateSlot(currentSelectedIndex);

        if (showDebugLog)
            Debug.Log($"[보관대 조작] 검수기 {currentSelectedIndex} → 보관대");
    }
}