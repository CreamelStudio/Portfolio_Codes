using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SheetDataLoad : MonoBehaviour
{
    private string sheetData;
    private const string googleSheetURL = "https://docs.google.com/spreadsheets/d/1gULSG-3ABNbsloeAEkxe-pNiRVk5frpB0zYrar1hj4o/export?format=csv&range=B5:F11";
    public CharStat charStat = new CharStat();
    public Character character;

    public class CharacterStat
    {
        public string penguinName;
        public int moveSpeed;
        public int dexterity;
        public int durationTime;
        public int coolTime;

        public CharacterStat(string name, int moveSpeed, int dexterity, int durationTime, int coolTime)
        {
            this.penguinName = name;
            this.moveSpeed = moveSpeed;
            this.dexterity = dexterity;
            this.durationTime = durationTime;
            this.coolTime = coolTime;
        }
    }

    public class CharStat
    {
        public List<CharacterStat> charStatList;
    }

    private void Start()
    {
        charStat.charStatList = new List<CharacterStat>();
        StartCoroutine(LoadSheetData());
        //StartCoroutine("LoadSheetData");
    }

    public void GetSheetData()
    {
        StartCoroutine(LoadSheetData());
    }

    private void DebugData()
    {
        for (int i = 0; i < 7; i++)
        {
            Debug.Log("name : " + charStat.charStatList[i].penguinName + "  moveSpeed : " + charStat.charStatList[i].moveSpeed + "  dexterity : " + charStat.charStatList[i].dexterity + "  durationTime : " + charStat.charStatList[i].durationTime + "  coolTime : " + charStat.charStatList[i].coolTime);
        }
    }

    private void OrganizeData()
    {
        for (int i = 0; i < 7; i++)
        {
            charStat.charStatList.Add(new CharacterStat(sheetData.Split("\n")[i].Split(",")[0], int.Parse(sheetData.Split("\n")[i].Split(",")[1]), int.Parse(sheetData.Split("\n")[i].Split(",")[2]), int.Parse(sheetData.Split("\n")[i].Split(",")[3]), int.Parse(sheetData.Split("\n")[i].Split(",")[4])));
        }

        CharacterSpawner character = GetComponent<CharacterSpawner>();
        character.InputStat();
    }

    public void ApplyStatToCharacter()
    {
        foreach (var stat in charStat.charStatList)
        {
            Debug.Log("ÇöÀç °Ë»çÇÒ Æë±Ï : " + stat.penguinName);
            if (!GameManager.instance.isMuilt)
            {
                if (stat.penguinName == GameManager.instance.penguinData.dataCharacterName)
                {
                    character.moveSpeed = stat.moveSpeed;
                    character.dexterity = stat.dexterity;
                    character.skillDuration = stat.durationTime;
                    character.skillCoolTime = stat.coolTime;
                    Debug.Log("Stat applied to: " + character.penguinName);
                    Debug.Log("½ºÅÝ ³Ö¾úÀ½");
                    break;
                }
            }
            else if (GameManager.instance.isMuilt)
            {
                foreach(var player in TestLocalDataManager.Instance.players)
                {
                    if (!player.inStat && stat.penguinName == player.characterProfile.dataCharacterName)
                    {
                        character.moveSpeed = stat.moveSpeed;
                        character.dexterity = stat.dexterity;
                        character.skillDuration = stat.durationTime;
                        character.skillCoolTime = stat.coolTime;
                        Debug.Log("Stat applied to: " + character.penguinName);
                        Debug.Log("½ºÅÝ ³Ö¾úÀ½");
                        continue;
                    }
                }
            }
        }
    }

    IEnumerator LoadSheetData()
    {
        using (UnityWebRequest sheetReq = UnityWebRequest.Get(googleSheetURL))
        {
            yield return sheetReq.SendWebRequest();
            if (sheetReq.isDone)
            {
                sheetData = sheetReq.downloadHandler.text;
                OrganizeData();
                DebugData();
            }
        }
    }
}
