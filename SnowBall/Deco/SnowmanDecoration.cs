using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Work.GMS.Code.Data;
using Work.KJY.Code.Manager;

public class SnowmanDecoration : MonoBehaviour
{
    public static SnowmanDecoration Instance;
    private DecorationType currentDecoType;

    public SnowmanData currentSnowmanData;

    private GameObject decoItem;
    private int totalPrice;
    private bool isOnUpParts = false;

    [SerializeField] private GameObject noItemAlertPannel;

    [Header("Snow Man")]
    [SerializeField, Range(0, 1f)] private float snowmanMergeRatio;
    [SerializeField] public float snowmanRotationSpeed = 50f;
    [SerializeField] private Transform snowmanRoot;
    [SerializeField] private GameObject snowmanObject;
    [SerializeField] private Transform snowmanUpPart;
    [SerializeField] private Transform snowmanDownPart;

    [Header("Decoration Button UI")]
    [SerializeField] private GameObject uiButtonPrefab;
    [SerializeField] private Transform uiButtonParent;

    [Header("Decoration Filter Button UI")]
    [SerializeField] private GameObject filterButtonPrefab;
    [SerializeField] private Transform filterButtonParent;

    [Header("Snowman Decoration Objects")]
    [SerializeField] private GameObject[] decoMufflers;
    [SerializeField] private GameObject[] decoArms;
    [SerializeField] private GameObject[] decoHats;
    [SerializeField] private GameObject[] decoButtons;
    [SerializeField] private GameObject[] decoEyes;
    [SerializeField] private GameObject[] decoMouths;
    [SerializeField] private GameObject[] decoNoses;

    [Header("Free Placement (Eye/Nose/Mouth/Arm etc)")]
    [SerializeField] private LayerMask snowmanSurfaceMask;    // 스노우맨 표면 레이어
    [SerializeField] private Transform placeablesRoot;        // 생성될 배치형 데코 부모(비워도 됨)
    [SerializeField] private float maxRayDistance = 200f;
    [SerializeField] private float defaultSurfaceOffset = 0.02f;

    [Header("Selling Price Rate")]
    [SerializeField] private GameObject sellingPannel;
    [SerializeField] private TMP_Text sellingDecoPriceText;
    [SerializeField] private TMP_Text sellingSnowPriceText;
    [SerializeField] private TMP_Text minusPriceText;
    [SerializeField] private TMP_Text sellingTotalPriceText;

    [Header("Stamp")]
    [SerializeField] private RawImage stampImage;
    [SerializeField] private Sprite successStamp;
    [SerializeField] private Sprite failedStamp;

    private DecorationItem pendingPlaceItem;
    private DecorationButtonInitiator currentPlacingButton;
    private GameObject previewInstance;

    // 싱글톤/단일 설치 타입(코/입 같은거 “1개만 유지” 하고 싶으면 여기에 등록)
    // 필요없으면 비워둬도 됨.
    private readonly HashSet<DecorationType> singletonPlaceTypes = new()
    {
    };

    public void Start()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 기본 선택 타입(원하면 여기서 원하는 초기값 지정)
        // currentDecoType = DecorationType.Muffler;
        if(DataContainer.Instance != null)InitSnowmanData(DataContainer.Instance.GetSnowmanSize().x, DataContainer.Instance.GetSnowmanSize().y);
        else
        {
            InitSnowmanData(10f, 10f);
            Debug.LogWarning("데이터 컨테이너가 존재하지 않습니다, 기본 사이즈를 불러옵니다");
        }
        
