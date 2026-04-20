namespace Lop.Survivor
{
    using Lop.Survivor.Manager;
    using System.Collections;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    public class TimeManager : MonoBehaviour, ITickable
    {
        public static TimeManager Instance { get; set; }

        [Header("Times Datas")]
        [SerializeField] private TimeData[] timeDatas;

        [Header("Timers")]
        [SerializeField] private float globalTimer;
        [SerializeField] private float timer;
        [SerializeField] private float currentTimer;
        [SerializeField] private int currentDay;
        private float maxDayTime;

        [Space(3f)]
        [SerializeField] private TimeData currentTimeData;
        [SerializeField] private int currentTimeDataIndex;


        [Space(10f)]
        [Header("Colors")]
        [SerializeField] private Volume mainVolume;
        private ColorAdjustments mainColorAdjustments;
        [SerializeField] private ColorAdjData currentColorData;
        private ColorAdjData startColor = new ColorAdjData(Color.white, 0, 0, 0);
        private ColorAdjData targetColor = new ColorAdjData(Color.white, 0, 0, 0);

        [Space(10f)]
        [Header("Objects")]
        [SerializeField] private GameObject dayPanel;
        private Vector2 skyTextureOffset;
        [SerializeField] private GameObject skyDome;
        [SerializeField] private Light light;
        private Material skyMaterial;

        public int CurrentDay => currentDay;
        public TimeType CurrentTimeType => currentTimeData.timeType;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this.gameObject);
        }

        private void Start()
        {
            InitPostProcess();
            InitTimeManager();
        }

        #region Init Methods
        private void InitPostProcess()
        {
            if(mainVolume != null && mainVolume.profile != null)
            {
                if(mainVolume.profile.TryGet<ColorAdjustments>(out var ca))
                {
                    mainColorAdjustments = ca;
                }
                else
                {
                    Debug.LogError("포스트 프로세싱에 Color Adjustments가 없습니다!");
                }
            }
            else
            {
                Debug.LogError("Volume 또는 Volume Profile이 설정되지 않았습니다!");
            }
        }

        private void InitTimeManager()
        {
            currentDay = 1;
            if(timeDatas.Length == 0)
            {
                Debug.LogError("TimeData가 설정되지 않았습니다!");
                return;
            }

            currentTimeData = timeDatas[0];
            startColor = currentTimeData.colorAdjData;
            targetColor = timeDatas[1].colorAdjData;

            skyMaterial = skyDome.gameObject.GetComponent<MeshRenderer>().material;
            skyTextureOffset = new Vector2(currentTimeData.materialMinOffsetX, 0);

            TickManager.Instance.RegisterTickEvent(ActionType.Tick, OnTick);
        }

        private void RetargetingTimeData()
        {
            timer = 0f;
            currentTimer = 0f;

            int prevIndex = (currentTimeDataIndex - 1 + timeDatas.Length) % timeDatas.Length;
            int nextIndex = (currentTimeDataIndex + 1) % timeDatas.Length;

            if(currentTimeDataIndex == 0)
            {
                startColor = timeDatas[currentTimeDataIndex].colorAdjData;
                targetColor = timeDatas[nextIndex].colorAdjData;
            }
            else
            {
                startColor = timeDatas[prevIndex].colorAdjData;
                targetColor = timeDatas[currentTimeDataIndex].colorAdjData;
            }
        }
        #endregion

        #region DayControl Methods
        private IEnumerator NextDayCoroutine()
        {
            if (timer >= currentTimeData.durationTime)
            {
                currentTimeDataIndex++;

                if (currentTimeDataIndex > timeDatas.Length - 1)
                {
                    dayPanel.GetComponentInChildren<TextMeshProUGUI>().text = "DAY " + (currentDay + 1);
                    dayPanel.GetComponent<Animator>().SetTrigger("isTrigger");

                    DayFadeManager.Instance.Fade();

                    currentTimeDataIndex = 0;
                    globalTimer = 0;

                    currentDay++;
                    TickManager.Instance.TriggerDayInitialize();
                    TreeManager.Instance.SpawnTree();
                }

                currentTimeData = timeDatas[currentTimeDataIndex];
                RetargetingTimeData();
            }
            yield return null;
        }
        #endregion

        #region System Methods
        public void SyncTime(float syncElapsedTime)
        {
            int diffTime = Mathf.RoundToInt((syncElapsedTime - globalTimer)) - (currentDay - 1) * 480;
            //Debug.Log($"Sync Time OnTick 호출 {syncElapsedTime} {globalTimer} {diffTime}");
            

            if (syncElapsedTime != globalTimer)
            {
                if (diffTime > 0)
                {
                    for (int i = 0; i < diffTime; i++)
                    {
                        OnTick();
                    }
                }
            }
        }

        public void OnTick()
        {
            timer += 1.0f;
            currentTimer += 1.0f;
            globalTimer += 1.0f;

            StartCoroutine(NextDayCoroutine());
        }

        private void Update()
        {
            if(mainColorAdjustments == null)
            {
                Debug.LogError("Volume이 설정되지 않았습니다!");
            }

            float fraction = currentTimer / currentTimeData.durationTime;
            fraction = Mathf.Clamp01(fraction);

            currentColorData.colorFilter = Color.Lerp(startColor.colorFilter, targetColor.colorFilter, fraction);
            currentColorData.PostExposure = Mathf.Lerp(startColor.PostExposure, targetColor.PostExposure, fraction);
            currentColorData.Contrast = Mathf.RoundToInt(Mathf.Lerp(startColor.Contrast, targetColor.Contrast, fraction));
            currentColorData.Saturation = Mathf.RoundToInt(Mathf.Lerp(startColor.Saturation, targetColor.Saturation, fraction));

            mainColorAdjustments.colorFilter.value = currentColorData.colorFilter;
            mainColorAdjustments.postExposure.value = currentColorData.PostExposure;
            mainColorAdjustments.contrast.value = currentColorData.Contrast;
            mainColorAdjustments.saturation.value = currentColorData.Saturation;
        }

        private void OnDisable()
        {
            TickManager.Instance?.DestroyTickEvent(ActionType.Tick, OnTick);
        }
        #endregion
    }
}
