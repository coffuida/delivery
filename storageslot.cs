using UnityEngine;

/// <summary>
/// 보관대 슬롯 데이터
/// ⭐ storedItem → itemPrefab으로 통일
/// </summary>
[System.Serializable]
public class StorageSlot
{
    public bool isEmpty = true;
    public string itemName = "";
    public GameObject itemPrefab; // ⭐ 필드명 통일
    internal GameObject storedItem;

    public void Clear()
    {
        isEmpty = true;
        itemName = "";
        itemPrefab = null;
    }
}