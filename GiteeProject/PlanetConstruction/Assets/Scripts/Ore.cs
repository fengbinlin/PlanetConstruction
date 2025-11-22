using System.Collections;
using System.Collections.Generic;
using DamageNumbersPro.Demo;
using UnityEngine;

public class Ore : MonoBehaviour
{
    [Header("矿石设置")]
    public List<MiningSlot> MiningMachines;
    public int mineValue = 1; // 每次挖掘获得的金币数
    public int mineMachineMineOutMultiplier = 1;
    public float miningInterval = 2f; // 挖矿间隔时间（秒）
    
    private Coroutine batchMiningCoroutine;
    private bool isBatchMiningActive = false;

    void Start()
    {
        // 如果有矿机连接，启动批量挖矿协程
        if (MiningMachines.Count > 0)
        {
            StartBatchMining();
        }
    }

    public void Mine(int outputMultiplier = 1)
    {
        Debug.Log($"执行挖掘操作 - 矿石: {gameObject.name}");
        
        // 手动挖掘时也获得金币
        if (GameValManager.gameValManager != null)
        {
            int minedAmount = mineValue * outputMultiplier;
            GameValManager.gameValManager.GetMoney(minedAmount);
            
            // 弹出矿物提示，显示实际获得的金币数
            ShowMiningEffect(minedAmount, new Vector3(1f, 0.5f, 0));
            
            Debug.Log($"手动挖掘获得 {minedAmount} 金币");
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
            yield return new WaitForSeconds(miningInterval);
            
            // 计算所有矿机的总产出
            int totalMinedAmount = 0;
            int activeMiningMachines = 0;
            
            foreach (var miningMachine in MiningMachines)
            {
                if (miningMachine != null)
                {
                    totalMinedAmount += mineValue * mineMachineMineOutMultiplier;
                    activeMiningMachines++;
                }
            }
            
            // 如果有活跃的矿机，执行挖矿操作
            if (activeMiningMachines > 0 && GameValManager.gameValManager != null)
            {
                GameValManager.gameValManager.GetMoney(totalMinedAmount);
                
                // 弹出总产出提示
                ShowMiningEffect(totalMinedAmount, new Vector3(-1f, 0.5f, 0));
                
                Debug.Log($"批量挖矿: {activeMiningMachines}台矿机共获得 {totalMinedAmount} 金币，间隔: {miningInterval}秒");
            }
        }
    }

    // 显示挖矿效果（使用实际获得的金币数）
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

    void OnDestroy()
    {
        // 停止批量挖矿协程
        StopBatchMining();
    }
}