using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(Image))]
public class DroppableUI : MonoBehaviour,
    IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover color")]
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 1f);
    [Header("Drop color")]
    [SerializeField] private Color dropColor = Color.white;
    [SerializeField] private float dropHighlightDuration = 0.2f;
    [SerializeField] private DraggableUI dragable;

    [SerializeField] public Hashtable hash = new Hashtable();

    private Image background;
    private Color originalColor;

    public Image SlotHighlight;
    public GameObject itemInfoObject;
    public TMP_Text itemInfoText;


    void Awake()
    {
        dragable = GetComponent<DraggableUI>();
        background = GetComponent<Image>();
        originalColor = SlotHighlight.color;
    }

    public void OnPointerEnter(PointerEventData eventData) // Hover될 때마다 실행
    {
        SlotHighlight.color = hoverColor;

        InventoryManager.Instance.StartSlotHover(ItemInfoShow);

        if (InventoryManager.Instance.isDragging && !hash.ContainsValue(eventData.hovered)) //아이템 들고 있는 상태에서 인벤토리 칸에 닿았는지 확인 && HashTable hoverd 확인
        {
            hash.Add((InventoryManager.Instance.DragCount), (eventData.hovered));
            InventoryManager.Instance.DragCount++;
            Debug.Log(InventoryManager.Instance.DragCount);

        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        SlotHighlight.color = originalColor;
        InventoryManager.Instance.EndSlotHover();
        itemInfoObject.SetActive(false);
    }
    
    public void ItemInfoShow()
    {
        if (GetComponent<InventorySlotUI>().currentItem.item == null) return;
        
        itemInfoObject.SetActive(true);
        itemInfoText.text = GetComponent<InventorySlotUI>().currentItem.item.itemName;
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("asdDrop");
        var drag = eventData.pointerDrag?.GetComponent<DraggableUI>();
        if (drag == null) return;

        // form이 내가 들고 있는애. to가 내가 내려놓는 자리
        var from = drag.GetComponent<InventorySlotUI>();
        var to = GetComponent<InventorySlotUI>();
        if (from == null || to == null) return;

        // 실제 아이템 이동 && 합치기 && 스왑 로직
        InventoryManager.Instance.DropItem(to, from);

        to.UpdateUI();
        from.UpdateUI();


        // Drop시 HighLight 효과.
        StartCoroutine(FlashDrop());
        if (from == to)
        {
            drag.enabled = false;
        }
        else
        {
            drag = gameObject.GetComponent<DraggableUI>();
            drag.enabled = false;
            dragable.enabled = true;
        }
    }

    private IEnumerator FlashDrop()
    {
        SlotHighlight.color = dropColor;
        yield return new WaitForSeconds(dropHighlightDuration);
        SlotHighlight.color = originalColor;
    }
}
