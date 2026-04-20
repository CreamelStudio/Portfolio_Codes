using UnityEngine;

public class PartBase : MonoBehaviour
{
    // normalObject는 이제 코드로 끄지 않을 거니까 연결만 잘 되어 있으면 돼요 (삭제해도 됨)
    public GameObject highlightObject; 

    public float detectRange = 1f;

    void Start()
    {
        // 시작할 때 하이라이트는 꺼두기
        if (highlightObject != null) highlightObject.SetActive(false);
    }

    void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        float dist = Vector2.Distance(transform.position, player.transform.position);

        bool isNear = dist <= detectRange;

        // 가까이 가면 하이라이트만 켰다 껐다 함 (노멀은 안 건드림!)
        if (highlightObject != null) highlightObject.SetActive(isNear);

        // F키 누르면 '부모 오브젝트'가 꺼지니까, 자식인 노멀도 그때 같이 사라짐
        if (isNear && Input.GetKeyDown(KeyCode.F))
        {
            PartManager.Instance.AddPart();
            gameObject.SetActive(false); 
        }
    }
}