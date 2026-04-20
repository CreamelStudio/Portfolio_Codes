using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceRefrigerator : MonoBehaviour
{
    [SerializeField] private GameObject icePrefab; // 아이스 프리팹
    [SerializeField] private float spawnInterval = 2f; // 생성 간격
    [SerializeField] private Transform spawnPoint;
    private void Start()
    {
        StartCoroutine(SpawnIce());
    }

    private IEnumerator SpawnIce()
    {
        while (true)
        {
            Instantiate(icePrefab, spawnPoint.position, Quaternion.Euler(0, 180, 0));
            yield return new WaitForSeconds(spawnInterval); // 대기
        }
    }
}
