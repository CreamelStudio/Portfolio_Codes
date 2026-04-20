using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class StoryDisplay : MonoBehaviour
{
    [SerializeField] private Dialog nowDialog; //현재 대화 변수

    [Space(10f)]
    [SerializeField] private GameObject miraePrefab; //캐릭터 미레 프리팹
    [SerializeField] private GameObject neaPrefab; //캐릭터 네아 프리팹
    [SerializeField] private GameObject lumiaPrefab; //캐릭터 루미아 프리팹

    [Space(10f)]
    [SerializeField] private Transform characterSpawnPos;

    [Space(10f)]
    [SerializeField] private CanvasGroup chatCanvas; //채팅 캔버스
    [SerializeField] private TMP_Text nameText; //이름 텍스트
    [SerializeField] private TMP_Text contentText; //대화 내용 텍스트
    [SerializeField] private RawImage backgroundImage;

    [Space(10f)]
    [SerializeField] private int textCount; // 텍스트 전체 글자 수
    [SerializeField] private IEnumerator typingCo; // 타이핑 코루틴
    
    [Space(10f)]
    [SerializeField] private bool isChatting;

    [Space(10f)]
    [SerializeField] private ChattingLogManager chatLogManager;

    private void Start()
    {
        DisplayText(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) {

            if (isChatting)
            {
                StopCoroutine(typingCo);
                contentText.text = nowDialog.content;
                isChatting = false;
                return;
            }

            StoryManager.instance.nowDialogIndex++;
            DisplayText(StoryManager.instance.nowDialogIndex);
        }
    }

    public void DisplayText(int index) // 대화 텍스트 출력
    {
        if (index >= StoryManager.instance.nowStage.dialogs.Length) SceneManager.LoadScene("StorySelect");
        nowDialog = StoryManager.instance.nowStage.dialogs[StoryManager.instance.nowDialogIndex];
        SpawnCharacter(true);
        if (nowDialog.backgroundImage)
        {
            backgroundImage.DOFade(0, 0.15f).OnComplete(() =>
            {
                backgroundImage.texture = nowDialog.backgroundImage;
                backgroundImage.DOFade(1, 0.15f);
            });
        }

        if (!nowDialog.isNotChat)
        {
            chatCanvas.DOFade(1, 0.35f); // 채팅창 보이기
            chatCanvas.transform.DOLocalMoveY(-270, 0.3f); // 위치 조정
        }
        else
        {
            chatCanvas.DOFade(0, 0.35f); // 채팅창 숨기기
            chatCanvas.transform.DOLocalMoveY(-700, 0.3f); // 위치 조정
        }

        nameText.text = nowDialog.Speaker; // 화자 이름 설정
        textCount = nowDialog.content.Length; // 텍스트 총 글자 수 설정

        contentText.text = ""; // 기존 텍스트 초기화

        if(typingCo != null && !nowDialog.isNotChat) StopCoroutine(typingCo); // 기존 코루틴 중지
        if(!nowDialog.isNotChat) typingCo = Co_TypingContent(nowDialog.content, nowDialog.textOffset); // 새 코루틴 설정 (텍스트, 타이핑 속도)
        if(!nowDialog.isNotChat)StartCoroutine(typingCo); // 코루틴 시작

        chatLogManager.AddChattingLog(nowDialog.Speaker, nowDialog.content);
    }

    IEnumerator Co_TypingContent(string text, float offset)
    {
        isChatting = true;
        yield return new WaitForSeconds(offset); // 타이핑 시작 전 대기 시간

        for(int i = 0; i < textCount + 2; i++)
        {
            contentText.text = text.Substring(0, i);
            yield return new WaitForSeconds(offset);
        }
        isChatting = false;
    }

    public void SpawnCharacter(bool isClear)
    {
        if (isClear) ResetCharacter();
        int characterCount = 0;
        GameObject obj;

        foreach (CharacterViewManager character in nowDialog.characters)
        {
            if(character.positionX == 0 && character.positionY == 0)
            {
                characterCount++;
            }
        }

        if (characterCount == nowDialog.characters.Length)
        {
            float spacing = 20f / (characterCount + 1); // 끝에 안 붙게 하려면 +1
            float startX = -10f;

            for (int i = 0; i < nowDialog.characters.Length; i++)
            {
                if (nowDialog.characters[i].character == CharacterEnum.None)
                {
                    Debug.LogError($"캐릭터 Enum이 선택되지 않았습니다 {StoryManager.instance.name}");
                    break;
                }

                if (nowDialog.characters[i].character == CharacterEnum.루미아) obj = Instantiate(lumiaPrefab, characterSpawnPos);
                else if (nowDialog.characters[i].character == CharacterEnum.미래) obj = Instantiate(miraePrefab, characterSpawnPos);
                else obj = Instantiate(neaPrefab, characterSpawnPos);

                Vector3 pos = obj.transform.position;
                pos.x = startX + spacing * (i + 1);
                obj.transform.position = pos;

                if (nowDialog.characters[i].charaterImage) obj.GetComponent<SpriteRenderer>().sprite = nowDialog.characters[i].charaterImage;
            }
        }
        else
        {
            foreach (CharacterViewManager character in nowDialog.characters)
            {
                if (character.character == CharacterEnum.None)
                {
                    Debug.LogError($"캐릭터 Enum이 선택되지 않았습니다 {StoryManager.instance.name}");
                    break;
                }

                if(character.character == CharacterEnum.루미아) obj = Instantiate(lumiaPrefab, characterSpawnPos);
                else if (character.character == CharacterEnum.미래) obj = Instantiate(miraePrefab, characterSpawnPos);
                else obj = Instantiate(neaPrefab, characterSpawnPos);

                if(character.charaterImage) obj.GetComponent<SpriteRenderer>().sprite = character.charaterImage;

            }
        }
        
    }
    public void ResetCharacter()
    {
        for(int i=0;i<characterSpawnPos.childCount; i++)
        {
            Destroy(characterSpawnPos.GetChild(i).gameObject);
        }
    }
}
