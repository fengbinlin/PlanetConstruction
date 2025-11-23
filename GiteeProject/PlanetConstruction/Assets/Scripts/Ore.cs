using System;
using System.Collections;
using System.Collections.Generic;
using DamageNumbersPro.Demo;
using UnityEngine;
using UnityEngine.UI;

public class Ore : MonoBehaviour
{
    [Header("矿石设置")]
    public List<MiningSlot> MiningMachines;
    public int mineValue = 1; // 基础每次挖掘获得的金币数
    public int mineMachineMineOutMultiplier = 1;
    public float baseMiningInterval = 2f; // 基础挖矿间隔时间（秒）
    
    [Header("手动挖矿进度条")]
    public Image miningProgressBar; // 进度条Image组件
    public float manualMiningTime = 2f; // 手动挖矿所需时间（秒）
    
    // 挖矿进度变化事件
    public event Action<float> OnMiningProgressChanged;
    
    // 玩家进入和离开矿石范围的事件
    public event Action<bool> OnPlayerProximityChanged;
    
    private Coroutine batchMiningCoroutine;
    private Coroutine manualMiningCoroutine;
    private bool isBatchMiningActive = false;
    private bool isManualMiningActive = false;
    private Collider2D oreTriggerCollider;
    private float currentMiningInterval; // 当前实际的挖矿间隔（受加成影响）
    private float currentMiningProgress = 0f; // 当前挖矿进度（0-1）

    void Start()
    {
        oreTriggerCollider = GetComponent<Collider2D>();
        if (oreTriggerCollider == null || !oreTriggerCollider.isTrigger)
        {
            Debug.LogError("Ore需要有一个触发器碰撞器用于检测玩家接近！");
        }
        
        // 初始化当前挖矿间隔
        currentMiningInterval = baseMiningInterval;
        
        // 初始化进度条
        if (miningProgressBar != null)
        {
            miningProgressBar.fillAmount = 0f;
            miningProgressBar.gameObject.SetActive(false);
        }
        
        // 如果有矿机连接，启动批量挖矿协程
        if (MiningMachines.Count > 0)
        {
            StartBatchMining();
        }
    }

