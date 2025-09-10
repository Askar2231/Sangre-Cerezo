using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public Image fadeImage;

    public IEnumerator FadeIn(float duration)
    {
        float t = 0;
        Color color = fadeImage.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(1, 0, t / duration); // de negro a transparente
            fadeImage.color = color;
            yield return null;
        }
    }

    public IEnumerator FadeOut(float duration)
    {
        float t = 0;
        Color color = fadeImage.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(0, 1, t / duration); // de transparente a negro
            fadeImage.color = color;
            yield return null;
        }
    }
}
