using System;
using Island;
using Lop.Survivor;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using GlobalAudio;
using UnityEngine;
using UnityEngine.SceneManagement;



#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.InputSystem;
[System.Serializable]
public class ToolItemList
{
    [Tooltip("3인칭 아이템 모델")]
    [SerializeField] private GameObject tpItemModelObject;
    [Tooltip("1인칭 아이템 모델")]
    [SerializeField] private GameObject fpItemModelObject;
    [SerializeField] private ToolItemType toolItemType;

    public ToolItemType ToolType => toolItemType;

    public void SetActive(bool isActive)
    {
        if (fpItemModelObject != null) fpItemModelObject.SetActive(isActive);
        if (tpItemModelObject != null) tpItemModelObject.SetActive(isActive);
    }

    public GameObject GetTpItemModel() { return tpItemModelObject; }
}

public partial class CharacterController : MonoBehaviour
{
    public static CharacterController Instance;

    [Header("Character Settings")]
    [SerializeField] private GameObject penguinObject;                          // 펭귄 모델링 오브젝트
    [SerializeField] private GameObject penguinMesh;
    [SerializeField] private GameObject penguinAudioObject;                          // 펭귄 오디오 오브젝트
    private Rigidbody rb;
    private Animator anim;
    public InventoryManager inv;                                               // 인벤토리 매니저
    public CharacterUIController characterUIController;
    public Building targetBuilding;

    private IInputProvider inputProvider;

    public event Action<float> OnHpChanged;
    public event Action OnDied;

    [Header("Networking (optional)")]
    [SerializeField] private int ownerId = -1;
    [SerializeField] private NetworkIdentity networkIdentity;
    public int OwnerId { get { return ownerId; } set { ownerId = value; } }


    /// <summary>
    /// </summary>
    public void Initialize(UnitCode unit, IInputProvider provider = null, int owner = -1)
    {
        inputProvider = provider;
        OwnerId = owner;
        unitCode = unit;

        characterStat = CharacterStat.Create(unitCode);
        characterController = this;

        if (characterUIController == null) characterUIController = GetComponent<CharacterUIController>();
    }


    private CharacterController characterController;


    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.2f;                  // 땅 감지 레이어 길이
    [SerializeField] private float waterCheckDistance = 0.2f;                   // 물 감지 레이어 길이
    [SerializeField] private LayerMask groundLayers;                            // 지상 Layer

    [Header("Building")]
    public bool isBuildingMode;                                                 // 빌딩 모드 여부
    [SerializeField] private LayerMask bulidingLayerMask;

    [Header("Status")]
    [SerializeField] private UnitCode unitCode;                                 // 펭귄 번호
    [SerializeField] private CharacterStat characterStat;                       // 펭귄 스탯
    public CharacterStat CharacterStat => characterStat;
    [SerializeField] private CharacterState characterState;                     // 펭귄 현 상태 ( 생존 / 죽음 )
    public CharacterState CharacterState => characterState;
    [Header("Swiming Settings")]
    [SerializeField] private GameObject shaddowObject;
    [SerializeField] private ParticleSystem swimingParticle;
    private bool isInWater = false;                                             // 물 위에 있는지

    public ParticleSystem SnowParticle;

    [Header("Hunger Effects")]
    [Tooltip("허기 비율 경계값들")]
    [Range(0f, 1f)][SerializeField] private float hungerTierHigh = 0.70f;
    [Range(0f, 1f)][SerializeField] private float hungerTierMiddle = 0.50f;
    [Range(0f, 1f)][SerializeField] private float hungerTierLow = 0.40f;

    [Tooltip("70~100% 구간: 10초마다 체력 회복 양")]
    [SerializeField] private int hungerRegenHighAmount = 5;
    [Tooltip("50~70% 구간: 10초마다 체력 회복 양")]
    [SerializeField] private int hungerRegenMidAmount = 3;
    [Tooltip("회복 간격(초)")]
    [SerializeField] private float hungerRegenInterval = 10f;

    [Tooltip("0% 구간: 3초마다 체력 감소 양")]
    [SerializeField] private int hungerHpDrainAmount = 5;
    [Tooltip("체력 감소 간격(초)")]
    [SerializeField] private float hungerHpDrainInterval = 3f;

    // 내부 상태 ( 스크립트 내부 )
    private enum HungerTier { T70_100, T50_70, T40_50, T0_40, T0 }  // 허기 상태 구간
    private HungerTier currentHungerTier = HungerTier.T70_100;      // 현재 허기 상태
    private Coroutine hungerEffectRoutine;                          // 허기 상태 효과 코루틴
    private bool isSprintEnabled = true;                            // 달리기 활성화 여부
    private float finalAttackPowerMultiplier = 1.0f;                // 최종데미지 배율

