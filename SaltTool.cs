using UnityEngine;

/// <summary>
/// Salt 검수기 (슬롯 2번)
/// 
/// 기능:
/// - G키로 활성화하면 소금 파티클 재생
/// - G키로 비활성화하면 소금 파티클 정지 및 연기 즉시 정지 (경우 1)
/// - 일반물품: 아무 일 없음 (연기 정지)
/// - 귀품: Scene에 배치된 연기 파티클 활성화 (saltSmoke = true) (경우 2)
/// 
/// ⭐ 버그 수정 및 최적화:
/// - Instantiate/Destroy 대신 Scene에 배치된 파티클의 Play/Stop을 직접 제어
/// - 비활성화 또는 조건 미충족 시 즉시 화면에서 입자를 제거(Clear)
/// </summary>
public class SaltTool : MonoBehaviour, IInspectionTool
{
    [Header("파티클 설정")]
    public ParticleSystem saltParticle;    // 소금 파티클
    public ParticleSystem smokeParticle;   // 연기 파티클 (이제 프리팹이 아닌 Scene의 오브젝트를 할당)

    [Header("⭐ 소금 파티클 위치/각도 (로컬)")]
    public Vector3 saltParticleOffset = new Vector3(0f, 0.5f, 0f);
    public Vector3 saltParticleRotation = new Vector3(-90f, 0f, 0f);

    [Header("⭐ 연기 파티클 위치 (아이템 기준)")]
    public Vector3 smokeOffset = new Vector3(0f, 1f, 0f);

    [Header("물품 감지")]
    public Transform itemPlacementSlot;
    public float effectDuration = 3f;      // 연기 지속 시간 (활성화 방식에서도 변수 유지)

    [Header("디버그")]
    public bool showDebugLog = true;

    // 내부 변수
    private bool isActive = false;

    void Start()
    {
        // ⭐ 소금 파티클 위치/각도 설정
        if (saltParticle != null)
        {
            saltParticle.transform.localPosition = saltParticleOffset;
            saltParticle.transform.localEulerAngles = saltParticleRotation;
            saltParticle.Stop();
        }

        // 시작 시 연기 파티클 초기화
        if (smokeParticle != null)
        {
            smokeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    // ⭐ GameObject가 활성화될 때 (스토레이지에서 꺼낼 때)
    void OnEnable()
    {
        // 파티클이 자동 재생되지 않도록 강제 정지
        if (saltParticle != null)
        {
            saltParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // 연기 파티클 초기화
        if (smokeParticle != null)
        {
            smokeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // 활성화 상태 초기화
        isActive = false;

        if (showDebugLog)
            Debug.Log("[Salt] OnEnable - 파티클 정지 상태로 초기화");
    }

    public void Activate()
    {
        isActive = true;

        // ⭐ 소금 파티클 활성화
        if (saltParticle != null)
        {
            saltParticle.Play();
        }

        // ⭐ 귀품 체크 및 연기 발생
        CheckAndSpawnSmoke();

        if (showDebugLog)
            Debug.Log("[Salt] 활성화 - 소금 파티클 재생");
    }

    public void Deactivate()
    {
        isActive = false;

        // ⭐ 소금 파티클 비활성화
        if (saltParticle != null)
        {
            // ⭐ 완전히 정지 (파티클 제거)
            saltParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // [경우 1] G키 등으로 도구 비활성화 시 연기도 즉시 정지 및 제거
        if (smokeParticle != null)
        {
            smokeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (showDebugLog)
            Debug.Log("[Salt] 비활성화 - 소금 및 연기 파티클 정지");
    }

    public void CheckItem(GameObject item)
    {
        // 아이템이 배치/변경될 때 호출
        if (isActive)
        {
            CheckAndSpawnSmoke();
        }
    }

    /// <summary>
    /// ⭐ 귀품 체크 및 연기 제어 (활성화/비활성화)
    /// </summary>
    void CheckAndSpawnSmoke()
    {
        // ⭐ itemPlacementSlot이 없으면 종료
        if (itemPlacementSlot == null)
        {
            if (showDebugLog)
                Debug.LogWarning("[Salt] itemPlacementSlot이 설정되지 않았습니다!");
            return;
        }

        // ⭐ 물품이 없으면 연기 정지 (경우 2-1)
        if (itemPlacementSlot.childCount == 0)
        {
            if (smokeParticle != null)
                smokeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            if (showDebugLog)
                Debug.Log("[Salt] 물품이 없습니다 - 연기 발생 안 함");
            return;
        }

        // ⭐ 물품 가져오기
        GameObject item = itemPlacementSlot.GetChild(0).gameObject;
        Item itemScript = item.GetComponent<Item>();

        // ⭐ Item 스크립트가 없으면 연기 정지 (경우 2-2)
        if (itemScript == null)
        {
            if (smokeParticle != null)
                smokeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            if (showDebugLog)
                Debug.LogWarning("[Salt] Item 스크립트가 없습니다!");
            return;
        }

        if (showDebugLog)
            Debug.Log($"[Salt] 물품 체크: {itemScript.itemName}");

        // ⭐ 귀품이고 saltSmoke = true일 때만 연기 발생
        if (itemScript.hauntedData.isHaunted && itemScript.hauntedData.saltSmoke)
        {
            // ⭐ 기존의 SpawnSmoke를 호출하여 위치 이동 및 재생
            SpawnSmoke(item.transform.position);

            if (showDebugLog)
                Debug.Log($"[Salt] ★ 귀품 감지! - 연기 발생: {itemScript.itemName}");
        }
        else
        {
            // [경우 2-3] 귀품이 아니거나 saltSmoke가 false면 즉시 정지
            if (smokeParticle != null)
            {
                smokeParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (showDebugLog)
            {
                if (!itemScript.hauntedData.isHaunted)
                    Debug.Log($"[Salt] 일반 물품 - 반응 없음: {itemScript.itemName}");
                else
                    Debug.Log($"[Salt] 귀품이지만 saltSmoke=false - 반응 없음: {itemScript.itemName}");
            }
        }
    }

    /// <summary>
    /// ⭐ 연기 제어 (Scene 파티클 위치 이동 및 재생)
    /// </summary>
    void SpawnSmoke(Vector3 itemPosition)
    {
        if (smokeParticle == null)
        {
            if (showDebugLog)
                Debug.LogWarning("[Salt] smokeParticle 오브젝트가 할당되지 않았습니다!");
            return;
        }

        // ⭐ 아이템 위치 + 오프셋에 연기 배치
        Vector3 targetPosition = itemPosition + smokeOffset;
        smokeParticle.transform.position = targetPosition;

        // 이미 재생 중이 아니라면 재생 시작
        if (!smokeParticle.isPlaying)
        {
            smokeParticle.Play();

            if (showDebugLog)
                Debug.Log($"[Salt] 연기 재생 시작: 위치={targetPosition}");
        }
    }

    // ⭐ GameObject가 비활성화될 때 (스토레이지에 넣을 때)
    void OnDisable()
    {
        // 강제로 비활성화 로직 실행
        Deactivate();

        if (showDebugLog)
            Debug.Log("[Salt] OnDisable - 강제 비활성화 및 연기 정지");
    }

    /// <summary>
    /// ⭐ Inspector에서 파티클 위치/각도 조정 시 즉시 반영 (에디터 전용)
    /// </summary>
    void OnValidate()
    {
        if (saltParticle != null)
        {
            saltParticle.transform.localPosition = saltParticleOffset;
            saltParticle.transform.localEulerAngles = saltParticleRotation;
        }
    }
}