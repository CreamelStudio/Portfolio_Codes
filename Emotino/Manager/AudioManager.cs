using UnityEngine;
using System.Collections.Generic; //List
using FMOD.Studio;
using FMODUnity;

using STOP_MODE = FMOD.Studio.STOP_MODE;

public class LoopMusicData //반복 음악들의 정보를 저장해두는 클래스
{
    public EventReference musicRef;
    public EventInstance musicInstance;
    public bool isPlay;

    public LoopMusicData(EventReference musicRef, EventInstance musicInstance) //생성자
    {
        this.musicRef = musicRef;
        this.musicInstance = musicInstance;
    }
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance; //싱글톤 인스턴스
    private List<LoopMusicData> loopMusicDatas; //반복 음악 데이터 리스트

    private void Awake()
    {
        
        if (instance == null) //싱글톤 인스턴스가 없을 때
        {
            loopMusicDatas = new List<LoopMusicData>(); //반복 음악 데이터 리스트 초기화
            instance = this; //싱글톤 인스턴스 설정
            DontDestroyOnLoad(instance); //씬 전환 시에도 파괴되지 않도록 설정
        }
        else Destroy(this); //이미 인스턴스가 존재하면 현재 오브젝트 파괴
    }

    public EventInstance? PlayMusic(EventReference musicRef)
    {
        EventInstance musicInstance = RuntimeManager.CreateInstance(musicRef); //전달받은 음악 래퍼런스를 참조하여 음악 인스턴스 생성
        musicInstance.start(); //음악 시작

        return musicInstance; //음악 인스턴스 반환
    }

    public LoopMusicData PlayLoopMusic(EventReference musicRef) //반복 음악 재생 메소드
    {
        LoopMusicData musicData = new LoopMusicData(musicRef, RuntimeManager.CreateInstance(musicRef)); //새로운 LoopMusicData 생성
        loopMusicDatas.Add(musicData); //리스트에 추가
        return musicData; //생성된 LoopMusicData 반환
    }
    public void StopLoopMusic(LoopMusicData musicData)
    {
        musicData.musicInstance.stop(STOP_MODE.ALLOWFADEOUT); //음악 인스턴스 정지
        loopMusicDatas.Remove(musicData); //리스트에서 제거
    }

    private void FixedUpdate()
    {
        foreach(LoopMusicData data in loopMusicDatas) //반복 음악 데이터 리스트를 순회
        {
            data.musicInstance.getPlaybackState(out PLAYBACK_STATE state); //현재 음악 인스턴스의 재생 상태 가져오기
            if (state != PLAYBACK_STATE.PLAYING) data.musicInstance.start(); //재생 상태가 아니면 음악 인스턴스 재시작
        }
    }
}
