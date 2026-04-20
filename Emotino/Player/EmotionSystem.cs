using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
[System.Serializable]
public class Emotion
{
    public Sprite moduleImg;
    public bool isEnable;
    public string name;
    public string desc;
    public bool isHave;

    public float debuffValue;
    public float originalValue;
}

public class EmotionSystem : MonoBehaviour
{
    public static EmotionSystem instance;
    public Volume volume;
    private Vignette vignette;

    private PlayerMovement move;

    public Emotion happiness;
    public Emotion excited;
    public Emotion sad;
    public Emotion angry;
    public Emotion fear;
    public Emotion tranquility;
    public Emotion love;

    public bool isEnableLove;
    public GameObject heartObj;
    int countFixedFrame = 0;

    public SpriteRenderer[] moduleImages;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        move = GetComponent<PlayerMovement>();
        InitEmotion();

        if (volume.profile.TryGet(out vignette))
        {
            vignette.intensity.value = 0.26f;
        }
    }

    public void InitEmotion()
    {
        for(int i = 0; i < 6; i++)
        {
            ManageEmotion(true, i);
        }
    }

    private void FixedUpdate()
    {
        ModuleFuncInit();
    }

    public void ModuleFuncInit()
    {

        countFixedFrame++;

        for (int i = 0; i < 7; i++)
        {
            switch (i)
            {
                case 0:
                    if (!happiness.isEnable) move.jumpForce = happiness.debuffValue;
                    else move.jumpForce = happiness.originalValue;
                    break;
                case 1:
                    if (!excited.isEnable) move.moveSpeed = excited.debuffValue;
                    else move.moveSpeed = excited.originalValue;
                    break;
                case 2:
                    if (!sad.isEnable) move.statRate = sad.debuffValue;
                    else move.statRate = sad.originalValue;
                    break;
                case 3:
                    if (!angry.isEnable) move.gun.coolTime = angry.debuffValue;
                    else move.gun.coolTime = angry.originalValue;
                    break;
                case 4:
                    if (!fear.isEnable)
                    {
                        DOTween.To(() => vignette.intensity.value, (x) => vignette.intensity.value = x, fear.debuffValue, 0.3f);
                        DOTween.To(() => vignette.smoothness.value, (x) => vignette.smoothness.value = x, fear.debuffValue, 0.3f);
                    }
                    else
                    {
                        DOTween.To(() => vignette.intensity.value, (x) => vignette.intensity.value = x, fear.originalValue, 0.3f);
                        DOTween.To(() => vignette.smoothness.value, (x) => vignette.smoothness.value = x, fear.originalValue, 0.3f);
                    }
                    break;
                case 5:
                    if (countFixedFrame % 15 == 0 && !tranquility.isEnable) move.OnNoise(tranquility.debuffValue);
                    else if (tranquility.isEnable) move.OnNoise(tranquility.originalValue);
                    break;
                case 6:
                    if (isEnableLove) move.statRate = love.debuffValue;
                    break;
            }
        }
    }

    public void ManageModule(bool isEnable, int ID)
    {
        switch (ID)
        {
            case 0:
                happiness.isHave = isEnable;
                break;
            case 1:
                excited.isHave = isEnable;
                break;
            case 2:
                sad.isHave = isEnable;
                break;
            case 3:
                angry.isHave = isEnable;
                break;
            case 4:
                fear.isHave = isEnable;
                break;
            case 5:
                tranquility.isHave = isEnable;
                break;
        }
    }

    public void ManageEmotion(bool isEnable, int ID)
    {

        Debug.Log($"ID : {ID} & Enable : {isEnable}");

        switch (ID)
        {
            case 0:
                happiness.isEnable = isEnable;
                break;
            case 1:
                excited.isEnable = isEnable;
                break;
            case 2:
                sad.isEnable = isEnable;
                break;
            case 3:
                angry.isEnable = isEnable;
                break;
            case 4:
                fear.isEnable = isEnable;
                break;
            case 5:
                tranquility.isEnable = isEnable;
                break;
        }
    }

    public void IsEnableHeart()
    {
        heartObj.SetActive(true);
        isEnableLove = true;
    }
}
