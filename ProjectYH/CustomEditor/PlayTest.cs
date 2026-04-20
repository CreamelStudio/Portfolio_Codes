using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class PlayTest : MonoBehaviour
{
    public Transform spawnPosition;
    public static PlayTest instance;

    public GameObject playTestObj;

    public GameObject nullTurn;
    public GameObject defaultTurn;

    public LevelData playTestLevel;
    public NoteSpawner noteSpawner;

    public EventReference musicReference;
    public EventInstance musicInstance;
    public EventReference keybombRef;

    public float dsp;
    public float dsptime;
    public float startOffset;
    public bool isStart;
    public bool isPlay;
    public float noteSpeed;

    public int defaultNoteCount;
    public int doubleNoteCount;
    public int longNoteCount;
    public int longNoteEndCount;

    [SerializeField] private Volume volume;

    private int effectCount;
    private int effectLoadCount;

    [SerializeField] private Animator glassAnim;
    [SerializeField] private RawImage flashImage;
    private Camera mainCamera;

    private float tempSpeed;

    private UnityEngine.Rendering.Universal.Vignette vignette;
    private UnityEngine.Rendering.Universal.Bloom bloom;
    private UnityEngine.Rendering.Universal.PaniniProjection panini;
    private UnityEngine.Rendering.Universal.ColorAdjustments colorAdjustments;

    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        Disable();

        mainCamera = Camera.main;
    }

    public void Disable()
    {
        Debug.Log("PlayTest 비활성화");

        playTestLevel.defaultMinuteSpeed = tempSpeed;

        isPlay = false;
        isStart = false;

        noteSpawner.longNoteRenderer.Clear(); 

        // 음악 인스턴스 상태 체크
        if (musicInstance.isValid())
        {
            musicInstance.stop(STOP_MODE.IMMEDIATE);
            musicInstance.release();
        }

        musicInstance.clearHandle(); // 인스턴스 포인터 제거

        if (spawnPosition.GetComponentsInChildren<Transform>().Length != 0)
        {
            foreach (Transform obj in spawnPosition.GetComponentsInChildren<Transform>())
            {
                if (obj != spawnPosition) Destroy(obj.gameObject);
            }
        }

        playTestObj.SetActive(false);
    }

    public void Enable()
    {
        Debug.Log("PlayTest 활성화");

        PlayDataManager.instance.LoadComplete(playTestLevel);
        LoadEffect();

        musicReference.Guid = new FMOD.GUID(new System.Guid(playTestLevel.guidPath));
        PlayTest.instance.musicInstance = RuntimeManager.CreateInstance(musicReference);

        if (PlayerPrefs.GetFloat("NoteSpeed") == 0) PlayerPrefs.SetFloat("NoteSpeed", 1);
        noteSpeed = PlayerPrefs.GetFloat("NoteSpeed");

        tempSpeed = playTestLevel.defaultMinuteSpeed;

        playTestLevel.defaultMinuteSpeed *= noteSpeed;

        defaultNoteCount = 0;
        doubleNoteCount = 0;
        longNoteCount = 0;
        longNoteEndCount = 0;
        isPlay = false;
        isStart = false;

        dsp = 0;
        nullTurn.transform.rotation = new Quaternion(0, 0, 0, 0);
        defaultTurn.transform.rotation = new Quaternion(0, 0, 0, 0);

        startOffset = 180 / playTestLevel.defaultMinuteSpeed;

        isStart = true;
        playTestObj.SetActive(true);
    }

    public void LoadEffect()
    {
        if (volume.profile.TryGet(out vignette)) effectLoadCount++;
        if (volume.profile.TryGet(out bloom)) effectLoadCount++;
        if (volume.profile.TryGet(out colorAdjustments)) effectLoadCount++;
        if (volume.profile.TryGet(out panini)) effectLoadCount++;

        if (effectLoadCount == 4) Debug.Log("All Effect Loaded");
        else Debug.Log("All Effect Load Failed");
    }

    private void Update()
    {
        if (isStart)
        {
            dsp += Time.deltaTime;
            SpawnNote();
            nullTurn.transform.Rotate(0, 0, playTestLevel.defaultMinuteSpeed * Time.deltaTime);
        }
        if (dsp >= startOffset && !isPlay && musicInstance.isValid())
        {
            isPlay = true;
            Debug.Log(musicInstance.start());
        }



        if (isPlay)
        {
            defaultTurn.transform.Rotate(0, 0, playTestLevel.defaultMinuteSpeed * Time.deltaTime);

            EffectControll();
            
        }
        
    }

    private void SpawnNote()
    {
        musicInstance.getTimelinePosition(out int position);
        if (position / 1000 > startOffset) dsptime = ((float)position / 1000f) + startOffset;
        else dsptime = dsp;


        if (defaultNoteCount < PlayDataManager.instance.defaultNote.Count && dsptime - startOffset >= PlayDataManager.instance.defaultNote[defaultNoteCount].StartDelay)
        {
            RuntimeManager.PlayOneShot(keybombRef, spawnPosition.position);
        }

        if (doubleNoteCount < PlayDataManager.instance.doubleNote.Count && dsptime - startOffset >= PlayDataManager.instance.doubleNote[doubleNoteCount].StartDelay)
        {
            RuntimeManager.PlayOneShot(keybombRef, spawnPosition.position);
        }


        if (longNoteCount < PlayDataManager.instance.longNote.Count && dsptime - startOffset >= PlayDataManager.instance.longNote[longNoteCount].StartDelay)
        {
            RuntimeManager.PlayOneShot(keybombRef, spawnPosition.position);
        }

        if (defaultNoteCount < PlayDataManager.instance.defaultNote.Count && dsptime >= PlayDataManager.instance.defaultNote[defaultNoteCount].StartDelay)
        {
            Debug.Log("Spawn Default Note");
            GameObject note = noteSpawner.SpawnDefaultNote();
            note.transform.parent = spawnPosition;
            Destroy(note, startOffset);
            defaultNoteCount++;
        }

        if (doubleNoteCount < PlayDataManager.instance.doubleNote.Count && dsptime >= PlayDataManager.instance.doubleNote[doubleNoteCount].StartDelay)
        {
            Debug.Log("Spawn Double Note");
            GameObject note = noteSpawner.SpawnDoubleNote();
            note.transform.parent = spawnPosition;
            Destroy(note, startOffset);
            doubleNoteCount++;
        }


        if (longNoteCount < PlayDataManager.instance.longNote.Count && dsptime >= PlayDataManager.instance.longNote[longNoteCount].StartDelay)
        {
            Debug.Log("Spawn Long Note");
            GameObject note = noteSpawner.SpawnLongNote(startOffset);
            note.transform.parent = spawnPosition;
            Destroy(note, startOffset);
            longNoteCount++;
        }

        if (longNoteEndCount < PlayDataManager.instance.longNote.Count && longNoteEndCount < noteSpawner.longNoteRenderer.Count && longNoteCount > 0 && longNoteCount - 1 < PlayDataManager.instance.longNote.Count)
        {
            LongNote longNoteData = PlayDataManager.instance.longNote[longNoteEndCount];
            float startDelay = (float)longNoteData.StartDelay;
            float endDelay = longNoteData.EndDelay;

            if (dsptime >= startDelay + endDelay)
            {
                Debug.Log("Long Note End Rendering");
                GameObject note = noteSpawner.StopLongNoteRenderer(longNoteEndCount);
                note.transform.parent = spawnPosition;
                Destroy(note, startOffset);
                longNoteEndCount++;
            }
        }
    }

    private void EffectControll()
    {
         musicInstance.getTimelinePosition(out int position);
        float timeline = position / 1000f;

        if (effectCount >= PlayDataManager.instance.baseEffect.Count) return;
        var effect = PlayDataManager.instance.baseEffect[effectCount];

        if (effect.StartDelay <= timeline)
        {
            switch (effect.EffectType)
            {
                case EffectEnum.Vignetting:
                    {
                        if (effect is Vignetting v) DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, v.Intensity, v.Duration);
                        break;
                    }
                case EffectEnum.Bloom:
                    {
                        if (effect is Bloom b) DOTween.To(() => bloom.intensity.value, x => bloom.intensity.value = x, b.Intensity, b.Duration);
                        break;
                    }
                case EffectEnum.ColorAdjustments:
                    {
                        if (effect is ColorAdjustments c)
                        {
                            DOTween.To(() => colorAdjustments.postExposure.value, x => colorAdjustments.postExposure.value = x, c.PostExposure, c.Duration);
                            DOTween.To(() => colorAdjustments.contrast.value, x => colorAdjustments.contrast.value = x, c.Contrast, c.Duration);
                            DOTween.To(() => colorAdjustments.hueShift.value, x => colorAdjustments.hueShift.value = x, c.HueShift, c.Duration);
                            DOTween.To(() => colorAdjustments.saturation.value, x => colorAdjustments.saturation.value = x, c.Saturation, c.Duration);
                        }
                        break;
                    }
                case EffectEnum.PaniniProjections:
                    {
                        if (effect is PaniniProjection p) DOTween.To(() => panini.distance.value, x => panini.distance.value = x, p.Distance, p.Duration);
                        break;
                    }
                case EffectEnum.Flash:
                    {
                        if (effect is Flash f)
                        {
                            DOTween.To(() => 0, x => flashImage.color = new Color(1, 1, 1, x), 1, f.FlashDuration).OnComplete(() =>
                            {
                                DOTween.To(() => 1, x => flashImage.color = new Color(1, 1, 1, x), 0, f.OffDuration);
                            });
                        }
                        break;
                    }
                case EffectEnum.GlassBreak:
                    {
                        if (effect is GlassBreak g)
                        {
                            glassAnim.speed = g.Option;
                            AnimatorStateInfo state = glassAnim.GetCurrentAnimatorStateInfo(0);
                            if (state.IsName("GlassBreakEffect") && state.normalizedTime < 1f) glassAnim.Play("GlassBreakEffect", 0);
                            else glassAnim.Play("GlassBreakEffect", 0, (g.Option == -1) ? 1 : 0);
                        }
                        break;
                    }
                case EffectEnum.CameraMove:
                    {
                        if (effect is CameraMove c)
                        {
                            mainCamera.transform.DOMove(new Vector3(c.Position.x, c.Position.y, c.Position.z), c.Duration);
                            mainCamera.transform.DORotate(new Vector3(c.Rotation.x, c.Rotation.y, c.Rotation.z), c.Duration);
                        }
                        break;
                    }
            }
            effectCount++;
        }
    }
}
