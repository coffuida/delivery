using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 검수기 Tool UI 시스템 - 완전히 새로운 버전
/// 
/// 단순하고 명확한 구조:
/// - 6개 Tool 슬롯 (FlashLight, Candle, Salt, Cross, SpiritBox, UVLight)
/// - 설치 시 자동 표시
/// - Z/X로 선택, G로 활성화
/// </summary>
public class InspectionToolUI : MonoBehaviour
{
    [System.Serializable]
    public class ToolSlot
    {
        [Header("UI 오브젝트")]
        public GameObject slotObject;           // 전체 슬롯 GameObject
        public Image backgroundImage;           // 배경 이미지
        public TextMeshProUGUI toolNameText;   // Tool 이름 텍스트
        public GameObject selectionBorder;          // 선택 테두리
        public Image activationIndicator;      // 활성화 표시

        [Header("상태")]
        public string toolName = "";
        public bool isInstalled = false;
        public bool isSelected = false;
        public bool isActive = false;

        [Header("색상")]
        public Color normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);      // 일반 (어두운 회색)
        public Color selectedColor = new Color(0.8f, 0.6f, 0.2f, 1f);    // 선택 (노란색)
        public Color activeColor = new Color(0.2f, 0.8f, 0.2f, 1f);      // 활성화 (녹색)

        /// <summary>
        /// 슬롯 초기화
        /// </summary>
        public void Initialize(string name)
        {
            toolName = name;
            isInstalled = false;
            isSelected = false;
            isActive = false;

            if (toolNameText != null)
                toolNameText.text = toolName;

            // 초기에는 숨김
            if (slotObject != null)
                slotObject.SetActive(false);

            // 테두리와 인디케이터는 비활성화
            if (selectionBorder != null)
                selectionBorder.gameObject.SetActive(false);

            if (activationIndicator != null)
                activationIndicator.gameObject.SetActive(false);
        }

        /// <summary>
        /// 설치 상태 설정
        /// </summary>
        public void SetInstalled(bool installed)
        {
            isInstalled = installed;

            if (slotObject != null)
                slotObject.SetActive(installed);

            if (!installed)
            {
                isSelected = false;
                isActive = false;
            }

            UpdateVisuals();
        }

        /// <summary>
        /// 선택 상태 설정
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;

            if (selectionBorder != null)
                selectionBorder.gameObject.SetActive(selected && isInstalled);

            UpdateVisuals();
        }

        /// <summary>
        /// 활성화 상태 설정
        /// </summary>
        public void SetActive(bool active)
        {
            isActive = active;

            if (activationIndicator != null)
                activationIndicator.gameObject.SetActive(active && isInstalled);

            UpdateVisuals();
        }

        /// <summary>
        /// 비주얼 업데이트
        /// </summary>
        void UpdateVisuals()
        {
            if (backgroundImage == null) return;

            // 배경색 우선순위: 활성화 > 선택 > 일반
            if (isActive)
                backgroundImage.color = activeColor;
            else if (isSelected)
                backgroundImage.color = selectedColor;
            else
                backgroundImage.color = normalColor;
        }
    }

    [Header("Tool 슬롯 (6개)")]
    public List<ToolSlot> toolSlots = new List<ToolSlot>();

    [Header("Tool 이름")]
    public string[] toolNames = new string[]
    {
        "FlashLight",
        "Candle",
        "Salt",
        "Cross",
        "SpiritBox",
        "UVLight"
    };

    void Awake()
    {
        // 6개 슬롯 초기화
        for (int i = 0; i < toolSlots.Count && i < toolNames.Length; i++)
        {
            if (toolSlots[i] != null && toolSlots[i].slotObject != null)
            {
                toolSlots[i].Initialize(toolNames[i]);
            }
        }

        Debug.Log("[ToolUI] 초기화 완료 - 6개 슬롯");
    }

    /// <summary>
    /// Tool 설치 상태 설정
    /// </summary>
    public void SetToolInstalled(int index, bool installed)
    {
        if (index < 0 || index >= toolSlots.Count) return;

        toolSlots[index].SetInstalled(installed);

        Debug.Log($"[ToolUI] Tool {index} ({toolNames[index]}) 설치: {installed}");
    }

    /// <summary>
    /// Tool 선택
    /// </summary>
    public void SelectTool(int index)
    {
        if (index < 0 || index >= toolSlots.Count) return;

        // 모든 슬롯 선택 해제
        for (int i = 0; i < toolSlots.Count; i++)
        {
            toolSlots[i].SetSelected(i == index);
        }

        Debug.Log($"[ToolUI] Tool {index} ({toolNames[index]}) 선택");
    }

    /// <summary>
    /// Tool 활성화 상태 설정
    /// </summary>
    public void SetToolActive(int index, bool active)
    {
        if (index < 0 || index >= toolSlots.Count) return;

        // 다른 Tool 비활성화
        if (active)
        {
            for (int i = 0; i < toolSlots.Count; i++)
            {
                if (i != index)
                    toolSlots[i].SetActive(false);
            }
        }

        toolSlots[index].SetActive(active);

        Debug.Log($"[ToolUI] Tool {index} ({toolNames[index]}) 활성화: {active}");
    }

    /// <summary>
    /// 모든 Tool 비활성화
    /// </summary>
    public void DeactivateAllTools()
    {
        for (int i = 0; i < toolSlots.Count; i++)
        {
            toolSlots[i].SetActive(false);
        }
    }
}