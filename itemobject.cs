using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 아이템 스크립트 - 기본 줍기 + 벽 충돌 방지 + 귀품 특성
/// </summary>
public class Item : MonoBehaviour
{
    [Header("아이템 정보")]
    public string itemName = "아이템";

    [Header("줍기 설정")]
    public bool canBePickedUp = true;

    [Tooltip("아직에서 E키로도 상호작용 가능 여부 (줍기 전)")]
    public bool canInteractBeforePickup = false;

    [Header("벽 충돌 방지용")]
    [Tooltip("벽 레이어 (충돌 방지용)")]
    public LayerMask wallLayer;

    [Tooltip("벽과의 최소 거리")]
    public float wallDistance = 0.3f;

    [Header("E키 상호작용 이벤트 (선택사항)")]
    [Tooltip("E키를 눌러 상호작용할 때 실행되는 이벤트")]
    public UnityEvent onInteract;

    // ══════════════════════════════════════════
    // ★ 귀품 특성 (검수기 시스템용)
    // ══════════════════════════════════════════
    [Header("귀품 특성")]
    [Tooltip("귀품 여부와 각 검수기별 반응을 여기서 설정")]
    public HauntedItemData hauntedData;

    // ══════════════════════════════════════════
    // 내부 변수
    // ══════════════════════════════════════════
    [HideInInspector] public bool isHeld = false;
    [HideInInspector] public Camera playerCamera;
    [HideInInspector] public float targetDistance = 2f;

    private Rigidbody rb;
    private Collider[] itemColliders;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        itemColliders = GetComponentsInChildren<Collider>();
    }

    void LateUpdate()
    {
        if (isHeld && playerCamera != null)
        {
            CheckWallCollision();
        }
    }

    // ══════════════════════════════════════════
    // 벽 충돌 방지
    // ══════════════════════════════════════════

    void CheckWallCollision()
    {
        Vector3 desiredPosition = playerCamera.transform.position +
                                 playerCamera.transform.forward * targetDistance;

        RaycastHit hit;
        if (Physics.Raycast(
            playerCamera.transform.position,
            playerCamera.transform.forward,
            out hit,
            targetDistance,
            wallLayer))
        {
            desiredPosition = hit.point - playerCamera.transform.forward * wallDistance;
        }

        transform.position = desiredPosition;
    }

    // ══════════════════════════════════════════
    // 줍기 / 내리놓기
    // ══════════════════════════════════════════

    public void OnPickedUp(Camera cam, float distance)
    {
        isHeld = true;
        playerCamera = cam;
        targetDistance = distance;

        Collider playerCollider = cam.GetComponentInParent<Collider>();
        if (playerCollider != null)
        {
            foreach (Collider col in itemColliders)
                Physics.IgnoreCollision(col, playerCollider, true);
        }

        foreach (Collider col in itemColliders)
            col.isTrigger = true;
    }

    public void OnDropped()
    {
        isHeld = false;

        if (playerCamera != null)
        {
            Collider playerCollider = playerCamera.GetComponentInParent<Collider>();
            if (playerCollider != null)
            {
                foreach (Collider col in itemColliders)
                    Physics.IgnoreCollision(col, playerCollider, false);
            }
        }

        foreach (Collider col in itemColliders)
            col.isTrigger = false;

        playerCamera = null;
    }

    // ══════════════════════════════════════════
    // 거리 업데이트
    // ══════════════════════════════════════════

    public void UpdateDistance(float distance)
    {
        targetDistance = distance;
    }

    // ══════════════════════════════════════════
    // E키 상호작용
    // ══════════════════════════════════════════

    public void Interact()
    {
        Debug.Log($"{itemName} E키 상호작용!");

        if (onInteract != null)
            onInteract.Invoke();

        OnInteract();
    }

    protected virtual void OnInteract()
    {
        // 서브클래스에서 오버라이드 가능
    }

    // ══════════════════════════════════════════
    // ★ 귀품 특성 조회 헬퍼 함수들
    //   검수기 스크립트에서 이를 호출하여 반응 여부 확인
    // ══════════════════════════════════════════

    /// <summary>귀품인가?</summary>
    public bool IsHaunted()
    {
        return hauntedData.isHaunted;
    }

    /// <summary>Flashlight: 그림자가 안 생기는가? (귀품일 때만 true)</summary>
    public bool HasNoShadow()
    {
        return hauntedData.isHaunted && hauntedData.noShadow;
    }

    /// <summary>Candle: 횃불이 흔들리는가? (귀품일 때만 true)</summary>
    public bool CandleFlicker()
    {
        return hauntedData.isHaunted && hauntedData.candleFlicker;
    }

    /// <summary>Salt: 연기가 나는가? (귀품일 때만 true)</summary>
    public bool SaltSmoke()
    {
        return hauntedData.isHaunted && hauntedData.saltSmoke;
    }

    /// <summary>Cross: 형태가 왜곡되는가? (귀품일 때만 true)</summary>
    public bool CrossGlitch()
    {
        return hauntedData.isHaunted && hauntedData.crossGlitch;
    }

    /// <summary>Spirit Box: 귀신 소리가 나는가? (귀품일 때만 true)</summary>
    public bool TriggersSpiritBox()
    {
        return hauntedData.isHaunted && hauntedData.triggersSpiritBox;
    }

    /// <summary>Spirit Box: 귀신 소리 AudioClip 반환</summary>
    public AudioClip GetSpiritBoxSound()
    {
        return hauntedData.spiritBoxSound;
    }

    /// <summary>
    /// UV Light: 귀품의 반응색 반환.
    /// 귀품이 아니거나 uvReactionColor가 None이면 UVColor.None 반환
    /// </summary>
    public UVColor GetUVReactionColor()
    {
        if (!hauntedData.isHaunted) return UVColor.None;
        return hauntedData.uvReactionColor;
    }
}