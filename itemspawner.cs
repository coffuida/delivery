using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 아이템 스포너 - 3종류 아이템을 확률에 따라 생성
/// </summary>
public class ItemSpawner : MonoBehaviour
{
    [Header("스폰 위치")]
    [Tooltip("아이템이 생성될 위치")]
    public Transform spawnPoint;

    [Header("아이템 프리팹 (빈 오브젝트)")]
    [Tooltip("일반물품 프리팹 (하위에 실제 아이템들)")]
    public GameObject normalItemsPrefab;

    [Tooltip("확정귀품 프리팹 (하위에 실제 아이템들)")]
    public GameObject contrabandItemsPrefab;

    [Tooltip("특수취급물품 프리팹 (하위에 실제 아이템들)")]
    public GameObject specialItemsPrefab;

    [Header("스폰 확률 (합계 100)")]
    [Tooltip("일반물품 스폰 확률 (%)")]
    [Range(0, 100)]
    public float normalItemChance = 60f;

    [Tooltip("확정귀품 스폰 확률 (%)")]
    [Range(0, 100)]
    public float contrabandItemChance = 25f;

    [Tooltip("특수취급물품 스폰 확률 (%)")]
    [Range(0, 100)]
    public float specialItemChance = 15f;

    [Header("일반물품 검수 확률")]
    [Tooltip("일반물품이 실제로 귀품일 확률 (%)")]
    [Range(0, 100)]
    public float normalToContrabandChance = 30f;

    [Header("스폰 설정")]
    [Tooltip("스폰 간격 (초)")]
    public float spawnInterval = 3f;

    [Tooltip("최대 동시 존재 아이템 수")]
    public int maxItems = 10;

    [Header("디버그")]
    public bool showDebugLog = true;

    private bool isSpawning = false;
    private float spawnTimer = 0f;
    private List<GameObject> spawnedItems = new List<GameObject>();

