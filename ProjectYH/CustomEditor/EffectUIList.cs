using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class NoteWSpeedUI
{
    public TMP_InputField longNote_Input;
    public TMP_Text longNote_Output;
    [Space(3f)]
    public TMP_InputField speedChange_Input;
    public TMP_Text speedChange_Output;
}


[System.Serializable]
public class PostProcessUI
{
    public TMP_InputField vignettingInten_Input;
    public TMP_Text vignettingInten_Output;
    public TMP_InputField vignettingDuration_Input;
    public TMP_Text vignettingDuration_Output;
    [Space(3f)]
    public TMP_InputField bloomInten_Input;
    public TMP_Text bloomInten_Output;
    public TMP_InputField bloomDuration_Input;
    public TMP_Text bloomDuration_Output;
    [Space(3f)]
    public TMP_InputField flashInten_Input;
    public TMP_Text flashInten_Output;
    public TMP_InputField flashInDuration_Input;
    public TMP_Text flashInDuration_Output;
    public TMP_InputField flashOutDuration_Input;
    public TMP_Text flashOutDuration_Output;
    [Space(3f)]
    public TMP_InputField glassBreakOption_Input;
    public TMP_Text glassBreakOption_Output;
    [Space(3f)]
    public TMP_InputField paniniLength_Input;
    public TMP_Text paniniLength_Output;
    public TMP_InputField paniniDuration_Input;
    public TMP_Text paniniDuration_Output;
}

[System.Serializable]
public class ColorAdjustmentUI
{
    public TMP_InputField colorAdjPostEx_Input;
    public TMP_Text colorAdjPostEx_Output;
    public TMP_InputField colorAdjContrast_Input;
    public TMP_Text colorAdjContrast_Output;
    public TMP_InputField colorAdjHueShift_Input;
    public TMP_Text colorAdjHueShift_Output;
    public TMP_InputField colorAdjSaturation_Input;
    public TMP_Text colorAdjSaturation_Output;
    public TMP_InputField colorAdjDuration_Input;
    public TMP_Text colorAdjDuration_Output;
}

[System.Serializable]
public class CameraMoveUI
{
    public TMP_InputField cameraMovePX_Input;
    public TMP_InputField cameraMovePY_Input;
    public TMP_InputField cameraMovePZ_Input;
    public TMP_Text cameraMoveP_Output;
    public TMP_InputField cameraMoveRX_Input;
    public TMP_InputField cameraMoveRY_Input;
    public TMP_InputField cameraMoveRZ_Input;
    public TMP_Text cameraMoveR_Output;
    public TMP_InputField cameraMoveDuration_Input;
    public TMP_Text cameraMoveDuration_Output;
}