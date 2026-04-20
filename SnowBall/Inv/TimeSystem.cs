using System.Collections;
using UnityEngine;

public class TimeSystem : MonoBehaviour
{
    public int hour;

    public int hourChangeInterval = 1;

    public Gradient timeColorGradient;
    public Light directionalLight;

    void Start()
    {
        StartCoroutine(Co_TestRoutine());
    }

    /// <summary>
    /// 테스트용 코루틴 | 추후 삭제 예정
    /// </summary>
    IEnumerator Co_TestRoutine()
    {
        while (true)
        {
            for (int i = 0; i < 24; i++)
            {
                hour = i;
                ChangeHour(hour);
                yield return new WaitForSeconds(hourChangeInterval);
            }
        }
    }

    public void ChangeHour(float hourNormalized)
    {
        Color lightColor = timeColorGradient.Evaluate(hourNormalized / 24f);
        directionalLight.color = lightColor;
    }
}
