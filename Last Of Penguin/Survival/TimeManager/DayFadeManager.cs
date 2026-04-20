namespace Lop.Survivor.Manager
{
    // # System
    using System.Collections;
    using System.Collections.Generic;

    // # Unity
    using UnityEngine;
    using UnityEngine.UI;

    // # Project

    public class DayFadeManager : MonoBehaviour
    {
        public static DayFadeManager Instance { get; private set; }

        [SerializeField]
        private Image panel;
        [SerializeField]
        private float fadeInDuration;
        [SerializeField]
        private float fadeOutDuration;

        private bool isFading = false;
        public bool IsFading => isFading;

        private void Awake()
        {
            Instance = this;
        }

        public void Fade()
        {
            StartCoroutine(FadeInAndOutCoroutine());
        }

        public IEnumerator FadeInAndOutCoroutine()
        {
            isFading = true;

            yield return StartCoroutine(FadeCoroutine(Color.black, 0.4f, 1, 1, fadeInDuration));

			//yield return new WaitForSeconds(0.5f);

			//SoundManager.Instance.PlaySFX("NextDay"); //���� ���� �ý����� �鿩���� �ּ� �����ϰڽ��ϴ�


			yield return StartCoroutine(FadeCoroutine(Color.white, 0.7f, 0.0f, 0, fadeOutDuration));
            isFading = false;
        }


        private IEnumerator FadeCoroutine(Color color, float start, float end, float total, float duration)
        {
            panel.color = color;

            float elapsed = 0.0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                panel.color = new Color(panel.color.r,
                                        panel.color.g,
                                        panel.color.b,
                                        Mathf.Lerp(start, end, elapsed / duration));
                yield return null;
            }

            panel.color = new Color(panel.color.r,
                            panel.color.g,
                            panel.color.b,
                            total);
        }
    }

}