using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public enum NoteEnum
{
    Default,
    Long,
    Double
}

[System.Serializable]
public enum EffectEnum
{
    Vignetting,
    Bloom,
    Flash,
    GlassBreak,
    PaniniProjections,
    ColorAdjustments,
    CameraMove,
    ChangeMinuteSpeed
}

[System.Serializable]
public class Vector3Serial
{
    public float x;
    public float y;
    public float z;

    public Vector3 ToVector3()
    {
        return new Vector3(this.x, this.y, this.z);
    }
}

[System.Serializable]
public class BaseEffect
{
    public EffectEnum EffectType;
    public double StartDelay;
}

[System.Serializable]
public class BaseNote
{
    public NoteEnum NoteType;
    public double StartDelay;
}

[System.Serializable]
public class NiddleSpeed : BaseEffect
{
    public float afterSpeed;

    public NiddleSpeed(double startDelay, float afterSpeed)
    {
        this.EffectType = EffectEnum.ChangeMinuteSpeed;
        this.StartDelay = startDelay;
        this.afterSpeed = afterSpeed;
    }
}

[System.Serializable]
public class Vignetting : BaseEffect
{
    public float Intensity;
    public float Duration;

    public Vignetting(double startDelay, float intensity, float duration)
    {
        this.EffectType = EffectEnum.Vignetting;
        this.StartDelay = startDelay;
        this.Intensity = intensity;
        this.Duration = duration;
    }
}

[System.Serializable]
public class Bloom : BaseEffect
{
    public float Intensity;
    public float Duration;

    public Bloom(double startDelay, float intensity, float duration)
    {
        this.EffectType = EffectEnum.Bloom;
        this.StartDelay = startDelay;
        this.Intensity = intensity;
        this.Duration = duration;
    }
}

[System.Serializable]
public class Flash : BaseEffect
{
    public float FlashDuration;
    public float OffDuration;
    public float Power;

    public Flash(double startDelay, float flashDuration, float offDuration, float Power)
    {
        this.EffectType = EffectEnum.Flash;
        this.StartDelay = startDelay;
        this.FlashDuration = flashDuration;
        this.OffDuration = offDuration;
        this.Power = Power;
    }
}

[System.Serializable]
public class GlassBreak : BaseEffect
{
    public int Option;

    public GlassBreak(double startDelay, int option)
    {
        this.EffectType = EffectEnum.GlassBreak;
        this.StartDelay = startDelay;
        this.Option = option;
    }
}

[System.Serializable]
public class PaniniProjection : BaseEffect
{
    public float Distance;
    public float Duration;

    public PaniniProjection(double startDelay, float distance, float duration)
    {
        this.EffectType = EffectEnum.PaniniProjections;
        this.StartDelay = startDelay;
        this.Distance = distance;
        this.Duration = duration;
    }
}

[System.Serializable]
public class ColorAdjustments : BaseEffect
{
    public float PostExposure;
    public float Contrast;
    public float HueShift;
    public float Saturation;
    public float Duration;

    public ColorAdjustments(double startDelay, float postExposure, float contrast, float hueShift, float saturation, float duration)
    {
        this.EffectType = EffectEnum.ColorAdjustments;
        this.StartDelay = startDelay;
        this.PostExposure = postExposure;
        this.Contrast = contrast;
        this.HueShift = hueShift;
        this.Saturation = saturation;
        this.Duration = duration;
    }
}

[System.Serializable]
public class CameraMove : BaseEffect
{
    public Vector3Serial Position;
    public Vector3Serial Rotation;
    public float Duration;

    public CameraMove(double startDelay, Vector3Serial position, Vector3Serial rotation, float duration)
    {
        this.EffectType = EffectEnum.CameraMove;
        this.StartDelay = startDelay;
        this.Position = position;
        this.Rotation = rotation;
        this.Duration = duration;
    }
}

[System.Serializable]
public class DefaultNote : BaseNote
{
    public DefaultNote(double startDelay)
    {
        this.NoteType = NoteEnum.Default;
        this.StartDelay = startDelay;
    }
}

[System.Serializable]
public class DoubleNote : BaseNote
{
    public DoubleNote(double startDelay)
    {
        this.NoteType = NoteEnum.Double;
        this.StartDelay = startDelay;
    }
}

[System.Serializable]
public class LongNote : BaseNote
{
    public float EndDelay;

    public LongNote(double startDelay, float endDelay)
    {
        this.NoteType = NoteEnum.Long;
        this.StartDelay = startDelay;
        this.EndDelay = endDelay;
    }
}