    [Header("Temperature Effects")]
    public TemperatureType currentBodyTemperatureTier;             // 현재 체온 상태 
    private Coroutine temperatureHpEffectRoutine;                   // 체온 상태 효과 코루틴
    private float temperatureSpeedMultiplier = 1.0f;                // 이동 속도 배율 ( 체온 상태에 따라 )
    private Map map;                                                // 맵 정보
    private BlockData selectBlockID;                                // 선택된 블럭 ID                        Todo: 이거 변수명 회의 때 확인 필요

    [Header("Cursor Block")]
    [SerializeField] private Transform highLightBlock;              // 현재 커서 블럭 위치 하이라이트 오브젝트
    [SerializeField] private Transform playerViewPos;               // 플레이어 시점 위치
    [SerializeField] private float placeBlockDistance = default;    // 플레이어가 블럭을 설치할 수 있는 거리
    [SerializeField] private LayerMask blockLayerMask;              // 블럭 레이어 마스크
    private Vector3Int highlightBlockPos;
    private GameObject highLightBlockObject;                          // 커서 블럭 위치 하이라이트 오브젝트
    [SerializeField] private LayerMask entityLayer;

    [Header("Tree")]
    [SerializeField] private LayerMask treeLayerMask;               // 나무 레이어 마스크

    [Header("Tent")]
    private bool wasFirstPersonBeforeTent = false;                  // 텐트 사용 전 1인칭 모드 여부
    private bool isTent = false;                                    // 현재 텐트 사용 중인지 여부
    private Transform wasPositionBeforeTent;                        // 텐트 사용 전 위치

    [Header("Audio")]
    [SerializeField] private AudioClip dirtWalkSFX;
    [SerializeField] private AudioClip sandWalkSFX;
    [SerializeField] private AudioClip iceWalkSFX;
    [SerializeField] private AudioClip snowWalkSFX;
    [SerializeField] private AudioClip soilWalkSFX;
    [SerializeField] private AudioClip jumpSFX;                     // 점프 사운드
    [SerializeField] private AudioClip axeSFX;                      // 도끼 사운드
    [SerializeField] private AudioClip hammerSFX;                   // 망치 사운드
    [SerializeField] private AudioClip swordSFX;                    // 검 사운드
    [SerializeField] private AudioClip swimmingSFX;                 // 수영 사운드
    [SerializeField] private AudioClip snowBreakSFX;                // 눈 파괴 사운드
    [SerializeField] private AudioClip soilBreakSFX;                // 흙 파괴 사운드
    [SerializeField] private AudioClip stoneBreakSFX;                // 흙 파괴 사운드
    [SerializeField] private AudioClip hitSFX;                      // 공격 사운드
    [SerializeField] private AudioClip DieSFX;                      // 죽음 사운드
    [SerializeField] private AudioClip blockInsSFX;                 // 블럭 설치 사운드
    [SerializeField] private AudioClip eatingSFX;                   // 음식 먹는 사운드

    [Header("Fishing")]
    [SerializeField] private bool isFishing = false;                 // 낚시 중 여부
    private bool wasFirstPersonBeforeFishing;                       // 낚시 사용 전 1인칭 모드 여부

    private PushCollider pushCollider;

    #region Unity Functions
    // 플레이어가 존제 할때만 몬스터 스폰을 활성화 해야하기 때문에 플레이어 생성시 이벤트 발생을 위해 추가함
    public static event Action<CharacterController> OnPlayerSpawned;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        if (highLightBlock == null)
        {
            highLightBlock = GameObject.FindGameObjectWithTag("HighLightBlock").transform;
        }
        highLightBlockObject = highLightBlock.gameObject;

        OnPlayerSpawned?.Invoke(this);
        Cursor.lockState = CursorLockMode.Locked;

        rb = GetComponent<Rigidbody>();
        currentSpeed = baseMoveSpeed;
        if (inv == null)
        {
            inv = InventoryManager.Instance;
        }

        if (penguinObject != null)
            anim = penguinObject.GetComponent<Animator>();
        else
            Debug.LogWarning("추가 안됨 " + gameObject.name);

