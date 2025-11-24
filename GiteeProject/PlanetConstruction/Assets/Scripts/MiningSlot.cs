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

    public GameObject MiningCanvas;
    // 玩家引用
    private GameObject player;
    public LayerMask clickLayerMask;

    void Start()
    {
        faOre.MiningSlots.Add(this);
        // 初始化状态
        if (MingingSlot != null)
            MingingSlot.SetActive(false);

        if (MiningMachine != null)
            MiningMachine.SetActive(false);

        MiningCanvas.SetActive(false);
        isFull = false;

        // 订阅Ore的事件
        if (faOre != null)
        {
            faOre.OnPlayerProximityChanged += HandlePlayerProximityChanged;
        }
        else
        {
            Debug.LogError("MiningSlot没有关联的Ore！");
        }
    }

    void OnDestroy()
    {
        // 取消订阅事件
        if (faOre != null)
        {
            faOre.OnPlayerProximityChanged -= HandlePlayerProximityChanged;
        }
    }

    void Update()
    {
        // 检测鼠标点击
        if (isPlayerInRange && Input.GetMouseButtonDown(0))
        {
            CheckClickOnSlot();
        }
    }

    // 处理玩家接近状态变化的事件
    private void HandlePlayerProximityChanged(bool playerInRange)
    {
        isPlayerInRange = playerInRange;

        if (playerInRange)
        {
            // 玩家进入范围，显示槽位（如果没有矿机）
            if (!isFull && MingingSlot != null)
            {
                MingingSlot.SetActive(true);
            }
        }
        else
        {
            // 玩家离开范围，隐藏槽位和UI
            if (MingingSlot != null)
            {
                MingingSlot.SetActive(false);
            }
            MiningCanvas.SetActive(false);
        }
    }

    //检查是否点击了槽位
    private void CheckClickOnSlot()
    {
        // 创建射线从鼠标位置
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, clickLayerMask);

        // 检查是否点击了可点击区域（小的碰撞器）
        if (hit.collider != null && hit.collider == clickableCollider)
        {


            if (MiningCanvas.activeInHierarchy)
            {
                for (int i = 0; i < faOre.MiningSlots.Count; i++)
                {
                    faOre.MiningSlots[i].MiningCanvas.SetActive(false);
                }
            }
            else
            {
                for (int i = 0; i < faOre.MiningSlots.Count; i++)
                {
                    faOre.MiningSlots[i].MiningCanvas.SetActive(false);
                }
                MiningCanvas.SetActive(true);
            }


        }
    }

    // 放置矿机
    public void PlaceMiningMachine()
    {

        if (isFull) return;
        if (GameValManager.gameValManager.valMoney >= 50)
        {
            GameValManager.gameValManager.valMoney -= 50;
        }
        else
        {
            return;
        }
        isFull = true;

        if (MiningMachine != null)
            MiningMachine.SetActive(true);

        Debug.Log("矿机已放置到槽位！");
        MiningCanvas.SetActive(false);

        // 隐藏槽位图标
        if (MingingSlot != null)
            MingingSlot.SetActive(false);

        // 通知Ore添加矿机
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

        // 通知Ore移除矿机
        faOre.RemoveMiningMachine(this);

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