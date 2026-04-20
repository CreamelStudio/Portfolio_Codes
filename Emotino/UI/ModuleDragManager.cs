using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleDragManager : MonoBehaviour
{
    public LayerMask moduleLayer;

    void Update()
    {
        //마우스 클릭시
        if (Input.GetMouseButtonDown(0))
        {
            //마우스 클릭한 좌표값 가져오기
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //해당 좌표에 있는 오브젝트 찾기
            RaycastHit2D[] hits = Physics2D.RaycastAll(pos, Vector2.zero, 0f);

            foreach(RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.transform.gameObject.CompareTag(Tags.Module))
                {
                    GameObject click_obj = hit.transform.gameObject;
                    Debug.Log(click_obj.name);
                    Draggable dragObj = click_obj.GetComponent<Draggable>();
                    if (dragObj != null) dragObj.OnEnableDrag();
                }
            }
        }

        //마우스 클릭시
        if (Input.GetMouseButtonDown(1))
        {
            //마우스 클릭한 좌표값 가져오기
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //해당 좌표에 있는 오브젝트 찾기
            RaycastHit2D[] hits = Physics2D.RaycastAll(pos, Vector2.zero, 0f);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.transform.gameObject.CompareTag(Tags.Module))
                {
                    GameObject click_obj = hit.transform.gameObject;
                    Debug.Log(click_obj.name);
                    Draggable dragObj = click_obj.GetComponent<Draggable>();
                    if (dragObj != null) dragObj.InitPosition();
                }
            }
        }
    }
}
