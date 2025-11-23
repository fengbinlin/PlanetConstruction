using UnityEngine;

public class SimpleMiningShake : MonoBehaviour
{
    [Header("震动设置")]
    public float shakeIntensity = 0.1f;
    public float shakeSpeed = 5f;
    
    [Header("震动方向")]
    public bool shakeX = true;
    public bool shakeY = true;
    public bool shakeRotation = true;
    
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float timeOffset;
    
    void Start()
    {
        // 保存原始位置和旋转
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        
        // 添加随机时间偏移，使不同矿机震动不同步
        timeOffset = Random.Range(0f, 10f);
    }
    
    void Update()
    {
        // 计算基于时间的震动值
        float time = Time.time * shakeSpeed + timeOffset;
        float shakeValue = Mathf.Sin(time) * shakeIntensity;
        
        // 应用位置震动
        if (shakeX || shakeY)
        {
            Vector3 newPosition = originalPosition;
            if (shakeX) newPosition.x += Mathf.Sin(time * 1.3f) * shakeIntensity;
            if (shakeY) newPosition.y += Mathf.Cos(time * 1.7f) * shakeIntensity;
            transform.localPosition = newPosition;
        }
        
        // 应用旋转震动
        if (shakeRotation)
        {
            float rotation = Mathf.Sin(time * 0.8f) * shakeIntensity * 10f;
            transform.localRotation = originalRotation * Quaternion.Euler(0, 0, rotation);
        }
    }
}