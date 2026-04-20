using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YetiSpawner : MonoBehaviour
{
    [SerializeField] GameObject yettiPrefab;
    // Start is called before the first frame update
    void Start()
    {
        Instantiate(yettiPrefab,transform.position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
