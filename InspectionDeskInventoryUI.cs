using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 검수대 인벤토리 UI
/// </summary>
public class InspectionDeskInventoryUI : MonoBehaviour
{
    [Header("슬롯 UI (2개)")]
    public List<InspectionInventorySlotUI> slotUIs = new List<InspectionInventorySlotUI>();

    private InspectionDeskController controller;

    public void Initialize(InspectionDeskController deskController)
    {
        controller = deskController;

        for (int i = 0; i < slotUIs.Count; i++)
        {
            if (slotUIs[i] != null)
                slotUIs[i].Initialize(i);
        }
    }

    public void UpdateAllSlots()
    {
        if (controller == null) return;

        var inventory = controller.GetInventory();
        if (inventory == null) return;

        for (int i = 0; i < slotUIs.Count && i < inventory.slots.Count; i++)
        {
            if (slotUIs[i] != null)
            {
                var deskSlot = inventory.slots[i];
                PlayerInventorySlot playerSlot = new PlayerInventorySlot();
                playerSlot.isEmpty = deskSlot.isEmpty;
                playerSlot.itemName = deskSlot.itemName;
                playerSlot.itemPrefab = deskSlot.itemPrefab;

                slotUIs[i].UpdateSlot(playerSlot);
            }
        }
    }

    public void UpdateSlot(int index)
    {
        if (controller == null || index < 0 || index >= slotUIs.Count) return;

        var inventory = controller.GetInventory();
        if (inventory == null || index >= inventory.slots.Count) return;

        var deskSlot = inventory.slots[index];
        PlayerInventorySlot playerSlot = new PlayerInventorySlot();
        playerSlot.isEmpty = deskSlot.isEmpty;
        playerSlot.itemName = deskSlot.itemName;
        playerSlot.itemPrefab = deskSlot.itemPrefab;

        slotUIs[index].UpdateSlot(playerSlot);
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slotUIs.Count) return;

        for (int i = 0; i < slotUIs.Count; i++)
        {
            if (slotUIs[i] != null)
                slotUIs[i].SetSelected(i == index);
        }
    }
}