
using UnityEngine;

public class scr_movement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;
    public float jumpForce = 10f;

    private Rigidbody2D rb;
    private float moveInput;
    private int facingDirection = 1;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.5f;
    public LayerMask groundLayer;

    private bool isGrounded;
    private bool wasGrounded;

    [Header("Gravity")]
    public float Gravity = 1f;
    public float FallingGravity = 3f;

    [Header("Coyote Time")]
    public float CoyoteTime = 0.2f;
    private float CoyoteTimetimer;

    [Header("Jump Settings")]
    public int extraJumpsVal = 2;
    private int extraJumps;

    public float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

    [Header("Physics Material")]
    public PhysicsMaterial2D physicsMaterial;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private float dashTime;
    private float dashCooldownTimer;
    private float dashDirection = 1f;

    private bool isDashing;

    [Header("Wall Detection")]
    public Transform wallCheck;
    public float wallCheckDistance = 0.5f;
    public float wallCheckOffset = 0.5f;
    public LayerMask wallLayer;

    private bool isTouchingWall;
    private int wallDirection;

    [Header("Wall Slide")]
    public float wallSlideSpeed = 2f;
    private bool isWallSliding;

    [Header("Wall Jump")]
    public float wallJumpForceX = 12f;
    public float wallJumpForceY = 12f;
    public float wallJumpLockTime = 0.2f;

    private bool isWallJumping;
    private float wallJumpLockTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        extraJumps = extraJumpsVal;
        CoyoteTimetimer = CoyoteTime;

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D missing!");
        }

        if (wallCheck == null)
        {
            Debug.LogError("WallCheck not assigned!");
        }
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        // Update facing direction
        if (moveInput != 0 && !isWallJumping)
        {
            facingDirection = moveInput > 0 ? 1 : -1;
        }

        // DASH COOLDOWN
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // DASH INPUT
        if (Input.GetKeyDown(KeyCode.LeftShift)
            && dashCooldownTimer <= 0
            && !isDashing)
        {
            StartDash();
        }

        // DASH PRIORITY
        if (isDashing)
        {
            Dash();
            return;
        }

        // WALL DETECTION
        CheckWall();

        // WALL JUMP LOCK
        if (wallJumpLockTimer > 0)
        {
            wallJumpLockTimer -= Time.deltaTime;
        }
        else
        {
            isWallJumping = false;
        }

        // GROUND CHECK LOGIC
        if (isGrounded)
        {
            extraJumps = extraJumpsVal;
            CoyoteTimetimer = CoyoteTime;

            if (physicsMaterial != null)
                physicsMaterial.bounciness = 0f;
        }
        else
        {
            if (wasGrounded)
                CoyoteTimetimer = CoyoteTime;

            CoyoteTimetimer -= Time.deltaTime;

            if (physicsMaterial != null)
                physicsMaterial.bounciness = 0.01f;
        }

        // WALL SLIDE
        isWallSliding = isTouchingWall
                        && !isGrounded
                        && rb.linearVelocity.y < 0
                        && moveInput != 0
                        && Mathf.Sign(moveInput) == wallDirection;

        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed)
            );
        }

        // GRAVITY
        if (!isWallSliding)
        {
            if (rb.linearVelocity.y < 0)
                rb.gravityScale = FallingGravity;
            else
                rb.gravityScale = Gravity;
        }

        // JUMP BUFFER
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // JUMP LOGIC
        if (jumpBufferCounter > 0f)
        {
            // WALL JUMP
            if (isTouchingWall && !isGrounded)
            {
                WallJump();
            }
            // NORMAL JUMP
            else if (CoyoteTimetimer > 0f)
            {
                Jump();
                CoyoteTimetimer = 0f;
            }
            // EXTRA JUMP
            else if (extraJumps > 0)
            {
                Jump();
                extraJumps--;
            }
        }

        // NORMAL HORIZONTAL MOVEMENT
        if (!isWallJumping)
        {
            rb.linearVelocity = new Vector2(
                moveInput * speed,
                rb.linearVelocity.y
            );
        }

        wasGrounded = isGrounded;
    }

    // NORMAL JUMP
    void Jump()
    {
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        jumpBufferCounter = 0f;
    }

    // WALL DETECTION
    void CheckWall()
    {
        if (wallCheck == null)
            return;

        // Move WallCheck to the side player is facing
        wallCheck.localPosition = new Vector3(
            wallCheckOffset * facingDirection,
            wallCheck.localPosition.y,
            wallCheck.localPosition.z
        );

        // Detect wall
        RaycastHit2D hit = Physics2D.Raycast(
            wallCheck.position,
            Vector2.right * facingDirection,
            wallCheckDistance,
            wallLayer
        );

        isTouchingWall = hit.collider != null;

        if (isTouchingWall)
        {
            wallDirection = facingDirection;
        }
    }

    // WALL JUMP
    void WallJump()
    {
        isWallJumping = true;
        wallJumpLockTimer = wallJumpLockTime;

        rb.linearVelocity = new Vector2(
            -wallDirection * wallJumpForceX,
            wallJumpForceY
        );

        // Face away from wall
        facingDirection = -wallDirection;

        jumpBufferCounter = 0f;
        CoyoteTimetimer = 0f;
        extraJumps = extraJumpsVal;
    }

    // START DASH
    void StartDash()
    {
        isDashing = true;

        dashTime = dashDuration;
        dashCooldownTimer = dashCooldown;

        if (moveInput != 0)
        {
            dashDirection = Mathf.Sign(moveInput);
        }
        else
        {
            dashDirection = facingDirection;
        }

        rb.gravityScale = 0f;

        rb.linearVelocity = new Vector2(
            dashDirection * dashSpeed,
            0f
        );
    }

    // DASH
    void Dash()
    {
        dashTime -= Time.deltaTime;

        rb.linearVelocity = new Vector2(
            dashDirection * dashSpeed,
            0f
        );

        if (dashTime <= 0f)
        {
            isDashing = false;

            rb.gravityScale = Gravity;

            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                0f
            );
        }
    }

    void FixedUpdate()
    {
        // GROUND DETECTION
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }
    }

    // GIZMOS
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawLine(
                wallCheck.position,
                wallCheck.position +
                Vector3.right * facingDirection * wallCheckDistance
            );
        }
    }
}