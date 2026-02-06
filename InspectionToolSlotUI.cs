using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 개별 검수기 슬롯 UI
/// 
/// 상태:
///   - 미설치 (회색, 비활성)
///   - 설치됨 (어두운 색)
///   - 선택됨 (노란색 테두리)
///   - 활성화 (녹색 배경, 활성화 인디케이터)
/// </summary>
public class InspectionToolSlotUI : MonoBehaviour
{
    [Header("UI 요소")]
    public Image background;
    public TextMeshProUGUI toolNameText;
    public GameObject selectedIndicator; // 선택 테두리 (부모)
    public GameObject activeIndicator;   // 활성화 표시 (별도 오브젝트)

    [Header("색상")]
    public Color colorNotInstalled = new Color(0.15f, 0.15f, 0.15f, 0.5f);
    public Color colorInstalled = new Color(0.25f, 0.25f, 0.3f, 1f);
    public Color colorSelected = new Color(0.4f, 0.35f, 0.2f, 1f);
    public Color colorActive = new Color(0.2f, 0.6f, 0.2f, 1f);

    private int toolIndex;
    private string toolName;
    private bool isInstalled = false;
    private bool isSelected = false;
    private bool isActive = false;

    public void Initialize(int index, string name)
    {
        toolIndex = index;
        toolName = name;

        if (toolNameText != null)
            toolNameText.text = toolName;

        // 선택 인디케이터 초기화
        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(true); // 부모는 활성화

            // 자식들(4방향 border)은 비활성화
            for (int i = 0; i < selectedIndicator.transform.childCount; i++)
            {
                selectedIndicator.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        // 활성화 인디케이터는 기본 비활성화
        if (activeIndicator != null)
            activeIndicator.SetActive(false);

        RefreshVisuals();
    }

    public void SetInstalled(bool installed)
    {
        isInstalled = installed;
        RefreshVisuals();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        // 선택 인디케이터 표시
        if (selectedIndicator != null)
        {
            // 부모는 항상 활성화 상태 유지
            selectedIndicator.SetActive(true);

            // 자식들만 토글
            for (int i = 0; i < selectedIndicator.transform.childCount; i++)
            {
                selectedIndicator.transform.GetChild(i).gameObject.SetActive(selected);
            }
        }

        RefreshVisuals();
    }

    public void SetActive(bool active)
    {
        isActive = active;

        // 활성화 인디케이터 표시
        if (activeIndicator != null)
            activeIndicator.SetActive(active);

        RefreshVisuals();
    }

    void RefreshVisuals()
    {
        if (background == null) return;

        // 배경색 우선순위: 활성화 > 선택 > 설치 > 미설치
        if (!isInstalled)
            background.color = colorNotInstalled;
        else if (isActive)
            background.color = colorActive;
        else if (isSelected)
            background.color = colorSelected;
        else
            background.color = colorInstalled;

        // 텍스트 색상
        if (toolNameText != null)
            toolNameText.color = isInstalled ? Color.white : new Color(0.4f, 0.4f, 0.4f);
    }
}