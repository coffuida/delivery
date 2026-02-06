using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어 인벤토리 시스템 - 저장/꺼내기 완전 구현 (2슬롯)
/// ⭐ Awake()에서 초기화하여 다른 스크립트보다 먼저 실행
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("슬롯 설정")]
    [Tooltip("슬롯 개수")]
    public int maxSlots = 2;

    [Tooltip("현재 활성 슬롯 (0 또는 1)")]
    public int currentSlotIndex = 0;

    [Header("슬롯 데이터")]
    public List<PlayerInventorySlot> inventorySlots = new List<PlayerInventorySlot>();

    [Header("UI")]
    public InventoryUI inventoryUI;

    [Header("참조")]
    public PlayerInteractionSystem interactionSystem;

    [Header("디버그")]
    public bool showDebugLog = true;

    private bool canSwitchSlot = true;

    // ⭐ Awake()로 이동하여 다른 스크립트의 Start()보다 먼저 초기화
    void Awake()
    {
        Debug.Log("=== PlayerInventory Awake ===");

        // ⭐ 슬롯 초기화 (가장 먼저 실행)
        inventorySlots.Clear();
        for (int i = 0; i < maxSlots; i++)
        {
            inventorySlots.Add(new PlayerInventorySlot());
        }

        Debug.Log($"✓ 인벤토리 슬롯 {inventorySlots.Count}개 초기화 완료");
    }

    void Start()
    {
        Debug.Log("=== PlayerInventory Start ===");

        // UI 초기화
        if (inventoryUI != null)
        {
            inventoryUI.Initialize(this);
            Debug.Log("✓ InventoryUI 초기화 완료");
        }
        else
        {
            Debug.LogWarning("✗ InventoryUI가 없습니다!");
        }

        // InteractionSystem 찾기
        if (interactionSystem == null)
        {
            interactionSystem = GetComponent<PlayerInteractionSystem>();
            if (interactionSystem != null)
            {
                Debug.Log("✓ InteractionSystem 자동 발견");
            }
            else
            {
                Debug.LogError("✗ InteractionSystem을 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.Log("✓ InteractionSystem 연결됨");
        }
    }

    void Update()
    {
        HandleSlotSwitching();
        HandleStoreOrTakeItem();
    }

    /// <summary>
    /// 슬롯 전환 처리 (휠, 1/2키)
    /// </summary>
    void HandleSlotSwitching()
    {
        int newSlotIndex = currentSlotIndex;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            newSlotIndex = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            newSlotIndex = 1;
        }
        else if (canSwitchSlot)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0.01f)
            {
                newSlotIndex = currentSlotIndex - 1;
                if (newSlotIndex < 0)
                    newSlotIndex = maxSlots - 1;
            }
            else if (scroll < -0.01f)
            {
                newSlotIndex = currentSlotIndex + 1;
                if (newSlotIndex >= maxSlots)
                    newSlotIndex = 0;
            }
        }

        if (newSlotIndex != currentSlotIndex)
        {
            SwitchSlot(newSlotIndex);
        }
    }

    /// <summary>
    /// F키로 아이템 저장 또는 꺼내기
    /// </summary>
    void HandleStoreOrTakeItem()
    {
        if (!Input.GetKeyDown(KeyCode.F))
            return;

        Debug.Log("========================================");
        Debug.Log("=== F키 눌림! ===");
        Debug.Log("========================================");

        if (interactionSystem == null)
        {
            Debug.LogError("✗ InteractionSystem이 null입니다!");
            return;
        }

        Debug.Log($"✓ InteractionSystem 존재: {interactionSystem.name}");

        bool isHoldingItem = interactionSystem.IsHoldingItem();
        Debug.Log($"→ IsHoldingItem() 결과: {isHoldingItem}");

        bool isCurrentSlotEmpty = inventorySlots[currentSlotIndex].isEmpty;
        Debug.Log($"→ 현재 슬롯({currentSlotIndex + 1}) 비어있음: {isCurrentSlotEmpty}");

        // 케이스 판단
        Debug.Log("--- 케이스 판단 ---");
        Debug.Log($"아이템 들고 있음: {isHoldingItem}");
        Debug.Log($"슬롯 비어있음: {isCurrentSlotEmpty}");

        // 케이스 1: 아이템 안 들고 있고 + 슬롯 비어있음
        if (!isHoldingItem && isCurrentSlotEmpty)
        {
            Debug.Log("→ [케이스 1] 아무것도 없음. 아무 기능 없음.");
            return;
        }

        // 케이스 2: 아이템 들고 있고 + 현재 슬롯 비어있음
        if (isHoldingItem && isCurrentSlotEmpty)
        {
            Debug.Log("→ [케이스 2] 아이템 들고 있고 슬롯 비어있음 → 저장 시작");
            StoreCurrentHeldItem();
            return;
        }

        // 케이스 3: 아이템 들고 있고 + 현재 슬롯 차있음
        if (isHoldingItem && !isCurrentSlotEmpty)
        {
            Debug.Log("→ [케이스 3] 아이템 들고 있고 슬롯 차있음 → 빈 슬롯 찾기");
            StoreInEmptySlot();
            return;
        }

        // 케이스 4: 아이템 안 들고 있고 + 현재 슬롯에 아이템 있음
        if (!isHoldingItem && !isCurrentSlotEmpty)
        {
            Debug.Log("→ [케이스 4] 아이템 안 들고 있고 슬롯에 아이템 있음 → 꺼내기");
            TakeItemFromCurrentSlot();
            return;
        }

        Debug.Log("→ 어떤 케이스에도 해당 안 됨!");
    }

    /// <summary>
    /// 현재 들고 있는 아이템을 현재 슬롯에 저장
    /// </summary>
    void StoreCurrentHeldItem()
    {
        Debug.Log("--- StoreCurrentHeldItem() 시작 ---");

        if (interactionSystem == null)
        {
            Debug.LogError("✗ InteractionSystem이 null!");
            return;
        }

        GameObject heldItem = interactionSystem.GetCurrentHeldItem();
        Debug.Log($"GetCurrentHeldItem() 결과: {(heldItem != null ? heldItem.name : "null")}");

        if (heldItem == null)
        {
            Debug.LogWarning("✗ 들고 있는 아이템이 null!");
            return;
        }

        // Item 컴포넌트 가져오기
        Item itemScript = heldItem.GetComponent<Item>();
        if (itemScript == null)
            itemScript = heldItem.GetComponentInParent<Item>();

        if (itemScript == null)
        {
            Debug.LogError("✗ Item 스크립트가 없습니다!");
            return;
        }

        Debug.Log($"✓ Item 스크립트 찾음: {itemScript.itemName}");

        // 슬롯에 저장
        inventorySlots[currentSlotIndex].itemName = itemScript.itemName;
        inventorySlots[currentSlotIndex].itemPrefab = heldItem;
        inventorySlots[currentSlotIndex].isEmpty = false;

        Debug.Log($"✓ 슬롯 {currentSlotIndex + 1}에 데이터 저장 완료");

        // 아이템 비활성화
        heldItem.SetActive(false);
        Debug.Log($"✓ 아이템 비활성화: {heldItem.name}");

        // InteractionSystem에 알림
        interactionSystem.StoreItemToInventory();
        Debug.Log("✓ InteractionSystem.StoreItemToInventory() 호출 완료");

        // UI 업데이트
        if (inventoryUI != null)
        {
            inventoryUI.UpdateSlot(currentSlotIndex);
            Debug.Log("✓ UI 업데이트 완료");
        }

        Debug.Log($"========================================");
        Debug.Log($"✓✓✓ 저장 완료: {itemScript.itemName} → 슬롯 {currentSlotIndex + 1}");
        Debug.Log($"========================================");
    }

    /// <summary>
    /// 비어있는 슬롯 찾아서 저장
    /// </summary>
    void StoreInEmptySlot()
    {
        Debug.Log("--- StoreInEmptySlot() 시작 ---");

        int emptySlotIndex = FindEmptySlot();
        Debug.Log($"FindEmptySlot() 결과: {emptySlotIndex}");

        if (emptySlotIndex == -1)
        {
            Debug.Log("✗ 빈 슬롯이 없습니다!");
            return;
        }

        int previousSlot = currentSlotIndex;
        currentSlotIndex = emptySlotIndex;

        Debug.Log($"→ 슬롯 {previousSlot + 1} → {emptySlotIndex + 1} 전환");

        StoreCurrentHeldItem();

        if (inventoryUI != null)
        {
            inventoryUI.UpdateActiveSlot(currentSlotIndex);
        }
    }

    /// <summary>
    /// 현재 슬롯에서 아이템 꺼내기
    /// </summary>
    void TakeItemFromCurrentSlot()
    {
        Debug.Log("--- TakeItemFromCurrentSlot() 시작 ---");

        if (inventorySlots[currentSlotIndex].isEmpty)
        {
            Debug.LogWarning("✗ 현재 슬롯이 비어있습니다!");
            return;
        }

        GameObject storedItem = inventorySlots[currentSlotIndex].itemPrefab;
        Debug.Log($"저장된 아이템: {(storedItem != null ? storedItem.name : "null")}");

        if (storedItem == null)
        {
            Debug.LogError("✗ 저장된 아이템이 null!");
            inventorySlots[currentSlotIndex].Clear();
            if (inventoryUI != null)
                inventoryUI.UpdateSlot(currentSlotIndex);
            return;
        }

        // 아이템 활성화
        storedItem.SetActive(true);
        Debug.Log($"✓ 아이템 활성화: {storedItem.name}");

        // InteractionSystem에 알림
        if (interactionSystem != null)
        {
            interactionSystem.TakeItemFromInventory(storedItem);
            Debug.Log("✓ InteractionSystem.TakeItemFromInventory() 호출 완료");
        }

        Debug.Log($"✓ 슬롯 {currentSlotIndex + 1}에서 꺼냄: {inventorySlots[currentSlotIndex].itemName}");

        // 슬롯 비우기
        inventorySlots[currentSlotIndex].Clear();

        // UI 업데이트
        if (inventoryUI != null)
        {
            inventoryUI.UpdateSlot(currentSlotIndex);
        }

        Debug.Log($"========================================");
        Debug.Log($"✓✓✓ 꺼내기 완료: {storedItem.name}");
        Debug.Log($"========================================");
    }

    public int FindEmptySlot()
    {
        Debug.Log($"========================================");
        Debug.Log($"[PlayerInventory] FindEmptySlot() 시작");
        Debug.Log($"[PlayerInventory] inventorySlots.Count: {inventorySlots.Count}");
        Debug.Log($"========================================");

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            Debug.Log($"[PlayerInventory] 슬롯 {i}: isEmpty={inventorySlots[i].isEmpty}, itemName={inventorySlots[i].itemName}");

            if (inventorySlots[i].isEmpty)
            {
                Debug.Log($"[PlayerInventory] ✓ 빈 슬롯 발견: {i}");
                return i;
            }
        }

        Debug.Log($"[PlayerInventory] ✗ 빈 슬롯 없음!");
        return -1;
    }

    void SwitchSlot(int newIndex)
    {
        if (newIndex < 0 || newIndex >= maxSlots)
            return;

        currentSlotIndex = newIndex;

        if (showDebugLog)
            Debug.Log($"슬롯 전환: {currentSlotIndex + 1}");

        if (inventoryUI != null)
            inventoryUI.UpdateActiveSlot(currentSlotIndex);
    }

    public void RemoveItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Count)
            return;

        if (!inventorySlots[slotIndex].isEmpty)
        {
            if (showDebugLog)
                Debug.Log($"✓ 슬롯 {slotIndex + 1}에서 제거");

            inventorySlots[slotIndex].Clear();

            if (inventoryUI != null)
                inventoryUI.UpdateSlot(slotIndex);
        }
    }

    public void SetCanSwitchSlot(bool canSwitch)
    {
        canSwitchSlot = canSwitch;
    }

    public bool IsCurrentSlotEmpty()
    {
        return inventorySlots[currentSlotIndex].isEmpty;
    }

    public void ClearInventory()
    {
        foreach (PlayerInventorySlot slot in inventorySlots)
        {
            slot.Clear();
        }

        if (inventoryUI != null)
            inventoryUI.UpdateAllSlots();

        if (showDebugLog)
            Debug.Log("인벤토리 초기화!");
    }
}

[System.Serializable]
public class PlayerInventorySlot
{
    public bool isEmpty = true;
    public string itemName = "";
    public GameObject itemPrefab;

    public void Clear()
    {
        isEmpty = true;
        itemName = "";
        itemPrefab = null;
    }
}