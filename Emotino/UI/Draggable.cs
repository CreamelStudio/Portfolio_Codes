using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
public class Draggable : MonoBehaviour
{
    public EventReference draggable;
    public EventReference dragEnd;
    public EventReference initDrag;

    public int moduleID;
    public bool isDraggable;
    public bool isCanDrag;
    public Transform defaultPos;
    public Collider2D col;
    public float swapDistance;
    public EmotionSystem emotionManager;

    private SpriteRenderer spr;

    public GameObject trail;

    public Transform outUI;

    private void Awake()
    {
        spr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        isCanDrag = true;
    }

    private void Update()
    {
        if (isDraggable)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            transform.position = mouseWorldPos;
            if (!Input.GetMouseButton(0)) OnDisableDrag();
            trail.SetActive(true);
           
            spr.color = new Color(0.7f, 0.7f, 0.7f); ;
        }
        else
        {
            spr.color = Color.white;
            trail.SetActive(false);
        }
    }

    public void InitPosition()
    {
        AudioManager.instance.PlayMusic(initDrag);
        transform.SetParent(defaultPos);
        transform.DOLocalMove(Vector3.zero, 0.1f);
        emotionManager.ManageEmotion(true, moduleID);
        isDraggable = false; 
        isCanDrag = true;
    }

    public void OnEnableDrag()
    {
        if (!isCanDrag) return;
        AudioManager.instance.PlayMusic(draggable);
        isDraggable = true;
        emotionManager.ManageEmotion(false, moduleID);
        PlayerAnim.instance.isOnChip = true;
        col.isTrigger = true;
    }
    public void OnDisableDrag()
    {
        if(Vector2.Distance(defaultPos.position, transform.position) < swapDistance) transform.position = defaultPos.position;
        AudioManager.instance.PlayMusic(dragEnd);
        isDraggable = false;
        isCanDrag = false;
        PlayerAnim.instance.isOnChip = false;
        col.isTrigger = false;
        transform.SetParent(outUI);
    }
}