    // 玩家进入触发器范围
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家进入矿石范围");
            OnPlayerProximityChanged?.Invoke(true);
        }
    }

    // 玩家离开触发器范围
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家离开矿石范围");
            OnPlayerProximityChanged?.Invoke(false);
        }
    }

    // 开始手动挖矿（外部调用）
    public bool StartManualMining()
    {
        if (isManualMiningActive)
        {
            Debug.Log("已经在挖矿中");
            return false;
        }
        
        if (manualMiningCoroutine != null)
            StopCoroutine(manualMiningCoroutine);
            
        manualMiningCoroutine = StartCoroutine(ManualMiningRoutine());
        return true;
    }
    
    // 停止手动挖矿
    public void StopManualMining()
    {
        if (isManualMiningActive)
        {
            if (manualMiningCoroutine != null)
                StopCoroutine(manualMiningCoroutine);
            isManualMiningActive = false;
            currentMiningProgress = 0f;
            
            // 触发进度变化事件
            OnMiningProgressChanged?.Invoke(0f);
            
            // 隐藏进度条
            if (miningProgressBar != null)
            {
                miningProgressBar.gameObject.SetActive(false);
            }
        }
    }

    // 手动挖矿协程
    private IEnumerator ManualMiningRoutine()
    {
        isManualMiningActive = true;
        currentMiningProgress = 0f;
        
        // 显示进度条
        if (miningProgressBar != null)
        {
            miningProgressBar.gameObject.SetActive(true);
            miningProgressBar.fillAmount = 0f;
        }
        
        float miningTime = 0f;
        float targetTime = manualMiningTime;
        
        Debug.Log($"开始手动挖矿，需要时间: {targetTime}秒");
        
        while (miningTime < targetTime)
        {
            miningTime += Time.deltaTime;
            currentMiningProgress = miningTime / targetTime;
            
            // 更新进度条
            if (miningProgressBar != null)
            {
                miningProgressBar.fillAmount = currentMiningProgress;
            }
            
            // 触发进度变化事件
            OnMiningProgressChanged?.Invoke(currentMiningProgress);
            
            yield return null;
        }
        
        // 挖矿完成，获得金币
        CompleteManualMining();
    }
    
    // 完成手动挖矿
    private void CompleteManualMining()
    {
        // 获取基础产出并应用手动采矿加成
        float baseMinedAmount = mineValue;
        float totalManualMiningBonus = GameValManager.gameValManager != null ? 
            GameValManager.gameValManager.totalManualMiningBonus : 0;
        float finalMinedAmount = baseMinedAmount + totalManualMiningBonus;
        
        // 确保至少获得基础产出
        int minedAmount = Mathf.Max(Mathf.RoundToInt(finalMinedAmount), mineValue);
        
        if (GameValManager.gameValManager != null)
        {
            GameValManager.gameValManager.GetMoney(minedAmount);
        }
        
        // 弹出矿物提示
        ShowMiningEffect(minedAmount, new Vector3(1f, 0.5f, 0));
        
        Debug.Log($"手动挖掘完成: 基础 {baseMinedAmount} + 加成 {totalManualMiningBonus} = 总共 {minedAmount} 金币");
        
        // 重置状态
        isManualMiningActive = false;
        currentMiningProgress = 0f;
        
        // 触发进度变化事件
        OnMiningProgressChanged?.Invoke(0f);
        
        // 隐藏进度条
        if (miningProgressBar != null)
        {
            miningProgressBar.gameObject.SetActive(false);
        }
    }

    // 启动批量挖矿协程
    public void StartBatchMining()
    {
        if (!isBatchMiningActive && MiningMachines.Count > 0)
        {
            batchMiningCoroutine = StartCoroutine(BatchMiningRoutine());
            isBatchMiningActive = true;
        }
    }

    // 停止批量挖矿
    public void StopBatchMining()
    {
        if (isBatchMiningActive)
        {
            if (batchMiningCoroutine != null)
                StopCoroutine(batchMiningCoroutine);
            isBatchMiningActive = false;
        }
    }

    // 批量挖矿协程
    private IEnumerator BatchMiningRoutine()
    {
        while (true)
        {
            // 更新当前的挖矿间隔（考虑矿机速率加成）
            UpdateMiningInterval();
            
            // 等待挖矿间隔时间
            yield return new WaitForSeconds(currentMiningInterval);
            
            // 计算所有矿机的总产出
            if (GameValManager.gameValManager != null && MiningMachines.Count > 0)
            {
                // 自动采矿不应用产出加成，只应用时间间隔加成
                int totalMinedAmount = mineValue * mineMachineMineOutMultiplier * MiningMachines.Count;
                
                GameValManager.gameValManager.GetMoney(totalMinedAmount);
                
                // 弹出总产出提示
                ShowMiningEffect(totalMinedAmount, new Vector3(-1f, 0.5f, 0));
                
                Debug.Log($"批量挖矿: {MiningMachines.Count}台矿机获得 {totalMinedAmount} 金币，间隔: {currentMiningInterval:F2}秒 (基础间隔: {baseMiningInterval}秒)");
            }
        }
    }
    
    // 更新挖矿间隔（应用矿机速率加成）
    private void UpdateMiningInterval()
    {
        if (GameValManager.gameValManager != null)
        {
            // 矿机速率加成越高，挖矿间隔越短
            float totalMinerRateBonus = GameValManager.gameValManager.totalMinerRateBonus;
            
            // 计算加成后的挖矿间隔
            currentMiningInterval = baseMiningInterval / (1f + totalMinerRateBonus);
            
            // 设置最小间隔限制，避免间隔过短导致性能问题
            currentMiningInterval = Mathf.Max(currentMiningInterval, 0.1f);
        }
        else
        {
            currentMiningInterval = baseMiningInterval;
        }
    }

    // 显示挖矿效果
    public void ShowMiningEffect(int minedAmount, Vector3 offset)
    {
        if (DNP_2DDemo.instance != null)
        {
            DNP_2DDemo.instance.CreateDamageNumber(transform.position + offset, minedAmount);
        }
    }

    // 添加矿机到列表
    public void AddMiningMachine(MiningSlot newMiningMachine)
    {
        if (newMiningMachine != null && !MiningMachines.Contains(newMiningMachine))
        {
            MiningMachines.Add(newMiningMachine);
            
            // 如果没有启动批量挖矿，启动它
            if (!isBatchMiningActive)
            {
                StartBatchMining();
            }
        }
    }

    // 从列表移除矿机
    public void RemoveMiningMachine(MiningSlot miningMachine)
    {
        if (miningMachine != null && MiningMachines.Contains(miningMachine))
        {
            MiningMachines.Remove(miningMachine);
            
            // 如果没有矿机了，停止批量挖矿
            if (MiningMachines.Count == 0)
            {
                StopBatchMining();
            }
        }
    }

    // 获取当前手动采矿产出（用于UI显示等）
    public float GetCurrentManualMiningOutput()
    {
        float baseOutput = mineValue;
        float bonus = GameValManager.gameValManager != null ? GameValManager.gameValManager.totalManualMiningBonus : 0;
        return baseOutput + bonus;
    }
    
    // 获取当前自动采矿产出速率（每秒，用于UI显示等）
    public float GetCurrentAutoMiningRate()
    {
        if (MiningMachines.Count == 0) return 0;
        
        // 每秒产出 = (单次产出 × 矿机数量) / 当前间隔
        float outputPerCycle = mineValue * mineMachineMineOutMultiplier * MiningMachines.Count;
        return outputPerCycle / currentMiningInterval;
    }
    
    // 获取当前挖矿间隔（用于UI显示等）
    public float GetCurrentMiningInterval()
    {
        return currentMiningInterval;
    }
    
    // 获取挖矿效率提升百分比（用于UI显示等）
    public float GetMiningEfficiencyPercentage()
    {
        if (baseMiningInterval <= 0) return 0;
        
        // 效率提升 = (基础间隔 - 当前间隔) / 基础间隔 × 100%
        float efficiency = (baseMiningInterval - currentMiningInterval) / baseMiningInterval * 100f;
        return Mathf.Max(0, efficiency);
    }
    
    // 获取手动挖矿时间（用于UI显示等）
    public float GetManualMiningTime()
    {
        return manualMiningTime;
    }
    
    // 设置手动挖矿时间
    public void SetManualMiningTime(float time)
    {
        manualMiningTime = Mathf.Max(0.1f, time); // 最小0.1秒
    }
    
    // 检查是否正在手动挖矿
    public bool IsManualMiningActive()
    {
        return isManualMiningActive;
    }
    
    // 获取当前挖矿进度（0-1）
    public float GetCurrentMiningProgress()
    {
        return currentMiningProgress;
    }

    void OnDestroy()
    {
        // 停止所有协程
        StopBatchMining();
        StopManualMining();
    }
}