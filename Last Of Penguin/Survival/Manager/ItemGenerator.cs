using System.Collections.Generic;
using UnityEngine;

public class ItemGenerator : MonoBehaviour
{
    public static ItemGenerator Instance;
    private Dictionary<string, ItemDatabase> itemDatabaseDictionary = new();
    private Dictionary<string, ItemDatabase> blockDatabaseDictionary = new();

    private void Awake()
    {
        Instance = this;

        LoadItemDatas();
    }

    private void LoadItemDatas()
    {
        ItemDatabase[] itemDatabase = Resources.LoadAll<ItemDatabase>("ScriptableObjects/ItemDatabase"); // Resources 폴더에 있어야 함

        foreach (ItemDatabase data in itemDatabase)
        {
            itemDatabaseDictionary[data.itemID] = data;
            if(data.metaType == ItemMetaType.Block)
            {
                if (data.metaData is BlockItem blockData)
                {
                    blockDatabaseDictionary[blockData.placedBlockId] = data;
                }

            }
        }
    }

    public ItemDatabase GetItemData(string ItemId)
    {
        if(itemDatabaseDictionary.TryGetValue(ItemId, out ItemDatabase value))
        {
            return value;
        }
        else
        {
            Debug.LogError($"찾고자 하는 ItemId에 맞는 아이템리소스가 없습니다. {ItemId}");
            return new ItemDatabase();
        }
    }

    public ItemDatabase GetItemDataFromBlock(string ItemId)
    {
        if (blockDatabaseDictionary.TryGetValue(ItemId, out ItemDatabase value))
        {
            return value;
        }
        else
        {
            Debug.LogError($"찾고자 하는 BlockID에 맞는 아이템리소스가 없습니다. {ItemId}");
            return new ItemDatabase();
        }
    }
}
