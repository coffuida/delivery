using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 검수대 전용 인벤토리
/// 
/// PlayerInventory의 슬롯 정보를 복사해서 사용
/// 종료 시 변경사항을 PlayerInventory에 다시 업로드
/// 
/// ⭐ 버그 수정: 아이템이 검수대에 배치되면 슬롯 비우기
/// </summary>
[System.Serializable]
public class InspectionDeskInventory
{
    [Header("슬롯 데이터 (2개)")]
    public List<InspectionDeskSlot> slots = new List<InspectionDeskSlot>();

    public int currentSelectedIndex = 0;

    /// <summary>
    /// PlayerInventory로부터 정보 복사
    /// </summary>
    public void CopyFromPlayerInventory(PlayerInventory playerInventory)
    {
        // ⭐ null 체크 추가
        if (playerInventory == null)
        {
            Debug.LogWarning("[InspectionDeskInventory] playerInventory가 null - 복사 실패");
            return;
        }

        if (playerInventory.inventorySlots == null)
        {
            Debug.LogWarning("[InspectionDeskInventory] inventorySlots가 null - 복사 실패");
            return;
        }

        slots.Clear();

        for (int i = 0; i < 2; i++)
        {
            InspectionDeskSlot newSlot = new InspectionDeskSlot();

            if (i < playerInventory.inventorySlots.Count)
            {
                var playerSlot = playerInventory.inventorySlots[i];
                newSlot.isEmpty = playerSlot.isEmpty;
                newSlot.itemName = playerSlot.itemName;
                newSlot.itemPrefab = playerSlot.itemPrefab;
            }
            else
            {
                newSlot.isEmpty = true;
                newSlot.itemName = "";
                newSlot.itemPrefab = null;
            }

            slots.Add(newSlot);
        }

        Debug.Log($"[InspectionDeskInventory] PlayerInventory로부터 복사 완료 (슬롯 {slots.Count}개)");
    }

    /// <summary>
    /// ⭐ PlayerInventory에 변경사항 업로드 (버그 수정)
    /// 검수대 모드 종료 시 호출되어 플레이어 인벤토리 동기화
    /// </summary>
    public void UploadToPlayerInventory(PlayerInventory playerInventory)
    {
        // ⭐ null 체크 강화
        if (playerInventory == null)
        {
            Debug.LogWarning("[InspectionDeskInventory] playerInventory가 null - 업로드 실패");
            return;
        }

        if (playerInventory.inventorySlots == null)
        {
            Debug.LogWarning("[InspectionDeskInventory] inventorySlots가 null - 업로드 실패");
            return;
        }

        // 슬롯 데이터 동기화
        for (int i = 0; i < slots.Count && i < playerInventory.inventorySlots.Count; i++)
        {
            playerInventory.inventorySlots[i].isEmpty = slots[i].isEmpty;
            playerInventory.inventorySlots[i].itemName = slots[i].itemName;
            playerInventory.inventorySlots[i].itemPrefab = slots[i].itemPrefab;
        }

        // ⭐ UI 업데이트 (null 체크 추가)
        if (playerInventory.inventoryUI != null)
        {
            playerInventory.inventoryUI.UpdateAllSlots();
            Debug.Log($"[InspectionDeskInventory] PlayerInventory에 업로드 완료 + UI 업데이트");
        }
        else
        {
            Debug.LogWarning($"[InspectionDeskInventory] PlayerInventory에 업로드 완료 (inventoryUI는 null)");
        }
    }

    /// <summary>
    /// 빈 슬롯 찾기
    /// </summary>
    public int FindEmptySlot()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].isEmpty)
                return i;
        }
        return -1;
    }
}

/// <summary>
/// 검수대 인벤토리 슬롯
/// </summary>
[System.Serializable]
public class InspectionDeskSlot
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