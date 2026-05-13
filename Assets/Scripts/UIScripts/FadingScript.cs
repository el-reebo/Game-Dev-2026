using UnityEngine;
using System.Collections;

public class FadingScript : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    public void FadeIn(float fadeDuration)
    {
        StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, fadeDuration));
    }

    public void FadeOut(float fadeDuration)
    {
        StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, fadeDuration));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }
        cg.alpha = endAlpha;
    }
}
