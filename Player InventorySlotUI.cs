using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 개별 슬롯 UI
/// Slot1, Slot2에 각각 붙이는 스크립트
/// </summary>
public class InventorySlotUI : MonoBehaviour
{
    [Header("UI 요소")]
    public Image background;
    public Image iconImage;
    public TextMeshProUGUI slotNumberText;
    public TextMeshProUGUI itemNameText; // ⭐ 아이템 이름 표시
    public GameObject activeIndicator;

    private int slotIndex;

    public void Initialize(int index)
    {
        slotIndex = index;
    }

    public void SetSlotNumber(string number)
    {
        if (slotNumberText != null)
        {
            slotNumberText.text = number;
        }
    }

    public void SetActive(bool isActive)
    {
        if (activeIndicator != null)
        {
            activeIndicator.SetActive(isActive);
        }
    }

    public void UpdateSlot(PlayerInventorySlot slot)
    {
        // 아이콘 업데이트 (나중에 추가 가능)
        if (iconImage != null)
        {
            if (slot.isEmpty)
            {
                iconImage.enabled = false;
            }
            else
            {
                // 나중에 아이콘 추가 가능
                iconImage.enabled = false;
            }
        }

        // 아이템 이름 표시 ⭐
        if (itemNameText != null)
        {
            if (slot.isEmpty)
            {
                itemNameText.text = "";
                itemNameText.enabled = false;
            }
            else
            {
                itemNameText.text = slot.itemName;
                itemNameText.enabled = true;
            }
        }
    }
}