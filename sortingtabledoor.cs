using UnityEngine;

/// <summary>
/// 분류대 문 - 버튼으로 열고 닫기
/// </summary>
public class SortingTableDoor : MonoBehaviour
{
    [Header("문 애니메이션")]
    [Tooltip("문 Animator")]
    public Animator doorAnimator;

    [Tooltip("문 열림 트리거 이름")]
    public string openTriggerName = "Open";

    [Tooltip("문 닫힘 트리거 이름")]
    public string closeTriggerName = "Close";

    [Header("타이밍")]
    [Tooltip("문 열린 시간 (초)")]
    public float doorOpenDuration = 3f;

    [Header("디버그")]
    public bool showDebugLog = true;

    private bool isOperating = false;

    /// <summary>
    /// 버튼에서 호출 - 문 열기/닫기
    /// </summary>
    public void OperateDoor()
    {
        if (isOperating)
        {
            if (showDebugLog)
                Debug.Log("문이 이미 작동 중입니다!");
            return;
        }

        StartCoroutine(DoorSequence());
    }

    /// <summary>
    /// 문 열고 닫기 시퀀스
    /// </summary>
    System.Collections.IEnumerator DoorSequence()
    {
        isOperating = true;

        // 1. 문 열기
        OpenDoor();

        // 2. 대기 (아이템 떨어질 시간)
        yield return new WaitForSeconds(doorOpenDuration);

        // 3. 문 닫기
        CloseDoor();

        isOperating = false;
    }

    void OpenDoor()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(openTriggerName);

            if (showDebugLog)
                Debug.Log("문 열림!");
        }
        else
        {
            if (showDebugLog)
                Debug.LogError("Door Animator가 없습니다!");
        }
    }

    void CloseDoor()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(closeTriggerName);

            if (showDebugLog)
                Debug.Log("문 닫힘!");
        }
        else
        {
            if (showDebugLog)
                Debug.LogError("Door Animator가 없습니다!");
        }
    }
}