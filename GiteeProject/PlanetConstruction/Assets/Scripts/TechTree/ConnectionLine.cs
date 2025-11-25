using UnityEngine;

public class ConnectionLine : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private TechTreeNode startNode;
    private TechTreeNode endNode;
    
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        SetupLineRenderer();
    }
    
    void SetupLineRenderer()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.15f;
        lineRenderer.endWidth = 0.15f;
        
        // 创建默认材质
        Material lineMaterial = new Material(Shader.Find("Custom/AlwaysOnTop"));
        lineMaterial.color = Color.gray;
        lineRenderer.material = lineMaterial;
        
        lineRenderer.startColor = Color.gray;
        lineRenderer.endColor = Color.gray;
        lineRenderer.useWorldSpace = true;
    }
    
    public void ConnectNodes(TechTreeNode fromNode, TechTreeNode toNode)
    {
        startNode = fromNode;
        endNode = toNode;
        UpdateLinePosition();
    }
    
    public void SetColor(Color color)
    {
        if (lineRenderer != null)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
    }
    
    void Update()
    {
        UpdateLinePosition();
    }
    
    void UpdateLinePosition()
    {
        if (lineRenderer == null || startNode == null || endNode == null || 
            startNode.lineEndPoint == null || endNode.lineStartPoint == null) 
            return;
        
        lineRenderer.SetPosition(0, startNode.lineEndPoint.position);
        lineRenderer.SetPosition(1, endNode.lineStartPoint.position);
    }
    
    public void UpdateLineColorBasedOnStatus()
    {
        if (startNode == null || endNode == null) return;
        
        Technology startTech = startNode.GetTechnology();
        Technology endTech = endNode.GetTechnology();
        
        if (startTech.isResearched && endTech.isResearched)
        {
            SetColor(Color.blue); // 两端都已研究完成
        }
        else if (startTech.isResearched && endTech.isUnlocked)
        {
            SetColor(Color.green); // 前置已完成，目标可研究
        }
        else if (startTech.isResearched)
        {
            SetColor(Color.yellow); // 前置已完成，但目标还不可研究
        }
        else
        {
            SetColor(Color.gray); // 前置未完成
        }
    }
}