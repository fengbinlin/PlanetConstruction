using UnityEngine;

public class AdvancedObjectFollower : MonoBehaviour
{
    public enum FollowType
    {
        Instant,        // 立即跟随
        Lerp,          // 线性插值
        SmoothDamp,    // 平滑阻尼
        PhysicsBased   // 基于物理
    }
    
    [Header("跟随目标")]
    public Transform target;
    
    [Header("跟随类型")]
    public FollowType followType = FollowType.Lerp;
    
    [Header("跟随设置")]
    public float followSpeed = 5f;
    public float stoppingDistance = 1f;
    public Vector3 offset = Vector3.zero; // 位置偏移
    
    [Header("平滑阻尼专用设置")]
    public float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero;
    
    [Header("物理跟随专用设置")]
    public float force = 10f;
    private Rigidbody rb;
    
    [Header("轴向限制")]
    public bool followX = true;
    public bool followY = true;
    public bool followZ = true;
    
    [Header("看向目标")]
    public bool lookAtTarget = false;
    public float rotationSpeed = 5f;
    
    void Start()
    {
        // 如果是物理跟随，获取Rigidbody组件
        if (followType == FollowType.PhysicsBased)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("物理跟随需要Rigidbody组件！");
                followType = FollowType.Lerp;
            }
        }
    }
    
    void Update()
    {
        if (target == null)
            return;
            
        // 处理跟随
        HandleFollowing();
        
        // 处理旋转
        HandleRotation();
    }
    
    void HandleFollowing()
    {
        float distance = Vector3.Distance(transform.position, target.position + offset);
        
        if (distance > stoppingDistance)
        {
            Vector3 targetPosition = target.position + offset;
            
            // 根据轴向限制调整目标位置
            Vector3 currentPosition = transform.position;
            if (!followX) targetPosition.x = currentPosition.x;
            if (!followY) targetPosition.y = currentPosition.y;
            if (!followZ) targetPosition.z = currentPosition.z;
            
            switch (followType)
            {
                case FollowType.Instant:
                    transform.position = targetPosition;
                    break;
                    
                case FollowType.Lerp:
                    transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
                    break;
                    
                case FollowType.SmoothDamp:
                    transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
                    break;
                    
                case FollowType.PhysicsBased:
                    // 物理跟随在FixedUpdate中处理
                    break;
            }
        }
    }
    
    void FixedUpdate()
    {
        // 物理跟随在FixedUpdate中处理
        if (followType == FollowType.PhysicsBased && target != null && rb != null)
        {
            float distance = Vector3.Distance(transform.position, target.position + offset);
            
            if (distance > stoppingDistance)
            {
                Vector3 targetPosition = target.position + offset;
                Vector3 direction = (targetPosition - transform.position).normalized;
                rb.AddForce(direction * force);
            }
        }
    }
    
    void HandleRotation()
    {
        if (lookAtTarget && target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
    
    // 公共方法
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
    
    public void SetFollowSpeed(float newSpeed)
    {
        followSpeed = newSpeed;
    }
    
    // 调试绘制
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.position + offset);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position + offset, stoppingDistance);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}