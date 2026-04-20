using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChattingLogManager : MonoBehaviour
{
    [SerializeField] private GameObject chattingLog;
    
    [SerializeField] private Transform chatLogTrans;
    [SerializeField] private GameObject chatLogPrefab;

    [SerializeField] private ScrollRect scrollRect;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            chattingLog.SetActive(true);
            scrollRect.verticalNormalizedPosition = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            chattingLog.SetActive(false);
        }
    }

    public void AddChattingLog(string name, string desc)
    {
        GameObject obj = Instantiate(chatLogPrefab, chatLogTrans);
        TMP_Text[] logTexts = obj.GetComponentsInChildren<TMP_Text>();
        logTexts[0].text = name;
        logTexts[1].text = desc;
    }
}
