using UnityEngine;
using System.Collections;  // 添加这个命名空间以使用IEnumerator

public class SmoothCameraController : MonoBehaviour
{
    [Header("跟随设置")]
    [SerializeField] private Transform target;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Vector2 offset = Vector2.zero;
    
    [Header("边界设置")]
    [SerializeField] private bool enableBounds = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-10, -10);
    [SerializeField] private Vector2 maxBounds = new Vector2(10, 10);
    
    [Header("高级设置")]
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSpeed = 3f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float defaultZoom = 5f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 10f;
    
    private Camera cam;
    private Vector3 currentVelocity;
    private Vector3 lookAheadTarget;
    private float targetZoom;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        
        targetZoom = defaultZoom;
        lookAheadTarget = Vector3.zero;
        
        // 初始位置设置为目标位置
        if (target != null)
        {
            transform.position = GetTargetPosition();
        }
    }

    void LateUpdate()
    {
        if (target == null) return;
        
        HandleZoom();
        UpdateLookAhead();
        FollowTarget();
        ApplyBounds();
    }

    private void FollowTarget()
    {
        Vector3 targetPosition = GetTargetPosition() + lookAheadTarget;
        
        // 使用SmoothDamp实现超平滑跟随
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, 1f / followSpeed);
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 targetPos = target.position + (Vector3)offset;
        targetPos.z = transform.position.z; // 保持z轴不变
        return targetPos;
    }

    private void UpdateLookAhead()
    {
        // 根据玩家移动方向预测镜头位置
        if (target.TryGetComponent<PlayerController>(out var player))
        {
            if (player.IsMoving)
            {
                Vector3 targetLookAhead = (Vector3)player.MoveDirection * lookAheadDistance;
                lookAheadTarget = Vector3.Lerp(lookAheadTarget, targetLookAhead, lookAheadSpeed * Time.deltaTime);
            }
            else
            {
                lookAheadTarget = Vector3.Lerp(lookAheadTarget, Vector3.zero, lookAheadSpeed * Time.deltaTime);
            }
        }
    }

    private void HandleZoom()
    {
        // 鼠标滚轮缩放
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            targetZoom -= scroll * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
        
        // 平滑缩放
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, zoomSpeed * Time.deltaTime);
    }

    private void ApplyBounds()
    {
        if (!enableBounds) return;
        
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
        
        transform.position = clampedPosition;
    }

    // 设置摄像机边界（可在运行时调用）
    public void SetCameraBounds(Vector2 newMinBounds, Vector2 newMaxBounds)
    {
        minBounds = newMinBounds;
        maxBounds = newMaxBounds;
        enableBounds = true;
    }

    // 临时聚焦某个位置（可用于过场动画）
    public void FocusOnPosition(Vector3 position, float duration = 1f)
    {
        // 可以使用协程实现平滑过渡
        StartCoroutine(FocusCoroutine(position, duration));
    }

    private IEnumerator FocusCoroutine(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = targetPosition;
    }
}