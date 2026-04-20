using UnityEngine;
using UnityEngine.UI;

public class EditorCursorManager : MonoBehaviour
{
    public static EditorCursorManager instance;

    public bool isEraser;
    public SpriteRenderer cursorImage;

    public Vector3 currentCursorPos;
    public Vector3 prevPos;
    public Vector3 playerDefaultPos;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }

    private void Start()
    {
        cursorImage = GetComponent<SpriteRenderer>();
    }

    #region Colliders
    public bool IsNotCollider()
    {
        int layerMask = LayerMask.GetMask("Struct");
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(GetPos(), new Vector2(0.4f, 0.4f), 0, layerMask); //커서 오브젝트와 겹쳐있는 구조물 감지
        InitCursorColor(hitColliders);

        if (isEraser) return false;
        else if (hitColliders.Length == 0) return true;
        else return false;
    }

    public GameObject collabObject()
    {
        int layerMask = LayerMask.GetMask("Struct");
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(GetPos(), new Vector2(0.4f, 0.4f), 0, layerMask); //커서 오브젝트와 겹쳐있는 구조물 감지
        if (hitColliders.Length != 0 && hitColliders[0].name != "CharacterVoid")
        {
            return hitColliders[0].gameObject;
        }
        else
        {
            return null;
        }
    }
    #endregion

    #region Cursor Color Init
    public void InitCursorColor()
    {
        int layerMask = LayerMask.GetMask("Struct");
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(GetPos(), new Vector2(0.4f, 0.4f), 0, layerMask); //커서 오브젝트와 겹쳐있는 구조물 감지

        if (isEraser)
        {
            cursorImage.color = new Color(0, 0, 1, 0.2f);
        }
        else if (hitColliders.Length == 0)
        {
            cursorImage.color = new Color(0, 1, 0, 0.2f);
        }
        else
        {
            cursorImage.color = new Color(1, 0, 0, 0.2f);
        }
    }

    public void InitCursorColor(Collider2D[] hitColliders)
    {
        if (isEraser)
        {
            cursorImage.color = new Color(0, 0, 1, 0.2f);
        }
        else if (hitColliders.Length == 0)
        {
            cursorImage.color = new Color(0, 1, 0, 0.2f);
        }
        else
        {
            cursorImage.color = new Color(1, 0, 0, 0.2f);
        }
    }
    #endregion

    public Vector3 GetPos()
    {
        return currentCursorPos;
    }

    private void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); //마우스 위치값 받기
        transform.position = new Vector3(Mathf.Round(mousePosition.x), Mathf.Round(mousePosition.y), 0); //마우스 위치 보정
        currentCursorPos = transform.position;
        if (prevPos != transform.position) //마우스 위치가 바뀌었을 때
        {
            InitCursorColor(); //커서 색상 초기화
        }
        prevPos = currentCursorPos; //이전 위치값 저장
    }
}
