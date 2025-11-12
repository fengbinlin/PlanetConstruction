using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 30f;
    [SerializeField] private float rotationSpeed = 720f;

    [Header("响应性设置")]
    [SerializeField] private float inputDeadZone = 0.1f;
    [SerializeField] private bool useRawInput = true;

    [Header("组件引用")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    // Floor 检测层
    [Header("地图设置")]
    [SerializeField] private LayerMask floorLayerMask; // 在 Inspector 设为 Floor 层

    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private bool isMoving;

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
        float horizontal = useRawInput ? Input.GetAxisRaw(HORIZONTAL) : Input.GetAxis(HORIZONTAL);
        float vertical = useRawInput ? Input.GetAxisRaw(VERTICAL) : Input.GetAxis(VERTICAL);

        moveInput = new Vector2(horizontal, vertical);

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
        Vector2 nextPosition = rb.position + targetVelocity * Time.fixedDeltaTime;

        // 检测下一步是否在floor上
        if (IsOnFloor(nextPosition))
        {
            if (isMoving)
            {
                currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
                if (Vector2.Distance(currentVelocity, targetVelocity) < 0.5f)
                    currentVelocity = targetVelocity;
            }
            else
            {
                currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
                if (currentVelocity.magnitude < 0.1f)
                    currentVelocity = Vector2.zero;
            }
        }
        else
        {
            // 出界就不动
            currentVelocity = Vector2.zero;
        }

        rb.velocity = currentVelocity;
    }

    private bool IsOnFloor(Vector2 position)
    {

        // 在这个位置检测是否有 Floor trigger
        Collider2D hit = Physics2D.OverlapPoint(position, floorLayerMask);
        // print(hit != null);
        return hit != null;
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
            // 可加入动画逻辑
        }
    }

    public bool IsMoving => isMoving;
    public Vector2 MoveDirection => moveInput;

    public void SetResponsiveness(float accelMultiplier, float decelMultiplier)
    {
        acceleration = 25f * accelMultiplier;
        deceleration = 30f * decelMultiplier;
    }
}