using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class StoryManager : MonoBehaviour
{
    public static StoryManager instance;
    public StoryStage nowStage;
    public int nowDialogIndex;

    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void ToIngame(int val)
    {
        if (val == 0) GameManager.instance.levelReference = nowStage.dialogs[nowDialogIndex].difficult1;
        else if (val == 1) GameManager.instance.levelReference = nowStage.dialogs[nowDialogIndex].difficult2;
        else if (val == 2) GameManager.instance.levelReference = nowStage.dialogs[nowDialogIndex].difficult3;
        else if (val == 3) GameManager.instance.levelReference = nowStage.dialogs[nowDialogIndex].difficult4;


        DontDestroyOnLoad(TransitionCanvas.instance.gameObject);
        TransitionCanvas.instance.OnOffTransition(true);
        TransitionCanvas.instance.InitText(nowStage.dialogs[nowDialogIndex].stageName, nowStage.dialogs[nowDialogIndex].composerName);
        StartCoroutine(Co_SceneLoad("Ingame"));
    }

    public void ToStory()
    {
        StartCoroutine(Co_SceneLoad("StoryNormalStage"));
    }

    IEnumerator Co_SceneLoad(string SceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}