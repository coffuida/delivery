using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 검수기 Tool Selector UI Manager
/// 
/// ⭐ 업데이트:
/// - Storage에서 꺼낼 때 슬롯 UI GameObject 활성화
/// - 반납할 때 슬롯 UI GameObject 비활성화
/// </summary>
public class InspectionToolSelectorUI : MonoBehaviour
{
    [Header("⭐ Tool Slot UI GameObjects (6개)")]
    [Tooltip("InspectionTool 하위의 1_FlashLight, 2_Candle, 3_Salt, 4_Cross, 5_SpiritBox, 6_UVLight")]
    public List<GameObject> toolSlotUIObjects = new List<GameObject>();

    [Header("Tool 이름")]
    public List<string> toolNames = new List<string>()
    {
        "Flash Light",
        "Candle",
        "Salt",
        "Cross",
        "Spirit Box",
        "UV Light"
    };

    [Header("디버그")]
    public bool showDebugLog = true;

    // 내부 변수
    private List<InspectionToolSlotUI> slotUIs = new List<InspectionToolSlotUI>();
    private int currentSelectedIndex = -1;

    /// <summary>
    /// 초기화
    /// </summary>
    public void Initialize()
    {
        slotUIs.Clear();

        if (showDebugLog)
            Debug.Log($"[ToolSelectorUI] 초기화 시작 (슬롯 {toolSlotUIObjects.Count}개)");

        // 각 슬롯 UI 초기화
        for (int i = 0; i < toolSlotUIObjects.Count && i < 6; i++)
        {
            if (toolSlotUIObjects[i] == null)
            {
                Debug.LogError($"[ToolSelectorUI] toolSlotUIObjects[{i}]가 null입니다!");
                continue;
            }

            // ⭐ GameObject에서 InspectionToolSlotUI 컴포넌트 가져오기
            InspectionToolSlotUI slotUI = toolSlotUIObjects[i].GetComponent<InspectionToolSlotUI>();
            if (slotUI == null)
            {
                Debug.LogError($"[ToolSelectorUI] toolSlotUIObjects[{i}]에 InspectionToolSlotUI 컴포넌트가 없습니다!");
                continue;
            }

            string toolName = i < toolNames.Count ? toolNames[i] : $"Tool {i + 1}";
            slotUI.Initialize(i, toolName);
            slotUI.SetInstalled(false);  // 기본: 미설치 상태

            slotUIs.Add(slotUI);

            // ⭐ 초기에는 모든 슬롯 UI GameObject 비활성화
            toolSlotUIObjects[i].SetActive(false);

            if (showDebugLog)
                Debug.Log($"[ToolSelectorUI] 슬롯 {i} ({toolName}) 초기화 완료 (비활성화)");
        }

        if (showDebugLog)
            Debug.Log($"[ToolSelectorUI] 초기화 완료 (총 {slotUIs.Count}개 슬롯, 모두 비활성화)");
    }

    /// <summary>
    /// ⭐⭐⭐ 검수기 설치 상태 설정 + UI GameObject 활성화/비활성화
    /// </summary>
    public void SetToolInstalled(int toolIndex, bool installed)
    {
        if (toolIndex < 0 || toolIndex >= 6)
        {
            Debug.LogError($"[ToolSelectorUI] SetToolInstalled - 잘못된 인덱스: {toolIndex}");
            return;
        }

        if (showDebugLog)
            Debug.Log($"[ToolSelectorUI] SetToolInstalled({toolIndex}, {installed}) 호출됨");

        // ⭐ 1. GameObject 활성화/비활성화
        if (toolIndex < toolSlotUIObjects.Count && toolSlotUIObjects[toolIndex] != null)
        {
            toolSlotUIObjects[toolIndex].SetActive(installed);

            if (showDebugLog)
                Debug.Log($"[ToolSelectorUI] 슬롯 {toolIndex} GameObject.SetActive({installed}) 완료");
        }
        else
        {
            Debug.LogError($"[ToolSelectorUI] toolSlotUIObjects[{toolIndex}]가 null이거나 범위 밖입니다!");
            return;
        }

        // ⭐ 2. UI 상태 업데이트 (slotUI가 있으면)
        if (toolIndex < slotUIs.Count && slotUIs[toolIndex] != null)
        {
            slotUIs[toolIndex].SetInstalled(installed);
        }

        if (showDebugLog)
        {
            string state = installed ? "설치됨 (UI 활성화)" : "제거됨 (UI 비활성화)";
            string toolName = toolIndex < toolNames.Count ? toolNames[toolIndex] : $"Tool {toolIndex}";
            Debug.Log($"[ToolSelectorUI] Tool {toolIndex} ({toolName}) {state}");
        }
    }

    /// <summary>
    /// 검수기 선택
    /// </summary>
    public void SelectTool(int toolIndex)
    {
        if (toolIndex < 0 || toolIndex >= slotUIs.Count)
        {
            Debug.LogError($"[ToolSelectorUI] SelectTool - 잘못된 인덱스: {toolIndex}");
            return;
        }

        // 이전 선택 해제
        if (currentSelectedIndex >= 0 && currentSelectedIndex < slotUIs.Count)
        {
            slotUIs[currentSelectedIndex].SetSelected(false);
        }

        // 새로 선택
        currentSelectedIndex = toolIndex;
        slotUIs[toolIndex].SetSelected(true);

        if (showDebugLog)
        {
            string toolName = toolIndex < toolNames.Count ? toolNames[toolIndex] : $"Tool {toolIndex}";
            Debug.Log($"[ToolSelectorUI] Tool {toolIndex} ({toolName}) 선택");
        }
    }

    /// <summary>
    /// 검수기 활성화 상태 설정
    /// </summary>
    public void SetToolActive(int toolIndex, bool active)
    {
        if (toolIndex < 0 || toolIndex >= slotUIs.Count)
        {
            Debug.LogError($"[ToolSelectorUI] SetToolActive - 잘못된 인덱스: {toolIndex}");
            return;
        }

        slotUIs[toolIndex].SetActive(active);

        if (showDebugLog)
        {
            string state = active ? "활성화" : "비활성화";
            string toolName = toolIndex < toolNames.Count ? toolNames[toolIndex] : $"Tool {toolIndex}";
            Debug.Log($"[ToolSelectorUI] Tool {toolIndex} ({toolName}) {state}");
        }
    }

    /// <summary>
    /// ⭐ 모든 슬롯 UI GameObject 비활성화 (게임 시작 시 호출 - 선택적)
    /// </summary>
    public void HideAllToolUIs()
    {
        for (int i = 0; i < toolSlotUIObjects.Count; i++)
        {
            if (toolSlotUIObjects[i] != null)
            {
                toolSlotUIObjects[i].SetActive(false);
            }
        }

        if (showDebugLog)
            Debug.Log("[ToolSelectorUI] 모든 슬롯 UI 비활성화 완료");
    }
}