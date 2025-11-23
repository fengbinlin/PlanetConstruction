using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MiningController : MonoBehaviour
{
    [Header("挖掘设置")]
    public float miningRange = 5f;
    public float minDistance = 2f;
    public GameObject miningMachinePrefab; // 确保在Inspector中赋值
    public LayerMask oreLayerMask = -1; // 用于射线检测的图层
    
    [Header("电钻震动")]
    public DrillVibration drillVibration; // 电钻震动脚本引用
    
    private Camera mainCamera;
    private Ore currentMiningOre; // 当前正在挖掘的矿石
    private bool isMining = false; // 是否正在挖矿
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // 如果没有手动指定电钻震动脚本，尝试自动查找
        if (drillVibration == null)
        {
            drillVibration = GetComponentInChildren<DrillVibration>();
            if (drillVibration == null)
            {
                Debug.LogWarning("未找到电钻震动脚本，将不会显示挖矿震动效果");
            }
        }
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isMining)
        {
            TryStartMiningByClick();
        }
        
        // 如果正在挖矿但玩家移动了，停止挖矿
        if (isMining && currentMiningOre != null)
        {
            float distance = Vector2.Distance(transform.position, currentMiningOre.transform.position);
            if (distance > miningRange || distance <= minDistance)
            {
                StopMining();
                Debug.Log("距离变化，停止挖矿");
            }
        }
        
        // 测试用：按空格键手动触发电钻震动
        if (Input.GetKeyDown(KeyCode.Space) && drillVibration != null)
        {
            drillVibration.StartVibration();
        }
    }
    
    void TryStartMiningByClick()
    {
        // 获取鼠标位置的世界坐标（2D）
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        
        // 使用2D射线检测
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero, 0f, oreLayerMask);
        
        // 如果没有检测到任何碰撞体，尝试从鼠标位置发射一条短距离射线
        if (hits.Length == 0)
        {
            hits = Physics2D.RaycastAll(mousePosition, Vector2.zero, 0.1f, oreLayerMask);
        }
        
        // 按距离排序，从近到远
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                print("检测到碰撞体: " + hit.collider.gameObject.name);
                Ore ore = hit.collider.GetComponent<Ore>();
                
                if (ore != null)
                {
                    print("找到矿石组件");
                    // 检查距离条件
                    float distance = Vector2.Distance(transform.position, ore.transform.position);
                    
                    if (distance <= miningRange && distance > minDistance)
                    {
                        // 开始挖矿
                        StartMining(ore);
                        return;
                    }
                    else
                    {
                        if (distance <= minDistance)
                        {
                            Debug.Log("距离太近，无法挖掘");
                        }
                        else if (distance > miningRange)
                        {
                            Debug.Log("距离太远，无法挖掘");
                        }
                    }
                }
            }
        }
        
        Debug.Log("没有点击到有效的矿石");
    }
    
    // 开始挖矿
    void StartMining(Ore ore)
    {
        if (ore.IsManualMiningActive())
        {
            Debug.Log("该矿石正在被挖掘");
            return;
        }
        
        if (ore.StartManualMining())
        {
            currentMiningOre = ore;
            isMining = true;
            
            // 设置电钻震动连接的矿石
            if (drillVibration != null)
            {
                drillVibration.SetConnectedOre(ore);
            }
            
            // 监听挖矿完成事件
            StartCoroutine(WaitForMiningComplete(ore));
            
            Debug.Log($"开始挖掘矿石: {ore.name}");
        }
    }
    
    // 停止挖矿
    void StopMining()
    {
        if (currentMiningOre != null)
        {
            currentMiningOre.StopManualMining();
        }
        
        // 停止电钻震动
        if (drillVibration != null)
        {
            drillVibration.StopVibration();
        }
        
        currentMiningOre = null;
        isMining = false;
    }
    
    // 等待挖矿完成
    IEnumerator WaitForMiningComplete(Ore ore)
    {
        // 等待直到挖矿完成或停止
        while (ore.IsManualMiningActive() && isMining)
        {
            yield return null;
        }
        
        // 挖矿完成
        if (isMining)
        {
            Debug.Log("挖矿完成");
        }
        
        // 停止电钻震动
        if (drillVibration != null)
        {
            drillVibration.StopVibration();
        }
        
        currentMiningOre = null;
        isMining = false;
    }
    
    // 在场景视图中绘制挖掘范围，便于调试
    void OnDrawGizmosSelected()
    {
        // 绘制最小距离圆
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minDistance);
        
        // 绘制最大挖掘范围圆
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, miningRange);
    }
    
    // 可选：保留原来的自动寻找最近矿石的方法
    void TryMineNearest()
    {
        Ore[] allOres = FindObjectsOfType<Ore>();
        
        if (allOres.Length == 0)
        {
            Debug.Log("范围内没有发现矿石");
            return;
        }
        
        // 筛选符合条件的矿石
        var validOres = allOres.Where(ore => 
        {
            float distance = Vector2.Distance(transform.position, ore.transform.position);
            return distance <= miningRange && distance > minDistance && !ore.IsManualMiningActive();
        }).ToArray();
        
        if (validOres.Length == 0)
        {
            Debug.Log("没有找到符合条件的矿石");
            return;
        }
        
        Ore nearestOre = validOres.OrderBy(ore => 
            Vector2.Distance(transform.position, ore.transform.position))
            .FirstOrDefault();
        
        if (nearestOre != null)
        {
            StartMining(nearestOre);
        }
    }
}