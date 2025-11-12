using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class MiningMachine : MonoBehaviour
{
    [Header("矿机设置")]
    public float miningInterval = 2f;
    public int moneyPerMine = 10;
    public Ore assignedOre;
    public MiningSlot assignedSlot;
    
    private float miningTimer = 0f;
    
    void Start()
    {
        miningTimer = miningInterval;
        Debug.Log("矿机启动，开始自动采矿");
    }
    
    void Update()
    {
        if (assignedOre != null)
        {
            miningTimer -= Time.deltaTime;
            if (miningTimer <= 0f)
            {
                MineOre();
                miningTimer = miningInterval;
            }
        }
        else
        {
            // 如果没有分配矿石，尝试查找附近的矿石
            TryFindNearbyOre();
        }
    }
    
    public void AssignOre(Ore ore)
    {
        assignedOre = ore;
        if (ore != null)
        {
            Debug.Log($"矿机已分配矿石: {ore.name}");
        }
    }
    
    void MineOre()
    {
        if (assignedOre != null && GameValManager.gameValManager != null)
        {
            // 调用矿石的挖掘方法
            assignedOre.Mine();
            // 矿机自动采矿获得更多金币
            GameValManager.gameValManager.GetMoney(moneyPerMine);
            Debug.Log($"矿机自动采矿，获得金钱: {moneyPerMine}");
        }
    }
    
    void TryFindNearbyOre()
    {
        Ore[] ores = FindObjectsOfType<Ore>();
        if (ores.Length > 0)
        {
            Ore nearestOre = ores.OrderBy(ore => 
                Vector3.Distance(transform.position, ore.transform.position))
                .FirstOrDefault();
                
            if (nearestOre != null)
            {
                AssignOre(nearestOre);
                Debug.Log($"矿机自动找到附近矿石: {nearestOre.name}");
            }
        }
    }
    
    void OnDestroy()
    {
        Debug.Log("矿机被销毁");
    }
}