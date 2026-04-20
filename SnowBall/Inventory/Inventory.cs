using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Work.KJY.Code.Event;
using Work.Utils.EventBus;
using UnityEngine.SceneManagement;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    public int money;

    public List<ItemBase> defaultItems;
    public List<ItemBase> usagedBuffItems;

    private Dictionary<ItemBase, int> itemCounts = new Dictionary<ItemBase, int>();
    

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else Destroy(gameObject);

        foreach(var item in defaultItems)
        {
            AddItem(item);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 체인을 걸어서 이 함수는 매 씬마다 호출된다.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene Loaded | Inventory Init");
        Bus<ChangeMoneyEvent>.Raise(new ChangeMoneyEvent(money));
    }

    private void Start()
    {
        Bus<ChangeMoneyEvent>.Raise(new ChangeMoneyEvent(money));
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.P))
        {
            AddMoney(10000);
        }
    }

    public void AddMoney(int amount)
    {
        money += amount;
        Bus<ChangeMoneyEvent>.Raise(new ChangeMoneyEvent(money));
    }

    public bool IsUsedBuffItem(string ItemName)
    {
        foreach(var item in usagedBuffItems)
        {
            if(item.itemName == ItemName)
                return true;
        }
        return false;
    }

    public bool CheckMoney(int amount)
    {
        if(money < amount) 
            return false;
        else 
            return true;
    }

    public bool SpendMoney(int amount)
    {
        if(money < amount) return false;
        money -= amount;
        Bus<ChangeMoneyEvent>.Raise(new ChangeMoneyEvent(money));
        return true;
    }

    public void AddItem(ItemBase item)
    {
        itemCounts[item] = itemCounts.ContainsKey(item) ? itemCounts[item] + 1 : 1;
    }

    public bool RemoveItem(ItemBase item)
    {
        if(itemCounts.ContainsKey(item) && itemCounts[item] > 0)
        {
            itemCounts[item]--;
            return true;
        }
        return false;
    }

    public bool UseBuffItem(ItemBase item)
    {
        if(usagedBuffItems.Contains(item))
        {
            Debug.LogWarning("이미 사용한 버프 아이템입니다.");
            return false;
        }

        if(RemoveItem(item))
        {
            usagedBuffItems.Add(item);
            Debug.Log("버프 아이템 사용: " + item.itemName);
            return true;
        }
        else
        {
            Debug.LogWarning("아이템이 인벤토리에 없습니다: " + item.itemName);
            return false;
        }
    }

    public bool UnUseBuffItem(ItemBase item)
    {
        if(usagedBuffItems.Contains(item))
        {
            usagedBuffItems.Remove(item);
            AddItem(item);
            Debug.Log("버프 아이템 제거: " + item.itemName);
            return true;
        }
        else
        {
            Debug.LogWarning("사용 중인 버프 아이템이 아닙니다: " + item.itemName);
            return false;
        }
    }

    public List<ItemBase> GetAllItems()
    {
        List<ItemBase> keys = new List<ItemBase>();
        foreach(var kvp in itemCounts)
        {
            keys.Add(kvp.Key);
        }

        return keys;
    }

    public int GetItemCount(ItemBase item)
    {
        return itemCounts.ContainsKey(item) ? itemCounts[item] : 0;
    }

    
}
