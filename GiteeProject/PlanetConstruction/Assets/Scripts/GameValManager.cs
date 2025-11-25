using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum oreType
{
    normalOre,
}
[System.Serializable]
public class OreMineMachineNum
{
    public oreType myOreType;
    public int nums;
}

public class GameValManager : MonoBehaviour
{    public List<OreMineMachineNum> oreMineMachineNum;
    public static GameValManager gameValManager;
    
    [Header("基础数值")]
    public int valMoney = 0;
    public int valTechPoints = 0; // 新增科技点
    public Text moneyValUI;
    public Text techPointsUI; // 科技点UI
    
    [Header("采矿系统")]
    public float baseManualMiningOutput = 1.0f;    // 基础手动采矿产出
    public float baseMinerRate = 1.0f;            // 基础矿机产出速率
    public float totalManualMiningBonus = 0f;     // 总手动采矿加成
    public float totalMinerRateBonus = 0f;        // 总矿机速率加成
    public int unlockedMinerCount = 1;           // 已解锁的矿机数量
    
    [Header("自动采矿设置")]
    public float minerProductionAccumulator = 0f; // 矿机产出累积器
    public float minerProductionInterval = 1.0f;  // 矿机产出间隔（秒）
    
    // 事件
    public static event Action<float> OnManualMiningUpdated;
    public static event Action<float> OnMinerRateUpdated;
    public static event Action<int> OnMinerUnlocked;
    
    void Awake()
    {
        gameValManager = this;
    }
    
    void Start()
    {
        UpdateMoneyUI();
        UpdateTechPointsUI();
        
        // 初始化第一个矿机
        UnlockNewMiner();
    }

    void Update()
    {
        // 处理矿机自动产出
        // ProcessMinerProduction();
    }
    
    // 手动采矿方法
    // public void ManualMine()
    // {
    //     float miningAmount = GetCurrentManualMiningOutput();
    //     GetMoney(Mathf.RoundToInt(miningAmount));
        
    //     Debug.Log($"手动采矿获得: {miningAmount} 金币");
    // }
    
    // 处理矿机自动产出
    // void ProcessMinerProduction()
    // {
    //     minerProductionAccumulator += Time.deltaTime;
        
    //     if (minerProductionAccumulator >= minerProductionInterval)
    //     {
    //         float production = GetTotalMinerRate() * minerProductionInterval;
    //         GetMoney(Mathf.RoundToInt(production));
            
    //         minerProductionAccumulator = 0f;
    //     }
    // }
    
    // 获取当前手动采矿产出
    public float GetCurrentManualMiningOutput()
    {
        return baseManualMiningOutput + totalManualMiningBonus;
    }
    
    // 获取当前矿机总产出速率（每秒）
    public float GetTotalMinerRate()
    {
        return (baseMinerRate + totalMinerRateBonus) * unlockedMinerCount;
    }
    
    // 增加手动采矿加成
    public void AddManualMiningBonus(float bonus)
    {
        totalManualMiningBonus += bonus;
        OnManualMiningUpdated?.Invoke(GetCurrentManualMiningOutput());
        Debug.Log($"手动采矿加成增加: {bonus}, 当前总产出: {GetCurrentManualMiningOutput()}");
    }
    
    // 增加矿机速率加成
    public void AddMinerRateBonus(float bonus)
    {
        totalMinerRateBonus += bonus;
        OnMinerRateUpdated?.Invoke(GetTotalMinerRate());
        Debug.Log($"矿机速率加成增加: {bonus}, 当前总速率: {GetTotalMinerRate()}");
    }
    
    // 解锁新矿机
    public void UnlockNewMiner()
    {
        unlockedMinerCount++;
        OnMinerUnlocked?.Invoke(unlockedMinerCount);
        Debug.Log($"解锁新矿机! 当前矿机数量: {unlockedMinerCount}");
    }
    
    // 原有的金钱方法
    public void GetMoney(int val)
    {
        valMoney += val;
        UpdateMoneyUI();
    }
    
    // 新增科技点方法
    public void GetTechPoints(int val)
    {
        valTechPoints += val;
        UpdateTechPointsUI();
    }
    
    // 检查资源是否足够
    public bool HasEnoughResources(int goldCost, int techPointCost)
    {
        return valMoney >= goldCost && valTechPoints >= techPointCost;
    }
    
    // 消耗资源
    public bool SpendResources(int goldCost, int techPointCost)
    {
        if (!HasEnoughResources(goldCost, techPointCost)) return false;
        
        valMoney -= goldCost;
        valTechPoints -= techPointCost;
        
        UpdateMoneyUI();
        UpdateTechPointsUI();
        
        return true;
    }
    
    void UpdateMoneyUI()
    {
        if (moneyValUI != null)
        {
            moneyValUI.text = valMoney.ToString();
        }
    }
    
    void UpdateTechPointsUI()
    {
        if (techPointsUI != null)
        {
            techPointsUI.text = valTechPoints.ToString();
        }
    }
    
    // 测试方法：添加资源（开发时使用）
    [ContextMenu("添加测试资源")]
    public void AddTestResources()
    {
        GetMoney(1000);
        GetTechPoints(100);
    }
}