        firstPersonController = GetComponent<FirstPersonController>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        pushCollider = GetComponent<PushCollider>();
        inv = InventoryManager.Instance;
        if (characterStat == null)
        {
            characterStat = CharacterStat.Create(unitCode);
            // NOTE: prefer calling Initialize(unit, provider) on spawn for multiplayer ownership
        }
        characterUIController = GetComponent<CharacterUIController>();
        characterController = this;
    }

    private void Start()
    {
        TickManager.Instance.OnTick += HungerDown;

        map = MapSettingManager.Instance?.Map;

        QuickslotNumberBtn.Instance.OnChangeItem.AddListener(ChangeItem);//초기화가 좀 이상함
        SetupCameras();
        //characterState = CharacterState.Alive;

        // 허기 상태 효과 강제 초기화
        ForceApplyHungerEffectsOnce();
        AudioPlayer.PlaySound(penguinObject, null, 1f);
        AudioPlayer.PlaySound(penguinObject, null, 1f);//브금넣기
    }

    private void FixedUpdate()
    {
        waterPlane.transform.position = new Vector3(transform.position.x, 6.7f, transform.position.z);

        HandleMovement();

        if (isInWater)
            HandleWaterClimb();
        walkAnimationTimer -= Time.deltaTime;
    }

    private void Update()
    {
        if (characterState == CharacterState.Dead
            || characterState == CharacterState.UsingUI || isFishing)
        {
            HandleAnimation();
            return;
        }

        if (inputProvider != null)
        {
            isSprint = inputProvider.GetSprint();

            if (inputProvider.GetTogglePerspectivePressed())
            {
                TogglePerspective();
            }
        }
        else
        {
            if (Keyboard.current != null)
                isSprint = Keyboard.current.leftCtrlKey.isPressed;

            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
                TogglePerspective();
        }

        if (inputVec == Vector2.zero)
            bodyCollider.material = null;
        else
            bodyCollider.material = jumpPhysicsMaterial;

        if (!isTent)
        {
            if (LOPNetworkManager.Instance.isConnected && !networkIdentity.IsOwner) return;
            CheckTemperature();

            PlaceCursorBlock();
            GroundCheck();
            CorrectDownwardVelocity();
            OnFinishJump();

            HandleCameraControl();
            SprintCameraFov();
            ZoomCameraFov();
            CheckWaterCam();
            LeftClickEvent();
            RightClickEvent();
            DropItem();
            HandleBuildMode();
            InteractionBuilding();
        }
        else characterStat.curBodyTemperature = TemperatureType.Normal;
        ShaddowMovement();

        HandleAnimation();


        //// 허기 상태 효과 갱신
        UpdateHungerEffects();

        //// 체온 상태 효과 갱신
        UpdateTemperatureEffects();

        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;
    }
    #endregion

    #region 캐릭터 초기화 및 상태 변경
    public void Init(int _unit)
    {
        unitCode = (UnitCode)_unit;
    }

    /// <summary>
    /// 캐릭터 상태를 변경하는 함수
    /// </summary>
    /// <param name="state">변경할 상태</param>
    public void ChangeCharacterState(CharacterState state)
    {
        characterState = state;
    }
    #endregion

    private void CheckTemperature()
    {
        if (!isTent)
        {
            //체온 계산 로직
            //대기 온도 확인
            var airTemp = TemperatureManager.Instance.AmbientTemperature;

            //발 아래 블록의 고유 온도 확인
            Vector3 groundCheckPos = transform.position + Vector3.down * 0.1f;
            groundCheckPos.y = Mathf.Max(0, groundCheckPos.y);

            var groundBlock = MapSettingManager.Instance.Map.GetBlockInChunk(groundCheckPos, ChunkType.Ground);
            if (groundBlock == null) return;
            var groundTemp = groundBlock.temperature;

            //더 추운 쪽을 최종 환경 온도로 결정
            var effectiveEnvTemp = (TemperatureType)Mathf.Min((int)airTemp, (int)groundTemp);

            //장비(파카) 효과 적용
            var finalBodyTemp = effectiveEnvTemp;
            if (QuickslotNumberBtn.Instance.IsParkaEquipped() && finalBodyTemp < TemperatureType.Normal)
            {
                finalBodyTemp = TemperatureType.Normal;
            }

            characterStat.curBodyTemperature = finalBodyTemp;
            //체온 계산 로직 끝
        }
    }

    private void UpdateTemperatureEffects()
    {
        var newTempTier = characterStat.curBodyTemperature;
        if (newTempTier == currentBodyTemperatureTier)
        {
            return;
        }

        currentBodyTemperatureTier = newTempTier;
        //기본 초기화
        if (temperatureHpEffectRoutine != null) StopCoroutine(temperatureHpEffectRoutine);
        temperatureSpeedMultiplier = 1.0f;

        switch (newTempTier)
        {
            case TemperatureType.VeryCold:
                temperatureHpEffectRoutine = StartCoroutine(Co_TemperatureHpEffect(0.02f, 1f)); // 1초당 2%씩 감소
                temperatureSpeedMultiplier = 0.6f; // 이동속도가 40% 감소
                break;
            case TemperatureType.Cold:
                temperatureHpEffectRoutine = StartCoroutine(Co_TemperatureHpEffect(0.01f, 1f)); // 1초당 1%씩 감소 
                temperatureSpeedMultiplier = 0.8f; // 이동속도가 20% 감소
                break;
            case TemperatureType.Normal:
                // All effects are reset, 
                break;
            case TemperatureType.Hot:
                temperatureHpEffectRoutine = StartCoroutine(Co_TemperatureHpEffect(0.04f, 1f)); // 1초당 4%씩 감소
                break;
            case TemperatureType.VeryHot:
                temperatureHpEffectRoutine = StartCoroutine(Co_TemperatureHpEffect(0.08f, 1f)); // 1초당 8%씩 감소
                break;
        }
    }

    private IEnumerator Co_TemperatureHpEffect(float percentage, float interval)
    {
        var wait = new WaitForSeconds(interval);
        while (true)
        {
            float amount = characterStat.maxHp * percentage;
            characterStat.curHp = Mathf.Max(0, characterStat.curHp - amount);
            if (characterStat.curHp <= 0) Die();
            characterUIController?.GaugeUpdate();
            yield return wait;
        }
    }

    /// <summary>
    /// 움직임 애니메이션 실행 함수
    /// </summary>
    private void HandleAnimation()
    {
        Vector3Int checkPos = Vector3Int.FloorToInt(this.gameObject.transform.position);
        checkPos.y = checkPos.y + -1;

        isInWater = false;
        if (map.IsVoxelInMap(checkPos))
        {
            BlockData block = map.GetBlockInChunk(checkPos, ChunkType.Water);

            isInWater = block is { id: BlockConstants.Water };
        }

        if (isInWater && !isFishing)
        {
            if (anim != null)
            {
                if (LOPNetworkManager.Instance.isConnected && networkIdentity.IsOwner)
                {
                    anim.SetBool(CharacterAnimatorParamMapper.Swim, true);
                    anim.SetBool(CharacterAnimatorParamMapper.Move, false);
                    LOPNetworkManager.Instance.SendAnimationState(networkIdentity.NetworkId, CharacterAnimatorParamMapper.Swim, true);
                    LOPNetworkManager.Instance.SendAnimationState(networkIdentity.NetworkId, CharacterAnimatorParamMapper.Move, false);
                }
                else
                {
                    anim.SetBool(CharacterAnimatorParamMapper.Swim, true);
                    anim.SetBool(CharacterAnimatorParamMapper.Move, false);
                }
            }
            if (currentTpItemObject != null && currentTpItemObject.activeSelf)
            {
                currentTpItemObject.SetActive(false);
                LOPNetworkManager.Instance.NetworkSetActive(currentTpItemObject, false);
            }
            shaddowObject.gameObject.SetActive(false);
        }
        else
        {
            bool isMoving = moveDir.sqrMagnitude > 0.01f;
            if (anim != null)
            {
                if (LOPNetworkManager.Instance.isConnected && networkIdentity.IsOwner)
                {
                    anim.SetBool(CharacterAnimatorParamMapper.Swim, false);
                    anim.SetBool(CharacterAnimatorParamMapper.Move, isMoving);
                    LOPNetworkManager.Instance.SendAnimationState(networkIdentity.NetworkId, CharacterAnimatorParamMapper.Swim, false);
                    LOPNetworkManager.Instance.SendAnimationState(networkIdentity.NetworkId, CharacterAnimatorParamMapper.Move, isMoving);
                }
                else
                {
                    anim.SetBool(CharacterAnimatorParamMapper.Swim, false);
                    anim.SetBool(CharacterAnimatorParamMapper.Move, isMoving);
                }
            }
            if (isMoving && isFirstPerson && walkAnimationTimer <= 0.0f)
            {
                walkAnimationTimer = 0.4f;
                WalkSound();
            }
            else if (!isMoving) walkAnimationTimer = 0.4f;

            if (currentTpItemObject != null && !currentTpItemObject.activeSelf) currentTpItemObject.SetActive(true);
            if (shaddowObject != null && !shaddowObject.activeSelf) shaddowObject.gameObject.SetActive(true);
        }

    }

    /// <summary>
    /// 낚시 종료   
    /// </summary>
    public void OnFinishFishing()
    {
        if (wasFirstPersonBeforeFishing) TogglePerspective();
        if (LOPNetworkManager.Instance.isConnected && networkIdentity.IsOwner)
        {
            anim.SetBool(CharacterAnimatorParamMapper.Swim, false);
            anim.SetBool(CharacterAnimatorParamMapper.Move, false);
            anim.SetTrigger(CharacterAnimatorParamMapper.EndFishing);
            LOPNetworkManager.Instance.SendAnimationState(networkIdentity.NetworkId, CharacterAnimatorParamMapper.Swim, false);
            LOPNetworkManager.Instance.SendAnimationState(networkIdentity.NetworkId, CharacterAnimatorParamMapper.Move, false);
            LOPNetworkManager.Instance.SendAnimationTrigger(networkIdentity.NetworkId, CharacterAnimatorParamMapper.EndFishing);
        }
        else
        {
            anim.SetBool(CharacterAnimatorParamMapper.Swim, false);
            anim.SetBool(CharacterAnimatorParamMapper.Move, false);
            anim.SetTrigger(CharacterAnimatorParamMapper.EndFishing);
        }
        QuickslotNumberBtn.Instance.cantChange = false;
        isFishing = false;
    }

    public void OnSwiming()
    {
        swimingParticle.Play();
    }
    #region 인바 상호작용
    private void PlaceCursorBlock()
    {
        if (!mainCam) return;

        Transform rayOrigin = GetRayOrigin();
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, placeBlockDistance, blockLayerMask))
        {
            Vector3Int hitBlockPos = Vector3Int.FloorToInt(hit.point - hit.normal * 0.01f);

            Vector3Int targetBlockPos = hitBlockPos + Vector3Int.RoundToInt(hit.normal);

            placeBlockPosition = targetBlockPos + Vector3.one * 0.5f;

            highLightBlock = highLightBlockObject.transform;

            highlightBlockPos = hitBlockPos;
            highLightBlock.position = hitBlockPos + Vector3.one * 0.5f;
            highLightBlock.rotation = Quaternion.identity;
            highLightBlock.gameObject.SetActive(true);
        }
        else
        {
            if (highLightBlock == null) return;
            placeBlockPosition = Vector3.zero;
            highlightBlockPos = Vector3Int.zero;
            highLightBlock.gameObject.SetActive(false);
            highLightBlock = null;
        }
    }

    private void DestroyBlock(Vector3 pos)
    {

        if (!highLightBlock.gameObject.activeSelf) return;
        BlockData blockData = map.GetBlockInChunk(pos, ChunkType.Ground);
        if (blockData != null && !map.GetBlockInChunk(pos, ChunkType.Ground).isDestroy) return;
        map.GetChunkFromPosition(pos, ChunkType.Ground)?.CrackVoxel(pos);
    }

    #endregion

    #region 이동속도 디버프 관리
    public void ApplySpeedDebuff(float debuffAmount)
    {
        moveSpeedDebuffMultiplier = Mathf.Clamp01(1f - debuffAmount);
    }

    public void RemoveSpeedDebuff()
    {
        moveSpeedDebuffMultiplier = 1.0f;
    }
    #endregion

    /// <summary>
    /// Ray Origin을 반환하는 함수
    /// </summary>
    /// <returns></returns>
    private Transform GetRayOrigin()
    {
        return isFirstPerson ? mainCam.transform : playerViewPos;
    }

    #region 아이템 변경
    ///// <summary>
    ///// 아이템 변경
    ///// </summary>
    private void ChangeItem()
    {
        // 1. 소유자(Owner)가 아니면 실행하지 않음
        if (LOPNetworkManager.Instance.isConnected && !networkIdentity.IsOwner) return;

        // 2. 장착할 아이템 타입 결정
        ToolItemType newItemType = ToolItemType.None;
        ItemDatabase selectedItemData = QuickslotNumberBtn.Instance.selectedItem?.item;

        if (QuickslotNumberBtn.Instance.selectedItem == null || QuickslotNumberBtn.Instance.selectedItem.amount == 0)
        {
            newItemType = ToolItemType.None; // 빈 슬롯
        }
        else if (selectedItemData != null)
        {
            newItemType = selectedItemData.toolType; // 아이템의 고유 타입
        }

        bool hasHandItem = false;
        if (QuickslotNumberBtn.Instance.selectedItem != null && QuickslotNumberBtn.Instance.selectedItem.amount > 0)
        {
            hasHandItem = (QuickslotNumberBtn.Instance.selectedItem.item.toolType == ToolItemType.None) || (QuickslotNumberBtn.Instance.selectedItem.item.toolType == ToolItemType.parka);
        }

        // [!] (신규) 전송할 아이템 ID 결정 (손 아이템이 아니면 빈 문자열)
        string iconItemId = "";
        if (hasHandItem && selectedItemData != null)
        {
            // ItemDatabase에 있는 고유 ID 필드 (예: "item_hand")
            // LOPNetworkManager의 ItemGenerator.GetItemData(string id)와 일치해야 합니다.
            // 여기서는 필드 이름을 'id'라고 가정합니다.
            iconItemId = selectedItemData.itemID;
        }

        // 3. 로컬 플레이어의 시각적 모델을 즉시 변경
        // [!] iconItemId를 넘겨주도록 수정
        SetEquippedItemVisuals(newItemType, hasHandItem, iconItemId);

        // 4. 네트워크로 RPC 전송
        if (LOPNetworkManager.Instance.isConnected)
        {
            LOPNetworkManager.Instance.RPC(
                this,                         // 이 characterController 컴포넌트
                nameof(Rpc_SetEquippedItem),  // RPC 수신 메서드 이름
                newItemType,                  // 파라미터 1 (ToolItemType)
                hasHandItem,                  // 파라미터 2 (bool)
                iconItemId                    // [!] 파라미터 3 (string) - quickslot 객체 대신 ID 전송
            );
        }
    }

    [LopRPC]
    // [!] 파라미터를 string으로 변경
    public void Rpc_SetEquippedItem(ToolItemType newType, bool hasHandItem, string iconItemId)
    {
        // 소유자(Owner)는 ChangeItem()에서 이미 로컬로 적용했으므로,
        // 네트워크로 다시 돌아온(Loopback) RPC는 무시해야 중복 실행을 막을 수 있습니다.
        if (networkIdentity != null && networkIdentity.IsOwner)
        {
            return;
        }

        // 다른 모든 원격 클라이언트는 이 함수를 실행하여
        // 해당 플레이어의 아이템 모델을 변경합니다.
        SetEquippedItemVisuals(newType, hasHandItem, iconItemId);
    }

    // [!] 파라미터를 string으로 변경
    public void SetEquippedItemVisuals(ToolItemType newType, bool hasHandItem, string iconItemId)
    {
        // 기존 ChangeItem 함수의 모델 활성화 로직
        currentToolType = newType;

        if (toolItemModelList == null) return;

        foreach (var item in toolItemModelList)
        {
            bool isSelected = item.ToolType == newType;
            item.SetActive(isSelected);

            if (isSelected)
            {
                currentTpItemObject = item.GetTpItemModel();
            }
        }

        // "없음" 또는 "손" 상태 처리 (아이템이 아예 없거나, '손' 아이템일 때)
        if (newType == ToolItemType.None || newType == ToolItemType.parka)
        {
            foreach (var item in toolItemModelList)
            {
                if (item.ToolType == ToolItemType.None || item.ToolType == ToolItemType.parka)
                {
                    // "손" 아이템 모델은 'hasHandItem'이 true일 때만 활성화
                    item.SetActive(hasHandItem);
                    if (hasHandItem) currentTpItemObject = item.GetTpItemModel();
                }
                else
                {
                    // 다른 모든 도구는 비활성화
                    item.SetActive(false);
                }
            }

            // 1인칭 손 아이콘 업데이트
            if (hasHandItem)
            {
                // [!] (수정) iconItemId를 사용해 아이콘을 찾습니다.
                Sprite iconToShow = null;
                if (!string.IsNullOrEmpty(iconItemId) && ItemGenerator.Instance != null)
                {
                    // LOPNetworkManager의 ItemGenerator를 사용해 ID로 아이템 정보 조회
                    ItemDatabase itemData = ItemGenerator.Instance.GetItemData(iconItemId);
                    if (itemData != null)
                    {
                        iconToShow = itemData.icon;
                    }
                }

                // [!] 찾은 아이콘으로 설정
                tphandleItem.sprite = iconToShow;
                fphandleItem.sprite = iconToShow; // (참고: 1인칭 핸들은 IsOwner가 아닐 땐 꺼주는 게 좋습니다)
            }

            if (!hasHandItem)
            {
                currentTpItemObject = null;
            }
        }
    }

    public void DropItem()
    {
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            if (QuickslotNumberBtn.Instance.selectedItem == null || QuickslotNumberBtn.Instance.selectedItem.amount == 0 || currentToolType != ToolItemType.None) return;
            InventoryManager.Instance.RemoveItem(new InventoryItem(QuickslotNumberBtn.Instance.selectedItem.item, 1));
            DropItemSpawner.Instance.SpawnItem(new InventoryItem(QuickslotNumberBtn.Instance.selectedItem.item, 1), transform.position, true);
            QuickslotNumberBtn.Instance.UpdateQuickSlot();
        }
    }
    #endregion

    #region 피격
    public void TakeDamage(float dmg)
    {
        if (characterStat == null) return;
        AudioPlayer.PlaySound(penguinObject, hitSFX, 1f);
        characterStat.curHp = Mathf.Max(0f, characterStat.curHp - dmg);
        OnHpChanged?.Invoke(characterStat.curHp);
        StartCoroutine(characterUIController.Co_ShowDamageEffect());

        if (characterUIController != null)
        {
            characterUIController.GaugeUpdate();

        }
        if (characterStat.curHp <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (characterState == CharacterState.Dead) return;
        if (isFirstPerson) TogglePerspective();
        rb.linearVelocity = Vector3.zero;
        AudioPlayer.PlaySound(penguinObject, DieSFX, 1f);
        anim?.SetTrigger(CharacterAnimatorParamMapper.Dead);
        if (LOPNetworkManager.Instance.isConnected && networkIdentity.IsOwner)
        {
            LOPNetworkManager.Instance?.SendAnimationTrigger(networkIdentity.NetworkId, CharacterAnimatorParamMapper.Dead);
            LOPNetworkManager.Instance.Disconnect();
        }
        OnDied?.Invoke();
        PanelManager.Instance.Show(PanelType.LoadingPanel, new LoadingPanel.Args("_01_Lobby"));
        characterState = CharacterState.Dead;
    }
    #endregion

    #region 허기 상호작용
    /// <summary>
    /// 허기 초기화용 함수
    /// </summary>
    private void ForceApplyHungerEffectsOnce()
    {
        float h = characterStat.curHunger;
        float hMax = characterStat.maxHunger;
        float ratio = (hMax > 0f) ? h / hMax : 0f;

        var tier = GetHungerTierFromRatio(ratio);
        currentHungerTier = tier;
        ApplyHungerTierEffects(tier);
    }

    private HungerTier GetHungerTierFromRatio(float ratio)
    {
        if (ratio <= 0f) return HungerTier.T0;
        else if (ratio <= hungerTierLow) return HungerTier.T0_40;
        else if (ratio <= hungerTierMiddle) return HungerTier.T40_50;
        else if (ratio <= hungerTierHigh) return HungerTier.T50_70;
        else return HungerTier.T70_100;
    }

    private void UpdateHungerEffects()
    {
        float h = characterStat.curHunger;
        float hMax = characterStat.maxHunger;
        float ratio = (hMax > 0f) ? h / hMax : 0f;

        HungerTier newTier;

        if (ratio <= 0f) newTier = HungerTier.T0;
        else if (ratio <= hungerTierLow) newTier = HungerTier.T0_40;
        else if (ratio <= hungerTierMiddle) newTier = HungerTier.T40_50;
        else if (ratio <= hungerTierHigh) newTier = HungerTier.T50_70;
        else newTier = HungerTier.T70_100;

        if (newTier != currentHungerTier)
        {
            currentHungerTier = newTier;
            ApplyHungerTierEffects(newTier);
        }
    }

    private void ApplyHungerTierEffects(HungerTier tier)
    {
        // 공통: 기존 주기 효과 정지
        if (hungerEffectRoutine != null)
        {
            StopCoroutine(hungerEffectRoutine);
            hungerEffectRoutine = null;
        }

        switch (tier)
        {
            case HungerTier.T70_100:
                isSprintEnabled = true;
                finalAttackPowerMultiplier = 1f;
                hungerEffectRoutine = StartCoroutine(Co_Regen(hungerRegenHighAmount, hungerRegenInterval));
                break;

            case HungerTier.T50_70:
                isSprintEnabled = true;
                finalAttackPowerMultiplier = 1f;
                hungerEffectRoutine = StartCoroutine(Co_Regen(hungerRegenMidAmount, hungerRegenInterval));
                break;

            case HungerTier.T40_50:
                isSprintEnabled = true;
                finalAttackPowerMultiplier = 1f;
                // 변화 없음 (주기 효과 없음)
                break;

            case HungerTier.T0_40:
                isSprintEnabled = false;                // 달리기 불가
                finalAttackPowerMultiplier = 0.90f; // 플레이어가 입히는 최종 데미지 -10%
                break;

            case HungerTier.T0:
                isSprintEnabled = false;
                finalAttackPowerMultiplier = 0.80f; // 플레이어가 입히는 최종 데미지 -20%
                hungerEffectRoutine = StartCoroutine(Co_HpDrain(hungerHpDrainAmount, hungerHpDrainInterval));
                break;
        }
    }
    #endregion

    #region 허기 관련 주기 코루틴
    /// <summary>
    /// 회복 코루틴
    /// </summary>
    /// <param name="amount">힐량</param>
    /// <param name="interval">주기</param>
    /// <returns></returns>
    private IEnumerator Co_Regen(int amount, float interval)
    {
        WaitForSeconds wait = new WaitForSeconds(Mathf.Max(0.01f, interval));
        while (true)
        {
            if (characterState != CharacterState.Dead)
            {
                characterStat.curHp = Mathf.Min(characterStat.curHp + amount, characterStat.maxHp);
                characterUIController?.GaugeUpdate();
            }
            yield return wait;
        }
    }

    /// <summary>
    /// 체력 감소 코루틴
    /// </summary>
    /// <param name="amount">감소량</param>
    /// <param name="interval">주기</param>
    /// <returns></returns>
    private IEnumerator Co_HpDrain(int amount, float interval)
    {
        WaitForSeconds wait = new WaitForSeconds(Mathf.Max(0.01f, interval));
        while (true)
        {
            if (characterState != CharacterState.Dead)
            {
                characterStat.curHp = Mathf.Max(0, characterStat.curHp - amount);
                if (characterStat.curHp <= 0) Die();
                characterUIController?.GaugeUpdate();
            }
            yield return wait;
        }
    }

    private void HungerDown()
    {
        float time = TickManager.Instance.elapsedTicks % 60f;
        float inWater = isInWater ? 1.4f : 1f;
        if (time == 0 && characterStat.curHunger > 0f)
        {
            characterStat.curHunger -= 5f * inWater;
            characterUIController?.GaugeUpdate();
        }
    }
    #endregion


    #region 텐트 상호작용
    /// <summary>
    /// 텐트 입장 함수
    /// </summary>
    /// <param name="tentTransform">텐트 위치</param>
    public void StartTentInteractive(Transform tentTransform)
    {
        if (!isTent)
        {
            wasPositionBeforeTent = tentTransform;         // 입장 전 위치 저장
            transform.position = tentTransform.position;            // 텐트속 지정 위치로 이동
            QuestPanel.Instance.IncreaseProgress("MQ_Antarctica_1_3", 1);

            if (isFirstPerson)
            {
                TogglePerspective();
                wasFirstPersonBeforeTent = true;
            }
            isTent = true;
            penguinObject.SetActive(false);
            shaddowObject.SetActive(false);
            pushCollider.enabled = false;
            rb.linearVelocity = Vector3.zero;
            rb.useGravity = false;
            isGrounded = false;
        }
    }

    /// <summary>
    /// 텐트 종료 함수
    /// </summary>
    /// <param name="tentFrontTransform">텐트 앞 위치</param>
    public void EndTentInteractive()
    {
        WorldUIManager.Instance.Hide();
        penguinObject.SetActive(true);
        shaddowObject.SetActive(true);
        pushCollider.enabled = true;
        rb.useGravity = true;
        transform.position = wasPositionBeforeTent.position;    // 입장 전 위치로 이동
        if (wasFirstPersonBeforeTent)
        {
            TogglePerspective();
            wasFirstPersonBeforeTent = false;
        }
        isTent = false;
        isGrounded = true;
    }
    #endregion

    public static Vector3 SnapToNearestHalf(Vector3 pos)
    {
        return new Vector3(
            Mathf.Round(pos.x * 2f) / 2f,
            Mathf.Round(pos.y * 2f) / 2f,
            Mathf.Round(pos.z * 2f) / 2f
        );
    }

    public void SetPenguinActive(bool active)
    {
        penguinMesh.SetActive(active);
    }

    #region 빌딩 모드
    private void HandleBuildMode()
    {
        if (GameManager.Instance.characterController.isBuildingMode)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                BuildingManager.Instance.PlaceBuilding();
            }

            if (Input.GetMouseButtonDown(1))
            {
                BuildingManager.Instance.ExitBuildMode();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                BuildingManager.Instance.RotationPreviewBuildingInstance();
            }
        }
    }

    public void InteractionBuilding()
    {
        //end
        Collider[] hit = Physics.OverlapSphere(gameObject.transform.position, 1f, bulidingLayerMask);

        if (hit.Length > 0)
        {
            if (LOPNetworkManager.Instance.isConnected == true)
            {
                NetworkIdentity network = hit[0].GetComponent<NetworkIdentity>();
                if (network.IsOwner)
                {
                    WorldUIManager.Instance.ShowSpeechBubble();
                }
                else
                {
                    WorldUIManager.Instance.Hide();
                }
            }
            else if (LOPNetworkManager.Instance.isConnected == false)
            {
                WorldUIManager.Instance.ShowSpeechBubble();
            }
            targetBuilding = hit[0].GetComponent<Building>();
            WorldUIManager.Instance.ShowSpeechBubble();
        }
        else
        {
            WorldUIManager.Instance.Hide();
            targetBuilding = null;
        }
    }
    #endregion
    private void OnDrawGizmos()
    {
        //블럭 설치할 좌표에 엔티티가 있나 검사
        if (highLightBlock == null) return;
        if (Physics.BoxCast(placeBlockPosition + Vector3.down * 2.2f, highLightBlock.localScale / 2, Vector3.up, out RaycastHit hit, highLightBlock.rotation, 2f, entityLayer))
        {
            Gizmos.color = Color.red;
            Debug.Log(hit.transform.gameObject.name);
            Gizmos.DrawRay(placeBlockPosition, Vector3.up * hit.distance);
            Gizmos.DrawWireCube(placeBlockPosition, highLightBlock.localScale / 2 * 2);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(placeBlockPosition, Vector3.up * 1);
            Gizmos.DrawWireCube(placeBlockPosition * 1, highLightBlock.localScale / 2 * 2);
        }
        //캐릭터 점프검사 히트박스
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.3f * -0.2f, transform.localScale / 3 * 2);
    }
}
