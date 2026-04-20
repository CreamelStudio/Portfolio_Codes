using UnityEngine;

[CreateAssetMenu]
public class StructPrefabList : ScriptableObject
{
    public Mesh defaultMesh;
    public Structs[] structList;

    [System.Serializable]
    public struct Structs
    {
        public string name;
        public Mesh mesh;
        public Texture2D image;
        public GameObject structPrefab;
        [Header("모델의 논리 사이즈")]
        public Vector3 modelSize;
        [Header("mesh 적용시 뷰어의 사이즈")]
        public Vector3 viewerSize;
        [Header("한번에 움직이는 칸 수")]
        public Vector3 moveOffset;
        [Header ("만약 모델의 Y가 뜨면 True")]
        public bool isYOffset;
        [Header("모델이 돌아서 생성될 때 Rotate Offset")]
        public Vector3 modelRotateOffset;
        [Header("모델이 이상한곳에 생성될 떄 ")]
        public Vector3 modelPositionOffset;
        [Header("물건의 종류 1. 벽 2. 수급시설 3. 생산시설 4. 기타")]
        public StructFilter wb;

        public bool disableButton;
    }
}
