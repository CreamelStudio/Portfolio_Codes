using System.Collections.Generic;
using UnityEngine;

public class JudgeRotation : MonoBehaviour
{
    [SerializeField] private GameObject splinklePrefab;
    public int numberOfSplinkles = 1;
    private List<GameObject> splinkles;

    public void InitSplinkle(int count)
    {
        numberOfSplinkles = count;
        splinkles = new List<GameObject>();
        for (int i = 0; i < numberOfSplinkles; i++)
        {
            splinkles.Add(Instantiate(splinklePrefab, transform));
            splinkles[splinkles.Count - 1].transform.SetSiblingIndex(0);
            splinkles[splinkles.Count - 1].transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        }
    }
}
