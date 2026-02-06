using UnityEngine;

/// <summary>
/// 검수대 조작 컨트롤러 (모드 2)
/// 
/// ⭐ 디버깅 버전 - UI 활성화 문제 해결
/// </summary>
public class InspectionDeskController : MonoBehaviour
{
    [Header("검수대 인벤토리")]
    public InspectionDeskInventory deskInventory;

    [Header("아이템 배치 슬롯")]
    public Transform itemPlacementSlot;
    private GameObject placedItem = null;

    [Header("검수기 배치 슬롯 (6개)")]
    public Transform[] inspectorToolSlots = new Transform[6];
    private GameObject[] placedTools = new GameObject[6];

    [Header("UI 참조")]
    public InspectionDeskInventoryUI inventoryUI;
    public InspectionToolSelectorUI toolSelectorUI;  // ⭐ 이게 연결되어 있는지 확인!

    [Header("참조")]
    public PlayerInventory playerInventory;
    public InspectionStorageController storageController;
    public InspectionModeManager modeManager;

    [Header("디버그")]
    public bool showDebugLog = true;

    private int selectedToolIndex = -1;
    private int activeToolIndex = -1;
    private bool hasBeenEnabled = false;

    void OnEnable()
    {
        if (playerInventory != null)
            deskInventory.CopyFromPlayerInventory(playerInventory);

        if (inventoryUI != null)
        {
            inventoryUI.Initialize(this);
            inventoryUI.UpdateAllSlots();
            inventoryUI.SelectSlot(0);
        }

        // ⭐ Tool Selector UI 디버깅
        if (toolSelectorUI != null)
        {
            toolSelectorUI.Initialize();
            Debug.Log("✓ [검수대] ToolSelectorUI 초기화 완료");
        }
        else
        {
            Debug.LogError("✗✗✗ [검수대] toolSelectorUI가 NULL입니다! Inspector에서 연결하세요!");
        }

        hasBeenEnabled = true;

        if (showDebugLog)
            Debug.Log("[검수대 조작] 모드 활성화");
    }

    void OnDisable()
    {
        if (!hasBeenEnabled)
        {
            if (showDebugLog)
                Debug.Log("[검수대 조작] 아직 활성화된 적 없음 - 업로드 생략");
            return;
        }

        if (playerInventory != null)
        {
            deskInventory.UploadToPlayerInventory(playerInventory);
        }
        else if (showDebugLog)
        {
            Debug.LogWarning("[검수대 조작] playerInventory가 null - 업로드 생략");
        }

        if (activeToolIndex >= 0)
            DeactivateTool(activeToolIndex);

        if (showDebugLog)
            Debug.Log("[검수대 조작] 모드 비활성화");
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (modeManager != null) modeManager.SwitchToPlayerMode();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (modeManager != null) modeManager.SwitchToStorageMode();
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            if (modeManager != null) modeManager.SwitchToNextAngle();
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            if (modeManager != null) modeManager.SwitchToPreviousAngle();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SelectSlot(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SelectSlot(1);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            int newIndex = deskInventory.currentSelectedIndex - 1;
            if (newIndex < 0) newIndex = 1;
            SelectSlot(newIndex);
        }
        else if (scroll < 0f)
        {
            int newIndex = deskInventory.currentSelectedIndex + 1;
            if (newIndex > 1) newIndex = 0;
            SelectSlot(newIndex);
        }

        if (Input.GetKeyDown(KeyCode.F))
            HandleItemPlacement();

        if (Input.GetKeyDown(KeyCode.Z))
            MoveToolSelection(-1);
        else if (Input.GetKeyDown(KeyCode.X))
            MoveToolSelection(1);

        if (Input.GetKeyDown(KeyCode.G))
            ToggleToolActivation();
    }

    void SelectSlot(int index)
    {
        if (index < 0 || index > 1) return;

        deskInventory.currentSelectedIndex = index;

        if (inventoryUI != null)
            inventoryUI.SelectSlot(index);

        if (showDebugLog)
            Debug.Log($"[검수대 조작] 슬롯 {index + 1} 선택 (index: {index})");
    }

    void HandleItemPlacement()
    {
        int slotIndex = deskInventory.currentSelectedIndex;
        var slot = deskInventory.slots[slotIndex];
        bool slotHasItem = !slot.isEmpty;
        bool placementHasItem = (placedItem != null);

        if (showDebugLog)
            Debug.Log($"[검수대 조작] F키: 슬롯={slotIndex + 1}, SlotHasItem={slotHasItem}, PlacementHasItem={placementHasItem}");

        if (slotHasItem && !placementHasItem)
            PlaceItemFromSlot(slotIndex);
        else if (slotHasItem && placementHasItem)
            SwapItems(slotIndex);
        else if (!slotHasItem && placementHasItem)
            RetrieveItemToSlot(slotIndex);
        else
            if (showDebugLog) Debug.Log("[검수대 조작] 둘 다 비어있음");
    }

