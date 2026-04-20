using UnityEngine;

public class EditorCanvasChanger : MonoBehaviour
{
    public static EditorCanvasChanger instance;

    public int width;
    public int height;

    private SpriteRenderer sr;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        ChangeCanvasSize(width, height);
    }

    public void ChangeCanvasSize(int width, int height)
    {
        this.width = width;
        this.height = height;

        sr.size = new Vector2(width, height);
        float offsetX = (width % 2 == 0) ? -0.5f : 0f;
        float offsetY = (height % 2 == 0) ? -0.5f : 0f;
        sr.transform.position = new Vector2(width / 2 + offsetX, height / 2 + offsetY);
    }
}
