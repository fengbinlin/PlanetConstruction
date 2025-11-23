using UnityEngine;

public class DrillVibration : MonoBehaviour
{
    [Header("电钻震动设置")]
    public float vibrationIntensity = 0.05f;
    public float vibrationSpeed = 10f;
    public bool enableVibration = true;
    
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Ore connectedOre;
    private bool isVibrating = false;
    
    void Start()
    {
        // 保存原始位置和旋转
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }
    
    void Update()
    {
        if (!enableVibration) return;
        
        // 检查是否应该震动
        CheckVibrationStatus();
        
        // 根据状态应用震动
        if (isVibrating)
        {
            ApplyVibration();
        }
        else
        {
            ResetDrill();
        }
    }
    
    // 检查震动状态
    void CheckVibrationStatus()
    {
        if (connectedOre != null)
        {
            // 如果连接的矿石正在挖矿，就震动
            isVibrating = connectedOre.IsManualMiningActive();
        }
    }
    
    // 应用震动效果
    void ApplyVibration()
    {
        float time = Time.time * vibrationSpeed;
        
        // 位置震动
        float posX = Mathf.Sin(time * 1.3f) * vibrationIntensity;
        float posY = Mathf.Cos(time * 1.7f) * vibrationIntensity;
        transform.localPosition = originalPosition + new Vector3(posX, posY, 0);
        
        // 旋转震动（轻微摆动）
        float rotZ = Mathf.Sin(time * 0.8f) * vibrationIntensity * 5f;
        transform.localRotation = originalRotation * Quaternion.Euler(0, 0, rotZ);
    }
    
    // 重置电钻位置
    void ResetDrill()
    {
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
    }
    
    // 设置连接的矿石
    public void SetConnectedOre(Ore ore)
    {
        connectedOre = ore;
    }
    
    // 手动开始震动（用于测试）
    public void StartVibration()
    {
        isVibrating = true;
    }
    
    // 手动停止震动
    public void StopVibration()
    {
        isVibrating = false;
        ResetDrill();
    }
}