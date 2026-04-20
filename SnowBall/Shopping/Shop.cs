using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public static Shop Instance;

    [Header("Shop Items Data")]
    [SerializeField] private ShoppingData[] shoppingDatas;
    [SerializeField] private ItemType filterType;


    [Header("UI Elements")]
    [SerializeField] private GameObject shopUI;
    [SerializeField] private Transform itemsParent;
    [SerializeField] private GameObject shopItemPrefab;

    [Header("Detail UI Elements")]
    [SerializeField] private GameObject itemDetailUI;
    [SerializeField] private RawImage itemDetailImage;
    [SerializeField] private TMP_Text itemDetailDescText;
    [SerializeField] private TMP_Text itemDetailNameText;

    void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        InitUI();
    }


    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && shopUI.activeSelf)
        {
            if(itemDetailUI.activeSelf) HideDetailUI();
            else ExitShop();
        }
    }

    public void ExitShop()
    {
        this.gameObject.SetActive(false);
    }

    //<summary>
    // 필터링에 맞춰 아이템 데이터 초기화
    //</summary>
    private void InitUI()
    {
        foreach(Transform child in itemsParent)
        {
            Destroy(child.gameObject);
        }

        foreach(var data in shoppingDatas)
        {
            if(data.item.itemType != filterType) continue;
            GameObject itemObj = Instantiate(shopItemPrefab, itemsParent);
            itemObj.GetComponent<ShopItemButtonInitiator>().Init(data);
        }
    }

    /// <summary>
    /// 아이템 구매 메서드
    /// </summary>
    /// <param name="data">구매 데이터</param>
    public bool PurchaseItem(ShoppingData data)
    {
        //구매 로직
        if(Inventory.Instance.SpendMoney(data.price))
        {
            Inventory.Instance.AddItem(data.item);
            Debug.Log("구매 완료: " + data.item.itemName + " | 가격: " + data.price + " | 남은 돈: " + Inventory.Instance.money);
            InitUI();
            return true;
        }
        else
        {
            Debug.LogWarning("돈이 부족합니다!");
            return false;
        }
    }

    /// <summary>
    /// 필터 타입 변경 메서드
    /// </summary>
    /// <param name="type">0 = Deco, 1 = Buff</param>
    public void ChangeFilterType(int type)
    {
        filterType = (ItemType)type;
        InitUI();
    }

    public void ShowDetailUI(ShoppingData data)
    {
        if(data.item.itemIcon == null) Debug.LogWarning($"아이템 아이콘이 설정되지 않았습니다 - Detail View | {data.item.itemName}");
        else itemDetailImage.texture = data.item.itemIcon.texture;
        itemDetailNameText.text = data.item.itemName;
        itemDetailDescText.text = data.item.itemDescription;

        itemDetailUI.SetActive(true);
    }

    public bool CanPurchaseItem(ShoppingData data)
    {
        return Inventory.Instance.money >= data.price;
    }

    public void HideDetailUI()
    {
        itemDetailUI.SetActive(false);
    }
}
