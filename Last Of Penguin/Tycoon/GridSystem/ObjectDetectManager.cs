using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDetectManager : MonoBehaviour
{
    void Update()
    {
        if (FindAnyObjectByType<Ice>() != null) Destroy(FindAnyObjectByType<Ice>().gameObject);
        if (FindAnyObjectByType<Plate>() != null) Destroy(FindAnyObjectByType<Plate>().gameObject);
    }
}
