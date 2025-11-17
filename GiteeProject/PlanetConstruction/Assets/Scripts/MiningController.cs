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
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryMine();
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            TryPlaceMiningMachine();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            TryRemoveMiningMachine();
        }
    }
    
    void TryMine()
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
            float distance = Vector3.Distance(transform.position, ore.transform.position);
            return distance <= miningRange && distance > minDistance;
        }).ToArray();
        
        if (validOres.Length == 0)
        {
            Debug.Log("没有找到符合条件的矿石");
            return;
        }
        
        Ore nearestOre = validOres.OrderBy(ore => 
            Vector3.Distance(transform.position, ore.transform.position))
            .FirstOrDefault();
        
        if (nearestOre != null)
        {
            nearestOre.Mine();
            Debug.Log($"开始挖掘矿石: {nearestOre.name}");
            
            // 添加金币奖励
            if (GameValManager.gameValManager != null)
            {
                GameValManager.gameValManager.GetMoney(5); // 手动挖掘获得5金币
                Debug.Log("手动挖掘获得5金币");
            }
        }
    }
    
    void TryPlaceMiningMachine()
    {
        if (miningMachinePrefab == null)
        {
            Debug.LogError("矿机预制体未赋值！请在Inspector中为miningMachinePrefab赋值");
            return;
        }
        
        // 查找所有矿机插槽
        MiningSlot[] allSlots = FindObjectsOfType<MiningSlot>();
        
        if (allSlots.Length == 0)
        {
            Debug.Log("场景中没有发现矿机插槽");
            return;
        }
        
        Debug.Log($"找到 {allSlots.Length} 个矿机插槽");
        
        // 筛选在范围内的插槽，并只选择空的插槽
        var emptySlotsInRange = allSlots
            .Where(slot => 
            {
                float distance = Vector3.Distance(transform.position, slot.transform.position);
                bool inRange = distance <= miningRange;
                bool isEmpty = slot.miningMachine == null;
                
                if (!inRange) Debug.Log($"插槽 {slot.name} 距离 {distance:F2} 超出范围");
                if (!isEmpty) Debug.Log($"插槽 {slot.name} 已有矿机");
                
                return inRange && isEmpty;
            })
            .ToArray();
        
        Debug.Log($"范围内找到 {emptySlotsInRange.Length} 个空插槽");
        
        if (emptySlotsInRange.Length == 0)
        {
            Debug.Log("没有在范围内找到空的矿机插槽");
            return;
        }
        
        // 找到最近的空插槽
        MiningSlot nearestEmptySlot = emptySlotsInRange
            .OrderBy(slot => Vector3.Distance(transform.position, slot.transform.position))
            .FirstOrDefault();
        
        if (nearestEmptySlot != null&&GameValManager.gameValManager.valMoney>50)
        {
            Debug.Log($"找到最近空插槽: {nearestEmptySlot.name}, 距离: {Vector3.Distance(transform.position, nearestEmptySlot.transform.position):F2}");
            GameValManager.gameValManager.GetMoney(-50);
            // 在插槽位置生成矿机
            GameObject miningMachineObj = Instantiate(miningMachinePrefab, 
                nearestEmptySlot.transform.position, 
                nearestEmptySlot.transform.rotation,MainRoot.instance.MainScene.gameObject.transform);
            
            // 确保矿机有MiningMachine组件
            MiningMachine miningMachine = miningMachineObj.GetComponent<MiningMachine>();
            if (miningMachine == null)
            {
                miningMachine = miningMachineObj.AddComponent<MiningMachine>();
                Debug.Log("为矿机添加了MiningMachine组件");
            }
            
            // 建立双向关联
            nearestEmptySlot.miningMachine = miningMachine;
            miningMachine.assignedSlot = nearestEmptySlot;
            
            // 为矿机分配最近的矿石
            Ore nearestOre = FindNearestOre(nearestEmptySlot.transform.position);
            if (nearestOre != null)
            {
                miningMachine.AssignOre(nearestOre);
                Debug.Log($"矿机已分配矿石: {nearestOre.name}");
            }
            else
            {
                Debug.LogWarning("附近没有找到矿石");
            }
            
            Debug.Log($"成功在插槽 {nearestEmptySlot.name} 放置矿机");
        }
    }
    
    void TryRemoveMiningMachine()
    {
        // 查找所有矿机
        MiningMachine[] allMachines = FindObjectsOfType<MiningMachine>();
        
        if (allMachines.Length == 0)
        {
            Debug.Log("场景中没有发现矿机");
            return;
        }
        
        // 筛选在范围内的矿机
        var validMachines = allMachines.Where(machine => 
            Vector3.Distance(transform.position, machine.transform.position) <= miningRange)
            .ToArray();
        
        if (validMachines.Length == 0)
        {
            Debug.Log("没有在范围内找到矿机");
            return;
        }
        
        // 找到最近的矿机
        MiningMachine nearestMachine = validMachines
            .OrderBy(machine => Vector3.Distance(transform.position, machine.transform.position))
            .FirstOrDefault();
        
        if (nearestMachine != null)
        {
            if (nearestMachine.assignedSlot != null)
            {
                nearestMachine.assignedSlot.miningMachine = null;
                Debug.Log("已从插槽移除矿机引用");
            }
            
            Destroy(nearestMachine.gameObject);
            Debug.Log("成功拆卸矿机");
        }
    }
    
    Ore FindNearestOre(Vector3 position)
    {
        Ore[] allOres = FindObjectsOfType<Ore>();
        if (allOres.Length == 0) return null;
        
        return allOres.OrderBy(ore => Vector3.Distance(position, ore.transform.position))
                      .FirstOrDefault();
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, miningRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minDistance);
    }
}