        InitDecoUI();
    }

    private void Update()
    {
        HandlePlacementUpdate();
        RotationSnowman();
    }

    public void RotationSnowman()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        snowmanRoot.localRotation *= Quaternion.Euler(new Vector3(v, -h, 0) * snowmanRotationSpeed * Time.deltaTime);
    }

    public void InitSnowmanData(float upSize, float downSize)
    {
        currentSnowmanData = new SnowmanData(upSize, downSize, new List<DecorationItem>());
        snowmanUpPart.localScale = new Vector3(upSize, upSize, upSize);
        snowmanDownPart.localScale = new Vector3(downSize, downSize, downSize);

        //중앙 위치 맞춰서 스노우맨의 파츠가 정렬됨
        snowmanUpPart.localPosition = new Vector3(0, (downSize / 2) - ((downSize) * snowmanMergeRatio), 0);
        snowmanDownPart.localPosition = new Vector3(0, -downSize / 2, 0);

        //Camera.main.fieldOfView = 60 + (upSize + downSize) * 2;
        Camera.main.transform.localPosition = new Vector3(0, 0, -10 - (upSize + downSize) * 1.2f);

        for(int i = 0; i < placeablesRoot.childCount; i++)
        {
            if(placeablesRoot.GetChild(i).gameObject.tag == "Decoration") Destroy(placeablesRoot.GetChild(i).gameObject);
        }

        decoItem = null;

        snowmanRoot.localRotation = Quaternion.identity;
    }

    private void InitDecoUI()
    {
        // currentDecoType 기준으로 필터된 아이템
        List<ItemBase> items = Inventory.Instance.GetAllItems()
            .Where(item => item is DecorationItem decoItem && decoItem.decorationType == currentDecoType)
            .ToList();

        // 기존 버튼 삭제
        for (int i = filterButtonParent.childCount - 1; i >= 0; i--)
            Destroy(filterButtonParent.GetChild(i).gameObject);

        for (int i = uiButtonParent.childCount - 1; i >= 0; i--)
            Destroy(uiButtonParent.GetChild(i).gameObject);

        // 필터 버튼 생성
        foreach (DecorationType type in System.Enum.GetValues(typeof(DecorationType)))
        {
            DecorationType localType = type; // 캡쳐 안전
            GameObject filterButtonObj = Instantiate(filterButtonPrefab, filterButtonParent);

            // 텍스트
            var txt = filterButtonObj.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = localType.ToString();

            // 클릭
            var btn = filterButtonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    currentDecoType = localType;
                    InitDecoUI();
                });
            }
        }

        // 아이템 버튼 생성
        foreach (var item in items)
        {
            if (item is not DecorationItem) continue;

            GameObject itemButtonObj = Instantiate(uiButtonPrefab, uiButtonParent);

            itemButtonObj.GetComponent<DecorationButtonInitiator>().Init(item as DecorationItem);
        }

        if(uiButtonParent.childCount == 0)
        {
            noItemAlertPannel.SetActive(true);
        }
        else
        {
            noItemAlertPannel.SetActive(false);
        }
    }


    /// <summary>
    /// 스노우맨에 데코레이션 적용 메서드
    /// </summary>
    /// <param name="item">데코레이션 아이템</param>
    /// <param name="buttonInitiator">해당 아이템 선택 버튼</param>
    public void DecoSnowman(DecorationItem item, DecorationButtonInitiator buttonInitiator)
    {
        // === 1) 자유 배치 타입이면 배치 모드로 전환 ===
        if (IsFreePlaceType(item))
        {
            // [수정] 이미 선택된 아이템을 또 눌렀다면? -> 배치 모드 종료 (토글 오프)
            if (pendingPlaceItem == item)
            {
                CancelPlacement();
                return;
            }

            // 새로운 아이템 선택 -> 배치 모드 시작
            BeginPlacement(item, buttonInitiator);
            return;
        }

        // === 2) 기존 토글 방식(목도리/모자/단추 등)은 그대로 유지 ===
        
        bool isCanPlace = false;

        if(decoItem != null && decoItem.activeSelf)
        {
            Inventory.Instance.AddItem(item);
            currentSnowmanData.RemoveDecorationItem(item);
        }
        else
        {
            currentSnowmanData.AddDecorationItem(item);
            isCanPlace = Inventory.Instance.RemoveItem(item);
        }


        if(isCanPlace == false)
        {
            Debug.LogWarning("아이템이 인벤토리에 없습니다: " + item.itemName);
            return;
        }
        
        switch (item.decorationType)
        {
            case DecorationType.Muffler:
                foreach (GameObject muffler in decoMufflers) muffler.SetActive(false);
                decoMufflers[item.decorationObjectIndex].SetActive(!decoMufflers[item.decorationObjectIndex].activeSelf);
                if(decoMufflers[item.decorationObjectIndex].activeSelf)
                decoItem = decoMufflers[item.decorationObjectIndex];
                break;

            case DecorationType.Arm:
                foreach (GameObject arm in decoArms) arm.SetActive(false);
                decoArms[item.decorationObjectIndex].SetActive(!decoArms[item.decorationObjectIndex].activeSelf);
                decoItem = decoArms[item.decorationObjectIndex];
                break;

            case DecorationType.Hat:
                foreach (GameObject hat in decoHats) hat.SetActive(false);
                decoHats[item.decorationObjectIndex].SetActive(!decoHats[item.decorationObjectIndex].activeSelf);
                decoItem = decoHats[item.decorationObjectIndex];
                break;

            case DecorationType.Button:
                foreach (GameObject button in decoButtons) button.SetActive(false);
                decoButtons[item.decorationObjectIndex].SetActive(!decoButtons[item.decorationObjectIndex].activeSelf);
                decoItem = decoButtons[item.decorationObjectIndex];
                break;

            case DecorationType.Eye:
                foreach (GameObject eye in decoEyes) eye.SetActive(false);
                decoEyes[item.decorationObjectIndex].SetActive(!decoEyes[item.decorationObjectIndex].activeSelf);
                decoItem = decoEyes[item.decorationObjectIndex];
                break;

            case DecorationType.Mouth:
                foreach (GameObject mouth in decoMouths) mouth.SetActive(false);
                decoMouths[item.decorationObjectIndex].SetActive(!decoMouths[item.decorationObjectIndex].activeSelf);
                decoItem = decoMouths[item.decorationObjectIndex];
                break;

            case DecorationType.Nose:
                foreach (GameObject nose in decoNoses) nose.SetActive(false);
                decoNoses[item.decorationObjectIndex].SetActive(!decoNoses[item.decorationObjectIndex].activeSelf);
                decoItem = decoNoses[item.decorationObjectIndex];
                break;
        }

        

        InitDecoUI();
    }

    private void CancelPlacement()
    {
        if (previewInstance != null) Destroy(previewInstance);
        previewInstance = null;
        pendingPlaceItem = null;
        currentPlacingButton = null;
        Debug.Log("배치 모드 종료/취소");
    }

    /// <summary>
    /// 자유 배치 타입인지 여부 확인
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private bool IsFreePlaceType(DecorationItem item)
    {
        return item.isPlaceable;
    }

