using UnityEngine;
using FMODUnity;
using FMOD.Studio;
public class TestFmod : MonoBehaviour
{
    public EventReference testMusicRef;
    public EventInstance testMusicInstance;

    private void Start()
    {
        testMusicInstance = RuntimeManager.CreateInstance(testMusicRef);
        //-> Play the music

        testMusicInstance.start();

        testMusicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        testMusicInstance.getTimelinePosition(out int position); //DSP clock in ms

        testMusicInstance.setTimelinePosition(5000); //Set position to 5 seconds
        //5√ 

        Debug.Log("Current position: " + position + " ms");

        testMusicInstance.release();
    }
}
