using FMODUnity;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Video;

public enum SkinLoadNumber
{
    Gear,
    Niddle
}

public enum NoteSkinLoadNumber
{
    Default,
    Double,
    Long
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private const string EmptyBgaPath = "NULL";
    private const string NoteSpeedKey = "NoteSpeed";
    private const string SoundOffsetKey = "OffsetValue";

    public float soundOffset;
    public float keyOffset;

    public SkinList noteDefaultList;
    public SkinList noteDoubleList;
    public SkinList noteLongList;
    public SkinList gearList;
    public SkinList niddleList;

    [Header("Play Mode Data")]
    public bool isEnding;
    public bool isPlaying;
    public bool isStart;
    public float dsptime;
    public AssetReference levelReference;
    public LevelData currentLevel;

    public float noteSpeed;

    public bool isMultiPlay;

    private void Awake()
    {
        noteSpeed = Mathf.Max(1f, PlayerPrefs.GetFloat(NoteSpeedKey, 1f));
        PlayerPrefs.SetFloat(NoteSpeedKey, noteSpeed);
        soundOffset = PlayerPrefs.GetFloat(SoundOffsetKey, 0f);

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Sprite LoadSkins(SkinLoadNumber val)
    {
        return val switch
        {
            SkinLoadNumber.Gear => GetSkinTexture(gearList, "gearSkin"),
            _ => GetSkinTexture(niddleList, "niddleSkin"),
        };
    }

    public GameObject LoadNoteObj(NoteSkinLoadNumber val)
    {
        return val switch
        {
            NoteSkinLoadNumber.Default => GetSkinPrefab(noteDefaultList, "noteDefaultSkin"),
            NoteSkinLoadNumber.Double => GetSkinPrefab(noteDoubleList, "noteDoubleSkin"),
            NoteSkinLoadNumber.Long => GetSkinPrefab(noteLongList, "noteLongSkin"),
            _ => null,
        };
    }

    public float ChangeSpeed(float speedValue)
    {
        noteSpeed += speedValue;
        if (noteSpeed <= 0f) noteSpeed -= speedValue;
        if (noteSpeed > 10000f) noteSpeed -= speedValue;
        noteSpeed = Mathf.Round(noteSpeed * 10f) / 10f;
        PlayerPrefs.SetFloat(NoteSpeedKey, noteSpeed);
        return noteSpeed;
    }

    public IEnumerator LoadJson()
    {
        // Addressable 로드 핸들 초기화 (nullable)
        AsyncOperationHandle<TextAsset>? levelHandle = null;

        // 이미 Addressable Asset이 연결되어 있으면 바로 사용한다.
        if (levelReference.Asset != null)
        {
            TextAsset loadedJson = levelReference.Asset as TextAsset;
            currentLevel = DeserializeLevelData(loadedJson.text);
        }
        else
        {
            // 연결된 Asset이 없으면 Addressable에서 비동기로 불러온다.
            levelHandle = levelReference.LoadAssetAsync<TextAsset>();
            yield return levelHandle;

            if (levelHandle.Value.Status == AsyncOperationStatus.Succeeded)
            {
                currentLevel = DeserializeLevelData(levelHandle.Value.Result.text);
            }
            else
            {
                Debug.LogError("레벨 JSON 로드에 실패했습니다.");
                yield break;
            }
        }

        if (levelHandle.HasValue)
        {
            Addressables.Release(levelHandle.Value);
        }

        bool hasBga = !string.IsNullOrWhiteSpace(currentLevel.bgaPath) && currentLevel.bgaPath != EmptyBgaPath;

        // BGA 경로가 있으면 비디오를 로드한다.
        if (hasBga)
        {
            AsyncOperationHandle<VideoClip> bgaHandle = Addressables.LoadAssetAsync<VideoClip>(currentLevel.bgaPath);
            yield return bgaHandle;

            if (bgaHandle.Status == AsyncOperationStatus.Succeeded)
            {
                NoteSpawnManager.instance.bgaVP.clip = bgaHandle.Result;
                NoteSpawnManager.instance.bgaVP.Prepare();
                NoteSpawnManager.instance.spaceImage.color = Color.black;

                yield return new WaitUntil(() => NoteSpawnManager.instance.bgaVP.isPrepared);
            }
            else
            {
                Debug.LogError("BGA 비디오 로드에 실패했습니다.");
            }
        }
        else
        {
            Debug.Log("BGA가 설정되어 있지 않습니다.");
            NoteSpawnManager.instance.bgaImage.gameObject.SetActive(false);
            NoteSpawnManager.instance.spaceImage.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        }

        // 유저 설정 노트 속도를 레벨 기본 속도에 반영한다.
        currentLevel.defaultMinuteSpeed *= noteSpeed;

        // FMOD 음악 인스턴스를 준비한다.
        NoteSpawnManager.instance.musicRef.Guid = new FMOD.GUID(new System.Guid(currentLevel.guidPath));
        NoteSpawnManager.instance.musicInstance = RuntimeManager.CreateInstance(NoteSpawnManager.instance.musicRef);

        EffectController.instance.LoadEffect();
        PlayDataManager.instance.LoadComplete(currentLevel);
    }

    public void ResetManager()
    {
        isEnding = false;
        isPlaying = false;
        isStart = false;

        dsptime = 0;
        currentLevel = new LevelData();
    }

    private static LevelData DeserializeLevelData(string json)
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
        };

        return JsonConvert.DeserializeObject<LevelData>(json, settings);
    }

    private static Sprite GetSkinTexture(SkinList list, string prefKey)
    {
        int index = GetSkinIndex(list, prefKey);
        return index >= 0 ? list.skin[index].skinTexture : null;
    }

    private static GameObject GetSkinPrefab(SkinList list, string prefKey)
    {
        int index = GetSkinIndex(list, prefKey);
        return index >= 0 ? list.skin[index].skinPrefab : null;
    }

    private static int GetSkinIndex(SkinList list, string prefKey)
    {
        if (list == null || list.skin == null || list.skin.Length == 0)
        {
            Debug.LogWarning($"스킨 목록이 비어 있습니다. key={prefKey}");
            return -1;
        }

        return Mathf.Clamp(PlayerPrefs.GetInt(prefKey, 0), 0, list.skin.Length - 1);
    }
}
