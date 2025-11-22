using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiningSlot : MonoBehaviour
{
    //玩家Tag为Player
    public Ore faOre;
    //是否配置矿机了
    public bool isFull;
    //槽位图标
    public GameObject MingingSlot;
    //矿机图标
    public GameObject MiningMachine;
    
    // 玩家是否在附近
    private bool playerInRange = false;
    // 玩家引用
    private GameObject player;
    
    void Start()
    {
        // 初始化状态
        if (MingingSlot != null)
            MingingSlot.SetActive(false);
        
        if (MiningMachine != null)
            MiningMachine.SetActive(false);
        
        isFull = false;
    }

    void Update()
    {
        // 检测鼠标点击
        if (playerInRange && !isFull && Input.GetMouseButtonDown(0))
        {
            CheckClickOnSlot();
        }
    }

    // 当玩家进入触发区域
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.gameObject;
            
            // 显示槽位（如果没有矿机）
            if (!isFull && MingingSlot != null)
            {
                MingingSlot.SetActive(true);
            }
        }
    }

    // 当玩家离开触发区域
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
            
            // 隐藏槽位
            if (MingingSlot != null)
            {
                MingingSlot.SetActive(false);
            }
        }
    }

    // 检查是否点击了槽位
    private void CheckClickOnSlot()
    {
        // 创建射线从鼠标位置
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        // 检查是否点击了这个槽位
        if (hit.collider != null && hit.collider.gameObject == this.gameObject)
        {
            PlaceMiningMachine();
        }
    }

    // 放置矿机
    private void PlaceMiningMachine()
    {
        if (isFull) return;
        
        isFull = true;
        
        // 隐藏槽位，显示矿机
        if (MingingSlot != null)
            MingingSlot.SetActive(false);
        
        if (MiningMachine != null)
            MiningMachine.SetActive(true);
        
        Debug.Log("矿机已放置到槽位！");
        
        // 可以在这里添加放置矿机后的其他逻辑
        // 比如开始采矿、播放音效等
    }

    // 可选：移除矿机的方法
    public void RemoveMiningMachine()
    {
        if (!isFull) return;
        
        isFull = false;
        
        if (MiningMachine != null)
            MiningMachine.SetActive(false);
        
        // 如果玩家在附近，重新显示槽位
        if (playerInRange && MingingSlot != null)
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