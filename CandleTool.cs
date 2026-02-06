using UnityEngine;

/// <summary>
/// Candle 검수기 (슬롯 1번)
/// 
/// 기능 수정:
/// - 일반물품: normalFlame 파티클 재생
/// - 귀품(candleFlicker = true): normalFlame을 정지하고 hauntedFlame(파란 불꽃 등)을 재생
/// - 에러 해결: Velocity 모듈의 모든 축 모드를 통일 (Two Constants)
/// </summary>
public class CandleTool : MonoBehaviour, IInspectionTool
{
    [Header("파티클 설정")]
    [Tooltip("일반 상태의 불꽃 파티클")]
    public ParticleSystem normalFlame;
    [Tooltip("귀품 감지 시 재생할 별도의 파티클 (예: 파란 불꽃)")]
    public ParticleSystem hauntedFlame;

    [Header("불꽃 효과 (흔들림 설정)")]
    public float normalSpeed = 1f;
    public float hauntedSpeed = 5f;
    public float normalSpread = 0.1f;
    public float hauntedSpread = 2f;

    [Header("물품 감지")]
    public Transform itemPlacementSlot;
    public float detectionRange = 2f;
    public float checkInterval = 0.2f;

    [Header("디버그")]
    public bool showDebugLog = true;

    private bool isActive = false;
    private float checkTimer = 0f;
    private bool isCurrentlyHauntedStatus = false;

    void Start()
    {
        if (normalFlame != null) normalFlame.Stop();
        if (hauntedFlame != null) hauntedFlame.Stop();
    }

    void OnEnable()
    {
        StopAllFlames();
        isActive = false;
        isCurrentlyHauntedStatus = false;
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
        SetFlameState(false);
        if (showDebugLog) Debug.Log("[Candle] 활성화");
    }

    public void Deactivate()
    {
        isActive = false;
        StopAllFlames();
        if (showDebugLog) Debug.Log("[Candle] 비활성화");
    }

    public void CheckItem(GameObject item)
    {
        if (item == null)
        {
            SetFlameState(false);
            return;
        }

        Item itemScript = item.GetComponent<Item>();
        if (itemScript == null) itemScript = item.GetComponentInParent<Item>();

        if (itemScript == null)
        {
            SetFlameState(false);
            return;
        }

        bool isHaunted = itemScript.hauntedData.candleFlicker;
        SetFlameState(isHaunted);
    }

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
                SetFlameState(false);
            }
        }
        else
        {
            SetFlameState(false);
        }
    }

    void SetFlameState(bool haunted)
    {
        if (isCurrentlyHauntedStatus == haunted && (normalFlame.isPlaying || hauntedFlame.isPlaying))
            return;

        isCurrentlyHauntedStatus = haunted;

        if (haunted)
        {
            if (normalFlame != null) normalFlame.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (hauntedFlame != null && !hauntedFlame.isPlaying)
            {
                ApplyFlickerSettings(hauntedFlame, hauntedSpeed, hauntedSpread);
                hauntedFlame.Play();
            }
        }
        else
        {
            if (hauntedFlame != null) hauntedFlame.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (normalFlame != null && !normalFlame.isPlaying)
            {
                ApplyFlickerSettings(normalFlame, normalSpeed, normalSpread);
                normalFlame.Play();
            }
        }
    }

    /// <summary>
    /// 에러 방지를 위해 모든 Velocity 축의 모드를 통일하여 적용
    /// </summary>
    void ApplyFlickerSettings(ParticleSystem ps, float speed, float size)
    {
        var main = ps.main;
        main.startSpeed = speed;
        main.startSize = size;

        var velocity = ps.velocityOverLifetime;
        if (speed > normalSpeed)
        {
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;

            // 핵심: 모든 축을 'Random Between Two Constants' 모드로 통일
            velocity.x = new ParticleSystem.MinMaxCurve(-1f, 1f);
            velocity.y = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.z = new ParticleSystem.MinMaxCurve(-1f, 1f);
        }
        else
        {
            velocity.enabled = false;
        }
    }

    void StopAllFlames()
    {
        if (normalFlame != null) normalFlame.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (hauntedFlame != null) hauntedFlame.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void OnDisable()
    {
        Deactivate();
    }
}
