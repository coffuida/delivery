using UnityEngine;

/// <summary>
/// 아이템 처리기 - 컨베이어 벨트 끝에서 아이템 제거
/// 3가지 상태: Idle(0) → Opening(1) → Processing(2)
/// </summary>
public class ItemProcessor : MonoBehaviour
{
    [Header("애니메이션")]
    [Tooltip("문 애니메이터 (Door 오브젝트의 Animator)")]
    public Animator processorAnimator;

    [Header("처리 설정")]
    [Tooltip("아이템 레이어")]
    public LayerMask itemLayer;

    [Tooltip("처리 효과 (선택사항)")]
    public GameObject processingEffect;

    [Tooltip("처리 사운드 (선택사항)")]
    public AudioClip processingSound;

    [Header("상태")]
    [Tooltip("처리기 작동 중 여부")]
    public bool isActive = false;

    [Header("통계")]
    [Tooltip("처리된 아이템 수")]
    public int processedItemCount = 0;

    [Header("디버그")]
    public bool showDebugLog = true;
    public bool showGizmos = true;

    private AudioSource audioSource;

    void Start()
    {
        // Collider 확인
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"[처리기] {gameObject.name}: Collider가 필요합니다!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"[처리기] {gameObject.name}: Collider의 'Is Trigger'를 체크해주세요!");
        }

        // AudioSource 설정
        if (processingSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
        }

        if (showDebugLog)
        {
            Debug.Log($"=== 아이템 처리기 초기화 ===");
            Debug.Log($"Door Animator: {(processorAnimator != null ? "✓" : "✗")}");
            Debug.Log($"Collider: {(col != null ? "✓" : "✗")}");
            Debug.Log($"Is Trigger: {(col != null && col.isTrigger ? "✓" : "✗")}");
            Debug.Log($"초기 상태: {(isActive ? "활성" : "비활성")}");
            Debug.Log($"=========================");
        }
    }

    /// <summary>
    /// 처리기 시작 (레버에서 호출)
    /// State = 1 (Opening) → 자동으로 2 (Processing)
    /// </summary>
    public void StartProcessor()
    {
        if (isActive)
        {
            if (showDebugLog)
                Debug.LogWarning("[처리기] 이미 활성 상태입니다!");
            return;
        }

        isActive = true;

        // State = 1 (Opening - 문 열림)
        if (processorAnimator != null)
        {
            processorAnimator.SetInteger("State", 1);

            if (showDebugLog)
                Debug.Log("[처리기] ✓ 문 열림 시작! (State = 1 → Opening → Processing)");
        }
        else
        {
            Debug.LogError("[처리기] Animator가 없습니다! Door 오브젝트의 Animator를 연결해주세요.");
        }

        if (showDebugLog)
            Debug.Log("[처리기] ✓ 시작됨! (아이템 처리 활성화)");
    }

    /// <summary>
    /// 처리기 정지 (레버에서 호출)
    /// State = 0 (Idle - 대기)
    /// </summary>
    public void StopProcessor()
    {
        if (!isActive)
        {
            if (showDebugLog)
                Debug.LogWarning("[처리기] 이미 비활성 상태입니다!");
            return;
        }

        isActive = false;

        // State = 0 (Idle - 대기)
        if (processorAnimator != null)
        {
            processorAnimator.SetInteger("State", 0);

            if (showDebugLog)
                Debug.Log("[처리기] ✓ 대기 상태로 복귀! (State = 0 → Idle)");
        }

        if (showDebugLog)
            Debug.Log("[처리기] ✓ 정지됨! (아이템 처리 비활성화)");
    }

    /// <summary>
    /// 아이템이 처리기에 들어올 때
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        // 처리기가 비활성 상태면 무시
        if (!isActive)
        {
            if (showDebugLog)
                Debug.Log($"[처리기] 비활성 상태 - {other.gameObject.name} 무시");
            return;
        }

        // Item 레이어 확인
        if (((1 << other.gameObject.layer) & itemLayer) == 0)
        {
            if (showDebugLog)
                Debug.Log($"[처리기] 레이어 불일치 - {other.gameObject.name} (Layer: {LayerMask.LayerToName(other.gameObject.layer)}) 무시");
            return;
        }

        // 아이템 처리
        ProcessItem(other.gameObject);
    }

    /// <summary>
    /// 아이템 처리 (제거)
    /// </summary>
    void ProcessItem(GameObject item)
    {
        if (item == null)
            return;

        // 통계 업데이트
        processedItemCount++;

        if (showDebugLog)
            Debug.Log($"[처리기] ✓ 아이템 처리: {item.name} (총 {processedItemCount}개)");

        // 처리 효과 생성
        if (processingEffect != null)
        {
            Instantiate(processingEffect, item.transform.position, Quaternion.identity);
        }

        // 처리 사운드 재생
        if (processingSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(processingSound);
        }

        // 아이템 제거
        Destroy(item);
    }

    /// <summary>
    /// 통계 초기화
    /// </summary>
    public void ResetStats()
    {
        processedItemCount = 0;
        if (showDebugLog)
            Debug.Log("[처리기] 통계 초기화");
    }

    /// <summary>
    /// Gizmo 표시
    /// </summary>
    void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        Collider col = GetComponent<Collider>();
        if (col == null)
            return;

        // 처리 영역 표시 (활성/비활성에 따라 색상 변경)
        Color fillColor;
        Color wireColor;

        if (isActive)
        {
            fillColor = new Color(0f, 1f, 0f, 0.3f); // 녹색 (활성)
            wireColor = Color.green;
        }
        else
        {
            fillColor = new Color(1f, 0f, 0f, 0.3f); // 빨간색 (비활성)
            wireColor = Color.red;
        }

        Gizmos.matrix = transform.localToWorldMatrix;

        // 채우기
        Gizmos.color = fillColor;
        if (col is BoxCollider)
        {
            BoxCollider box = col as BoxCollider;
            Gizmos.DrawCube(box.center, box.size);
        }
        else if (col is SphereCollider)
        {
            SphereCollider sphere = col as SphereCollider;
            Gizmos.DrawSphere(sphere.center, sphere.radius);
        }

        // 외곽선
        Gizmos.color = wireColor;
        if (col is BoxCollider)
        {
            BoxCollider box = col as BoxCollider;
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider)
        {
            SphereCollider sphere = col as SphereCollider;
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
    }
}