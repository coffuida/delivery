using System;
using UnityEngine;

/// <summary>
/// UV Light 검수기 (슬롯 5번)
/// 
/// 기능:
/// - Q 키로 색상 전환 (White → Yellow → Red → Blue → White 순환)
/// - 일반물품: 비춘 UV 색 그대로 반사
/// - 귀품: 귀품의 고유 반응색(uvReactionColor)이 반사됨 (UV 색과 다를 수 있음!)
/// 
/// 예시:
///   UV를 Red로 비춤
///   → 일반물품: 빨강색으로 보임
///   → 귀품(uvReactionColor = Yellow): 노란색으로 보임!
/// </summary>
public class UVLightTool : MonoBehaviour, IInspectionTool
{
    [Header("조명 설정")]
    public Light uvLight;                  // UV 조명
    public float lightIntensity = 2f;      // 조명 강도
    public float lightRange = 5f;          // 조명 범위

    [Header("색상 설정")]
    public Color whiteColor = Color.white;     // ⭐ 하얀색 추가
    public Color yellowColor = Color.yellow;
    public Color redColor = Color.red;
    public Color blueColor = Color.blue;

    [Header("물품 감지")]
    public LayerMask itemLayer;            // 물품 레이어
    public float detectionRange = 3f;      // 감지 범위

    [Header("디버그")]
    public bool showDebugLog = true;

    // 내부 변수
    private bool isActive = false;
    private UVLightColor currentUVColor = UVLightColor.White;  // ⭐ 기본: 하얀색
    private GameObject currentItem = null;
    private Material originalMaterial = null;
    private Material glowMaterial = null;

    /// <summary>
    /// UV Light 전용 색상 enum (White 포함)
    /// </summary>
    public enum UVLightColor
    {
        White,      // 하얀색 (기본)
        Yellow,     // 노랑
        Red,        // 빨강
        Blue        // 파랑
    }

    void Start()
    {
        if (uvLight == null)
            uvLight = GetComponentInChildren<Light>();

        if (uvLight != null)
        {
            uvLight.enabled = false;
            uvLight.type = LightType.Point;
            uvLight.intensity = lightIntensity;
            uvLight.range = lightRange;
        }
    }

    void Update()
    {
        if (!isActive) return;

        // Q 키로 색상 전환
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CycleColor();
        }

