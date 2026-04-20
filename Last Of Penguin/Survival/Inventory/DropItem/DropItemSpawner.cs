using UnityEngine;

public class DropItemSpawner : MonoBehaviour
{
    public GameObject dropItemPrefab;
    public GameObject dropItemByPlayer;
    public static DropItemSpawner Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }


    }

    public void SpawnItem(InventoryItem item, Vector3 position, bool spawnByPlayer = false)
    {
        if (!LOPNetworkManager.Instance.isConnected)
        {
            GameObject dropItem;
            if(spawnByPlayer) dropItem = Instantiate(dropItemByPlayer, position, Quaternion.identity);
            else dropItem = Instantiate(dropItemPrefab, position, Quaternion.identity);
            dropItem.GetComponent<DropItemManage>().item = item;
            dropItem.GetComponent<DropItemManage>().DropItemUIInit();
            StartCoroutine(dropItem.GetComponent<DropItemManage>().EnablePickUp());
        }
        else if (LOPNetworkManager.Instance.isConnected)
        {
            if(spawnByPlayer) LOPNetworkManager.Instance.NetworkInstantiate(dropItemByPlayer, position, Quaternion.identity, item.item.itemID, 1);
            else LOPNetworkManager.Instance.NetworkInstantiate(dropItemPrefab, position, Quaternion.identity, item.item.itemID, 1);
        }
        else
        {
            Debug.LogError("[DropItemSpawner] 아이템 스폰 실패");
        }
    }
}
