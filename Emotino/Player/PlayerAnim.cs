using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    public static PlayerAnim instance;

    private SpriteRenderer sp;
    private Animator anim;
    private PlayerMovement player;

    public Sprite chip;

    public bool isOnChip;

    private void Awake()
    {
        sp = GetComponent<SpriteRenderer>();
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        player = PlayerMovement.instance;
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isOnChip)
        {
            anim.enabled = false;
            sp.sprite = chip;
        }
        else
        {
            anim.enabled = true;
        }

        if (player.isMove)
        {
            anim.SetInteger("playerState",1);
        }
        else
        {
            anim.SetInteger("playerState", 0);
        }
            
    }
}
