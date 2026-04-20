using System.Collections.Generic;
using UnityEngine;

public class PlateSpawn : MonoBehaviour
{
    [SerializeField] private GameObject platePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float forceStrength = 5f;
    [SerializeField] private Vector3 boxSize = new Vector3(2f, 1f, 2f);
    [SerializeField] private string plateTag = "Plate";

    private HashSet<Collider> previousPlates = new HashSet<Collider>();

    void Start()
    {
        SpawnPlate();
    }

    void Update()
    {
        // CheckForExit(); // 선택적으로 유지 가능
    }

    // Plate가 사라졌을 때 직접 호출되는 메서드
    public void OnPlateDestroyed(Plate plate)
    {
        if (!plate.isSpawnProcessed)
        {
            plate.isSpawnProcessed = true;
            SpawnPlate();
        }
    }

    public void SpawnPlate()
    {
        GameObject plate = Instantiate(platePrefab, spawnPoint.position, spawnPoint.rotation);

        Plate plateScript = plate.GetComponent<Plate>();
        if (plateScript != null)
        {
            plateScript.plateSpawner = this;
        }

        Rigidbody plateRigidbody = plate.GetComponent<Rigidbody>();
        if (plateRigidbody != null)
        {
            Vector3 forceDirection = spawnPoint.forward;
            plateRigidbody.AddForce(forceDirection * forceStrength, ForceMode.Impulse);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
}
