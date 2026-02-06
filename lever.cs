using UnityEngine;

/// <summary>
/// 레버 - 컨베이어 벨트와 아이템 스포너를 제어
/// </summary>
public class Lever : InteractableObject
{
    [Header("레버 애니메이션")]
    [Tooltip("레버 애니메이터")]
    public Animator leverAnimator;

    [Header("연결된 시스템")]
    [Tooltip("제어할 아이템 스포너")]
    public ItemSpawner itemSpawner;

    [Tooltip("제어할 컨베이어 벨트들")]
    public ConveyorBelt[] conveyorBelts;

    [Tooltip("제어할 아이템 처리기들")]
    public ItemProcessor[] itemProcessors;

    [Tooltip("목표 관리자")]
    public ObjectiveManager objectiveManager;

    [Header("사운드")]
    public AudioClip leverSound;
    private AudioSource audioSource;

    [Header("현재 상태")]
    public bool isOn = false;

    [Header("디버그")]
    public bool showDebugLog = true;

    void Start()
    {
        objectName = "레버";

        if (leverAnimator == null)
        {
            leverAnimator = GetComponent<Animator>();
            if (showDebugLog)
            {
                if (leverAnimator != null)
                    Debug.Log("✓ Animator 자동 발견!");
                else
                    Debug.LogError("✗ Animator를 찾을 수 없습니다!");
            }
        }

        audioSource = GetComponent<AudioSource>();

        // 초기 상태 설정
        if (leverAnimator != null)
        {
            leverAnimator.SetBool("isOn", isOn);
            if (showDebugLog)
                Debug.Log($"초기 상태: isOn = {isOn}");
        }

        // 연결 확인
        if (showDebugLog)
        {
            Debug.Log($"=== 레버 초기화 ===");
            Debug.Log($"Animator: {(leverAnimator != null ? "✓" : "✗")}");
            Debug.Log($"ItemSpawner: {(itemSpawner != null ? "✓" : "✗")}");
            Debug.Log($"ConveyorBelts: {(conveyorBelts != null && conveyorBelts.Length > 0 ? conveyorBelts.Length + "개" : "✗")}");
            Debug.Log($"ItemProcessors: {(itemProcessors != null && itemProcessors.Length > 0 ? itemProcessors.Length + "개" : "✗")}");
            Debug.Log($"ObjectiveManager: {(objectiveManager != null ? "✓" : "✗")}");
        }
    }

    protected override void OnInteract()
    {
        if (showDebugLog)
            Debug.Log("========== E키 눌림! ==========");

        // 상태 전환
        isOn = !isOn;

        if (showDebugLog)
            Debug.Log($"상태 변경: isOn = {isOn}");

        // 애니메이터 파라미터 설정
        if (leverAnimator != null)
        {
            leverAnimator.SetBool("isOn", isOn);

            if (showDebugLog)
            {
                Debug.Log($"✓ Animator.SetBool('isOn', {isOn}) 실행!");

                // 파라미터 확인
                bool currentValue = leverAnimator.GetBool("isOn");
                Debug.Log($"현재 Animator의 isOn 값: {currentValue}");
            }
        }
        else
        {
            Debug.LogError("✗ leverAnimator가 null입니다!");
        }

        // 연결된 시스템 제어
        UpdateSystems();

        // 사운드 재생
        if (audioSource != null && leverSound != null)
            audioSource.PlayOneShot(leverSound);

        Debug.Log($"===== 레버 {(isOn ? "ON" : "OFF")} 완료 =====");
    }

    void UpdateSystems()
    {
        if (showDebugLog)
            Debug.Log("--- 시스템 업데이트 시작 ---");

        // 아이템 스포너 제어
        if (itemSpawner != null)
        {
            if (isOn)
            {
                itemSpawner.StartSpawning();
                if (showDebugLog)
                    Debug.Log("✓ ItemSpawner.StartSpawning() 호출");
            }
            else
            {
                itemSpawner.StopSpawning();
                if (showDebugLog)
                    Debug.Log("✓ ItemSpawner.StopSpawning() 호출");
            }
        }
        else
        {
            if (showDebugLog)
                Debug.LogWarning("✗ ItemSpawner가 연결되지 않음!");
        }

        // 컨베이어 벨트 제어
        if (conveyorBelts != null && conveyorBelts.Length > 0)
        {
            for (int i = 0; i < conveyorBelts.Length; i++)
            {
                if (conveyorBelts[i] != null)
                {
                    conveyorBelts[i].isActive = isOn;
                    if (showDebugLog)
                        Debug.Log($"✓ ConveyorBelt[{i}].isActive = {isOn}");
                }
            }
        }
        else
        {
            if (showDebugLog)
                Debug.LogWarning("✗ ConveyorBelt가 연결되지 않음!");
        }

        // 아이템 처리기 제어 ⭐ 새로 추가!
        if (itemProcessors != null && itemProcessors.Length > 0)
        {
            for (int i = 0; i < itemProcessors.Length; i++)
            {
                if (itemProcessors[i] != null)
                {
                    if (isOn)
                    {
                        itemProcessors[i].StartProcessor();
                        if (showDebugLog)
                            Debug.Log($"✓ ItemProcessor[{i}].StartProcessor() 호출");
                    }
                    else
                    {
                        itemProcessors[i].StopProcessor();
                        if (showDebugLog)
                            Debug.Log($"✓ ItemProcessor[{i}].StopProcessor() 호출");
                    }
                }
            }
        }
        else
        {
            if (showDebugLog)
                Debug.LogWarning("✗ ItemProcessor가 연결되지 않음!");
        }

        // 목표 관리자 제어
        if (objectiveManager != null)
        {
            if (isOn)
            {
                objectiveManager.StartObjective();
                if (showDebugLog)
                    Debug.Log("✓ ObjectiveManager.StartObjective() 호출");
            }
            else
            {
                objectiveManager.StopObjective();
                if (showDebugLog)
                    Debug.Log("✓ ObjectiveManager.StopObjective() 호출");
            }
        }
        else
        {
            if (showDebugLog)
                Debug.LogWarning("✗ ObjectiveManager가 연결되지 않음!");
        }

        if (showDebugLog)
            Debug.Log("--- 시스템 업데이트 완료 ---");
    }
}
