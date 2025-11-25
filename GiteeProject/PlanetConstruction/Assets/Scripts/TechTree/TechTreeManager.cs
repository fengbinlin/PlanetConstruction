using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class TechTreeManager : MonoBehaviour
{
    [Header("预制体")]
    public GameObject techNodePrefab;
    public Transform nodeContainer; // 节点容器
    
    [Header("详细信息面板")]
    public GameObject detailPanel;
    public Text techNameText;
    public Text descriptionText;
    public Text costText;
    public Text prerequisitesText;
    public Text effectsText;
    public Button researchButton;
    
    [Header("布局设置")]
    public float horizontalSpacing = 200f;
    public float verticalSpacing = 150f;
    public Vector2 startPosition = new Vector2(-400, 0);
    
    private Dictionary<int, TechTreeNode> techNodes = new Dictionary<int, TechTreeNode>();
    private TechTreeNode selectedNode;
    private TechResearchLab researchLab;
    
    void Start()
    {
        researchLab = GetComponent<TechResearchLab>();
        if (researchLab == null)
        {
            Debug.LogError("TechResearchLab component not found!");
            return;
        }
        
        // 正确的事件订阅方式
        researchLab.OnTechnologyResearched += OnTechnologyResearched;
        
        InitializeTechTree();
        CreateTechTreeUI();
        HideDetailPanel();
    }
    
    void InitializeTechTree()
    {
        // 确保科技数据已初始化
        if (researchLab.allTechnologies.Count == 0)
        {
            researchLab.InitializeTechnologies();
        }
    }
    
    void CreateTechTreeUI()
    {
        if (techNodePrefab == null || nodeContainer == null) return;
        
        // 清空现有节点
        foreach (Transform child in nodeContainer)
        {
            Destroy(child.gameObject);
        }
        techNodes.Clear();
        
        // 计算每个科技的层级（基于前置关系）
        Dictionary<int, int> techLevels = CalculateTechLevels();
        
        // 按层级分组科技
        Dictionary<int, List<Technology>> techsByLevel = new Dictionary<int, List<Technology>>();
        foreach (var tech in researchLab.allTechnologies)
        {
            int level = techLevels.ContainsKey(tech.id) ? techLevels[tech.id] : 0;
            
            if (!techsByLevel.ContainsKey(level))
            {
                techsByLevel[level] = new List<Technology>();
            }
            techsByLevel[level].Add(tech);
        }
        
        // 创建节点
        foreach (var levelPair in techsByLevel)
        {
            int level = levelPair.Key;
            List<Technology> techsInLevel = levelPair.Value;
            
            // 计算垂直位置（居中排列）
            float totalHeight = (techsInLevel.Count - 1) * verticalSpacing;
            float startY = totalHeight / 2f;
            
            for (int i = 0; i < techsInLevel.Count; i++)
            {
                Technology tech = techsInLevel[i];
                Vector2 position = new Vector2(
                    startPosition.x + level * horizontalSpacing,
                    startPosition.y + startY - i * verticalSpacing
                );
                
                CreateTechNode(tech, position);
            }
        }
        
        // 绘制连接线
        DrawConnectionLines();
    }
    
    Dictionary<int, int> CalculateTechLevels()
    {
        Dictionary<int, int> levels = new Dictionary<int, int>();
        
        // 初始化所有科技层级为-1（未计算）
        foreach (var tech in researchLab.allTechnologies)
        {
            levels[tech.id] = -1;
        }
        
        bool changed;
        do
        {
            changed = false;
            foreach (var tech in researchLab.allTechnologies)
            {
                if (levels[tech.id] != -1) continue; // 已经计算过
                
                if (tech.prerequisites.Count == 0)
                {
                    // 没有前置条件，层级为0
                    levels[tech.id] = 0;
                    changed = true;
                }
                else
                {
                    // 检查所有前置条件是否都已计算层级
                    int maxPrereqLevel = -1;
                    bool allPrereqsCalculated = true;
                    
                    foreach (int prereqId in tech.prerequisites)
                    {
                        if (levels[prereqId] == -1)
                        {
                            allPrereqsCalculated = false;
                            break;
                        }
                        maxPrereqLevel = Mathf.Max(maxPrereqLevel, levels[prereqId]);
                    }
                    
                    if (allPrereqsCalculated)
                    {
                        levels[tech.id] = maxPrereqLevel + 1;
                        changed = true;
                    }
                }
            }
        } while (changed);
        
        return levels;
    }
    
    void CreateTechNode(Technology tech, Vector2 position)
    {
        GameObject nodeObj = Instantiate(techNodePrefab, nodeContainer);
        nodeObj.transform.localPosition = position;
        
        TechTreeNode node = nodeObj.GetComponent<TechTreeNode>();
        if (node != null)
        {
            node.Initialize(tech, this);
            techNodes[tech.id] = node;
        }
    }
    
    void DrawConnectionLines()
    {
        foreach (var nodePair in techNodes)
        {
            TechTreeNode node = nodePair.Value;
            Technology tech = node.GetTechnology();
            
            // 为每个前置科技绘制连接线
            foreach (int prereqId in tech.prerequisites)
            {
                if (techNodes.ContainsKey(prereqId))
                {
                    node.DrawConnectionTo(techNodes[prereqId]);
                }
            }
        }
    }
    
    public void SelectTechnology(TechTreeNode node)
    {
        // 取消之前的选择
        if (selectedNode != null)
        {
            selectedNode.SetSelected(false);
        }
        
        selectedNode = node;
        selectedNode.SetSelected(true);
        
        ShowTechnologyDetails(node.GetTechnology());
    }
    
    void ShowTechnologyDetails(Technology tech)
    {
        if (detailPanel == null) return;
        
        detailPanel.SetActive(true);
        
        // 更新详细信息
        techNameText.text = tech.techName;
        descriptionText.text = tech.description;
        costText.text = $"金币: {tech.costGold}\n科技点: {tech.costTechPoint}";
        
        // 前置条件
        if (tech.prerequisites.Count > 0)
        {
            string prereqText = "";
            foreach (int prereqId in tech.prerequisites)
            {
                Technology prereqTech = researchLab.GetTechnologyById(prereqId);
                if (prereqTech != null)
                {
                    prereqText += prereqTech.techName + "\n";
                }
            }
            prerequisitesText.text = prereqText;
        }
        else
        {
            prerequisitesText.text = "无";
        }
        
        // 效果描述
        string effects = "";
        if (tech.manualMiningBonus > 0)
            effects += $"手动采矿效率 +{tech.manualMiningBonus}\n";
        if (tech.minerRateBonus > 0)
            effects += $"矿机效率 +{tech.minerRateBonus}\n";
        if (tech.unlocksNewMiner)
            effects += "解锁新矿机\n";
        
        effectsText.text = string.IsNullOrEmpty(effects) ? "无特殊效果" : effects;
        
        // 研究按钮状态
        researchButton.interactable = researchLab.CanResearchTechnology(tech.id);
        researchButton.onClick.RemoveAllListeners();
        researchButton.onClick.AddListener(() => ResearchSelectedTechnology());
    }
    
    void HideDetailPanel()
    {
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }
    }
    
    void ResearchSelectedTechnology()
    {
        if (selectedNode != null && researchLab != null)
        {
            bool success = researchLab.ResearchTechnology(selectedNode.GetTechId());
            if (success)
            {
                // 更新节点外观
                selectedNode.UpdateNodeAppearance();
                // 重新绘制连接线（可能颜色变化）
                DrawConnectionLines();
                // 更新详情面板按钮状态
                researchButton.interactable = false;
                
                // 不需要手动调用OnTechnologyResearched，因为ResearchLab内部会触发事件
            }
        }
    }
    
    // 正确的事件处理方法
    void OnTechnologyResearched(Technology tech)
    {
        // 当科技研究完成时更新对应的节点
        if (techNodes.ContainsKey(tech.id))
        {
            techNodes[tech.id].UpdateNodeAppearance();
        }
        
        // 重新绘制连接线
        DrawConnectionLines();
        
        // 如果当前选中的科技被研究了，更新详情面板
        if (selectedNode != null && selectedNode.GetTechId() == tech.id)
        {
            researchButton.interactable = false;
        }
    }
    
    void OnDestroy()
    {
        // 正确的事件取消订阅方式
        if (researchLab != null)
        {
            researchLab.OnTechnologyResearched -= OnTechnologyResearched;
        }
    }
    
    // 添加关闭详情面板的方法
    public void CloseDetailPanel()
    {
        if (selectedNode != null)
        {
            selectedNode.SetSelected(false);
            selectedNode = null;
        }
        HideDetailPanel();
    }
}