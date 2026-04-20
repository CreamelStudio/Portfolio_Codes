using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class ClearDataManager : MonoBehaviour
{
    public TMP_Text mapTitle;
    public TMP_Text mapDesc;
    public TMP_Text eta;
    public TMP_Text coin;
    public TMP_Text score;

    public void InitData(string mapTitle, string mapDesc, float time, int coin)
    {
        this.mapTitle.text = mapTitle;
        this.mapDesc.text = mapDesc;
        int minutes = Mathf.FloorToInt(time / 60f);
        float seconds = time % 60f;
        this.eta.text = string.Format("Elapsed time : {0:00}:{1:00.00}", minutes, seconds);
        this.coin.text = $"Coin : {coin.ToString()}";
        this.score.text = Mathf.RoundToInt(time * 10 + coin * 100).ToString();
    }

    public void GoToSelect()
    {
        SceneManager.LoadScene(Scenes.select);
    }
}
