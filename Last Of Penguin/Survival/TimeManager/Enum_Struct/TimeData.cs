using UnityEngine;

[System.Serializable]
public class TimeData
{
    public string id;             // ������ ������ ���� ID
    public TimeType timeType;       // �ð� Ÿ��
    public float durationTime;   // ���� �ð� 

    [Header("Material Setting Value")]
    public float materialMinOffsetX;
    public float materialMaxOffsetX;

    [Header("Color Adj Data")]
    public ColorAdjData colorAdjData;
}

[System.Serializable]
public class ColorAdjData
{
    public ColorAdjData(Color colorFilter, float PostExposure, int Contrast, int Saturation)
    {
        this.colorFilter = colorFilter;
        this.PostExposure = PostExposure;
        this.Contrast = Contrast;
        this.Saturation = Saturation;
    }

    public Color colorFilter;
    public float PostExposure;
    public int Contrast;
    public int Saturation;
}