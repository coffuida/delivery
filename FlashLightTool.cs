using UnityEngine;

/// <summary>
/// Flash Light 검수기 (슬롯 0번)
/// 
/// 기능:
/// - 물품에 빛을 비춤
/// - 일반물품: 그림자 생성
/// - 귀품: 그림자 없음 (noShadow = true)
/// </summary>
public class FlashLightTool : MonoBehaviour, IInspectionTool
{
    [Header("조명 설정")]
    public Light flashLight;
    public float lightIntensity = 3f;
    public float lightRange = 10f;
    public float spotAngle = 45f;

    [Header("물품 감지")]
    public Transform itemPlacementSlot;    // 검수대 물품 슬롯
    public float checkInterval = 0.5f;      // 체크 간격

    [Header("디버그")]
    public bool showDebugLog = true;

    // 내부 변수
    private bool isActive = false;
    private float checkTimer = 0f;

    void Start()
    {
        if (flashLight == null)
            flashLight = GetComponentInChildren<Light>();

        if (flashLight != null)
        {
            flashLight.enabled = false;
            flashLight.type = LightType.Spot;
            flashLight.intensity = lightIntensity;
            flashLight.range = lightRange;
            flashLight.spotAngle = spotAngle;
        }
    }

    void Update()
    {
        if (!isActive) return;

        // 주기적으로 물품 체크
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckCurrentItem();
        }
    }

    public void Activate()
    {
        isActive = true;

        if (flashLight != null)
            flashLight.enabled = true;

        CheckCurrentItem();

        if (showDebugLog)
            Debug.Log("[Flash Light] 활성화");
    }

    public void Deactivate()
    {
        isActive = false;

        if (flashLight != null)
            flashLight.enabled = false;

        if (showDebugLog)
            Debug.Log("[Flash Light] 비활성화");
    }

    public void CheckItem(GameObject item)
    {
        if (item == null) return;

        Item itemScript = item.GetComponent<Item>();
        if (itemScript == null) return;

        // noShadow = true면 그림자 없음 (귀품)
        bool shouldCastShadow = !itemScript.hauntedData.noShadow;
        SetShadowCasting(item, shouldCastShadow);
    }

    /// <summary>
    /// 현재 검수대에 놓인 물품 체크
    /// </summary>
    void CheckCurrentItem()
    {
        if (itemPlacementSlot == null) return;

        // 슬롯의 첫 번째 자식이 물품
        if (itemPlacementSlot.childCount > 0)
        {
            GameObject item = itemPlacementSlot.GetChild(0).gameObject;
            CheckItem(item);
        }
    }

    /// <summary>
    /// 물품의 그림자 설정
    /// </summary>
    void SetShadowCasting(GameObject item, bool castShadow)
    {
        Renderer[] renderers = item.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (castShadow)
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            else
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        if (showDebugLog)
        {
            string shadowStatus = castShadow ? "있음 (일반)" : "없음 (귀품)";
            Debug.Log($"[Flash Light] 물품 그림자: {shadowStatus}");
        }
    }

    void OnDisable()
    {
        Deactivate();
    }
}