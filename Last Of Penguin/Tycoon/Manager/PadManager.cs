using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PadManager : MonoBehaviour
{
    public static PadManager instance;

    [Header("GamePad")]
    [SerializeField] private Gamepad pad;
    private IEnumerator stopCorutine;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        pad = Gamepad.current;
    }

    public void VivrationPad()
    {
        if (pad == null) return;

        pad.SetMotorSpeeds(0.6f, 0.6f);

        //stopCorutine = StartCoroutine(Co_StopPadVivration(1f));
        if(stopCorutine != null)
        {
            StopCoroutine(stopCorutine);
        }
        stopCorutine = Co_StopPadVivration(0.5f);
        StartCoroutine(stopCorutine);
    }

    private IEnumerator Co_StopPadVivration(float time)
    {
        yield return new WaitForSeconds(time);

        pad.SetMotorSpeeds(0f, 0f);
    }
}
