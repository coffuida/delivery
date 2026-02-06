using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 컨베이어 벨트 - 위에 있는 아이템을 특정 방향으로 이동 (텍스처 방향 조절 추가본)
/// </summary>
public class ConveyorBelt : MonoBehaviour
{
    [Header("컨베이어 설정")]
    [Tooltip("컨베이어 활성화 여부")]
    public bool isActive = false;

    [Tooltip("컨베이어 이동 속도")]
    public float speed = 2f;

    [Tooltip("이동 방향 (World Space)")]
    public Vector3 direction = Vector3.forward;

    [Header("감지 설정")]
    [Tooltip("컨베이어 위의 아이템을 감지할 레이어")]
    public LayerMask itemLayer;

    [Header("시각 효과 (텍스처 제어)")]
    [Tooltip("컨베이어 표면 (텍스처 스크롤용)")]
    public Renderer conveyorSurface;

    [Tooltip("텍스처 스크롤 방향 (X, Y축 비율)")]
    public Vector2 textureDirection = new Vector2(0, 1);

    [Tooltip("텍스처 스크롤 속도")]
    public float textureScrollSpeed = 0.5f;

    private HashSet<Rigidbody> itemsOnBelt = new HashSet<Rigidbody>();
    private Vector2 currentOffset = Vector2.zero;

    void Start()
    {
        // 방향 정규화
        direction = direction.normalized;

        // Collider 확인
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"{gameObject.name}: Collider의 'Is Trigger'를 체크해주세요!");
        }
    }

    void Update()
    {
        // 레버 등으로 활성화되었을 때만 텍스처 이동
        if (isActive)
        {
            UpdateTexture();
        }
    }

    void FixedUpdate()
    {
        if (isActive)
        {
            MoveItems();
        }
    }

    /// <summary>
    /// 아이템이 벨트에 들어올 때
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & itemLayer) == 0)
            return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && !rb.isKinematic)
        {
            itemsOnBelt.Add(rb);
            Debug.Log($"✓ 컨베이어 진입: {other.gameObject.name}");
        }
    }

    /// <summary>
    /// 아이템이 벨트 위에 있을 때 (매 프레임)
    /// </summary>
    void OnTriggerStay(Collider other)
    {
        if (((1 << other.gameObject.layer) & itemLayer) == 0)
            return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            if (rb.isKinematic)
            {
                itemsOnBelt.Remove(rb);
            }
            else
            {
                itemsOnBelt.Add(rb);
            }
        }
    }

    /// <summary>
    /// 아이템이 벨트를 벗어날 때
    /// </summary>
    void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            itemsOnBelt.Remove(rb);
            Debug.Log($"✗ 컨베이어 이탈: {other.gameObject.name}");
        }
    }

    /// <summary>
    /// 아이템 이동 - Rigidbody.velocity 사용
    /// </summary>
    void MoveItems()
    {
        Vector3 moveVelocity = direction * speed;

        itemsOnBelt.RemoveWhere(rb => rb == null);

        foreach (Rigidbody rb in itemsOnBelt)
        {
            if (rb == null || rb.isKinematic) continue;

            rb.velocity = new Vector3(
                moveVelocity.x,
                rb.velocity.y,
                moveVelocity.z
            );
        }
    }

    /// <summary>
    /// [수정됨] 사용자가 지정한 방향으로 텍스처 오프셋 이동
    /// </summary>
    void UpdateTexture()
    {
        if (conveyorSurface != null && conveyorSurface.material != null)
        {
            // textureDirection 수치에 따라 X 또는 Y축으로 오프셋 누적
            currentOffset += textureDirection * textureScrollSpeed * Time.deltaTime;
            conveyorSurface.material.mainTextureOffset = currentOffset;
        }
    }

    /// <summary>
    /// Gizmo로 방향 표시
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = isActive ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, direction * 2f);
    }
}