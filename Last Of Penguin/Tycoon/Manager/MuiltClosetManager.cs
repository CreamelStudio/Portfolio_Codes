using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuiltClosetManager : MonoBehaviour
{
    [Header("PenguinList")]
    [SerializeField] private List<CharacterProfile> penguinData;
    [SerializeField] private List<Transform> spawnTransform;

    private void Start()
    {

    }

    private void SpawnPenguin()
    {
        foreach (Transform t in spawnTransform)
        {
            for (int i = 0; i < penguinData.Count; i++)
            {
                //Instantiate(penguinData[i].uiObject, new Ve)
            }
        }
    }
}
