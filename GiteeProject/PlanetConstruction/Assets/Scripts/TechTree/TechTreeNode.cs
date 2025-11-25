using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

// 科技节点控制器
public class TechTreeNode : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI组件")]
    public Image nodeBackground;
    public TextMeshProUGUI techNameText;
    public Image iconImage;
    public GameObject lockedOverlay;
    public GameObject researchedOverlay;
    public GameObject availableIndicator;
    
    [Header("连接线")]
    public LineRenderer connectionLine;
    public Transform lineStartPoint;
    public Transform lineEndPoint;
    
    [Header("状态颜色")]
    public Color availableColor = Color.green;
    public Color unavailableColor = Color.gray;
    public Color researchedColor = Color.blue;
    public Color lockedColor = Color.red;
    
    private Technology assignedTechnology;
    private TechTreeManager treeManager;
    private bool isSelected = false;
    
    public void Initialize(Technology tech, TechTreeManager manager)
    {
        assignedTechnology = tech;
        treeManager = manager;
        
        UpdateNodeAppearance();
        
        // 设置节点名称
        techNameText.text = tech.techName;
    }
    
    public void UpdateNodeAppearance()
    {
        if (assignedTechnology == null) return;
        
        if (assignedTechnology.isResearched)
        {
            // 已研究
            nodeBackground.color = researchedColor;
            lockedOverlay.SetActive(false);
            researchedOverlay.SetActive(true);
            availableIndicator.SetActive(false);
        }
        else if (assignedTechnology.isUnlocked)
        {
            // 已解锁可研究
            nodeBackground.color = availableColor;
            lockedOverlay.SetActive(false);
            researchedOverlay.SetActive(false);
            availableIndicator.SetActive(true);
        }
        else
        {
            // 未解锁
            nodeBackground.color = lockedColor;
            lockedOverlay.SetActive(true);
            researchedOverlay.SetActive(false);
            availableIndicator.SetActive(false);
        }
        
        // 更新选中状态
        if (isSelected)
        {
            nodeBackground.color = Color.yellow;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (assignedTechnology != null && treeManager != null)
        {
            treeManager.SelectTechnology(this);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 鼠标悬停效果
        transform.localScale = Vector3.one * 1.1f;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // 恢复原始大小
        transform.localScale = Vector3.one;
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateNodeAppearance();
    }
    
    public Technology GetTechnology()
    {
        return assignedTechnology;
    }
    
    public int GetTechId()
    {
        return assignedTechnology != null ? assignedTechnology.id : -1;
    }
    
    // 绘制连接到其他节点的线
    public void DrawConnectionTo(TechTreeNode targetNode)
    {
        if (connectionLine == null || targetNode == null) return;
        
        connectionLine.positionCount = 2;
        connectionLine.SetPosition(0, lineStartPoint.position);
        connectionLine.SetPosition(1, targetNode.lineEndPoint.position);
        
        // 根据连接状态设置线条颜色
        if (assignedTechnology.isResearched && targetNode.assignedTechnology.isUnlocked)
        {
            connectionLine.startColor = availableColor;
            connectionLine.endColor = availableColor;
        }
        else
        {
            connectionLine.startColor = lockedColor;
            connectionLine.endColor = lockedColor;
        }
    }
}