/// <summary>
/// 자유 배치 모드 시작
/// </summary>
/// <param name="item"></param>
    private void BeginPlacement(DecorationItem item, DecorationButtonInitiator buttonInitiator)
    {
        pendingPlaceItem = item;
        currentPlacingButton = buttonInitiator;

        if (previewInstance != null)
        {
            Destroy(previewInstance);
            previewInstance = null;
        }

        Debug.Log($"배치 모드 시작: {item.itemName} (좌클릭=설치, 우클릭=취소)");
    }

    /// <summary>
    /// 자유 배치 모드 업데이트 처리
    /// </summary>
    private void HandlePlacementUpdate()
    {
        if (pendingPlaceItem == null) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject() && previewInstance != null)
        {
            previewInstance.SetActive(false);
            return;
        }
        else if (previewInstance != null)
        {
            previewInstance.SetActive(true);
        }

        

        if (TryGetSnowmanSurfaceHit(out RaycastHit hit))
        {
            // 프리뷰가 없으면 생성
            if (previewInstance == null)
            {
                if (!TryGetPlacePrefab(pendingPlaceItem, out GameObject prefab))
                {
                    pendingPlaceItem = null;
                    return;
                }
                previewInstance = Instantiate(prefab, isOnUpParts ? snowmanUpPart : snowmanDownPart);
                previewInstance.tag = "Decoration";
                SetPreviewVisual(previewInstance, true);
            }
            

            previewInstance.transform.parent = isOnUpParts ? snowmanUpPart : snowmanDownPart;

            // 위치/회전 업데이트
            previewInstance?.SetActive(true);
            float offset = GetSurfaceOffset(pendingPlaceItem);
            Vector3 pos = hit.point + hit.normal * offset;
            Quaternion rot = Quaternion.LookRotation(hit.normal, pendingPlaceItem.placementTarget == PlacementTarget.Up ? Vector3.up : Vector3.down) * Quaternion.Euler(pendingPlaceItem.rotationOffset);
            previewInstance.transform.SetPositionAndRotation(pos, rot);
        }
        else
        {
            previewInstance?.SetActive(false);
        }

        // [수정] 좌클릭: 설치 확정 후 모드를 유지함
        if (Input.GetMouseButtonDown(0) && previewInstance.activeSelf && previewInstance != null && Inventory.Instance.RemoveItem(pendingPlaceItem))
        {
            // 싱글톤(코, 입 등) 처리
            if (singletonPlaceTypes.Contains(pendingPlaceItem.decorationType))
            {
                RemovePlacedSingleton(pendingPlaceItem.decorationType);
            }

            currentPlacingButton.Init(pendingPlaceItem);

            // 현재 프리뷰를 일반 오브젝트로 전환
            SetPreviewVisual(previewInstance, false);
            TagPlacedInstance(previewInstance, pendingPlaceItem.decorationType);
            currentSnowmanData.AddDecorationItem(pendingPlaceItem);
            

            // [중요] previewInstance만 null로 만들어 다음 프레임에 새 프리뷰가 생기게 함
            // pendingPlaceItem은 그대로 유지하여 '연속 설치' 가능하게 함
            previewInstance = null; 
            SoundManager.Instance.Play2DSFX("DECO");
            Debug.Log("데코레이션 적용 완료: " + pendingPlaceItem.itemName);
        }

        // 우클릭: 현재 선택된 아이템 배치 취소
        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }

    /// <summary>
    /// 스노우맨 표면에 대한 레이캐스트 히트 시도
    /// </summary>
    /// <param name="hit"></param>
    /// <returns></returns>
    private bool TryGetSnowmanSurfaceHit(out RaycastHit hit)
    {
        hit = default;
        if (Camera.main == null) return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out hit, maxRayDistance, snowmanSurfaceMask))
            return false;

        // 스노우맨 오브젝트(또는 Up/Down 파트) 하위인지 체크해서 이상한 곳에 설치 방지
        if (snowmanObject != null)
        {
            if (!hit.transform.IsChildOf(snowmanObject.transform) && hit.transform != snowmanObject.transform)
                return false;
        }
        else
        {
            // snowmanObject 미지정이면 Up/Down 기준으로라도 체크
            if (snowmanUpPart != null && hit.transform.IsChildOf(snowmanUpPart))
            {
                isOnUpParts = true;
                return true;
            }
            if (snowmanDownPart != null && hit.transform.IsChildOf(snowmanDownPart))
            {
                isOnUpParts = false;
                return true;
            }

            // 둘 다 아니면 실패
            return false;
        }

        return true;
    }



    /// <summary>
    /// 표면 오프셋 값 가져오기
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private float GetSurfaceOffset(DecorationItem item)
    {
        // DecorationItem에 offset 필드가 없다면 기본값 사용
        // (있으면 그걸로 쓰게 TryGetSurfaceOffset 같은 걸로 확장 가능)
        return defaultSurfaceOffset;
    }

    /// <summary>
    /// 프리뷰 시각적 처리
    /// </summary>
    /// <param name="go">설치된 프리뷰 게임 오브젝트</param>
    /// <param name="isPreview">데코레이션 타입</param>
    private void SetPreviewVisual(GameObject go, bool isPreview)
    {
        // 프리뷰 중엔 콜라이더 꺼서 레이캐스트/겹침 방해 줄이기
        foreach (var col in go.GetComponentsInChildren<Collider>())
            col.enabled = !isPreview;
    }

    /// <summary>
    /// 설치된 인스턴스 태그 지정
    /// </summary>
    /// <param name="go">설치된 프리뷰 게임 오브젝트</param>
    /// <param name="type">데코레이션 타입</param>
    private void TagPlacedInstance(GameObject go, DecorationType type)
    {
        // 나중에 싱글톤 제거/관리용으로 name에 타입 넣기(변수명/레퍼런스 영향 없음)
        go.name = $"Placed_{type}_{go.name}";
    }


