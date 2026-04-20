using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemButtonInitiator : MonoBehaviour
{
    [SerializeField] private Button viewDetailButton;
    [SerializeField] private RawImage itemIcon;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private RawImage purchaseButtonImage;
    [SerializeField] private TMP_Text itemPriceText;
    [SerializeField] private TMP_Text itemCountText;

    [Header("구매 버튼 색상 설정")]
    [SerializeField] private Sprite canPurchaseSprite;
    [SerializeField] private Sprite cannotPurchaseSprite;

    /// <summary>
    /// 아이템 버튼 초기화 메서드
    /// </summary>
    /// <param name="data">초기화에 사용할 쇼핑 데이터</param>
    public void Init(ShoppingData data)
    {
        if(data.item.itemIcon == null) Debug.LogWarning($"아이템 아이콘이 설정되지 않았습니다 - Shop Item Button Initiator | {data.item.itemName}");
        else itemIcon.texture = data.item.itemIcon.texture;
        itemPriceText.text = data.price.ToString() + "$";
        itemCountText.text = Inventory.Instance.GetItemCount(data.item).ToString();

        if(Shop.Instance.CanPurchaseItem(data))
        {
            purchaseButtonImage.texture = canPurchaseSprite.texture;
        }
        else
        {
            purchaseButtonImage.texture = cannotPurchaseSprite.texture;
        
        }
        viewDetailButton.onClick.AddListener(() =>
        {
            Shop.Instance.ShowDetailUI(data);
        });

        purchaseButton.onClick.AddListener(() =>
        {
            if(Shop.Instance.PurchaseItem(data)) //구매 성공 시 아이템 소지 개수 갱신
            {
                itemCountText.text = Inventory.Instance.GetItemCount(data.item).ToString();
                if(Shop.Instance.CanPurchaseItem(data))
                {
                    purchaseButtonImage.texture = canPurchaseSprite.texture;
                }
                else
                {
                    purchaseButtonImage.texture = cannotPurchaseSprite.texture;
                
                } }
        });
    }
    
}
