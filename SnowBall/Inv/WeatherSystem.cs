using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Work.Utils.EventBus;

public enum WeatherType
{
    Sunny,
    Rainy,
    
    Snowy,
    Cloudy,
}


public struct WeatherChangeEvent : IEvent
{
    public WeatherType NewWeather;
    public WeatherChangeEvent(WeatherType newWeather)
    {
        NewWeather = newWeather;
    }
}

public class WeatherSystem : MonoBehaviour
{
    public int weatherChangeInterval = 5;

    private WeatherType currentWeather;
    public GameObject rainEffect;
    public GameObject snowEffect;
    public GameObject cloudyEffect;
    public GameObject sunnyEffect;

    public bool isIngame;

    void Start()
    {
        if(!Inventory.Instance.IsUsedBuffItem("테루 테루 보즈") && isIngame)StartCoroutine(Co_TestRoutine());
        else ChangeWeather(WeatherType.Snowy);
    }

    /// <summary>
    /// 테스트용 코루틴 | 추후 삭제 예정
    /// </summary>
    IEnumerator Co_TestRoutine()
    {
        while (true)
        {
            ChangeWeather((WeatherType)Random.Range(0, 4));
            yield return new WaitForSeconds(weatherChangeInterval);
        }
        
    }
    /// <summary>
    /// 날씨 변경 메서드
    /// </summary>
    /// <param name="newWeather">변경할 날씨 타입</param>
    public void ChangeWeather(WeatherType newWeather)
    {
        currentWeather = newWeather;

        rainEffect.SetActive(currentWeather == WeatherType.Rainy);
        snowEffect.SetActive(currentWeather == WeatherType.Snowy);
        cloudyEffect.SetActive(currentWeather == WeatherType.Cloudy);
        sunnyEffect.SetActive(currentWeather == WeatherType.Sunny);

        //여기다 추가 로직 작성하심 될거같아여
        Bus<WeatherChangeEvent>.Raise(new WeatherChangeEvent(newWeather));

        Debug.Log("날씨가 " + newWeather.ToString() + "(으)로 변경되었습니다.");
    }

    public WeatherType GetCurrentWeather()
    {
        Debug.Log("현재 날씨: " + currentWeather.ToString());
        return currentWeather;
    }

}
