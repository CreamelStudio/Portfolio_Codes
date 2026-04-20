using UnityEngine;
using UnityEngine.EventSystems;
public class EditorStructManager : MonoBehaviour
{
    public static EditorStructManager instance;

    public int currentBlockID;
    [SerializeField] private BlockInfoList blockDatas;
    private bool isEraser;
    private bool isDragging;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }

    private void Start()
    {
        blockDatas = EditorDataManager.instance.blockInfos;
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject() || Input.GetKey(KeyCode.Return) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (!isEraser && EditorCursorManager.instance.IsNotCollider()) { //겹쳐있는 오브젝트가 없거나 지우개 상태가 아니면
                Instantiate(blockDatas.blockInfos[currentBlockID].prefab, EditorCursorManager.instance.GetPos(), EditorCursorManager.instance.gameObject.transform.rotation);
                EditorDataManager.instance.AddBlockData(currentBlockID, EditorCursorManager.instance.GetPos(), EditorCursorManager.instance.gameObject.transform.rotation);
            }
        }

        if (Input.GetMouseButton(1) && !EventSystem.current.IsPointerOverGameObject() || Input.GetKey(KeyCode.RightShift) && !EventSystem.current.IsPointerOverGameObject())
        {
            EditorCursorManager.instance.isEraser = true;
            EditorCursorManager.instance.IsNotCollider(); //색을 변경하기 위한
            GameObject collabObj = EditorCursorManager.instance.collabObject();
            if(collabObj)
            {
                EditorDataManager.instance.RemoveBlockData(collabObj.transform.position);
                Destroy(collabObj);
            }
            isEraser = true;
        }
        if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.RightShift))
        {
            EditorCursorManager.instance.isEraser = false;
            EditorCursorManager.instance.IsNotCollider();
            isEraser = false;
        }
    }
}
