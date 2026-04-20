using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static DraggableUI currentDraggable; //현재 드래그 중인 UI를 저장

    CanvasGroup cg;
    Canvas rootCanvas; //Canva 중 최상위
    RectTransform previewRect;
    GameObject previewGO; //드래그 중인 아이템 미리보기
    InventorySlotUI slotUI;
    [SerializeField] private DroppableUI dropUI; // TODO: DroppableUI를 직렬화를 해서 가져와서 다른 주소값을 가져오는 방식으로 진행해서 제대로 할당이 안됨

    [Header("드래그 시작 버튼 판별")]
    private bool isDraggingNow = false;
    private PointerEventData.InputButton dragButton; // 드래그 시작한 마우스 버튼 인식

    void Awake()
    {
        dropUI = GetComponent<DroppableUI>();
        cg = GetComponent<CanvasGroup>(); //Raycast 막는 용도
        rootCanvas = GetComponentInParent<Canvas>();
        slotUI = GetComponent<InventorySlotUI>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slotUI.GetItem().item == null || isDraggingNow || previewGO != null) return; // 중복 드래그 방지

        dragButton = eventData.button; // 드래그 시작 버튼 저장
        isDraggingNow = true;

        cg.blocksRaycasts = false;
        cg.alpha = 0.6f;

        currentDraggable = this;

        previewGO = new GameObject("DragPreview", typeof(RectTransform), typeof(Image)); // 드래그 중 마우스를 따라다닐 프리뷰 이미지 생성 (가상)
        previewGO.transform.SetParent(rootCanvas.transform, false);

        var img = previewGO.GetComponent<Image>();
        img.sprite = slotUI.IconImage.sprite;
        img.raycastTarget = false;
        img.SetNativeSize(); //GPT -> 이미지

        previewRect = previewGO.GetComponent<RectTransform>();
        previewRect.localScale = Vector3.one * 0.45f; //Drag 상태에 item 이미지 크기

        if (dragButton == PointerEventData.InputButton.Right) // 우클릭일 때만 Split 상태 처리
            InventoryManager.Instance.isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (previewRect != null)
            previewRect.position = eventData.position;

        if (dragButton == PointerEventData.InputButton.Left && Input.GetMouseButton(1)) return; // 좌클릭 중 우클릭 차단
        if (dragButton == PointerEventData.InputButton.Right && Input.GetMouseButton(0)) return; // 우클릭 중 좌클릭 차단

        if (dragButton == PointerEventData.InputButton.Right)
        {
            InventoryManager.Instance.isDragging = true; // 우클릭 드래그 중 상태
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        currentDraggable = null;

        cg.blocksRaycasts = true;
        cg.alpha = 1f;

        if (previewGO != null)
        {
            Destroy(previewGO);
            previewGO = null; //잔상 방지 참조 초기화
        }

        isDraggingNow = false;

        InventoryManager.Instance.isDragging = false;
        InventoryManager.Instance.DragCount = 0;
        dropUI.hash = new Hashtable();
        Debug.Log($"{dropUI.hash.Count}해쉬 갯수");
    }

    // 인벤토리/퀵슬롯 UI 위에 마우스 여부 판정 함수
    private bool IsPointerOverSlotUI(PointerEventData eventData)
    {
        // 현재 마우스 아래의 모든 UI 오브젝트 탐색
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var result in results)
        {
            if (result.gameObject.GetComponent<InventorySlotUI>() != null && CompareTag("Inventory"))
                return true;
            Debug.LogWarning("Item in the Inventory");
        }
        return false; // 어느 슬롯 위도 아니면 false
    }
}