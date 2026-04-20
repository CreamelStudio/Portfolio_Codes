using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Coffee.UIEffects;
using Unity.VisualScripting;
using DG.Tweening;
using UnityEngine.SceneManagement;
public class EditorUIManager : MonoBehaviour
{
    public BlockInfoList prefabInfos;
    public GameObject buttonPrefab;
    public Transform buttonSpawnPos;
    public RectTransform contentPosition;

    public List<UIEffect> buttonObjs;

    public float keycodeUpTime = 0;
    public float keycodeDownTime = 0;
    public float keycodeInputUpTime = 0;
    public float keycodeInputDownTime = 0;

    public float keycodeDelay = 0.2f; //Ű���� ���� �̵� ������
    public float keycodeInputDelay = 0.4f; //Ű���� ���� �Է� ������

    public TMP_InputField mapName;
    public GameObject savePanel;

    private void Start()
    {
        ButtonsInit(); //��ư �ʱ�ȭ
        OnClickButton(0); //�⺻ ��ư
    }

    private void Update()
    {
        UIKeyInput();
    }

    public void ButtonUIUp()
    {
        int tempBlockID = EditorStructManager.instance.currentBlockID - 1;
        tempBlockID = Mathf.Clamp(tempBlockID, 0, prefabInfos.blockInfos.Length - 1);
        contentPosition.DOAnchorPosY((256 * 0.7f) * tempBlockID, 0.1f);
        OnClickButton(tempBlockID);
        keycodeUpTime += Time.deltaTime; //Ű���� �Է� �ð� ����
    }
    public void ButtonUIDown()
    {
        int tempBlockID = EditorStructManager.instance.currentBlockID + 1;
        tempBlockID = Mathf.Clamp(tempBlockID, 0, prefabInfos.blockInfos.Length - 1); //���� ����
        contentPosition.DOAnchorPosY((256 * 0.7f) * tempBlockID, 0.1f); //��ư�� �°� ��ũ�� ��ġ ����
        OnClickButton(tempBlockID); //��ư ����
        keycodeDownTime += Time.deltaTime; //Ű���� �Է� �ð� ����
    }

    public void UIKeyInput()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ButtonUIDown(); //��ư �Ʒ��� �̵�
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ButtonUIUp(); //��ư ���� �̵�
        }

        if (keycodeUpTime >= keycodeInputDelay)
        {
            keycodeInputUpTime += Time.deltaTime;
            if (keycodeInputUpTime >= keycodeDelay)
            {
                keycodeInputUpTime = 0; //�Է� ������ �ʱ�ȭ
                ButtonUIUp();
            }
        }
        if (keycodeDownTime >= keycodeInputDelay)
        {
            keycodeInputDownTime += Time.deltaTime;
            if (keycodeInputDownTime >= keycodeDelay)
            {
                keycodeInputDownTime = 0; //�Է� ������ �ʱ�ȭ
                ButtonUIDown();
            }
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            keycodeDownTime += Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            keycodeUpTime += Time.deltaTime;
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            keycodeDownTime = 0;
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            keycodeUpTime = 0;
        }
    }

    public void ButtonsInit()
    {
        for (int i = 0; i < prefabInfos.blockInfos.Length; i++)
        {
            GameObject obj = Instantiate(buttonPrefab, buttonSpawnPos);

            obj.GetComponentInChildren<RawImage>().texture = prefabInfos.blockInfos[i].blockImage;
            obj.GetComponentsInChildren<TMP_Text>()[0].text = prefabInfos.blockInfos[i].blockName;
            obj.GetComponentsInChildren<TMP_Text>()[1].text = prefabInfos.blockInfos[i].blockDesc;

            int val = i; //Capture;
            obj.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnClickButton(val);
            });

            buttonObjs.Add(obj.GetComponent<UIEffect>());
        }
    }

    public void OnEnableSavePanel(bool isEnable)
    {
        savePanel.SetActive(isEnable);
    }

    public void OnClickButton(int val)
    {
        EditorStructManager.instance.currentBlockID = val;
        foreach(UIEffect effect in buttonObjs)
        {
            effect.color = new Color(1, 1, 1);
        }

        buttonObjs[val].color = new Color(0.7f, 0.7f, 0.7f);
    }

    public void OnSave()
    {
        EditorDataManager.instance.SaveEditData(mapName.text);
        mapName.text = "";
    }

    public void GoHome()
    {
        SceneManager.LoadScene(Scenes.title);
    }
}
