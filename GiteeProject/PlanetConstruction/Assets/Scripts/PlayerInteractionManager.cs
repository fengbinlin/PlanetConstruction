using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionManager : MonoBehaviour
{
    [Header("UI Settings")]
    public CanvasGroup spaceshipUICanvasGroup; // 飞船UI的CanvasGroup组件
    public float animationDuration = 0.3f;     // 动画持续时间
    public float elasticIntensity = 0.5f;      // 弹性动画强度

    private bool isInSpaceshipArea = false;    // 是否在飞船区域内
    private Coroutine currentAnimationCoroutine; // 当前正在运行的动画协程

    // Start is called before the first frame update
    void Start()
    {
        // 确保UI初始状态为隐藏
        if (spaceshipUICanvasGroup != null)
        {
            spaceshipUICanvasGroup.alpha = 0f;
            spaceshipUICanvasGroup.interactable = false;
            spaceshipUICanvasGroup.blocksRaycasts = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 可以在这里添加其他交互逻辑
    }


    private void OnCollisionEnter2D(Collision2D other)
    {

        SpaceShip spaceship = other.gameObject.GetComponent<SpaceShip>();
        if (spaceship != null && !isInSpaceshipArea)
        {
            print("进入飞船");
            isInSpaceshipArea = true;
            ShowSpaceshipUI();
        }
    }

    /// <summary>
    /// 触发器退出检测
    /// </summary>
    private void OnCollisionExit2D(Collision2D other)
    {

        // 检查碰撞对象是否有SpaceShip组件
        SpaceShip spaceship = other.gameObject.GetComponent<SpaceShip>();
        if (spaceship != null && isInSpaceshipArea)
        {

            print("离开飞船");
            isInSpaceshipArea = false;
            HideSpaceshipUI();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        SpaceShip spaceship = other.gameObject.GetComponent<SpaceShip>();
        if (spaceship != null && !isInSpaceshipArea)
        {
            print("进入飞船");
            isInSpaceshipArea = true;
            ShowSpaceshipUI();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {

        // 检查碰撞对象是否有SpaceShip组件
        SpaceShip spaceship = other.gameObject.GetComponent<SpaceShip>();
        if (spaceship != null && isInSpaceshipArea)
        {

            print("离开飞船");
            isInSpaceshipArea = false;
            HideSpaceshipUI();
        }
    }

    /// <summary>
    /// 显示飞船UI（带弹性动画）
    /// </summary>
    private void ShowSpaceshipUI()
    {
        if (spaceshipUICanvasGroup == null) return;

        // 停止当前正在运行的动画
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }

        // 启动显示动画
        currentAnimationCoroutine = StartCoroutine(AnimateUI(true));
    }

    /// <summary>
    /// 隐藏飞船UI（带弹性动画）
    /// </summary>
    private void HideSpaceshipUI()
    {
        GameUIManager.instance.TechnologyLabUI.SetActive(false);
        if (spaceshipUICanvasGroup == null) return;

        // 停止当前正在运行的动画
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }

        // 启动隐藏动画
        currentAnimationCoroutine = StartCoroutine(AnimateUI(false));
    }

    /// <summary>
    /// UI动画协程
    /// </summary>
    /// <param name="show">true为显示，false为隐藏</param>
    private IEnumerator AnimateUI(bool show)
    {
        float startAlpha = spaceshipUICanvasGroup.alpha;
        float targetAlpha = show ? 1f : 0f;

        float startScale = show ? 0.8f : 1f;
        float targetScale = show ? 1f : 0.8f;

        float elapsedTime = 0f;

        // 启用/禁用交互
        spaceshipUICanvasGroup.interactable = show;
        spaceshipUICanvasGroup.blocksRaycasts = show;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;

            // 应用弹性效果
            float elasticProgress = ApplyElasticEffect(progress, elasticIntensity);

            // 渐变透明度
            spaceshipUICanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elasticProgress);

            // 缩放动画（仅当显示时）
            if (show)
            {
                float currentScale = Mathf.Lerp(startScale, targetScale, elasticProgress);
                spaceshipUICanvasGroup.transform.localScale = Vector3.one * currentScale;
            }

            yield return null;
        }

        // 确保最终状态正确
        spaceshipUICanvasGroup.alpha = targetAlpha;
        if (show)
        {
            spaceshipUICanvasGroup.transform.localScale = Vector3.one;
        }

        currentAnimationCoroutine = null;
    }

    /// <summary>
    /// 应用弹性动画效果
    /// </summary>
    private float ApplyElasticEffect(float progress, float intensity)
    {
        // 使用正弦函数创建弹性效果
        float elastic = Mathf.Sin(progress * Mathf.PI * 2f) * intensity;
        return progress + elastic * (1f - progress);
    }

    /// <summary>
    /// 手动设置飞船区域状态（用于外部调用）
    /// </summary>
    public void SetSpaceshipAreaState(bool inArea)
    {
        if (inArea != isInSpaceshipArea)
        {
            isInSpaceshipArea = inArea;
            if (inArea)
            {
                ShowSpaceshipUI();
            }
            else
            {
                HideSpaceshipUI();
            }
        }
    }

    /// <summary>
    /// 检查当前是否在飞船区域内
    /// </summary>
    public bool IsInSpaceshipArea()
    {
        return isInSpaceshipArea;
    }
}