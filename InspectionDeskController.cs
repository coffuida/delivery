using UnityEngine;

/// <summary>
/// 검수대 조작 컨트롤러 (모드 2)
/// 
/// ⭐ 새로운 Tool UI 시스템 연동
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
    public InspectionToolUI toolUI;  // ⭐ 새로운 Tool UI

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

        // ⭐ Tool UI는 Awake에서 자동 초기화됨 (별도 호출 불필요)

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
            Debug.Log($"[검수대 조작] 슬롯 {index + 1} 선택");
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

                if (toolUI != null)
                    toolUI.SelectTool(selectedToolIndex);

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
                if (toolUI != null)
                    toolUI.SelectTool(i);

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

        if (toolUI != null)
            toolUI.SetToolActive(toolIndex, true);

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

        if (toolUI != null)
            toolUI.SetToolActive(toolIndex, false);

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

    public void PlaceTool(int toolType, GameObject tool)
    {
        if (toolType < 0 || toolType >= 6) return;
        if (inspectorToolSlots[toolType] == null) return;

        tool.transform.position = inspectorToolSlots[toolType].position;
        tool.transform.rotation = inspectorToolSlots[toolType].rotation;
        tool.transform.SetParent(inspectorToolSlots[toolType]);
        tool.SetActive(true);

        placedTools[toolType] = tool;

        SetToolItemPlacementSlot(tool);

        // ⭐ 새 Tool UI 시스템 사용
        if (toolUI != null)
            toolUI.SetToolInstalled(toolType, true);

        if (selectedToolIndex < 0)
            SelectFirstAvailableTool();

        if (showDebugLog)
            Debug.Log($"[검수대 조작] 검수기 {toolType} 설치");
    }

    void SetToolItemPlacementSlot(GameObject tool)
    {
        SaltTool saltTool = tool.GetComponent<SaltTool>();
        if (saltTool != null)
        {
            saltTool.itemPlacementSlot = itemPlacementSlot;
            return;
        }

        CandleTool candleTool = tool.GetComponent<CandleTool>();
        if (candleTool != null)
        {
            candleTool.itemPlacementSlot = itemPlacementSlot;
            return;
        }

        UVLightTool uvTool = tool.GetComponent<UVLightTool>();
        if (uvTool != null)
        {
            uvTool.itemPlacementSlot = itemPlacementSlot;
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

        if (toolUI != null)
            toolUI.SetToolInstalled(toolType, false);

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