    void PlaceItemFromSlot(int slotIndex)
    {
        var slot = deskInventory.slots[slotIndex];
        if (slot.isEmpty || slot.itemPrefab == null) return;

        GameObject item = slot.itemPrefab;
        item.transform.position = itemPlacementSlot.position;
        item.transform.rotation = itemPlacementSlot.rotation;
        item.transform.SetParent(itemPlacementSlot);
        item.SetActive(true);

        placedItem = item;

        slot.Clear();
        inventoryUI?.UpdateSlot(slotIndex);

        Item itemScript = item.GetComponent<Item>();
        if (itemScript != null)
        {
            itemScript.hauntedData.RandomizeHaunted();
            if (showDebugLog)
                Debug.Log($"[검수대 조작] {itemScript.itemName} 배치 (귀품: {itemScript.IsHaunted()})");
        }

        NotifyActiveToolOfItemChange();
    }

    void SwapItems(int slotIndex)
    {
        var slot = deskInventory.slots[slotIndex];
        if (slot.isEmpty || placedItem == null) return;

        GameObject temp = placedItem;
        PlaceItemFromSlot(slotIndex);

        Item tempScript = temp.GetComponent<Item>();
        if (tempScript != null)
        {
            slot.isEmpty = false;
            slot.itemName = tempScript.itemName;
            slot.itemPrefab = temp;
            temp.SetActive(false);
        }

        inventoryUI?.UpdateSlot(slotIndex);
        if (showDebugLog) Debug.Log("[검수대 조작] 아이템 교체 완료");
    }

    void RetrieveItemToSlot(int slotIndex)
    {
        var slot = deskInventory.slots[slotIndex];
        if (!slot.isEmpty || placedItem == null) return;

        Item itemScript = placedItem.GetComponent<Item>();
        if (itemScript != null)
        {
            slot.isEmpty = false;
            slot.itemName = itemScript.itemName;
            slot.itemPrefab = placedItem;
        }

        placedItem.SetActive(false);
        placedItem.transform.SetParent(null);
        placedItem = null;

        inventoryUI?.UpdateSlot(slotIndex);

        NotifyActiveToolOfItemChange();

        if (showDebugLog) Debug.Log($"[검수대 조작] 슬롯 {slotIndex + 1}로 회수");
    }

    void MoveToolSelection(int direction)
    {
        int next = selectedToolIndex + direction;
        int attempts = 0;

        while (attempts < 6)
        {
            if (next < 0) next = 5;
            if (next > 5) next = 0;

            if (placedTools[next] != null)
            {
                selectedToolIndex = next;

                if (toolSelectorUI != null)
                    toolSelectorUI.SelectTool(selectedToolIndex);

                if (activeToolIndex >= 0 && activeToolIndex != selectedToolIndex)
                    DeactivateTool(activeToolIndex);

                if (showDebugLog)
                    Debug.Log($"[검수대 조작] 검수기 {selectedToolIndex} 선택");
                return;
            }

            next += direction;
            attempts++;
        }

        if (showDebugLog)
            Debug.Log("[검수대 조작] 선택 가능한 검수기 없음");
    }

    void SelectFirstAvailableTool()
    {
        for (int i = 0; i < 6; i++)
        {
            if (placedTools[i] != null)
            {
                selectedToolIndex = i;
                if (toolSelectorUI != null)
                    toolSelectorUI.SelectTool(i);

                if (showDebugLog)
                    Debug.Log($"[검수대 조작] 첫 검수기 {i} 자동 선택");
                return;
            }
        }
    }

    void ToggleToolActivation()
    {
        if (selectedToolIndex < 0)
        {
            if (showDebugLog) Debug.Log("[검수대 조작] 선택된 검수기 없음");
            return;
        }

        if (activeToolIndex == selectedToolIndex)
            DeactivateTool(selectedToolIndex);
        else
            ActivateTool(selectedToolIndex);
    }

    void ActivateTool(int toolIndex)
    {
        if (placedTools[toolIndex] == null) return;

        if (activeToolIndex >= 0)
            DeactivateTool(activeToolIndex);

        activeToolIndex = toolIndex;

        IInspectionTool tool = placedTools[toolIndex].GetComponent<IInspectionTool>();
        if (tool != null)
        {
            tool.Activate();

            if (placedItem != null)
            {
                tool.CheckItem(placedItem);
            }
        }

        if (toolSelectorUI != null)
            toolSelectorUI.SetToolActive(toolIndex, true);

        if (showDebugLog)
            Debug.Log($"[검수대 조작] 검수기 {toolIndex} 활성화");
    }

