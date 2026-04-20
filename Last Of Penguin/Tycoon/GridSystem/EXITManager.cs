using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EXITManager : MonoBehaviour
{
    public GameObject exitToMenu;
    public GameObject exitToFile;

    public void MenuEnable(bool v)
    {
        exitToMenu.SetActive(v);
    }

    public void FileEnable(bool v)
    {
        exitToFile.SetActive(v);
    }
}
