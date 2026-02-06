using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 검수기 보관대 개별 슬롯 UI
/// </summary>
public class InspectionStorageSlotUI : MonoBehaviour
{
    [Header("UI 요소")]
    public Image background;
    public Image iconImage;
    public Image emptySlotImage; // ⭐ 빈 슬롯 이미지
    public TextMeshProUGUI itemNameText;
    public GameObject selectedIndicator;

    [Header("색상 설정")]
    public Color emptySlotColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color filledSlotColor = new Color(0.4f, 0.4f, 0.4f, 0.8f);
    public Color selectedColor = new Color(0.8f, 0.6f, 0.2f, 1f);

    [Header("텍스트 설정")]
    public string emptySlotText = "빈 슬롯"; // ⭐ 빈 슬롯 텍스트

    private int slotIndex;
    private bool isSelected = false;

    public void Initialize(int index)
    {
        slotIndex = index;

        // ⭐ 부모(Activeindecator)는 항상 활성화
        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(true);

            // 자식들(4방향 border)만 비활성화
            for (int i = 0; i < selectedIndicator.transform.childCount; i++)
            {
                selectedIndicator.transform.GetChild(i).gameObject.SetActive(false);
            }

            Debug.Log($"[StorageSlotUI] Slot {slotIndex + 1} - Initialize: 부모 활성화, 자식 비활성화");
        }
    }

    /// <summary>
    /// 슬롯 업데이트
    /// </summary>
    public void UpdateSlot(StorageSlot slot)
    {
        if (slot.isEmpty)
        {
            // 슬롯 비어있음
            if (iconImage != null)
                iconImage.enabled = false;

            // ⭐ 빈 슬롯 이미지 표시
            if (emptySlotImage != null)
                emptySlotImage.enabled = true;

            if (itemNameText != null)
            {
                itemNameText.text = emptySlotText; // ⭐ "빈 슬롯" 또는 설정한 텍스트
                itemNameText.color = new Color(0.5f, 0.5f, 0.5f, 1f); // 회색
            }

            if (background != null && !isSelected)
                background.color = emptySlotColor;
        }
        else
        {
            // 슬롯에 아이템 있음
            if (iconImage != null)
            {
                // 나중에 아이템별 아이콘 추가 가능
                iconImage.enabled = false;
            }

            // ⭐ 빈 슬롯 이미지 숨김
            if (emptySlotImage != null)
                emptySlotImage.enabled = false;

            if (itemNameText != null)
            {
                itemNameText.text = slot.itemName;
                itemNameText.color = Color.white;
            }

            if (background != null && !isSelected)
                background.color = filledSlotColor;
        }
    }

    /// <summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (selectedIndicator == null) return;
        selectedIndicator.SetActive(true);
        for (int i = 0; i < selectedIndicator.transform.childCount; i++)
        {
            Transform child = selectedIndicator.transform.GetChild(i);
            if (child != null)
                child.gameObject.SetActive(selected);
        }
        if (background != null)
        {
            Color newColor = selected ? selectedColor : filledSlotColor;
            background.color = newColor;
        }
    }
}