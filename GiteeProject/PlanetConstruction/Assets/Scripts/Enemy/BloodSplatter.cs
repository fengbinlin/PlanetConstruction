using UnityEngine;
using System.Collections;

public class BloodSplatter : MonoBehaviour, IPoolable
{
    public float fadeDuration = 2f;

    private SpriteRenderer sr;
    private Color startColor;
    private Coroutine fadeCoroutine;

    public void OnSpawnFromPool()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            startColor = sr.color;
            sr.color = new Color(startColor.r, startColor.g, startColor.b, 1f);

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeAndRecycle());
        }
        else
        {
            Debug.LogWarning("BloodSplatter: 未找到 SpriteRenderer！");
            RecycleSelf();
        }
    }

    public void OnReturnToPool()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        if (sr != null)
        {
            sr.color = new Color(startColor.r, startColor.g, startColor.b, 1f);
        }
    }

    void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            startColor = sr.color;
            fadeCoroutine = StartCoroutine(FadeAndRecycle());
        }
        else
        {
            Debug.LogWarning("BloodSplatter: 未找到 SpriteRenderer！");
            RecycleSelf();
        }
    }

    IEnumerator FadeAndRecycle()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        RecycleSelf();
    }

    private void RecycleSelf()
    {
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.RecycleBloodSplatter(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}