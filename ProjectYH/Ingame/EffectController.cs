using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class EffectController : MonoBehaviour
{
    public static EffectController instance;
    [SerializeField] private Volume volume;

    private int effectCount;
    private int effectLoadCount;

    [SerializeField] private Animator glassAnim;
    [SerializeField] private RawImage flashImage;
    private Camera mainCamera;

    private UnityEngine.Rendering.Universal.Vignette vignette;
    private UnityEngine.Rendering.Universal.Bloom bloom;
    private UnityEngine.Rendering.Universal.PaniniProjection panini;
    private UnityEngine.Rendering.Universal.ColorAdjustments colorAdjustments;

    private void Awake()
    {
        instance = this;

        
    }

    private void Update()
    {
        if (GameManager.instance.isStart)
        {
            CheckEffectControll();
        }
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

    private void CheckEffectControll()
    {
        NoteSpawnManager.instance.musicInstance.getTimelinePosition(out int position);
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
                        if(effect is CameraMove c)
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
