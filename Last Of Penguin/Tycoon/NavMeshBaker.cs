using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation; // NavMeshSurface를 사용하기 위한 네임스페이스

public class NavMeshBaker : MonoBehaviour
{
    [SerializeField] private NavMeshSurface navMeshSurface;  // NavMeshSurface를 참조

    void Start()
    {
        StartCoroutine(BakeNavMeshAfterDelay(0.3f));  // 1초 딜레이 후 NavMesh 빌드
    }

    private IEnumerator BakeNavMeshAfterDelay(float delay)
    {
        // 주어진 시간(1초) 동안 대기
        yield return new WaitForSeconds(delay);

        // NavMesh 빌드
        navMeshSurface.BuildNavMesh();

        Debug.Log("NavMesh has been baked after " + delay + " seconds.");
    }
}
