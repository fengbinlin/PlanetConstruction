using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiningSlot : MonoBehaviour
{
    //玩家Tag为Player
    public Ore faOre;
    //是否配置矿机了
    public bool isFull;
    private bool isPlayerInRange = false;
    //槽位图标
    public GameObject MingingSlot;
    //矿机图标
    public GameObject MiningMachine;
    
    // 可点击区域的碰撞器（小的）
    public Collider2D clickableCollider;
    // 触发器区域的碰撞器（大的）
    public Collider2D triggerCollider;
    
    public GameObject MiningCanvas;
    // 玩家引用
    private GameObject player;
    public LayerMask clickLayerMask;
    void Start()
    {
        // // 获取碰撞器组件
        // Collider2D[] colliders = GetComponents<Collider2D>();
        // foreach (Collider2D collider in colliders)
        // {
        //     if (collider.isTrigger)
        //     {
        //         triggerCollider = collider; // 触发器碰撞器（大的）
        //     }
        //     else
        //     {
        //         clickableCollider = collider; // 可点击碰撞器（小的）
        //     }
        // }
        
        // // 如果没有找到两个碰撞器，尝试在子对象中查找
        // if (triggerCollider == null || clickableCollider == null)
        // {
        //     Collider2D[] childColliders = GetComponentsInChildren<Collider2D>();
        //     foreach (Collider2D collider in childColliders)
        //     {
        //         if (collider.isTrigger && triggerCollider == null)
        //         {
        //             triggerCollider = collider;
        //         }
        //         else if (!collider.isTrigger && clickableCollider == null)
        //         {
        //             clickableCollider = collider;
        //         }
        //     }
        // }
        
        // 初始化状态
        if (MingingSlot != null)
            MingingSlot.SetActive(false);
        
        if (MiningMachine != null)
            MiningMachine.SetActive(false);
        MiningCanvas.SetActive(false);
        isFull = false;
    }

    void Update()
    {
        // 检测鼠标点击
        // if (isPlayerInRange && !isFull && Input.GetMouseButtonDown(0))
        // {
        //     CheckClickOnSlot();
        // }
    }

    // 当玩家进入触发区域
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            player = other.gameObject;
            
            // 显示槽位（如果没有矿机）
            if (!isFull && MingingSlot != null)
            {
                MingingSlot.SetActive(true);
                MiningCanvas.SetActive(true);
            }
        }
    }

    // 当玩家离开触发区域
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            player = null;
            
            // 隐藏槽位
            if (MingingSlot != null)
            {
                MingingSlot.SetActive(false);
                MiningCanvas.SetActive(false);
            }
        }
    }

    // 检查是否点击了槽位
    // private void CheckClickOnSlot()
    // {
    //     // 创建射线从鼠标位置
    //     Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //     RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, clickLayerMask);

    //     // 检查是否点击了可点击区域（小的碰撞器）
    //     if (hit.collider != null && hit.collider == clickableCollider)
    //     {
    //         PlaceMiningMachine();
    //     }
    // }

    // 放置矿机
    public void PlaceMiningMachine()
    {
        if (isFull) return;
        
        isFull = true;
        
        // 隐藏槽位，显示矿机
        if (MingingSlot != null)
            MingingSlot.SetActive(false);
        
        if (MiningMachine != null)
            MiningMachine.SetActive(true);
        
        Debug.Log("矿机已放置到槽位！");
        MiningCanvas.SetActive(false);
        // 可以在这里添加放置矿机后的其他逻辑
        // 比如开始采矿、播放音效等

        faOre.AddMiningMachine(this);
    }

    // 可选：移除矿机的方法
    public void RemoveMiningMachine()
    {
        if (!isFull) return;
        
        isFull = false;
        
        if (MiningMachine != null)
            MiningMachine.SetActive(false);
        
        // 如果玩家在附近，重新显示槽位
        if (isPlayerInRange && MingingSlot != null)
        {
            MingingSlot.SetActive(true);
        }
        
        Debug.Log("矿机已从槽位移除！");
    }

    // 可选：获取矿机状态的方法
    public bool IsSlotFull()
    {
        return isFull;
    }

    // 可选：获取关联矿石的方法
    public Ore GetConnectedOre()
    {
        return faOre;
    }
}