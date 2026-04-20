using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 캐릭터 인풋 생성 오브젝트 및 캐릭터 선택용도
public class LocalDevice : MonoBehaviour
{
    public int index;
    public bool canChange = true;
    public WaitForSeconds delay = new WaitForSeconds(0.1f);

    public virtual void OnChange(InputAction.CallbackContext context)
    {
        if (!canChange) return;

        canChange = false;
        //TestSelectManager.instance.ChangeCharacter(context.ReadValue<Vector2>().x);
        StartCoroutine(Co_Delay());
    }

    IEnumerator Co_Delay()
    {
        yield return delay;
        canChange = true;
    }
}
