using UnityEngine;

/// <summary>
/// UV Light 검수기 (슬롯 5번)
/// 
/// 기능:
/// - Q키로 색상 전환 (플레이어 제어)
/// - 일반물품: 플레이어가 Q키로 자유롭게 색상 전환
/// - 귀품: G키로 활성화 시 자동으로 색상 순환 (플레이어 제어 불가)
/// 
/// ⭐ 개선사항:
/// - 귀품 감지 시 자동 색상 순환
/// - 플레이어 제어권 차단
/// </summary>
public class UVLightTool : MonoBehaviour, IInspectionTool
{
    [Header("UV Light 설정")]
    public Light uvLight;                   // UV Light 컴포넌트
    public float colorChangeInterval = 1f;  // 색상 변경 주기 (초)

    [Header("UV 색상 (3가지)")]
    public Color colorRed = Color.red;
    public Color colorGreen = Color.green;
    public Color colorBlue = Color.blue;

    [Header("물품 감지")]
    public Transform itemPlacementSlot;

    [Header("디버그")]
    public bool showDebugLog = true;

    // 내부 변수
    private bool isActive = false;
    private int currentColorIndex = 0;  // 0: Red, 1: Green, 2: Blue
    private GameObject currentItem = null;
    private bool isHauntedItem = false;

    // 자동 순환 타이머
    private float colorChangeTimer = 0f;
    private bool isAutoRotating = false;

    void Start()
    {
        if (uvLight != null)
        {
            uvLight.enabled = false;
            uvLight.color = colorRed;
        }

        currentColorIndex = 0;
    }

    void Update()
    {
        if (!isActive) return;

        // ⭐ 귀품 + 자동 순환 모드
        if (isAutoRotating && isHauntedItem)
        {
            colorChangeTimer += Time.deltaTime;

            if (colorChangeTimer >= colorChangeInterval)
            {
                colorChangeTimer = 0f;
                CycleColorAutomatically();
            }
        }
        // ⭐ 일반 물품 또는 귀품이 아닌 경우: 플레이어 제어
        else if (!isHauntedItem)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                CycleColorManually();
            }
        }
        // ⭐ 귀품인데 자동 순환 꺼져있으면 무시 (플레이어 제어 차단)
    }

    public void Activate()
    {
        isActive = true;

        if (uvLight != null)
        {
            uvLight.enabled = true;
            ApplyCurrentColor();
        }

        // 현재 아이템 체크
        CheckCurrentItem();

        if (showDebugLog)
            Debug.Log("[UVLight] 활성화");
    }

    public void Deactivate()
    {
        isActive = false;
        isAutoRotating = false;
        colorChangeTimer = 0f;

        if (uvLight != null)
            uvLight.enabled = false;

        if (showDebugLog)
            Debug.Log("[UVLight] 비활성화");
    }

    public void CheckItem(GameObject item)
    {
        currentItem = item;
        CheckCurrentItem();
    }

    /// <summary>
    /// ⭐ 현재 아이템 체크 및 자동 순환 설정
    /// </summary>
    void CheckCurrentItem()
    {
        if (!isActive) return;

        // 아이템이 없으면 자동 순환 중지
        if (currentItem == null)
        {
            isHauntedItem = false;
            isAutoRotating = false;
            colorChangeTimer = 0f;

            if (showDebugLog)
                Debug.Log("[UVLight] 아이템 없음 - 플레이어 제어 가능");
            return;
        }

        // 아이템 스크립트 가져오기
        Item itemScript = currentItem.GetComponent<Item>();
        if (itemScript == null)
        {
            isHauntedItem = false;
            isAutoRotating = false;
            return;
        }

        // ⭐ 귀품인지 확인
        isHauntedItem = itemScript.IsHaunted();

        if (isHauntedItem)
        {
            // 귀품이면 자동 순환 시작
            isAutoRotating = true;
            colorChangeTimer = 0f;

            if (showDebugLog)
                Debug.Log($"[UVLight] ★ 귀품 감지! 자동 색상 순환 시작 (주기: {colorChangeInterval}초)");
        }
        else
        {
            // 일반 물품이면 플레이어 제어
            isAutoRotating = false;
            colorChangeTimer = 0f;

            if (showDebugLog)
                Debug.Log("[UVLight] 일반 물품 - 플레이어 제어 가능 (Q키)");
        }
    }

    /// <summary>
    /// ⭐ 자동 색상 순환 (귀품)
    /// </summary>
    void CycleColorAutomatically()
    {
        currentColorIndex++;
        if (currentColorIndex > 2)
            currentColorIndex = 0;

        ApplyCurrentColor();

        if (showDebugLog)
        {
            string colorName = GetCurrentColorName();
            Debug.Log($"[UVLight] 자동 순환 → {colorName}");
        }
    }

    /// <summary>
    /// ⭐ 수동 색상 전환 (일반 물품, Q키)
    /// </summary>
    void CycleColorManually()
    {
        currentColorIndex++;
        if (currentColorIndex > 2)
            currentColorIndex = 0;

        ApplyCurrentColor();

        if (showDebugLog)
        {
            string colorName = GetCurrentColorName();
            Debug.Log($"[UVLight] 수동 전환 (Q키) → {colorName}");
        }
    }

    /// <summary>
    /// 현재 색상 적용
    /// </summary>
    void ApplyCurrentColor()
    {
        if (uvLight == null) return;

        switch (currentColorIndex)
        {
            case 0:
                uvLight.color = colorRed;
                break;
            case 1:
                uvLight.color = colorGreen;
                break;
            case 2:
                uvLight.color = colorBlue;
                break;
        }
    }

    /// <summary>
    /// 현재 색상 이름 반환
    /// </summary>
    string GetCurrentColorName()
    {
        switch (currentColorIndex)
        {
            case 0: return "Red";
            case 1: return "Green";
            case 2: return "Blue";
            default: return "Unknown";
        }
    }

    void OnDisable()
    {
        Deactivate();
    }
}
