using UnityEngine;

public class MapDataManager : MonoBehaviour
{
    public static MapDataManager instance;
    public EditorData loadedMapData;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
}
