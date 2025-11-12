using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ore : MonoBehaviour
{
    [Header("矿石设置")]
    public List<MiningSlot> miningSlots;
    public int mineValue = 1; // 每次挖掘获得的金币数
    
    public void Mine(int outputMultiplier=1)
    {
        Debug.Log($"执行挖掘操作 - 矿石: {gameObject.name}");
        
        // 手动挖掘时也获得金币
        if (GameValManager.gameValManager != null)
        {
            GameValManager.gameValManager.GetMoney(mineValue*outputMultiplier);
            Debug.Log($"手动挖掘获得 {mineValue} 金币");
        }
    }
}