using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PlaylistFunc : MonoBehaviour
{
    public TMP_Text noteSpeedText;

    private void Start()
    {
        noteSpeedText.text = $"<   {GameManager.instance.ChangeSpeed(0)}X   >";
    }

    void Update()
    {
        SpeedChangeKey();
    }

    private void SpeedChangeKey()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1)) noteSpeedText.text = $"<   {GameManager.instance.ChangeSpeed(-0.1f)}X   >";
        if(Input.GetKeyDown(KeyCode.Alpha2)) noteSpeedText.text = $"<   {GameManager.instance.ChangeSpeed(0.1f)}X   >";
    }
}