/// <summary>
/// 싱글톤 타입 설치된 오브젝트 제거
/// </summary>
/// <param name="type"></param>
    private void RemovePlacedSingleton(DecorationType type)
    {
        Transform root = placeablesRoot ? placeablesRoot : transform;
        // 뒤에서부터 찾아서 삭제(여러 개 있으면 전부 삭제)
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            var child = root.GetChild(i);
            if (child != null && child.name.StartsWith($"Placed_{type}_"))
            {
                Destroy(child.gameObject);
            }
        }
    }
    /// <summary>
    /// 배치 프리팹 가져오기
    /// </summary>
    /// <param name="item">아이템 데이터</param>
    /// <param name="prefab">프리팹 게임 오브젝트</param>
    private bool TryGetPlacePrefab(DecorationItem item, out GameObject prefab)
    {
        prefab = item.placeablePrefab;
        return prefab != null;
    }

    public void SellingSnowman()
    {
        sellingPannel.SetActive(true);
        SoundManager.Instance.Play2DSFX("POPUP");
        minusPriceText.text = "";

        int snowPrice = Mathf.RoundToInt((currentSnowmanData.snowmanUpSize + currentSnowmanData.snowmanDownSize) * 500);
        int decoPrice = 0;
        foreach(var deco in currentSnowmanData.decorationItems)
        {
            decoPrice += deco.itemValuePrice;
        }

        totalPrice = snowPrice + decoPrice;

        if(currentSnowmanData.snowmanUpSize > currentSnowmanData.snowmanDownSize)
        {
            totalPrice -= 500;
            minusPriceText.text += "-500$ | 몸통보다 머리가 커요!\n";
        }

        if(currentSnowmanData.decorationItems.Count == 0)
        {
            totalPrice -= 2000;
            minusPriceText.text += "-2000$ | 장식이 하나도 없어요!\n";
        }

        if(currentSnowmanData.decorationItems.Count >= 30)
        {
            totalPrice -= 5000;
            minusPriceText.text += "-5000$ | 장식이 이상하게 너무 많아요!\n";
        }
        
        if(currentSnowmanData.snowmanUpSize < 0.5f || currentSnowmanData.snowmanDownSize < 0.5f)
        {
            totalPrice -= 500;
            minusPriceText.text += "-500$ | 눈사람이 너무 작아요!\n";
        }

        if(totalPrice < 0)
        {
            stampImage.texture = failedStamp.texture;
        }
        else
        {
            stampImage.texture = successStamp.texture;
        }

        sellingDecoPriceText.text = "장식 가격: " + decoPrice + "$";
        sellingSnowPriceText.text = "눈사람 가격: " + snowPrice + "$";
        sellingTotalPriceText.text = "총 판매 가격: " + totalPrice + "$";
    }

    public void CloseSellingPannel()
    {
        sellingPannel.SetActive(false);
        minusPriceText.text = "";

        Inventory.Instance.AddMoney(totalPrice);

        IrisFadeManager.Instance.FadeIn(1f, "Merge");
    }
}
