using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 목표 UI
/// </summary>
public class ObjectiveUI : MonoBehaviour
{
    [Header("UI 요소")]
    public GameObject objectivePanel;
    public TextMeshProUGUI objectiveText;
    public TextMeshProUGUI contrabandText;
    public TextMeshProUGUI timerText;
    public GameObject completePanel;
    public GameObject failedPanel;
    public TextMeshProUGUI failedReasonText;

    [Header("색상")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;
    public Color successColor = Color.green;

    /// <summary>
    /// 목표 UI 표시
    /// </summary>
    public void Show()
    {
        if (objectivePanel != null)
            objectivePanel.SetActive(true);

        if (completePanel != null)
            completePanel.SetActive(false);

        if (failedPanel != null)
            failedPanel.SetActive(false);
    }

    /// <summary>
    /// 목표 UI 숨기기
    /// </summary>
    public void Hide()
    {
        if (objectivePanel != null)
            objectivePanel.SetActive(false);
    }

    /// <summary>
    /// 목표 진행도 업데이트
    /// </summary>
    public void UpdateObjective(int current, int target)
    {
        if (objectiveText != null)
        {
            objectiveText.text = $"Sorted: {current} / {target}";

            // 색상 변경
            if (current >= target)
            {
                objectiveText.color = successColor;
            }
            else
            {
                objectiveText.color = normalColor;
            }
        }
    }

    /// <summary>
    /// 귀품 카운트 업데이트
    /// </summary>
    public void UpdateContraband(int current, int max)
    {
        if (contrabandText != null)
        {
            contrabandText.text = $"Cursed: {current} / {max}";

            // 색상 변경
            if (current > max)
            {
                contrabandText.color = dangerColor;
            }
            else if (current >= max - 1)
            {
                contrabandText.color = warningColor;
            }
            else
            {
                contrabandText.color = normalColor;
            }
        }
    }

    /// <summary>
    /// 타이머 업데이트
    /// </summary>
    public void UpdateTimer(float remainingTime)
    {
        if (timerText != null)
        {
            if (remainingTime < 0)
            {
                // 무제한
                timerText.text = "time: ∞";
                timerText.color = normalColor;
            }
            else
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                timerText.text = $"time: {minutes:00}:{seconds:00}";

                // 색상 변경
                if (remainingTime < 30f)
                {
                    timerText.color = dangerColor;
                }
                else if (remainingTime < 60f)
                {
                    timerText.color = warningColor;
                }
                else
                {
                    timerText.color = normalColor;
                }
            }
        }
    }

    /// <summary>
    /// 완료 표시
    /// </summary>
    public void ShowComplete()
    {
        if (objectivePanel != null)
            objectivePanel.SetActive(false);

        if (completePanel != null)
            completePanel.SetActive(true);
    }

    /// <summary>
    /// 실패 표시
    /// </summary>
    public void ShowFailed(string reason)
    {
        if (objectivePanel != null)
            objectivePanel.SetActive(false);

        if (failedPanel != null)
        {
            failedPanel.SetActive(true);

            if (failedReasonText != null)
                failedReasonText.text = reason;
        }
    }
}