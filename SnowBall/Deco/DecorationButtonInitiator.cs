using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecorationButtonInitiator : MonoBehaviour
{
    [SerializeField] private Button installButton;
    [SerializeField] private RawImage itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemCountText;

    /// <summary>
    /// 아이템 버튼 초기화 메서드
    /// </summary>
    /// <param name="data">초기화에 사용할 데코레이션 데이터</param>
    public void Init(DecorationItem data)
    {
        if(data.itemIcon == null) Debug.LogWarning($"아이템 아이콘이 설정되지 않았습니다 - Decoration Button Initiator | {data.itemName}");
        else itemIcon.texture = data.itemIcon.texture;
        itemCountText.text = Inventory.Instance.GetItemCount(data).ToString();
        itemNameText.text = data.itemName;
        installButton.onClick.RemoveAllListeners();
        installButton.onClick.AddListener(() =>
        {
            SnowmanDecoration.Instance.DecoSnowman(data, this);
        });
    }
    
}
