using UnityEngine;
using System.Collections.Generic;
public class LevelObjectDataManager : MonoBehaviour
{
    public static LevelObjectDataManager Instance;

    public List<GameObject> level1Objs = new(); 
    public List<GameObject> level2Objs = new(); 
    public List<GameObject> level3Objs = new(); 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
