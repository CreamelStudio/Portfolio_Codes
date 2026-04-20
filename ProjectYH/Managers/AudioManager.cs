using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Debug.LogError("AudioManager가 두 개 이상 존재합니다.");
    }

    public EventInstance PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        EventInstance tempInstance = RuntimeManager.CreateInstance(sound);
        tempInstance.set3DAttributes(RuntimeUtils.To3DAttributes(worldPos));
        tempInstance.start();
        return tempInstance;
    }
}
