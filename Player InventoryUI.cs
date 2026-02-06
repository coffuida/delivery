using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 인벤토리 UI - 2개 슬롯 관리
/// SlotContainer에 붙이는 스크립트
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("슬롯 UI (2개)")]
    public InventorySlotUI slot1UI;
    public InventorySlotUI slot2UI;

    [Header("활성 표시 설정")]
    public Color activeColor = new Color(1f, 1f, 1f, 1f);
    public Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private PlayerInventory inventory;
    private InventorySlotUI[] slotUIs;

    public void Initialize(PlayerInventory inv)
    {
        inventory = inv;

        slotUIs = new InventorySlotUI[2];
        slotUIs[0] = slot1UI;
        slotUIs[1] = slot2UI;

        if (slot1UI != null)
        {
            slot1UI.Initialize(0);
            slot1UI.SetSlotNumber("1");
        }

        if (slot2UI != null)
        {
            slot2UI.Initialize(1);
            slot2UI.SetSlotNumber("2");
        }

        UpdateAllSlots();
        UpdateActiveSlot(0);
    }

    public void UpdateActiveSlot(int activeIndex)
    {
        if (slotUIs == null) return;

        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] != null)
            {
                bool isActive = (i == activeIndex);

                // 활성/비활성 표시 (테두리만!)
                slotUIs[i].SetActive(isActive);

                // 배경 색상은 고정 (변경하지 않음)
            }
        }
    }

    public void UpdateSlot(int slotIndex)
    {
        // ⭐ null 체크 강화
        if (slotUIs == null)
        {
            Debug.LogWarning("[InventoryUI] slotUIs가 null!");
            return;
        }

        if (slotIndex < 0 || slotIndex >= slotUIs.Length)
        {
            Debug.LogWarning($"[InventoryUI] 잘못된 슬롯 인덱스: {slotIndex}");
            return;
        }

        if (inventory == null)
        {
            Debug.LogWarning("[InventoryUI] inventory가 null!");
            return;
        }

        // ⭐ inventorySlots 체크 추가
        if (inventory.inventorySlots == null)
        {
            Debug.LogWarning("[InventoryUI] inventory.inventorySlots가 null!");
            return;
        }

        if (slotIndex >= inventory.inventorySlots.Count)
        {
            Debug.LogWarning($"[InventoryUI] 슬롯 인덱스 {slotIndex}가 범위를 벗어남 (Count: {inventory.inventorySlots.Count})");
            return;
        }

        if (slotUIs[slotIndex] != null)
        {
            PlayerInventorySlot slot = inventory.inventorySlots[slotIndex];
            slotUIs[slotIndex].UpdateSlot(slot);
        }
    }

    public void UpdateAllSlots()
    {
        // ⭐ null 체크 추가
        if (slotUIs == null)
        {
            Debug.LogWarning("[InventoryUI] slotUIs가 초기화되지 않음!");
            return;
        }

        for (int i = 0; i < slotUIs.Length; i++)
        {
            UpdateSlot(i);
        }
    }
}