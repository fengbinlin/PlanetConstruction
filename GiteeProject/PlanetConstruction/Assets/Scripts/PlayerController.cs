using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 25f;  // 增加加速度，让启动更快
    [SerializeField] private float deceleration = 30f;  // 增加减速度，让停止更快
    [SerializeField] private float rotationSpeed = 720f;
    
    [Header("响应性设置")]
    [SerializeField] private float inputDeadZone = 0.1f;  // 输入死区
    [SerializeField] private bool useRawInput = true;     // 使用原始输入获得更直接响应
    
    [Header("组件引用")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private bool isMoving;
    
    // 输入相关
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";

    void Update()
    {
        HandleInput();
        HandleAnimation();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleInput()
    {
        // 根据设置选择输入类型
        float horizontal = useRawInput ? Input.GetAxisRaw(HORIZONTAL) : Input.GetAxis(HORIZONTAL);
        float vertical = useRawInput ? Input.GetAxisRaw(VERTICAL) : Input.GetAxis(VERTICAL);
        
        moveInput = new Vector2(horizontal, vertical);
        
        // 应用死区过滤
        if (moveInput.magnitude < inputDeadZone)
        {
            moveInput = Vector2.zero;
        }
        else if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }
        
        isMoving = moveInput.magnitude > inputDeadZone;
    }

    private void HandleMovement()
    {
        Vector2 targetVelocity = moveInput * moveSpeed;
        
        if (isMoving)
        {
            // 加速时使用更直接的插值
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            
            // 当接近目标速度时直接设置，避免过度平滑
            if (Vector2.Distance(currentVelocity, targetVelocity) < 0.5f)
            {
                currentVelocity = targetVelocity;
            }
        }
        else
        {
            // 减速时更快停止
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            
            // 当速度很小时直接停止
            if (currentVelocity.magnitude < 0.1f)
            {
                currentVelocity = Vector2.zero;
            }
        }
        
        rb.velocity = currentVelocity;
    }

    private void HandleRotation()
    {
        if (isMoving)
        {
            float targetAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void HandleAnimation()
    {
        if (animator != null)
        {
            // animator.SetBool("IsMoving", isMoving);
            // animator.SetFloat("MoveX", moveInput.x);
            // animator.SetFloat("MoveY", moveInput.y);
            
            // if (isMoving)
            // {
            //     animator.SetFloat("LastMoveX", moveInput.x);
            //     animator.SetFloat("LastMoveY", moveInput.y);
            // }
        }
    }

    public bool IsMoving => isMoving;
    public Vector2 MoveDirection => moveInput;
    
    // 公开方法用于调整手感
    public void SetResponsiveness(float accelMultiplier, float decelMultiplier)
    {
        acceleration = 25f * accelMultiplier;
        deceleration = 30f * decelMultiplier;
    }
}