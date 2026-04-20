using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndingDialogue : MonoBehaviour
{
    public static EndingDialogue Instance;

    public GameObject dialogueUI;
    public TMP_Text dialogueText;
    public PropData currentProp;
    public DialogueData currentDialogue;
    public int currentDialogueIndex = 0;

    private bool isWritingText = false;
    public bool isDialogueActive = false;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(Co_StartRoutine());
    }

    IEnumerator Co_StartRoutine()
    {
        yield return new WaitForSeconds(1.7f);
        StartDialogue(GuageManager.Instance.GetEndingProp(GameManager.Instance.GetLanguageSetting()));
    }

    void Update()
    {
        if (!isDialogueActive) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (isWritingText)
            {
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                    typingCoroutine = null;
                }

                isWritingText = false;
                dialogueText.text = currentDialogue.dialogueText;

                
            }
            else
            {
                NextDialogue();
            }
        }
    }

    private void NextDialogue()
    {
        currentDialogueIndex++;

        if (currentProp != null && currentDialogueIndex < currentProp.dialogues.Length)
        {
            DialogueData dialogue = currentProp.dialogues[currentDialogueIndex];
            StartDialogue(dialogue);
        }
        else
        {
            isDialogueActive = false;
            dialogueUI.SetActive(false);
            SceneManager.LoadScene("Title");
        }
    }

    public void StartDialogue(PropData propData)
    {
        isDialogueActive = true;
        dialogueUI.SetActive(true);

        currentProp = propData;
        currentDialogueIndex = 0;

        DialogueData dialogue = propData.dialogues[currentDialogueIndex];
        StartDialogue(dialogue);
    }

    public void StartDialogue(DialogueData dialogue)
    {
        isDialogueActive = true;
        dialogueUI.SetActive(true);

        currentDialogue = dialogue;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogue.dialogueType == DialogueType.Normal)
        {
            typingCoroutine = StartCoroutine(Co_WriteText(dialogue.dialogueText));
        }
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
