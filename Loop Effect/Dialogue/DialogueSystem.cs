using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Button 컴포넌트 접근용
using DG.Tweening;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance;

    public GameObject dialogueUI;
    public TMP_Text characterNameText;
    public TMP_Text dialogueText;
    public GameObject[] optionButtons;

    public PropData currentProp;
    public DialogueData currentDialogue;
    public int currentDialogueIndex = 0;

    private bool isWritingText = false;
    public bool isDialogueActive = false;
    private Coroutine typingCoroutine;

    private bool isRandomOneShot = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!isDialogueActive) return;
        if (currentDialogue == null) return;

        // Branch 타입일 때는 Enter 키 입력을 막고, 오직 버튼 클릭으로만 진행
        if (currentDialogue.dialogueType == DialogueType.Branch) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (isWritingText)
            {
                SkipTyping();
            }
            else
            {
                if (isRandomOneShot) EndRandomDialogue();
                else NextDialogue();
            }
        }
    }

    private void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isWritingText = false;
        dialogueText.text = currentDialogue.dialogueText;
    }

    private void EndRandomDialogue()
    {
        // 게이지 업데이트 로직 유지
        GuageManager.Instance.AddGuageValue(GuageType.Evironment, currentDialogue.evironmentGuageChange);
        GuageManager.Instance.AddGuageValue(GuageType.Economy, currentDialogue.economyGuageChange);
        GuageManager.Instance.AddGuageValue(GuageType.Happiness, currentDialogue.happinessGuageChange);
        GuageManager.Instance.AddGuageValue(GuageType.Security, currentDialogue.securityGaugeChange);
        GuageManager.Instance.UpdateGuageUI();

        isDialogueActive = false;
        dialogueUI.SetActive(false);
        isRandomOneShot = false;
    }

    private void NextDialogue()
    {
        currentDialogueIndex++;

        // 게이지 업데이트 로직 유지
        GuageManager.Instance.AddGuageValue(GuageType.Evironment, currentDialogue.evironmentGuageChange);
        GuageManager.Instance.AddGuageValue(GuageType.Economy, currentDialogue.economyGuageChange);
        GuageManager.Instance.AddGuageValue(GuageType.Happiness, currentDialogue.happinessGuageChange);
        GuageManager.Instance.AddGuageValue(GuageType.Security, currentDialogue.securityGaugeChange);
        GuageManager.Instance.UpdateGuageUI();

        if (currentProp != null && currentDialogueIndex < currentProp.dialogues.Length)
        {
            DialogueData dialogue = currentProp.dialogues[currentDialogueIndex];
            StartDialogue(dialogue);
        }
        else
        {
            isDialogueActive = false;
            dialogueUI.SetActive(false);
        }
    }

    public void StartDialogue(PropData propData)
    {
        isDialogueActive = true;
        dialogueUI.SetActive(true);
        currentProp = propData;
        currentDialogueIndex = 0;
        StartDialogue(propData.dialogues[currentDialogueIndex]);
    }

    public void StartDialogue(DialogueData dialogue)
    {
        isRandomOneShot = false;

        // 1. 기존 UI 초기화
        ResetUI();

        // 2. 랜덤 다이얼로그 처리
        if (dialogue.dialogueType == DialogueType.Random)
        {
            HandleRandomDialogue(dialogue);
            return;
        }

        // 3. 일반 다이얼로그 처리
        SetupDialogueUI(dialogue);

        if (dialogue.dialogueType == DialogueType.Normal)
        {
            typingCoroutine = StartCoroutine(Co_WriteText(dialogue.dialogueText));
        }
        else if (dialogue.dialogueType == DialogueType.Branch)
        {
            typingCoroutine = StartCoroutine(Co_WriteText(dialogue.dialogueText));
            SetupBranchButtons(dialogue);
        }
    }

    private void ResetUI()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        foreach (var btn in optionButtons)
        {
            // 1. 버튼 기능 초기화
            var buttonComp = btn.GetComponent<UnityEngine.UI.Button>();
            buttonComp.onClick.RemoveAllListeners();
            buttonComp.interactable = true; // 다시 클릭 가능하게 복구

            // 2. DOTween 애니메이션 중단 (진행 중인 연출 멈춤)
            // transform과 Image에 걸린 트윈을 모두 kill
            btn.transform.DOKill();
            var img = btn.GetComponent<Image>();
            img.DOKill();

            // 3. 상태 원상복구 (초기값으로 리셋)
            var rect = btn.GetComponent<RectTransform>();
            rect.localScale = Vector3.one; // 크기 복구 (1,1,1)
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y); // Y축 위치가 바뀌었다면 여기서 초기 Y값으로 되돌려야 함. 
                                                                                                   // (DialogueButtonEffect에서 originalPos를 쓰므로, 여기서는 스케일과 알파값 위주로 복구하면 됩니다)

            img.color = new Color(img.color.r, img.color.g, img.color.b, 1); // 투명도 복구
            if (img.material != null)
            {
                img.material.SetFloat("_DissolveAmount", 0); // 쉐이더 값 복구 (모래 효과 리셋)
            }

            // 아웃라인 끄기
            var outline = btn.GetComponent<Outline>();
            if (outline != null) outline.enabled = false;

            // 4. 비활성화
            btn.SetActive(false);
        }
    }

    private void HandleRandomDialogue(DialogueData dialogue)
    {
        if (currentProp == null || currentProp.dialogues == null || currentProp.dialogues.Length <= 1) return;

        int randomIndex = Random.Range(1, currentProp.dialogues.Length);
        DialogueData randomDialogue = currentProp.dialogues[randomIndex];

        isRandomOneShot = true;
        isDialogueActive = true;
        dialogueUI.SetActive(true);

        currentDialogue = randomDialogue;
        characterNameText.text = randomDialogue.characterName;
        typingCoroutine = StartCoroutine(Co_WriteText(randomDialogue.dialogueText));
    }

    private void SetupDialogueUI(DialogueData dialogue)
    {
        isDialogueActive = true;
        dialogueUI.SetActive(true);
        currentDialogue = dialogue;
        characterNameText.text = dialogue.characterName;
    }

    private void SetupBranchButtons(DialogueData dialogue)
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < dialogue.branches.Length)
            {
                GameObject btnObj = optionButtons[i];

                // 텍스트 설정
                TMP_Text optionText = btnObj.GetComponentInChildren<TMP_Text>();
                optionText.text = dialogue.branches[i].optionText;

                // 클릭 이벤트 설정
                int index = i;
                Button button = btnObj.GetComponent<Button>();
                button.onClick.AddListener(() => StartCoroutine(Co_SelectBranch(index, dialogue.branches[index].nextDialogue)));

                // 버튼 연출 스크립트 가져오기 (없으면 추가)
                var effect = btnObj.GetComponent<DialogueButtonEffect>();
                if (effect == null) effect = btnObj.AddComponent<DialogueButtonEffect>();

                // 등장 연출 실행 (순차적으로 약간의 딜레이를 주면 더 멋짐)
                effect.PlayOpenAnimation(i * 0.1f);
            }
            else
            {
                optionButtons[i].SetActive(false);
            }
        }
    }

    // 버튼 선택 시 연출을 기다려주는 코루틴
    private IEnumerator Co_SelectBranch(int selectedIndex, DialogueData nextDialogue)
    {
        // 1. 모든 버튼 클릭 방지
        foreach (var btn in optionButtons)
        {
            btn.GetComponent<Button>().interactable = false;
        }

        // 2. 연출 재생
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (!optionButtons[i].activeSelf) continue;

            var effect = optionButtons[i].GetComponent<DialogueButtonEffect>();

            if (i == selectedIndex)
            {
                // 선택된 버튼: 클릭 효과 -> TV 꺼짐 효과
                effect.PlayClickAnimation();
                // 클릭 진동을 잠시 느낄 시간
                yield return new WaitForSeconds(0.2f);
                effect.PlaySelectedCloseAnimation();
            }
            else
            {
                // 선택 안 된 버튼: 모래처럼 부서짐
                effect.PlayDiscardCloseAnimation();
            }
        }

        // 3. 연출이 끝날 때까지 대기 (약 0.6~0.8초)
        yield return new WaitForSeconds(0.7f);

        // 4. 다음 다이얼로그 시작
        StartDialogue(nextDialogue);
    }

    public IEnumerator Co_WriteText(string fullText)
    {
        isWritingText = true;
        dialogueText.text = "";

        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.02f);
        }

        isWritingText = false;
        typingCoroutine = null;
    }
}