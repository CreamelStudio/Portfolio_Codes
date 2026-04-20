using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using static Unity.Burst.Intrinsics.X86.Avx;
using static UnityEngine.EventSystems.EventTrigger;

public class TestSelectManager : MonoBehaviour
{
    public static TestSelectManager instance;

    [System.Serializable]
    public class LocalUIPlayerIndex
    {
        public int playerNum;
        public GameObject playerUIParent;
        public GameObject selectWarningText;
        public GameObject infoUI;

        public RectTransform charPrefabParent;

        public GameObject[] speedRate;
        public GameObject[] efficiencyRate;

        public Image characterSkillUI;

        public Image characterSkilInfolUI;
        public TMP_Text characterSkillName;
        public TMP_Text characterSkillfever;
        public TMP_Text characterSkillDesc;

        public CharacterProfile characterProfile;
    }

    [SerializeField] private CharacterProfile[] penguinDatas;
    [SerializeField] private List<LocalUIPlayerIndex> playerIndexes;

    [SerializeField] EventTrigger[] characterButtons;
    [SerializeField] private GameObject characterDragPrefab;
    [SerializeField] private Transform dragPrefabSpawnPosition;

    [SerializeField] public bool[] selectCheck;
    [SerializeField] private RawImage[] selectPanel;

    private void Awake()
    {
        instance = this;
        selectCheck = new bool[selectPanel.Length];

        for (int i=0; i<characterButtons.Length; i++)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            int cap = i;
            entry.callback.AddListener((data) => { DrawStart(cap); });
            characterButtons[i].triggers.Add(entry);
        }

        for(int i=0;i< selectPanel.Length; i++)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            int cap = i;
            entry.callback.AddListener((data) => { Debug.Log(cap); selectCheck[cap] = true; selectPanel[cap].color = new Color(1, 1, 1, 0.125f);});
            selectPanel[i].GetComponent<EventTrigger>().triggers.Add(entry);

            entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            cap = i;
            entry.callback.AddListener((data) => { Debug.Log(cap); selectCheck[cap] = false; selectPanel[cap].color = new Color(1, 1, 1, 0);});
            selectPanel[i].GetComponent<EventTrigger>().triggers.Add(entry);
        }
    }

    private void Start()
    {
        Reset();
    }

    private void DrawStart(int index)
    {
        GameObject obj = Instantiate(characterDragPrefab, dragPrefabSpawnPosition);
        obj.GetComponent<DragButton>().isFollowing = true;
        obj.GetComponent<DragButton>().characterIndex = index;
        obj.GetComponent<Image>().sprite = penguinDatas[index].uiSprite;
    }

    public void Reset()
    {
        foreach(LocalUIPlayerIndex i in playerIndexes)
        {
            i.playerUIParent.SetActive(false);
        }
        TestLocalDataManager.Instance.players.Clear();
    }

    public void InitUI(int i)
    {
        if (playerIndexes[i].characterProfile == null) //만약 캐릭터 프로필이 널이면
        {
            playerIndexes[i].infoUI.SetActive(false); //캐릭터의 정보를 띄워주는 오브젝트 비활성화
            playerIndexes[i].selectWarningText.SetActive(true); //캐릭터를 선택하라는 경고 텍스트 활성화
            return;
        }
        else //만약 널이 아니면
        {
            playerIndexes[i].infoUI.SetActive(true); //캐릭터의 정보를 띄워주는 오브젝트 활성화
            playerIndexes[i].selectWarningText.SetActive(false); //경고 텍스트 비활성화
        }

        for (int j = 0; j < playerIndexes[i].charPrefabParent.childCount; j++) //캐릭터 오브젝트를 가지고 있는 부모 오브젝트의 자녀를 모두 삭제
        {
            Destroy(playerIndexes[i].charPrefabParent.GetChild(j).gameObject);
        }
        Instantiate(playerIndexes[i].characterProfile.uiObject, playerIndexes[i].charPrefabParent); //새 캐릭터 오브젝트 생

        playerIndexes[i].characterSkillUI.sprite = playerIndexes[i].characterProfile.skillImage; //캐릭터 스킬 이미지 변경
        playerIndexes[i].characterSkilInfolUI.sprite = playerIndexes[i].characterProfile.skillImage; //캐릭터 스킬 정보속 이미지 변경
        playerIndexes[i].characterSkillName.text = playerIndexes[i].characterProfile.skillName; //캐릭터 스킬 이름 변경
        playerIndexes[i].characterSkillfever.text = playerIndexes[i].characterProfile.skillType; //캐릭터 스킬 타입 변경
        playerIndexes[i].characterSkillDesc.text = playerIndexes[i].characterProfile.skillExplanation; //캐릭터 스킬 설명 변경

        for (int j = 0; j < playerIndexes[i].speedRate.Length; j++) //별 최대 개수까지 반복
        {
            if (j <= playerIndexes[i].characterProfile.ordinary) playerIndexes[i].speedRate[j].SetActive(true); //만약 별의 번호가 속도 값보다 낮거나 같으면 별 활성화
            else playerIndexes[i].speedRate[j].SetActive(false);

            if (j <= playerIndexes[i].characterProfile.dexterity) playerIndexes[i].efficiencyRate[j].SetActive(true); //만약 손재주의 번호가 속도 값보다 낮거나 같으면 별 활성화
            else playerIndexes[i].efficiencyRate[j].SetActive(false);
        }
    }

    public void OnPlayerJoined()
    {
        if (PlayerInput.all.Count == 0)
        {
            Debug.LogError("No players found!");
            return;
        }

        PlayerInput lastJoinedPlayer = PlayerInput.all.Last(); // ���� �ֱٿ� ���� �÷��̾� ��������
        int playerIndex = lastJoinedPlayer.playerIndex;
        CharacterProfile defaultCharacter = GetDefaultCharacter();
        InputDevice device = lastJoinedPlayer.devices.Count > 0 ? lastJoinedPlayer.devices[0] : null;

        //UI �۾�
        playerIndexes[playerIndex].characterProfile = null;
        playerIndexes[playerIndex].playerUIParent.SetActive(true); // Ui Ȱ��ȭ

        TestLocalDataManager.Instance.AddPlayer(playerIndex, null, device);
        InitUI(TestLocalDataManager.Instance.players.Count - 1);
        Debug.Log($"Player {playerIndex} joined with device: {device?.displayName}");
    }

    public void ChangeCharacter(int characterIndex, int playerIndex)
    {
        playerIndexes[playerIndex].characterProfile = penguinDatas[characterIndex];
        TestLocalDataManager.Instance.players[playerIndex].characterProfile = penguinDatas[characterIndex];
        InitUI(playerIndex);
    }

    private CharacterProfile GetDefaultCharacter()
    {
        return penguinDatas[0]; // �⺻ ĳ���� ����
    }

    public void StartGame()
    {
        SceneManager.LoadScene("99.TestScene");
    }
}
