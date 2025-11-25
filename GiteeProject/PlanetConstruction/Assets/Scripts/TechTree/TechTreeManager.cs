using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; 
public class TechTreeManager : MonoBehaviour
{
    [Header("预制体")]
    public GameObject techNodePrefab;
    public GameObject connectionLinePrefab;
    public Transform nodeContainer;
    public Transform connectionLinesContainer;

    [Header("详细信息面板")]
    public GameObject detailPanel;
    public Text techNameText;
    public Text descriptionText;
    public Text costText;
    public Text prerequisitesText;
    public Text effectsText;
    public Button researchButton;
    public Button closeButton;

    [Header("布局设置")]
    public float horizontalSpacing = 200f;
    public float verticalSpacing = 150f;
    public Vector2 startPosition = new Vector2(-400, 0);

    private Dictionary<int, TechTreeNode> techNodes = new Dictionary<int, TechTreeNode>();
    private List<ConnectionLine> connectionLines = new List<ConnectionLine>();
    private TechTreeNode selectedNode;
    private TechResearchLab researchLab;

    void Start()
    {
        Debug.Log("TechTreeManager Start called");

        researchLab = GetComponent<TechResearchLab>();
        if (researchLab == null)
        {
            Debug.LogError("TechResearchLab component not found!");
            return;
        }

        Debug.Log("TechResearchLab found successfully");

        // 订阅事件
        researchLab.OnTechnologyResearched += OnTechnologyResearched;

        InitializeTechTree();
        CreateTechTreeUI();
        HideDetailPanel();

        // 设置关闭按钮
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseDetailPanel);
        }

        // 【新增】创建完成后立即更新所有节点UI
        StartCoroutine(UpdateNodesUIAfterFrame());
    }
    IEnumerator UpdateNodesUIAfterFrame()
    {
        yield return null; // 等待一帧
        UpdateAllNodesUI();
        Debug.Log("初始节点UI更新完成");
    }
    void InitializeTechTree()
    {
        Debug.Log($"初始化科技树，当前科技数量: {researchLab.allTechnologies.Count}");

        if (researchLab.allTechnologies.Count == 0)
        {
            Debug.Log("科技列表为空，调用InitializeTechnologies");
            researchLab.InitializeTechnologies();
        }

        Debug.Log($"初始化后科技数量: {researchLab.allTechnologies.Count}");
    }

    void CreateTechTreeUI()
    {
        Debug.Log("开始创建科技树UI");

        if (techNodePrefab == null)
        {
            Debug.LogError("Tech Node Prefab 未赋值!");
            return;
        }

        if (nodeContainer == null)
        {
            Debug.LogError("Node Container 未赋值!");
            return;
        }

        Debug.Log("预制体和容器检查通过");

        // 清空现有节点和连接线
        foreach (Transform child in nodeContainer)
        {
            Destroy(child.gameObject);
        }

        if (connectionLinesContainer != null)
        {
            foreach (Transform child in connectionLinesContainer)
            {
                Destroy(child.gameObject);
            }
        }

        techNodes.Clear();
        connectionLines.Clear();

        // 计算每个科技的层级（基于前置关系）
        Dictionary<int, int> techLevels = CalculateTechLevels();
        Debug.Log($"计算完成，科技层级数量: {techLevels.Count}");

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

        Debug.Log($"科技按层级分组完成，共有 {techsByLevel.Count} 个层级");

        // 创建节点
        int nodeCount = 0;
        foreach (var levelPair in techsByLevel)
        {
            int level = levelPair.Key;
            List<Technology> techsInLevel = levelPair.Value;

            Debug.Log($"层级 {level} 有 {techsInLevel.Count} 个科技");

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
                nodeCount++;
            }
        }

        Debug.Log($"总共创建了 {nodeCount} 个科技节点");

        // 创建连接线
        CreateConnectionLines();
    }
    // 更新所有节点的UI状态
    public void UpdateAllNodesUI()
    {
        if (techNodes == null || techNodes.Count == 0) return;

        Debug.Log("开始更新所有节点UI状态");
        int updatedCount = 0;

        foreach (var nodePair in techNodes)
        {
            Technology tech = nodePair.Value.GetTechnology();

            // 记录更新前的状态（用于调试）
            bool wasAvailableBefore = nodePair.Value.availableIndicator.activeInHierarchy;
            bool wasLockedBefore = nodePair.Value.lockedOverlay.activeInHierarchy;

            // 更新节点外观
            nodePair.Value.UpdateNodeAppearance();
            updatedCount++;

            // 调试信息
            bool isAvailableNow = nodePair.Value.availableIndicator.activeInHierarchy;
            bool isLockedNow = nodePair.Value.lockedOverlay.activeInHierarchy;

            if (wasAvailableBefore != isAvailableNow || wasLockedBefore != isLockedNow)
            {
                Debug.Log($"节点 {tech.techName} 状态变化: " +
                         $"Available[{wasAvailableBefore}→{isAvailableNow}] " +
                         $"Locked[{wasLockedBefore}→{isLockedNow}] " +
                         $"解锁={tech.isUnlocked}, 研究={tech.isResearched}");
            }
        }

        Debug.Log($"UI更新完成，共更新了 {updatedCount} 个节点");
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
        Debug.Log($"创建科技节点: {tech.techName} (ID: {tech.id}) 位置: {position}");

        GameObject nodeObj = Instantiate(techNodePrefab, nodeContainer);
        if (nodeObj == null)
        {
            Debug.LogError("实例化节点失败!");
            return;
        }

        nodeObj.transform.localPosition = position;

        TechTreeNode node = nodeObj.GetComponent<TechTreeNode>();
        if (node != null)
        {
            node.Initialize(tech, this);
            techNodes[tech.id] = node;
            Debug.Log($"科技节点 {tech.techName} 初始化成功");
        }
        else
        {
            Debug.LogError($"TechTreeNode 组件未在预制体上找到!");
        }
    }

    void CreateConnectionLines()
    {
        if (connectionLinePrefab == null || connectionLinesContainer == null)
        {
            Debug.LogWarning("连接线预制体或容器未设置，跳过创建连接线");
            return;
        }

        foreach (Technology tech in researchLab.allTechnologies)
        {
            foreach (int prereqId in tech.prerequisites)
            {
                CreateConnectionLine(prereqId, tech.id);
            }
        }

        Debug.Log($"创建了 {connectionLines.Count} 条连接线");
    }

    void CreateConnectionLine(int fromTechId, int toTechId)
    {
        if (!techNodes.ContainsKey(fromTechId) || !techNodes.ContainsKey(toTechId))
        {
            Debug.LogWarning($"无法创建连接线: 节点 {fromTechId} -> {toTechId} 不存在");
            return;
        }

        TechTreeNode fromNode = techNodes[fromTechId];
        TechTreeNode toNode = techNodes[toTechId];

        if (fromNode.lineEndPoint == null || toNode.lineStartPoint == null)
        {
            Debug.LogWarning($"连接点未设置: {fromTechId} -> {toTechId}");
            return;
        }

        GameObject lineObj = Instantiate(connectionLinePrefab, connectionLinesContainer);
        ConnectionLine connectionLine = lineObj.GetComponent<ConnectionLine>();

        if (connectionLine != null)
        {
            connectionLine.ConnectNodes(fromNode, toNode);
            connectionLine.UpdateLineColorBasedOnStatus();
            connectionLines.Add(connectionLine);
        }
    }

    void UpdateAllConnectionLines()
    {
        foreach (ConnectionLine line in connectionLines)
        {
            line.UpdateLineColorBasedOnStatus();
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
                    string status = prereqTech.isResearched ? "" : "";
                    prereqText += $"{status} {prereqTech.techName}\n";
                }
            }
            prerequisitesText.text = prereqText;
        }
        else
        {
            prerequisitesText.text = "无前置条件";
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
                // 更新详情面板按钮状态
                researchButton.interactable = false;
            }
        }
    }

    public void CloseDetailPanel()
    {
        if (selectedNode != null)
        {
            selectedNode.SetSelected(false);
            selectedNode = null;
        }
        HideDetailPanel();
    }

    void OnTechnologyResearched(Technology tech)
    {
        Debug.Log($"收到科技研究完成事件: {tech.techName}");

        // 1. 更新被研究的节点
        if (techNodes.ContainsKey(tech.id))
        {
            techNodes[tech.id].UpdateNodeAppearance();
            Debug.Log($"更新研究节点: {tech.techName}");
        }

        // 2. 检查并解锁后续科技
        List<Technology> newlyUnlockedTechs = new List<Technology>();

        foreach (Technology subsequentTech in researchLab.allTechnologies)
        {
            if (!subsequentTech.isUnlocked && subsequentTech.prerequisites.Contains(tech.id))
            {
                bool allPrerequisitesMet = true;
                foreach (int preReqId in subsequentTech.prerequisites)
                {
                    Technology preReqTech = researchLab.GetTechnologyById(preReqId);
                    if (preReqTech == null || !preReqTech.isResearched)
                    {
                        allPrerequisitesMet = false;
                        break;
                    }
                }

                if (allPrerequisitesMet)
                {
                    subsequentTech.isUnlocked = true;
                    newlyUnlockedTechs.Add(subsequentTech);
                    Debug.Log($"✅ 解锁科技: {subsequentTech.techName}");
                }
            }
        }

        // 3. 立即更新所有新解锁科技对应的节点UI
        foreach (Technology unlockedTech in newlyUnlockedTechs)
        {
            if (techNodes.ContainsKey(unlockedTech.id))
            {
                techNodes[unlockedTech.id].UpdateNodeAppearance();
                Debug.Log($"✅ 立即更新解锁节点UI: {unlockedTech.techName}");
            }
        }

        // 4. 更新所有连接线
        UpdateAllConnectionLines();

        // 5. 如果当前选中的科技被研究了，更新详情面板
        if (selectedNode != null && selectedNode.GetTechId() == tech.id)
        {
            researchButton.interactable = false;

            // 刷新详情面板显示
            ShowTechnologyDetails(tech);
        }

        // 6. 【关键修改】强制更新所有节点的UI状态
        UpdateAllNodesUI();

        Debug.Log($"科技研究完成处理完毕，新解锁了 {newlyUnlockedTechs.Count} 个科技");
    }

    void OnDestroy()
    {
        // 取消事件订阅
        if (researchLab != null)
        {
            researchLab.OnTechnologyResearched -= OnTechnologyResearched;
        }
    }

    // 测试方法：手动重新创建科技树
    [ContextMenu("手动创建科技树")]
    public void ManualCreateTechTree()
    {
        Debug.Log("手动创建科技树");
        CreateTechTreeUI();
    }
}