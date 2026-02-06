using UnityEngine;

/// <summary>
/// Candle 검수기 (슬롯 1번)
/// 
/// 기능:
/// - 물품 근처에 가져다 댐
/// - 일반물품: 불꽃 안정적
/// - 귀품: 불꽃 불안정하게 흔들림 (candleFlicker = true)
/// 
/// ⭐ 버그 수정:
/// - 스토레이지에 넣었다 빼도 파티클이 자동 재생되지 않음
/// - "Particle Velocity curves must all be in the same mode" 에러 수정
/// </summary>
public class CandleTool : MonoBehaviour, IInspectionTool
{
    [Header("파티클 설정")]
    public ParticleSystem candleFlame;     // 촛불 파티클

    [Header("⭐ 촛불 파티클 위치/각도 (로컬)")]
    public Vector3 flameParticleOffset = new Vector3(0f, 0.5f, 0f);
    public Vector3 flameParticleRotation = new Vector3(0f, 0f, 0f);

    [Header("불꽃 효과")]
    public float normalSpeed = 1f;         // 일반 속도
    public float hauntedSpeed = 5f;        // 귀품 근처 속도
    public float normalSpread = 0.1f;      // 일반 퍼짐
    public float hauntedSpread = 2f;       // 귀품 퍼짐

    [Header("물품 감지")]
    public Transform itemPlacementSlot;
    public float detectionRange = 2f;      // 감지 범위
    public float checkInterval = 0.2f;

    [Header("디버그")]
    public bool showDebugLog = true;

    // 내부 변수
    private bool isActive = false;
    private float checkTimer = 0f;
    private ParticleSystem.VelocityOverLifetimeModule velocityModule;

    void Start()
    {
        if (candleFlame == null)
            candleFlame = GetComponentInChildren<ParticleSystem>();

        if (candleFlame != null)
        {
            // ⭐ 파티클 위치/각도 설정
            candleFlame.transform.localPosition = flameParticleOffset;
            candleFlame.transform.localEulerAngles = flameParticleRotation;

            candleFlame.Stop();
            velocityModule = candleFlame.velocityOverLifetime;
        }
    }

    // ⭐ GameObject가 활성화될 때 (스토레이지에서 꺼낼 때)
    void OnEnable()
    {
        // 파티클이 자동 재생되지 않도록 강제 정지
        if (candleFlame != null)
        {
            candleFlame.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // 활성화 상태 초기화
        isActive = false;

        if (showDebugLog)
            Debug.Log("[Candle] OnEnable - 파티클 정지 상태로 초기화");
    }

    void Update()
    {
        if (!isActive) return;

        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckNearbyItem();
        }
    }

    public void Activate()
    {
        isActive = true;

        if (candleFlame != null)
        {
            candleFlame.Play();
            SetFlameStable(true);
        }

        if (showDebugLog)
            Debug.Log("[Candle] 활성화");
    }

    public void Deactivate()
    {
        isActive = false;

        if (candleFlame != null)
        {
            // ⭐ 완전히 정지 (파티클 제거)
            candleFlame.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (showDebugLog)
            Debug.Log("[Candle] 비활성화");
    }

    public void CheckItem(GameObject item)
    {
        if (item == null)
        {
            SetFlameStable(true);
            return;
        }

        Item itemScript = item.GetComponent<Item>();
        if (itemScript == null)
        {
            SetFlameStable(true);
            return;
        }

        // ⭐ candleFlicker = true면 불꽃 흔들림 (귀품)
        bool isHaunted = itemScript.hauntedData.candleFlicker;
        SetFlameStable(!isHaunted);

        if (showDebugLog && isHaunted)
            Debug.Log($"[Candle] ★ 귀품 감지! 불꽃 흔들림: {itemScript.itemName}");
    }

    /// <summary>
    /// ⭐ 근처 물품 자동 감지
    /// </summary>
    void CheckNearbyItem()
    {
        if (itemPlacementSlot == null) return;

        if (itemPlacementSlot.childCount > 0)
        {
            GameObject item = itemPlacementSlot.GetChild(0).gameObject;
            float distance = Vector3.Distance(transform.position, item.transform.position);

            if (distance <= detectionRange)
            {
                CheckItem(item);
            }
            else
            {
                SetFlameStable(true);
            }
        }
        else
        {
            // 아이템 없으면 안정적
            SetFlameStable(true);
        }
    }

    /// <summary>
    /// ⭐ 불꽃 안정성 설정 (Velocity 에러 수정)
    /// </summary>
    void SetFlameStable(bool isStable)
    {
        if (candleFlame == null) return;

        var main = candleFlame.main;

        if (isStable)
        {
            // 안정적 (일반)
            main.startSpeed = normalSpeed;
            main.startSize = normalSpread;
            velocityModule.enabled = false;
        }
        else
        {
            // 흔들림 (귀품)
            main.startSpeed = hauntedSpeed;
            main.startSize = hauntedSpread;

            // ⭐ Velocity 모듈 설정 (모든 축을 같은 모드로)
            velocityModule.enabled = true;
            velocityModule.space = ParticleSystemSimulationSpace.Local;

            // ⭐ x, y, z 모두 RandomBetweenTwoConstants 모드로 설정
            velocityModule.x = new ParticleSystem.MinMaxCurve(-1f, 1f);
            velocityModule.y = new ParticleSystem.MinMaxCurve(0f, 0f);  // ⭐ y도 설정
            velocityModule.z = new ParticleSystem.MinMaxCurve(-1f, 1f);
        }
    }

    // ⭐ GameObject가 비활성화될 때 (스토레이지에 넣을 때)
    void OnDisable()
    {
        // 강제로 비활성화
        Deactivate();

        if (showDebugLog)
            Debug.Log("[Candle] OnDisable - 강제 비활성화");
    }

    /// <summary>
    /// ⭐ Inspector에서 파티클 위치/각도 조정 시 즉시 반영 (에디터 전용)
    /// </summary>
    void OnValidate()
    {
        if (candleFlame != null)
        {
            candleFlame.transform.localPosition = flameParticleOffset;
            candleFlame.transform.localEulerAngles = flameParticleRotation;
        }
    }
}