    void DeactivateTool(int toolIndex)
    {
        if (placedTools[toolIndex] == null) return;

        IInspectionTool tool = placedTools[toolIndex].GetComponent<IInspectionTool>();
        if (tool != null)
        {
            tool.Deactivate();
        }

        if (toolSelectorUI != null)
            toolSelectorUI.SetToolActive(toolIndex, false);

        activeToolIndex = -1;

        if (showDebugLog)
            Debug.Log($"[검수대 조작] 검수기 {toolIndex} 비활성화");
    }

    void NotifyActiveToolOfItemChange()
    {
        if (activeToolIndex >= 0 && placedTools[activeToolIndex] != null)
        {
            IInspectionTool tool = placedTools[activeToolIndex].GetComponent<IInspectionTool>();
            if (tool != null)
            {
                tool.CheckItem(placedItem);
            }
        }
    }

    /// <summary>
    /// ⭐⭐⭐ 보관대에서 검수기 설치 (UI 활성화 핵심 함수)
    /// </summary>
    public void PlaceTool(int toolType, GameObject tool)
    {
        Debug.Log($"========================================");
        Debug.Log($"[검수대] PlaceTool() 호출됨!");
        Debug.Log($"[검수대] toolType: {toolType}");
        Debug.Log($"[검수대] tool: {(tool != null ? tool.name : "NULL")}");
        Debug.Log($"========================================");

        if (toolType < 0 || toolType >= 6)
        {
            Debug.LogError($"✗ [검수대] toolType이 범위 밖: {toolType}");
            return;
        }

        if (inspectorToolSlots[toolType] == null)
        {
            Debug.LogError($"✗ [검수대] inspectorToolSlots[{toolType}]이 NULL!");
            return;
        }

        // Transform 설정
        tool.transform.position = inspectorToolSlots[toolType].position;
        tool.transform.rotation = inspectorToolSlots[toolType].rotation;
        tool.transform.SetParent(inspectorToolSlots[toolType]);
        tool.SetActive(true);

        placedTools[toolType] = tool;
        Debug.Log($"✓ [검수대] placedTools[{toolType}] 설정 완료");

        // Tool에 itemPlacementSlot 설정
        SetToolItemPlacementSlot(tool);

        // ⭐⭐⭐ UI 업데이트 (핵심!)
        Debug.Log($"[검수대] toolSelectorUI 상태 체크:");
        Debug.Log($"  - toolSelectorUI == null? {toolSelectorUI == null}");

        if (toolSelectorUI != null)
        {
            Debug.Log($"✓ [검수대] toolSelectorUI.SetToolInstalled({toolType}, true) 호출 시작");
            toolSelectorUI.SetToolInstalled(toolType, true);
            Debug.Log($"✓ [검수대] toolSelectorUI.SetToolInstalled({toolType}, true) 호출 완료");
        }
        else
        {
            Debug.LogError($"✗✗✗ [검수대] toolSelectorUI가 NULL입니다!");
            Debug.LogError($"✗✗✗ Inspector에서 InspectionDeskController의 Tool Selector UI 필드를 연결하세요!");
        }

        // 첫 검수기 설치 시 자동 선택
        if (selectedToolIndex < 0)
        {
            SelectFirstAvailableTool();
        }

        Debug.Log($"========================================");
        Debug.Log($"✓✓✓ [검수대] 검수기 {toolType} 설치 완료");
        Debug.Log($"========================================");
    }

    void SetToolItemPlacementSlot(GameObject tool)
    {
        SaltTool saltTool = tool.GetComponent<SaltTool>();
        if (saltTool != null)
        {
            saltTool.itemPlacementSlot = itemPlacementSlot;
            if (showDebugLog)
                Debug.Log("[검수대 조작] SaltTool에 itemPlacementSlot 설정");
            return;
        }

        CandleTool candleTool = tool.GetComponent<CandleTool>();
        if (candleTool != null)
        {
            candleTool.itemPlacementSlot = itemPlacementSlot;
            if (showDebugLog)
                Debug.Log("[검수대 조작] CandleTool에 itemPlacementSlot 설정");
            return;
        }
    }

    public GameObject RemoveTool(int toolType)
    {
        if (toolType < 0 || toolType >= 6) return null;

        GameObject tool = placedTools[toolType];
        if (tool == null) return null;

        if (activeToolIndex == toolType)
            DeactivateTool(toolType);

        placedTools[toolType] = null;

        if (toolSelectorUI != null)
            toolSelectorUI.SetToolInstalled(toolType, false);

        if (selectedToolIndex == toolType)
        {
            selectedToolIndex = -1;
            SelectFirstAvailableTool();
        }

        if (showDebugLog)
            Debug.Log($"[검수대 조작] 검수기 {toolType} 제거");

        return tool;
    }

    public GameObject GetPlacedItem()
    {
        return placedItem;
    }

    public InspectionDeskInventory GetInventory()
    {
        return deskInventory;
    }
}