        // 물품 감지 및 반사
        DetectAndIlluminateItem();
    }

    public void Activate()
    {
        isActive = true;

        // ⭐ 활성화 시 하얀색으로 시작
        currentUVColor = UVLightColor.White;

        if (uvLight != null)
        {
            uvLight.enabled = true;
            SetUVLightColor(currentUVColor);
        }

        if (showDebugLog)
            Debug.Log($"[UV Light] 활성화 - 색상: {GetColorName(currentUVColor)}");
    }

    public void Deactivate()
    {
        isActive = false;

        if (uvLight != null)
            uvLight.enabled = false;

        // 물품 발광 효과 제거
        RemoveGlowFromItem();

        if (showDebugLog)
            Debug.Log("[UV Light] 비활성화");
    }

    public void CheckItem(GameObject item)
    {
        // Update에서 자동으로 처리
    }

    /// <summary>
    /// ⭐ Q키로 색상 순환 (White → Yellow → Red → Blue → White)
    /// </summary>
    void CycleColor()
    {
        switch (currentUVColor)
        {
            case UVLightColor.White:
                currentUVColor = UVLightColor.Yellow;
                break;
            case UVLightColor.Yellow:
                currentUVColor = UVLightColor.Red;
                break;
            case UVLightColor.Red:
                currentUVColor = UVLightColor.Blue;
                break;
            case UVLightColor.Blue:
                currentUVColor = UVLightColor.White;
                break;
            default:
                currentUVColor = UVLightColor.White;
                break;
        }

        SetUVLightColor(currentUVColor);

        // 현재 물품이 있으면 다시 체크 (색상이 바뀌었으므로)
        if (currentItem != null)
        {
            RemoveGlowFromItem();
            ApplyGlowToItem(currentItem);
        }

        if (showDebugLog)
            Debug.Log($"[UV Light] 색상 전환: {GetColorName(currentUVColor)}");
    }

    /// <summary>
    /// UV 조명 색상 설정
    /// </summary>
    void SetUVLightColor(UVLightColor uvColor)
    {
        if (uvLight == null) return;

        switch (uvColor)
        {
            case UVLightColor.White:
                uvLight.color = whiteColor;
                break;
            case UVLightColor.Yellow:
                uvLight.color = yellowColor;
                break;
            case UVLightColor.Red:
                uvLight.color = redColor;
                break;
            case UVLightColor.Blue:
                uvLight.color = blueColor;
                break;
        }
    }

    /// <summary>
    /// 물품 감지 및 발광 효과
    /// </summary>
    void DetectAndIlluminateItem()
    {
        // Raycast로 물품 감지
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, detectionRange, itemLayer))
        {
            GameObject detectedItem = hit.collider.gameObject;

            // 새로운 물품 감지
            if (detectedItem != currentItem)
            {
                RemoveGlowFromItem();
                ApplyGlowToItem(detectedItem);
            }
        }
        else
        {
            // 물품이 범위 밖으로 나감
            RemoveGlowFromItem();
        }
    }

    /// <summary>
    /// 물품에 발광 효과 적용
    /// </summary>
    void ApplyGlowToItem(GameObject item)
    {
        if (item == null) return;

        Item itemScript = item.GetComponent<Item>();
        if (itemScript == null) return;

        Renderer renderer = item.GetComponent<Renderer>();
        if (renderer == null) return;

        currentItem = item;

        // 원본 머티리얼 저장
        originalMaterial = renderer.material;

        // 새 머티리얼 생성 (발광)
        glowMaterial = new Material(originalMaterial);
        glowMaterial.EnableKeyword("_EMISSION");

        // 반사 색상 결정
        Color responseColor = GetResponseColor(itemScript.hauntedData);

        // Emission 설정
        glowMaterial.SetColor("_EmissionColor", responseColor * 2f);
        renderer.material = glowMaterial;

        if (showDebugLog)
        {
            string itemType = itemScript.hauntedData.isHaunted ? "귀품" : "일반";
            string uvColorName = GetColorName(currentUVColor);

            // ⭐ UVColor를 UVLightColor로 변환하여 표시
            string responseColorName = GetResponseColorName(itemScript.hauntedData);

            Debug.Log($"[UV Light] {itemType} 감지 - UV 색: {uvColorName}, 반사색: {responseColorName}");
        }
    }

    /// <summary>
    /// 물품 발광 효과 제거
    /// </summary>
    void RemoveGlowFromItem()
    {
        if (currentItem != null && originalMaterial != null)
        {
            Renderer renderer = currentItem.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = originalMaterial;
            }

            if (glowMaterial != null)
                Destroy(glowMaterial);

            currentItem = null;
            originalMaterial = null;
            glowMaterial = null;
        }
    }

    /// <summary>
    /// 물품의 반사 색상 결정
    /// 일반물품: UV 색 그대로
    /// 귀품: uvReactionColor (None이면 반응 없음 = UV 색 그대로)
    /// </summary>
    Color GetResponseColor(HauntedItemData hauntedData)
    {
        // 일반물품이면 UV 색 그대로
        if (!hauntedData.isHaunted)
        {
            return UVLightColorToColor(currentUVColor);
        }

        // 귀품이지만 UV 반응색이 None이면 UV 색 그대로
        if (hauntedData.uvReactionColor == UVColor.None)
        {
            return UVLightColorToColor(currentUVColor);
        }

        // 귀품의 고유 반응색 반환 (UVColor → Color 변환)
        return UVColorToColor(hauntedData.uvReactionColor);
    }

    /// <summary>
    /// UVLightColor enum을 Unity Color로 변환
    /// </summary>
    Color UVLightColorToColor(UVLightColor uvColor)
    {
        switch (uvColor)
        {
            case UVLightColor.White:
                return whiteColor;
            case UVLightColor.Yellow:
                return yellowColor;
            case UVLightColor.Red:
                return redColor;
            case UVLightColor.Blue:
                return blueColor;
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// HauntedItemData의 UVColor enum을 Unity Color로 변환
    /// </summary>
    Color UVColorToColor(UVColor uvColor)
    {
        switch (uvColor)
        {
            case UVColor.Yellow:
                return yellowColor;
            case UVColor.Red:
                return redColor;
            case UVColor.Blue:
                return blueColor;
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// UVLightColor를 한글 이름으로 변환
    /// </summary>
    string GetColorName(UVLightColor uvColor)
    {
        switch (uvColor)
        {
            case UVLightColor.White:
                return "하얀색";
            case UVLightColor.Yellow:
                return "노랑";
            case UVLightColor.Red:
                return "빨강";
            case UVLightColor.Blue:
                return "파랑";
            default:
                return "알 수 없음";
        }
    }

    /// <summary>
    /// 귀품의 반응색 이름 가져오기
    /// </summary>
    string GetResponseColorName(HauntedItemData hauntedData)
    {
        if (!hauntedData.isHaunted)
        {
            return GetColorName(currentUVColor);
        }

        if (hauntedData.uvReactionColor == UVColor.None)
        {
            return GetColorName(currentUVColor);
        }

        // UVColor를 한글로 변환
        switch (hauntedData.uvReactionColor)
        {
            case UVColor.Yellow:
                return "노랑";
            case UVColor.Red:
                return "빨강";
            case UVColor.Blue:
                return "파랑";
            default:
                return "알 수 없음";
        }
    }

    void OnDisable()
    {
        Deactivate();
    }

    internal void CycleColor(int direction)
    {
        throw new NotImplementedException();
    }
}