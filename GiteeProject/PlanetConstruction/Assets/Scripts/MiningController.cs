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
    
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryMineByClick();
        }
    }
    
    void TryMineByClick()
    {
        // 获取鼠标位置的世界坐标（2D）
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // 确保z坐标为0，因为这是2D游戏
        
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
                        ore.Mine();
                        Debug.Log($"开始挖掘矿石: {ore.name}");
                        
                        // 添加金币奖励
                        if (GameValManager.gameValManager != null)
                        {
                            // GameValManager.gameValManager.GetMoney(5); // 手动挖掘获得5金币
                            Debug.Log("手动挖掘获得5金币");
                        }
                        
                        // 找到一个有效的矿石后就返回，不继续检测
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
        
        // 如果没有找到有效的矿石，可以添加其他逻辑
        Debug.Log("没有点击到有效的矿石");
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
    
    // 可选：保留原来的自动寻找最近矿石的方法，但改为手动调用
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
            return distance <= miningRange && distance > minDistance;
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
            nearestOre.Mine();
            Debug.Log($"开始挖掘矿石: {nearestOre.name}");
            
            // 添加金币奖励
            if (GameValManager.gameValManager != null)
            {
                // GameValManager.gameValManager.GetMoney(5); // 手动挖掘获得5金币
                Debug.Log("手动挖掘获得5金币");
            }
        }
    }
}