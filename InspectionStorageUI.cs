using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 검수기 보관대 UI
/// 
/// InspectionStorage (기존) 또는 InspectionStorageController (새버전) 모두 지원
/// </summary>
public class InspectionStorageUI : MonoBehaviour
{
    [Header("슬롯 UI (6개)")]
    public List<InspectionStorageSlotUI> slotUIs = new List<InspectionStorageSlotUI>();

    // 기존 버전 (InspectionStorage)
    private InspectionStorage legacyStorage;

    // 새 버전 (InspectionStorageController)
    private InspectionStorageController storageController;

    /// <summary>
    /// 기존 InspectionStorage용 초기화
    /// </summary>
    public void Initialize(InspectionStorage storage)
    {
        legacyStorage = storage;
        storageController = null;

        for (int i = 0; i < slotUIs.Count; i++)
        {
            if (slotUIs[i] != null)
                slotUIs[i].Initialize(i);
        }
    }

    /// <summary>
    /// 새 InspectionStorageController용 초기화
    /// </summary>
    public void Initialize(InspectionStorageController controller)
    {
        storageController = controller;
        legacyStorage = null;

        for (int i = 0; i < slotUIs.Count; i++)
        {
            if (slotUIs[i] != null)
                slotUIs[i].Initialize(i);
        }
    }

    public void UpdateAllSlots()
    {
        List<StorageSlot> slots = GetSlots();
        if (slots == null) return;

        for (int i = 0; i < slotUIs.Count && i < slots.Count; i++)
        {
            if (slotUIs[i] != null)
                slotUIs[i].UpdateSlot(slots[i]);
        }
    }

    public void UpdateSlot(int index)
    {
        List<StorageSlot> slots = GetSlots();
        if (slots == null || index < 0 || index >= slotUIs.Count) return;
        if (index >= slots.Count) return;

        if (slotUIs[index] != null)
            slotUIs[index].UpdateSlot(slots[index]);
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slotUIs.Count) return;

        // 이전 선택 해제
        for (int i = 0; i < slotUIs.Count; i++)
        {
            if (slotUIs[i] != null)
                slotUIs[i].SetSelected(i == index);
        }
    }

    /// <summary>
    /// 기존/새 버전 구분해서 슬롯 가져오기
    /// </summary>
    private List<StorageSlot> GetSlots()
    {
        if (storageController != null)
            return storageController.slots;
        else if (legacyStorage != null)
            return legacyStorage.slots;
        else
            return null;
    }
}