using UnityEngine;

/// <summary>
/// Cross 검수기 (슬롯 3번)
/// 
/// 기능:
/// - 물품을 십자가 위에 올림
/// - 일반물품: 아무 일 없음
/// - 귀품: 형태 지직거림 (glitch 효과) + 흐릿해짐 (crossGlitch = true)
/// </summary>
public class CrossTool : MonoBehaviour, IInspectionTool
{
    [Header("효과 설정")]
    public float glitchIntensity = 0.2f;   // Glitch 강도
    public float glitchSpeed = 10f;        // Glitch 속도
    public float fadeAlpha = 0.5f;         // 흐릿함 정도 (0~1)

    [Header("물품 감지")]
    public Transform itemPlacementSlot;
    public float checkInterval = 0.1f;

    [Header("디버그")]
    public bool showDebugLog = true;

    // 내부 변수
    private bool isActive = false;
    private float checkTimer = 0f;
    private GameObject currentItem = null;
    private Material[] originalMaterials = null;
    private Material[] glitchMaterials = null;

    void Update()
    {
        if (!isActive) return;

        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckCurrentItem();
        }

        // Glitch 효과 업데이트
        if (currentItem != null)
        {
            UpdateGlitchEffect();
        }
    }

    public void Activate()
    {
        isActive = true;

        CheckCurrentItem();

        if (showDebugLog)
            Debug.Log("[Cross] 활성화");
    }

    public void Deactivate()
    {
        isActive = false;

        RemoveEffects();

        if (showDebugLog)
            Debug.Log("[Cross] 비활성화");
    }

    public void CheckItem(GameObject item)
    {
        if (item == null) return;

        Item itemScript = item.GetComponent<Item>();
        if (itemScript == null) return;

        // crossGlitch = true면 glitch 효과 (귀품)
        if (itemScript.hauntedData.crossGlitch)
        {
            ApplyGlitchEffect(item);
        }
        else
        {
            RemoveEffects();
        }
    }

    /// <summary>
    /// 현재 물품 체크
    /// </summary>
    void CheckCurrentItem()
    {
        if (itemPlacementSlot == null) return;

        if (itemPlacementSlot.childCount > 0)
        {
            GameObject item = itemPlacementSlot.GetChild(0).gameObject;

            if (item != currentItem)
            {
                RemoveEffects();
                currentItem = item;
                CheckItem(item);
            }
        }
        else
        {
            RemoveEffects();
        }
    }

    /// <summary>
    /// Glitch 효과 적용
    /// </summary>
    void ApplyGlitchEffect(GameObject item)
    {
        Renderer[] renderers = item.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        // 원본 머티리얼 저장
        originalMaterials = new Material[renderers.Length];
        glitchMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;
            glitchMaterials[i] = new Material(originalMaterials[i]);

            // 투명도 설정
            SetMaterialTransparent(glitchMaterials[i]);
            renderers[i].material = glitchMaterials[i];
        }

        if (showDebugLog)
            Debug.Log("[Cross] 귀품 감지 - Glitch 효과 시작");
    }

    /// <summary>
    /// Glitch 효과 업데이트 (매 프레임)
    /// </summary>
    void UpdateGlitchEffect()
    {
        if (currentItem == null || glitchMaterials == null) return;

        Renderer[] renderers = currentItem.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length && i < glitchMaterials.Length; i++)
        {
            Material mat = glitchMaterials[i];

            // 위치 흔들림
            Vector3 offset = new Vector3(
                Mathf.Sin(Time.time * glitchSpeed) * glitchIntensity,
                Mathf.Sin(Time.time * glitchSpeed * 1.3f) * glitchIntensity,
                Mathf.Sin(Time.time * glitchSpeed * 0.8f) * glitchIntensity
            );
            renderers[i].transform.localPosition += offset;

            // 투명도 흔들림
            Color color = mat.color;
            color.a = fadeAlpha + Mathf.Sin(Time.time * glitchSpeed) * 0.2f;
            mat.color = color;
        }
    }

    /// <summary>
    /// 머티리얼을 투명 모드로 변경
    /// </summary>
    void SetMaterialTransparent(Material mat)
    {
        mat.SetFloat("_Mode", 3); // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        Color color = mat.color;
        color.a = fadeAlpha;
        mat.color = color;
    }

    /// <summary>
    /// 효과 제거
    /// </summary>
    void RemoveEffects()
    {
        if (currentItem != null && originalMaterials != null)
        {
            Renderer[] renderers = currentItem.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < renderers.Length && i < originalMaterials.Length; i++)
            {
                renderers[i].material = originalMaterials[i];
                renderers[i].transform.localPosition = Vector3.zero;
            }

            if (glitchMaterials != null)
            {
                foreach (Material mat in glitchMaterials)
                {
                    if (mat != null)
                        Destroy(mat);
                }
            }

            currentItem = null;
            originalMaterials = null;
            glitchMaterials = null;
        }
    }

    void OnDisable()
    {
        Deactivate();
    }
}