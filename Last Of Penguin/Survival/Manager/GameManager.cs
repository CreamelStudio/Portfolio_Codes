using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public CharacterController characterController;
    public GameObject pengin;
    public GameObject[] pengins;
    public int penuginId = 0;
    public bool isGame = false;
    public bool isSpawn = false;

    public Vector3 spawnPos;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("GameManager가 초기화되었습니다.");
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

    }

    public void SetCharacterController(CharacterController controller)
    {
        this.characterController = controller;
        this.pengin = controller.gameObject;
        Debug.Log("[GameManager] characterController 참조 설정 완료.");

        if (controller != null)
        {
            controller.ChangeCharacterState(CharacterState.Alive);
        }
    }
    public void SpawnPengin()
    {
        if (!isSpawn) return;
        GameObject penginClone = Instantiate(pengins[penuginId], spawnPos, Quaternion.identity);
        pengin = pengins[penuginId];
        characterController = penginClone.GetComponent<CharacterController>();
        isSpawn = false;
    }

    public void Setpengin(int _penginId)
    {
        penuginId = _penginId;
        pengin = pengins[_penginId];
        characterController = pengins[_penginId].GetComponent<CharacterController>();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!LOPNetworkManager.Instance.isConnected)
        {
            SpawnPengin();
        }
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}
