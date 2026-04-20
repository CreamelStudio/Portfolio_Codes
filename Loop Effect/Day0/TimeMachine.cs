using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeMachine : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite normalSprite;
    public Sprite completeSprite;

    // ✨ 추가된 변수: 하이라이트(히팅) 효과 오브젝트
    public GameObject highlightObject; 

    public float detectRange = 4f;
    public GameObject hintText; 

    bool isActivated = false;
    bool isActivated2 = false;

    void Start()
    {
        hintText.SetActive(false);
        
        // 시작할 때 하이라이트는 꺼두기
        if (highlightObject != null) highlightObject.SetActive(false);
    }

    void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        float dist = Vector2.Distance(transform.position, player.transform.position);
        bool isNear = dist <= detectRange;

        // 1. 상태에 따른 이미지 변경
        if (!isActivated)
            spriteRenderer.sprite = normalSprite;
        else
            spriteRenderer.sprite = completeSprite;

        // ✨ 2. 거리 감지해서 하이라이트 켜고 끄기 (히팅 효과)
        // 수리가 안 끝났고(isActivated == false), 가까이 있으면(isNear) 켜짐
        if (highlightObject != null)
        {
            // 수리가 이미 끝났으면 하이라이트 안 나오게 하려면 아래처럼 작성
            //if (isActivated) 
            //    highlightObject.SetActive(false);
            //else 
                highlightObject.SetActive(isNear);
        }

        if(!isActivated2 && isActivated && Input.GetKeyDown(KeyCode.F))
        {
            isActivated2 = true;
            GlobalEffectController.Instance.currentActive = 1;
            FadeManager.Instance.DayIndex++;
            StartCoroutine(LoadSceneAfterDelay(3f));
        }

        // 3. 상호작용 로직 (부품 3개 모았을 때만)
        if (PartManager.Instance.partsCount == 3 && !isActivated)
        {
            hintText.SetActive(isNear);

            if (isNear && Input.GetKeyDown(KeyCode.F))
            {
                isActivated = true;
                hintText.SetActive(false);
                // 수리 완료 즉시 하이라이트도 끄기
                if (highlightObject != null) highlightObject.SetActive(false);
            }
        }
        else
        {
            hintText.SetActive(false);
        }
    }

    System.Collections.IEnumerator LoadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        FadeManager.Instance.LoadSceneWithFade("Day1");
    }
}