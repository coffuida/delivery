using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 분류 영역 - 떨어진 아이템 감지 및 판정
/// </summary>
public class SortingZone : MonoBehaviour
{
    [Header("감지 설정")]
    [Tooltip("감지 영역 Collider (Trigger)")]
    public Collider detectionCollider;

    [Header("판정 설정")]
    [Tooltip("자동 판정 (들어오면 즉시)")]
    public bool autoJudge = true;

    [Tooltip("판정 딜레이 (초) - 문 닫힌 후")]
    public float judgeDelay = 0.5f;

    [Header("목표 시스템")]
    public MonoBehaviour objectiveManager;

    [Header("디버그")]
    public bool showDebugLog = true;

    private List<GameObject> itemsInZone = new List<GameObject>();

    void Start()
    {
        if (detectionCollider == null)
        {
            detectionCollider = GetComponent<Collider>();
        }

        if (detectionCollider != null)
        {
            detectionCollider.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Item 스크립트 확인
        Item item = other.GetComponent<Item>();
        if (item == null)
            item = other.GetComponentInParent<Item>();

        if (item != null)
        {
            if (showDebugLog)
                Debug.Log($"아이템 감지: {other.gameObject.name}");

            // 리스트에 추가
            if (!itemsInZone.Contains(other.gameObject))
            {
                itemsInZone.Add(other.gameObject);
            }

            // 자동 판정
            if (autoJudge)
            {
                Invoke("JudgeItems", judgeDelay);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // 나간 아이템 제거
        itemsInZone.Remove(other.gameObject);
    }

    /// <summary>
    /// 수동 판정 (외부에서 호출 가능)
    /// </summary>
    public void JudgeItems()
    {
        if (itemsInZone.Count == 0)
        {
            if (showDebugLog)
                Debug.Log("판정할 아이템이 없습니다!");
            return;
        }

        if (showDebugLog)
            Debug.Log($"=== 판정 시작: {itemsInZone.Count}개 아이템 ===");

        // 복사본 만들기 (삭제 중에도 안전)
        List<GameObject> itemsToJudge = new List<GameObject>(itemsInZone);

        foreach (GameObject itemObj in itemsToJudge)
        {
            if (itemObj == null) continue;

            // ItemData 가져오기
            ItemData itemData = itemObj.GetComponent<ItemData>();
            if (itemData == null)
            {
                if (showDebugLog)
                    Debug.LogWarning($"{itemObj.name}에 ItemData가 없습니다!");
                continue;
            }

            // 판정
            bool isContraband = JudgeItem(itemData);

            // 목표 시스템 업데이트
            if (objectiveManager != null)
            {
                var method = objectiveManager.GetType().GetMethod("OnItemProcessed");
                if (method != null)
                {
                    method.Invoke(objectiveManager, new object[] { isContraband });
                }
            }

            // 로그
            if (showDebugLog)
            {
                string result = isContraband ? "귀품" : "정상물품";
                Debug.Log($"판정: {itemData.name} → {result}");
            }

            // 아이템 삭제
            Destroy(itemObj);
        }

        // 리스트 비우기
        itemsInZone.Clear();

        if (showDebugLog)
            Debug.Log("=== 판정 완료 ===");
    }

    /// <summary>
    /// 아이템 판정 로직
    /// </summary>
    bool JudgeItem(ItemData itemData)
    {
        // 확정 귀품
        if (itemData.itemType == ItemType.Contraband)
        {
            return true;
        }

        // 일반 물품이지만 실제로는 귀품
        if (itemData.itemType == ItemType.Normal && itemData.isActuallyContraband)
        {
            return true;
        }

        // 특수 취급 물품은 정상물품으로 판정
        if (itemData.itemType == ItemType.Special)
        {
            return false;
        }

        // 나머지는 정상물품
        return false;
    }

    /// <summary>
    /// Gizmo로 영역 표시
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (detectionCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(detectionCollider.bounds.center, detectionCollider.bounds.size);
        }
    }
}