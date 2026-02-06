using UnityEngine;

/// <summary>
/// 분류대 전용 버튼 - 문만 열기
/// </summary>
public class SortingButton : InteractableObject
{
    [Header("버튼 애니메이션")]
    [Tooltip("버튼 애니메이터 (선택사항)")]
    public Animator buttonAnimator;

    [Tooltip("버튼 눌림 애니메이션 이름")]
    public string pressAnimationName = "Press";

    [Header("연결된 시스템")]
    [Tooltip("분류대 문")]
    public SortingTableDoor sortingTableDoor;

    [Header("버튼 설정")]
    [Tooltip("쿨다운 시간 (초) - 연속 클릭 방지")]
    public float cooldownTime = 1f;

    [Header("사운드")]
    public AudioClip buttonSound;
    private AudioSource audioSource;

    [Header("디버그")]
    public bool showDebugLog = true;

    private float lastPressTime = -999f;

    void Start()
    {
        objectName = "분류 버튼";

        if (buttonAnimator == null)
        {
            buttonAnimator = GetComponent<Animator>();
        }

        audioSource = GetComponent<AudioSource>();

        if (showDebugLog)
        {
            Debug.Log($"=== 분류 버튼 초기화 ===");
            Debug.Log($"Animator: {(buttonAnimator != null ? "✓" : "✗")}");
            Debug.Log($"SortingTableDoor: {(sortingTableDoor != null ? "✓" : "✗")}");
        }
    }

    protected override void OnInteract()
    {
        // 쿨다운 체크
        if (Time.time - lastPressTime < cooldownTime)
        {
            if (showDebugLog)
                Debug.Log($"쿨다운 중... ({cooldownTime - (Time.time - lastPressTime):F1}초 남음)");
            return;
        }

        lastPressTime = Time.time;

        if (showDebugLog)
            Debug.Log("========== 분류 버튼 눌림! ==========");

        // 버튼 애니메이션
        if (buttonAnimator != null)
        {
            buttonAnimator.SetTrigger(pressAnimationName);

            if (showDebugLog)
                Debug.Log($"✓ 버튼 애니메이션 트리거: {pressAnimationName}");
        }

        // 문 열기
        OpenDoor();

        // 사운드 재생
        if (audioSource != null && buttonSound != null)
        {
            audioSource.PlayOneShot(buttonSound);
        }

        if (showDebugLog)
            Debug.Log("===== 버튼 작동 완료 =====");
    }

    /// <summary>
    /// 문 열기
    /// </summary>
    void OpenDoor()
    {
        if (sortingTableDoor != null)
        {
            sortingTableDoor.OperateDoor();

            if (showDebugLog)
                Debug.Log("✓ SortingTableDoor.OperateDoor() 호출!");
        }
        else
        {
            if (showDebugLog)
                Debug.LogError("✗ SortingTableDoor가 연결되지 않았습니다!");
        }
    }
}