using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public enum GuageType
{
    Evironment,
    Economy,
    Happiness,
    Security
}

public class GuageManager : MonoBehaviour
{
    public static GuageManager Instance;

    public float evironmentValue;
    public float economyValue;
    public float happinessValue;
    public float securityValue;

    public GameObject canvasGuageUI;

    public Slider evironmentSlider;
    public Slider economySlider;
    public Slider happinessSlider;
    public Slider securitySlider;

    public Transform player;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            DontDestroyOnLoad(canvasGuageUI);
        }
        else
        {
            Destroy(canvasGuageUI);
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateGuageUI();
    }

    public void UpdateGuageUI()
    {
        DOTween.To(() => evironmentSlider.value, x => evironmentSlider.value = x, evironmentValue / 100f, 0.5f);
        DOTween.To(() => economySlider.value, x => economySlider.value = x, economyValue / 100f, 0.5f);
        DOTween.To(() => happinessSlider.value, x => happinessSlider.value = x, happinessValue / 100f, 0.5f);
        DOTween.To(() => securitySlider.value, x => securitySlider.value = x, securityValue / 100f, 0.5f);

        Debug.Log("Gauge UI Updated");
    }

    void FixedUpdate()
    {
        if(SceneManager.GetActiveScene().name == "Title")
        {
            canvasGuageUI.SetActive(false);
        }
        else
        {
            canvasGuageUI.SetActive(true);
            UpdateGuageUI();
        }
    }

    public void AddGuageValue(GuageType type, float value)
    {
        switch (type)
        {
            case GuageType.Evironment:
                evironmentValue += value;
                FloatingPairSpawner.Instance.SpawnFloatingPair(0, (int)value, player);
                break;
            case GuageType.Economy:
                economyValue += value;
                FloatingPairSpawner.Instance.SpawnFloatingPair(1, (int)value, player);
                break;
            case GuageType.Happiness:
                happinessValue += value;
                FloatingPairSpawner.Instance.SpawnFloatingPair(2, (int)value, player);
                break;
            case GuageType.Security:
                securityValue += value;
                FloatingPairSpawner.Instance.SpawnFloatingPair(3, (int)value, player);
                break;
        }

        Debug.Log("" + type.ToString() + " Gauge Updated: " + value);
    }

    public void SetGuageValue(GuageType type, float value)
    {
        switch (type)
        {
            case GuageType.Evironment:
                evironmentValue = value;
                break;
            case GuageType.Economy:
                economyValue = value;
                break;
            case GuageType.Happiness:
                happinessValue = value;
                break;
            case GuageType.Security:
                securityValue = value;
                break;
        }
        Debug.Log("" + type.ToString() + " Gauge Set: " + value);
    }

    // ------------------ 여기부터 엔딩 로직 ------------------

    private bool InRange(float value, float min, float max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// [핵심] 런타임에 엔딩 PropData 생성
    /// </summary>
    private PropData CreateEndingProp(string id, string title, string[] lines)
    {
        // ScriptableObject 인스턴스 메모리에 생성
        PropData prop = ScriptableObject.CreateInstance<PropData>();
        prop.propID = id;

        // 대사 배열 초기화
        prop.dialogues = new DialogueData[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            DialogueData data = new DialogueData();

            // 엔딩용 기본 설정
            data.dialogueType = DialogueType.Normal;
            data.characterName = title;              // 화자 이름에 엔딩 제목 사용
            data.dialogueText = lines[i];            // 대사 내용

            // 엔딩이므로 스탯 변화 없음
            data.actionGaugeReductionRate = 0f;
            data.evironmentGuageChange = 0f;
            data.economyGuageChange = 0f;
            data.happinessGuageChange = 0f;
            data.securityGaugeChange = 0f;
            data.branches = null;

            prop.dialogues[i] = data;
        }

        return prop;
    }

    /// <summary>
    /// EndingDataManager(싱글톤)에서 텍스트를 가져와 PropData로 변환하는 중간 다리 함수
    /// </summary>
    private PropData GetEndingData(string endingID, Language language)
    {
        // 1. 싱글톤에서 데이터 참조
        var titleDict = EndingDataManager.Instance.endingTitles;
        var textDict = EndingDataManager.Instance.endingTexts;

        string title = "Unknown Title";
        string[] lines = new string[] { "Content missing..." };

        // 2. 타이틀 가져오기 (예외 처리 포함)
        if (titleDict.TryGetValue(endingID, out var langTitleMap))
        {
            if (langTitleMap.TryGetValue(language, out string foundTitle))
            {
                title = foundTitle;
            }
        }

        // 3. 본문 텍스트 가져오기
        if (textDict.TryGetValue(endingID, out var langTextMap))
        {
            if (langTextMap.TryGetValue(language, out string[] foundLines))
            {
                lines = foundLines;
            }
        }

        // 4. CreateEndingProp 호출하여 최종 데이터 반환
        return CreateEndingProp(endingID, title, lines);
    }

    // -----------------------------------------------------------
    // 2. 메인 엔딩 분기 로직
    // -----------------------------------------------------------

    public PropData GetEndingProp(Language language)
    {
        // 값 클램핑 (0 ~ 100)
        evironmentValue = Mathf.Clamp(evironmentValue, 0f, 100f);
        economyValue = Mathf.Clamp(economyValue, 0f, 100f);
        happinessValue = Mathf.Clamp(happinessValue, 0f, 100f);
        securityValue = Mathf.Clamp(securityValue, 0f, 100f);

        // 가독성을 위한 로컬 변수
        float env = evironmentValue;
        float eco = economyValue;
        float hap = happinessValue;
        float sec = securityValue;

        // =======================
        // [Priority 0] 8. Destruction (Game Over)
        // Any stat <= 0
        // =======================
        if (env <= 0f || eco <= 0f || hap <= 0f || sec <= 0f)
        {
            return GetEndingData("ENDING_08_DESTRUCTION", language);
        }

        // =======================
        // [Priority 1] Hidden Endings
        // =======================

        // 14. Mechanical Paradise (All >= 70)
        if (env >= 70f && eco >= 70f && sec >= 70f && hap >= 70f)
        {
            return GetEndingData("ENDING_14_MECHANICAL_PARADISE", language);
        }

        // 15. The Fading Lantern (All < 40)
        if (env < 40f && eco < 40f && hap < 40f && sec < 40f)
        {
            return GetEndingData("ENDING_15_FADING_LANTERN", language);
        }

        // =======================
        // [Priority 2] Extreme Bad Endings
        // =======================

        // 1. Masked Tycoon
        if (env <= 20f && eco >= 80f)
        {
            return GetEndingData("ENDING_01_MASKED_TYCOON", language);
        }

        // 2. Return to Forest
        if (env >= 80f && eco <= 20f)
        {
            return GetEndingData("ENDING_02_RETURN_FOREST", language);
        }

        // 3. Forbidden City
        if (sec >= 80f && hap <= 20f)
        {
            return GetEndingData("ENDING_03_FORBIDDEN_CITY", language);
        }

        // 4. Lawless Pleasure
        if (sec <= 20f && hap >= 80f)
        {
            return GetEndingData("ENDING_04_LAWLESS_PLEASURE", language);
        }

        // =======================
        // [Priority 3] Complex Normal Endings
        // =======================

        // 5. Cold Steel Empire
        if (eco >= 70f && sec >= 70f && env < 70f && hap < 70f)
        {
            return GetEndingData("ENDING_05_COLD_STEEL", language);
        }

        // 6. Slum Utopia
        if (env >= 70f && hap >= 70f && eco < 70f && sec < 70f)
        {
            return GetEndingData("ENDING_06_SLUM_UTOPIA", language);
        }

        // 10. Glass Garden
        if (env >= 60f && eco >= 60f && sec < 40f && hap < 40f)
        {
            return GetEndingData("ENDING_10_GLASS_GARDEN", language);
        }

        // =======================
        // [Priority 4] Special Conditions
        // =======================

        // 9. Golden Desert
        if (eco >= 70f && env <= 30f)
        {
            return GetEndingData("ENDING_09_GOLDEN_DESERT", language);
        }

        // 11. Blue Prison
        if (sec >= 70f && InRange(hap, 40f, 60f) && env < 70f && eco < 70f)
        {
            return GetEndingData("ENDING_11_BLUE_PRISON", language);
        }

        // 12. Twilight Smiles
        if (hap >= 70f && sec <= 40f && env <= 70f && eco <= 70f)
        {
            return GetEndingData("ENDING_12_TWILIGHT_SMILES", language);
        }

        // =======================
        // [Priority 5] Balanced / Neutral
        // =======================

        // 7. Precarious Peace (True Ending)
        if (InRange(env, 40f, 60f) && InRange(eco, 40f, 60f) &&
            InRange(sec, 40f, 60f) && InRange(hap, 40f, 60f))
        {
            return GetEndingData("ENDING_07_PRECARIOUS_PEACE", language);
        }

        // 13. Colorless Middle (Default Fallback)
        return GetEndingData("ENDING_13_COLORLESS_MID", language);
    }
}
