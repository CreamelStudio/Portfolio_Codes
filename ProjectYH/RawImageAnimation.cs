using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RawImageAnimation : MonoBehaviour
{
    private RawImage image;
    public Texture[] animTextures;
    public bool isPlay;
    public int imageCount;

    private void Awake()
    {
        image = GetComponent<RawImage>();
    }

    private void FixedUpdate()
    {
        if(imageCount <= animTextures.Length && isPlay)
        {
            image.texture = animTextures[imageCount];
            imageCount++;
        }
    }
}
