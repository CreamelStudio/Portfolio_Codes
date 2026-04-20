using System;
using System.IO;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public static DebugManager instance;

    [SerializeField]private bool isDebug;

    [SerializeField]private bool isLog = true;
    [SerializeField] private bool isWarning = true;
    [SerializeField]private bool isError = true;

    private string fileName;
    private string filePath = Path.Combine(Application.dataPath, "Logs");


    public void Awake()
    {
        if (instance == null) { instance = this; }
        else Destroy(this.gameObject);

        if (!isDebug) Destroy(this.gameObject);
        DontDestroyOnLoad(this);
        // 오늘 날짜로 파일의 이름을 결정
        fileName = $"log-{System.DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss")}.txt";
    }


    private void OnEnable()
    {
        Application.logMessageReceived += LogToTxt;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= LogToTxt;
    }

    public void LogToTxt(string logString, string stackTrace, LogType type)
    {
        //에러 구분하여 특정 에러 필터링
        if (type == LogType.Log) 
        {
            if (!isLog)
                return;
        }
        else if (type == LogType.Warning)
        {
            if (!isWarning)
                return;
        }
        else if (type == LogType.Error)
        {
            if (!isError)
                return;
        }

        if (Directory.Exists(filePath) == false)
        {
            try
            {
                Directory.CreateDirectory(filePath);
                Debug.Log("폴더가 존재하지 않아 생성합니다");
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.LogError($"폴더 생성 권한이 없습니다 : {ex.Message}");
            }
        }

        using (StreamWriter sw = new StreamWriter(Path.Combine(filePath, fileName), true))
        {
            sw.WriteLine($"[{System.DateTime.Now}] {logString} \n");
        }
    }
}
