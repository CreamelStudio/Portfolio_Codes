using System;
using UnityEngine;

public enum DialogueType
{
    Normal,
    Branch,
    Random
}

[Serializable]
public class DialogueBranch
{
    [Tooltip("선택지에 표시될 텍스트")]
    public string optionText;

    [Tooltip("이 선택지를 골랐을 때 진행될 다음 대사 데이터")]
    public DialogueData nextDialogue;
}

[Serializable]
public class DialogueData
{
    [Header("공통")]
    public DialogueType dialogueType = DialogueType.Normal;

    [Range(0f, 100f)]
    [Tooltip("행동 게이지 감소 비율 (0~1)")]
    public float actionGaugeReductionRate = 0f;

    [Header("기타 게이지 변화")]
    public float evironmentGuageChange = 0f;
    public float economyGuageChange = 0f;
    public float happinessGuageChange = 0f;
    public float securityGaugeChange = 0f;

    public string characterName;

    [TextArea]
    public string dialogueText;

    [Header("Branch 타입에서만 사용")]
    public DialogueBranch[] branches;
}

[CreateAssetMenu(fileName = "PropData", menuName = "Game/Prop Data", order = 0)]
public class PropData : ScriptableObject
{
    [Header("식별자")]
    public string propID;

    [Header("인터랙션 범위")]
    public Vector2 interactiveAreaSize = Vector2.one;
    public Vector2 interactiveAreaOffset;

    [Header("대사 설정")]
    public DialogueData[] dialogues;
}