    void Update()
    {
        if (isSpawning)
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnInterval)
            {
                SpawnItem();
                spawnTimer = 0f;
            }
        }

        // 파괴된 아이템 리스트에서 제거
        spawnedItems.RemoveAll(item => item == null);
    }

    public void StartSpawning()
    {
        isSpawning = true;
        spawnTimer = 0f;
        if (showDebugLog)
            Debug.Log("아이템 스포너 시작");
    }

    public void StopSpawning()
    {
        isSpawning = false;
        if (showDebugLog)
            Debug.Log("아이템 스포너 정지");
    }

    void SpawnItem()
    {
        // 최대 아이템 수 확인
        if (spawnedItems.Count >= maxItems)
        {
            if (showDebugLog)
                Debug.LogWarning("최대 아이템 수에 도달!");
            return;
        }

        // 스폰 위치 확인
        if (spawnPoint == null)
        {
            Debug.LogError("Spawn Point가 설정되지 않았습니다!");
            return;
        }

        // 확률 정규화 (합계가 100이 아닐 경우 대비)
        float totalChance = normalItemChance + contrabandItemChance + specialItemChance;
        float normalNormalized = normalItemChance / totalChance;
        float contrabandNormalized = contrabandItemChance / totalChance;

        // 랜덤 선택
        float randomValue = Random.Range(0f, 1f);
        GameObject prefabToSpawn = null;
        string itemType = "";
        bool isContrabandVariant = false;

        if (randomValue < normalNormalized)
        {
            // 일반물품
            prefabToSpawn = normalItemsPrefab;
            itemType = "일반물품";

            // 일반물품이 실제로 귀품인지 결정
            if (Random.Range(0f, 100f) < normalToContrabandChance)
            {
                isContrabandVariant = true;
                itemType += " (실제: 귀품)";
            }
            else
            {
                itemType += " (실제: 정상)";
            }
        }
        else if (randomValue < normalNormalized + contrabandNormalized)
        {
            // 확정귀품
            prefabToSpawn = contrabandItemsPrefab;
            itemType = "확정귀품";
        }
        else
        {
            // 특수취급물품
            prefabToSpawn = specialItemsPrefab;
            itemType = "특수취급물품";
        }

        // 프리팹 확인
        if (prefabToSpawn == null)
        {
            Debug.LogError($"{itemType} 프리팹이 설정되지 않았습니다!");
            return;
        }

        // 하위 아이템 중 랜덤 선택
        GameObject spawnedItem = SpawnRandomChild(prefabToSpawn, isContrabandVariant);

        if (spawnedItem != null)
        {
            spawnedItems.Add(spawnedItem);

            // === 특수 아이템 초기화 (안전한 방법) ===
            // 특정 컴포넌트가 있으면 초기화 호출
            // 컴포넌트가 없어도 에러 없음!

            // 예시 1: MonoBehaviour의 SendMessage 사용 (안전)
            spawnedItem.SendMessage("OnSpawned", SendMessageOptions.DontRequireReceiver);

            // 예시 2: 인터페이스 사용 (더 안전)
            ISpawnable spawnable = spawnedItem.GetComponent<ISpawnable>();
            if (spawnable != null)
            {
                spawnable.OnSpawned();
            }

            // 참고: 특정 스크립트 필요 시 주석 해제
            // 예: Seraphim seraphim = spawnedItem.GetComponent<Seraphim>();
            // if (seraphim != null) seraphim.StartTimer();
            // =====================================

            if (showDebugLog)
                Debug.Log($"✓ {itemType} 스폰: {spawnedItem.name}");
        }
    }

    GameObject SpawnRandomChild(GameObject prefab, bool isContrabandVariant)
    {
        // 프리팹의 하위 오브젝트 개수 확인
        int childCount = prefab.transform.childCount;

        if (childCount == 0)
        {
            Debug.LogError($"{prefab.name}에 하위 오브젝트가 없습니다!");
            return null;
        }

        // 랜덤 하위 오브젝트 선택
        int randomIndex = Random.Range(0, childCount);
        Transform childTransform = prefab.transform.GetChild(randomIndex);

        // 인스턴스 생성
        GameObject spawnedItem = Instantiate(
            childTransform.gameObject,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // ItemData 컴포넌트 추가/설정 (검수 시스템용)
        ItemData itemData = spawnedItem.GetComponent<ItemData>();
        if (itemData == null)
        {
            itemData = spawnedItem.AddComponent<ItemData>();
        }

        // 일반물품인 경우 귀품 여부 설정
        if (prefab == normalItemsPrefab)
        {
            itemData.itemType = ItemType.Normal;
            itemData.isActuallyContraband = isContrabandVariant;
        }
        else if (prefab == contrabandItemsPrefab)
        {
            itemData.itemType = ItemType.Contraband;
            itemData.isActuallyContraband = true;
        }
        else if (prefab == specialItemsPrefab)
        {
            itemData.itemType = ItemType.Special;
            itemData.isActuallyContraband = false;
        }

        return spawnedItem;
    }
}

/// <summary>
/// 스폰 가능 인터페이스 (선택사항)
/// 특수 아이템이 이 인터페이스를 구현하면 자동으로 초기화됨
/// </summary>
public interface ISpawnable
{
    void OnSpawned();
}

/// <summary>
/// 아이템 타입
/// </summary>
public enum ItemType
{
    Normal,      // 일반물품 (검수 필요)
    Contraband,  // 확정귀품
    Special      // 특수취급물품
}

/// <summary>
/// 아이템 데이터 - 검수 시스템용
/// </summary>
public class ItemData : MonoBehaviour
{
    [Header("아이템 정보")]
    public ItemType itemType = ItemType.Normal;

    [Tooltip("실제로 귀품인가? (일반물품만 해당)")]
    public bool isActuallyContraband = false;

    [Tooltip("검수 완료 여부")]
    public bool isInspected = false;

    /// <summary>
    /// 검수 실행
    /// </summary>
    public bool Inspect()
    {
        isInspected = true;

        if (itemType == ItemType.Normal)
        {
            Debug.Log($"검수 결과: {(isActuallyContraband ? "귀품!" : "정상")}");
            return isActuallyContraband;
        }
        else if (itemType == ItemType.Contraband)
        {
            Debug.Log("확정귀품 발견!");
            return true;
        }
        else
        {
            Debug.Log("특수취급물품");
            return false;
        }
    }
}