using UnityEngine;

/// <summary>
/// 공장 범위 정의 - 세라핌이 순찰할 영역
/// </summary>
public class FactoryZone : MonoBehaviour
{
    [Header("범위 설정")]
    [Tooltip("범위의 크기 (로컬 스케일)")]
    public Vector3 zoneSize = new Vector3(20f, 5f, 20f);

    [Header("시각화")]
    public Color gizmoColor = new Color(0f, 1f, 0f, 0.3f);
    public Color wireColor = Color.green;

    /// <summary>
    /// 범위 내 랜덤 위치 반환
    /// </summary>
    public Vector3 GetRandomPosition()
    {
        Vector3 randomLocal = new Vector3(
            Random.Range(-zoneSize.x / 2f, zoneSize.x / 2f),
            Random.Range(-zoneSize.y / 2f, zoneSize.y / 2f),
            Random.Range(-zoneSize.z / 2f, zoneSize.z / 2f)
        );

        return transform.TransformPoint(randomLocal);
    }

    /// <summary>
    /// Y축 고정 랜덤 위치 (부유 높이용)
    /// </summary>
    public Vector3 GetRandomPositionAtHeight(float height)
    {
        Vector3 randomLocal = new Vector3(
            Random.Range(-zoneSize.x / 2f, zoneSize.x / 2f),
            height,
            Random.Range(-zoneSize.z / 2f, zoneSize.z / 2f)
        );

        return transform.TransformPoint(randomLocal);
    }

    /// <summary>
    /// 특정 위치가 범위 내에 있는지 확인
    /// </summary>
    public bool IsInside(Vector3 worldPosition)
    {
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

        return Mathf.Abs(localPosition.x) <= zoneSize.x / 2f &&
               Mathf.Abs(localPosition.y) <= zoneSize.y / 2f &&
               Mathf.Abs(localPosition.z) <= zoneSize.z / 2f;
    }

    /// <summary>
    /// Gizmo 표시
    /// </summary>
    void OnDrawGizmos()
    {
        // 반투명 박스
        Gizmos.color = gizmoColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, zoneSize);

        // 외곽선
        Gizmos.color = wireColor;
        Gizmos.DrawWireCube(Vector3.zero, zoneSize);
    }
}
