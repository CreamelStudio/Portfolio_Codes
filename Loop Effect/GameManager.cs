using UnityEngine;
using UnityEngine.UI;

public enum Language
{
    English,
    Korean
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Language language;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Language GetLanguageSetting()
    {
        return language;
    }
}
