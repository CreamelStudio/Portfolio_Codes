using UnityEngine;

public class DragButton : MonoBehaviour
{
    public bool isFollowing = false;
    public int characterIndex;

    private void Update()
    {
        if(isFollowing) 
        {
            Vector3 mousePosition = Input.mousePosition; //마우스위 좌표값 받기
            mousePosition.z = 10f; //Z offset 설정
            transform.position = Camera.main.ScreenToWorldPoint(mousePosition); //위치 적용
        }

        if(Input.GetMouseButtonUp(0)) //마우스를 떼면
        {
            isFollowing = false; //드래그 중지
            EndDraw(); //드래그 종료
        }
    }

    public void EndDraw()
    {
        for(int i=0;i< TestSelectManager.instance.selectCheck.Length; i++) 
        {
            if (TestSelectManager.instance.selectCheck[i])
            {
                TestSelectManager.instance.ChangeCharacter(characterIndex, i); //선택된 플레이어 인덱스에 캐릭터 변경
                break;
            }
        }
        Destroy(this.gameObject); //드래그 오브젝트 삭제
    }
}
