using System.Collections;
using UnityEngine;

public class SpriteBounceEffect : MonoBehaviour
{
    [Header("弹动效果设置")]
    [SerializeField] private float bounceScale = 1.5f;    // 弹动时的最大缩放
    [SerializeField] private float bounceDuration = 0.3f; // 弹动总时长
    [SerializeField] private AnimationCurve bounceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 弹动动画曲线

    private Vector3 originalScale; // 原始大小
    private bool isBouncing = false; // 是否正在弹动中

    void Start()
    {
        // 记录原始大小
        originalScale = transform.localScale;
    }

    /// <summary>
    /// 公开函数：触发弹动效果
    /// </summary>
    public void PlayBounce()
    {
        if (!isBouncing)
        {
            StartCoroutine(BounceRoutine());
        }
    }

    /// <summary>
    /// 公开函数：触发弹动效果（可自定义参数）
    /// </summary>
    /// <param name="customBounceScale">自定义弹动缩放</param>
    /// <param name="customDuration">自定义持续时间</param>
    public void PlayBounce(float customBounceScale, float customDuration)
    {
        if (!isBouncing)
        {
            StartCoroutine(BounceRoutine(customBounceScale, customDuration));
        }
    }

    /// <summary>
    /// 弹动动画协程
    /// </summary>
    private IEnumerator BounceRoutine()
    {
        isBouncing = true;
        
        float timer = 0f;
        Vector3 targetScale = originalScale * bounceScale;

        // 第一阶段：变大
        while (timer < bounceDuration / 2)
        {
            timer += Time.deltaTime;
            float progress = timer / (bounceDuration / 2);
            float curveValue = bounceCurve.Evaluate(progress);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, curveValue);
            yield return null;
        }

        // 第二阶段：变小回原始大小
        timer = 0f;
        while (timer < bounceDuration / 2)
        {
            timer += Time.deltaTime;
            float progress = timer / (bounceDuration / 2);
            float curveValue = bounceCurve.Evaluate(progress);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, curveValue);
            yield return null;
        }

        // 确保最终回到精确的原始大小
        transform.localScale = originalScale;
        isBouncing = false;
    }

    /// <summary>
    /// 带自定义参数的弹动协程
    /// </summary>
    private IEnumerator BounceRoutine(float customBounceScale, float customDuration)
    {
        isBouncing = true;
        
        float timer = 0f;
        Vector3 targetScale = originalScale * customBounceScale;

        // 第一阶段：变大
        while (timer < customDuration / 2)
        {
            timer += Time.deltaTime;
            float progress = timer / (customDuration / 2);
            float curveValue = bounceCurve.Evaluate(progress);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, curveValue);
            yield return null;
        }

        // 第二阶段：变小回原始大小
        timer = 0f;
        while (timer < customDuration / 2)
        {
            timer += Time.deltaTime;
            float progress = timer / (customDuration / 2);
            float curveValue = bounceCurve.Evaluate(progress);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, curveValue);
            yield return null;
        }

        transform.localScale = originalScale;
        isBouncing = false;
    }

    /// <summary>
    /// 重置为原始大小
    /// </summary>
    public void ResetScale()
    {
        StopAllCoroutines();
        transform.localScale = originalScale;
        isBouncing = false;
    }
}