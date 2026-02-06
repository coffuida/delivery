using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 검수 모드 - 개별 인벤토리 슬롯 UI
/// </summary>
public class InspectionInventorySlotUI : MonoBehaviour
{
    [Header("UI 요소")]
    public Image background;
    public TextMeshProUGUI itemNameText;
    public GameObject selectedIndicator;

    [Header("색상")]
    public Color colorEmpty = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color colorFilled = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    public Color colorSelected = new Color(0.5f, 0.4f, 0.2f, 1f);
    public Color colorBorder = new Color(1f, 0.8f, 0.2f, 1f);

    private int slotIndex;
    private bool isSelected = false;
    private bool hasItem = false;
    private Image[] borders;

    public void Initialize(int index)
    {
        slotIndex = index;

        if (selectedIndicator != null)
        {
            int count = selectedIndicator.transform.childCount;
            borders = new Image[count];
            for (int i = 0; i < count; i++)
                borders[i] = selectedIndicator.transform.GetChild(i).GetComponent<Image>();
        }

        RefreshVisuals();
    }

    public void UpdateSlot(PlayerInventorySlot slot) // ⭐ PlayerInventorySlot으로 변경
    {
        hasItem = !slot.isEmpty;

        if (itemNameText != null)
        {
            if (slot.isEmpty)
            {
                itemNameText.text = " ";
                itemNameText.color = new Color(0.5f, 0.5f, 0.5f);
            }
            else
            {
                itemNameText.text = slot.itemName;
                itemNameText.color = Color.white;
            }
        }

        RefreshVisuals();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        RefreshVisuals();
    }

    void RefreshVisuals()
    {
        // 배경색
        if (background != null)
        {
            if (isSelected)
                background.color = colorSelected;
            else
                background.color = hasItem ? colorFilled : colorEmpty;
        }

        // 테두리
        if (borders != null)
        {
            for (int i = 0; i < borders.Length; i++)
            {
                if (borders[i] != null)
                {
                    borders[i].enabled = isSelected;
                    borders[i].color = colorBorder;
                }
            }
        }
    }
}