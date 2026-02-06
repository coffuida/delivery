using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 검수기 보관대 (검수대 전용)
/// 
/// 변경사항:
///   - E키 → TAB키로 변경
///   - 검수 모드 중에만 열기 가능
///   - 검수기 꺼내면 검수대에 직접 설치 (손에 안 쥠)
/// </summary>
public class InspectionStorage : MonoBehaviour
{
    // ══════════════════════════════════════════
    // Inspector 설정
    // ══════════════════════════════════════════

    [Header("보관대 설정")]
    [Tooltip("최대 슬롯 개수")]
    public int maxSlots = 6;

    [Header("초기 아이템")]
    [Tooltip("게임 시작 시 보관대에 있을 검수기 프리팹들")]
    public List<GameObject> initialTools = new List<GameObject>();

    [Header("슬롯 데이터")]
    public List<StorageSlot> slots = new List<StorageSlot>();

    [Header("UI")]
    [Tooltip("InspectionStorageCanvas 오브젝트")]
    public GameObject storageUI;
    [Tooltip("InspectionStorageUI 스크립트가 있는 오브젝트")]
    public InspectionStorageUI storageUIScript;

    [Header("검수대 참조")]
    public InspectionDeskController
        inspectionDesk;

    [Header("디버그")]
    public bool showDebugLog = true;

    // ══════════════════════════════════════════
    // 내부 변수
    // ══════════════════════════════════════════

    private bool isUIOpen = false;
    private int currentSelectedSlotIndex = 0;

    // ══════════════════════════════════════════
    // Start / Update
    // ══════════════════════════════════════════

    void Start()
    {
        // 슬롯 초기화
        slots.Clear();
        for (int i = 0; i < maxSlots; i++)
            slots.Add(new StorageSlot());

        // 초기 검수기 배치
        PlaceInitialTools();

        // UI 비활성화
        if (storageUI != null)
            storageUI.SetActive(false);

        // 검수대 자동 찾기
        if (inspectionDesk == null)
            inspectionDesk = FindObjectOfType<InspectionDeskController>();

        if (showDebugLog)
            Debug.Log($"[검수기 보관대] 초기화 완료 - {maxSlots}개 슬롯");
    }

    void Update()
    {
        if (isUIOpen)
            HandleUIInput();
    }

    // ══════════════════════════════════════════
    // 초기 검수기 배치
    // ══════════════════════════════════════════

    void PlaceInitialTools()
    {
        for (int i = 0; i < initialTools.Count && i < maxSlots; i++)
        {
            if (initialTools[i] == null) continue;

            GameObject tool = Instantiate(initialTools[i]);
            tool.SetActive(false);

            Item itemScript = tool.GetComponent<Item>();
            if (itemScript != null)
            {
                slots[i].itemName = itemScript.itemName;
                slots[i].storedItem = tool;
                slots[i].isEmpty = false;

                if (showDebugLog)
                    Debug.Log($"[검수기 보관대] 슬롯 {i + 1} ← {itemScript.itemName}");
            }
        }
    }

    // ══════════════════════════════════════════
    // 외부에서 열기 (InspectionDesk에서 호출)
    // ══════════════════════════════════════════

    public void OpenStorage()
    {
        if (storageUI == null) return;

        isUIOpen = true;
        storageUI.SetActive(true);

        // InspectionDesk 입력 차단
        if (inspectionDesk != null)
            inspectionDesk.enabled = false;

        // 커서 표시 (검수 모드 중이므로 이미 표시됨)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // UI 초기화·갱신·첫 슬롯 선택
        if (storageUIScript != null)
        {
            storageUIScript.Initialize(this);
            storageUIScript.UpdateAllSlots();
            storageUIScript.SelectSlot(0);
        }
        currentSelectedSlotIndex = 0;

        if (showDebugLog)
            Debug.Log("[검수기 보관대] UI 열림");
    }

    public void CloseStorage()
    {
        if (storageUI == null) return;


        // InspectionDesk 입력 복구
        if (inspectionDesk != null)
            inspectionDesk.enabled = true;
        isUIOpen = false;
        storageUI.SetActive(false);

        if (showDebugLog)
            Debug.Log("[검수기 보관대] UI 닫힘");
    }

    // ══════════════════════════════════════════
    // UI 키 입력 처리
    // ══════════════════════════════════════════

    void HandleUIInput()
    {
        // TAB / ESC → 닫기
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
        {
            CloseStorage();
            return;
        }

        // 슬롯 이동
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            MoveSlot(-1);
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            MoveSlot(1);

        // F → 검수기 꺼내기
        if (Input.GetKeyDown(KeyCode.F))
            TakeTool();
    }

    void MoveSlot(int dir)
    {
        int next = currentSelectedSlotIndex + dir;

        if (next < 0) next = maxSlots - 1;
        if (next >= maxSlots) next = 0;

        currentSelectedSlotIndex = next;

        if (storageUIScript != null)
            storageUIScript.SelectSlot(currentSelectedSlotIndex);

        if (showDebugLog)
            Debug.Log($"[검수기 보관대] 슬롯 {currentSelectedSlotIndex + 1} 선택");
    }

    // ══════════════════════════════════════════
    // 검수기 꺼내기 → 검수대에 설치
    // ══════════════════════════════════════════

    void TakeTool()
    {
        StorageSlot slot = slots[currentSelectedSlotIndex];

        if (slot.isEmpty)
        {
            if (showDebugLog) Debug.Log("[검수기 보관대] 슬롯이 비어있음");
            return;
        }

        GameObject tool = slot.storedItem;
        if (tool == null)
        {
            Debug.LogError("[검수기 보관대] storedItem이 null!");
            slot.Clear();
            storageUIScript?.UpdateSlot(currentSelectedSlotIndex);
            return;
        }

        // 검수대에 설치
        if (inspectionDesk != null)
        {
            // 검수기 타입 결정 (슬롯 인덱스 = 검수기 타입)
            int toolType = currentSelectedSlotIndex;
            inspectionDesk.PlaceTool(toolType, tool);

            // 슬롯 비우기 + UI 갱신
            slot.Clear();
            storageUIScript?.UpdateSlot(currentSelectedSlotIndex);

            if (showDebugLog)
                Debug.Log($"[검수기 보관대] ✓ 검수기 {toolType} 검수대에 설치");
        }
    }

    // ══════════════════════════════════════════
    // 검수기 반납 (외부 호출)
    // ══════════════════════════════════════════

    public bool ReturnTool(int toolType, GameObject tool)
    {
        if (toolType < 0 || toolType >= slots.Count) return false;
        if (tool == null) return false;

        // 이미 있으면 반납 불가
        if (!slots[toolType].isEmpty)
        {
            if (showDebugLog)
                Debug.Log($"[검수기 보관대] 슬롯 {toolType + 1} 이미 차있음");
            return false;
        }

        Item itemScript = tool.GetComponent<Item>();
        if (itemScript == null) return false;

        // 슬롯에 저장
        slots[toolType].itemName = itemScript.itemName;
        slots[toolType].storedItem = tool;
        slots[toolType].isEmpty = false;
        tool.SetActive(false);

        // UI 업데이트
        if (storageUIScript != null && isUIOpen)
            storageUIScript.UpdateSlot(toolType);

        if (showDebugLog)
            Debug.Log($"[검수기 보관대] ✓ {itemScript.itemName} 반납 → 슬롯 {toolType + 1}");

        return true;
    }

    // ══════════════════════════════════════════
    // UI 열림 상태 확인 (InspectionDesk에서 호출)
    // ══════════════════════════════════════════

    public bool IsOpen()
    {
        return isUIOpen;
    }
}