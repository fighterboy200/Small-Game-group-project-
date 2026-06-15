using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Double Jump")]
    public bool DoubleJumpUnlocked = false;
    public float doubleJumpForce;
    private bool canDoubleJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Dashing")]
    public bool DashUnlocked = false;
    public float dashSpeed;
    public float dashDuration;
    public float dashCooldown;

    public KeyCode dashKey = KeyCode.Q;

    private bool readyToDash = true;
    private bool isDashing;

    [Header("Bullet Time")]
    public bool BulletTimeUnlocked = false;
    public float slowTimeScale = 0.3f; // The speed of time when active (0.3 = 30% speed)
    public float transitionSpeed = 5f; // How fast the time changes



    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;
    private bool wasGrounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Stamina System")]
    public float maxStamina = 5f;
    public float staminaDrainRate = 1f;
    public float staminaRegenRate = 1.5f;
    public bool canSprint = true;

    public float currentStamina;

    private bool sprintReleasedLock = false;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;

    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        dashing,
        air
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;

        currentStamina = maxStamina;
    }

    private void Update()
    {
        // ground check
            grounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            playerHeight * 0.5f + 0.2f,
            whatIsGround);

        // reset double jump when grounded
        if (grounded && !wasGrounded)
        {
            canDoubleJump = true;
            readyToJump = true;
        }

        wasGrounded = grounded;

        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0f;
        }

        HandleStamina();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        
        if (Input.GetMouseButton(1) && BulletTimeUnlocked==true) // 1 is Right Click
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, slowTimeScale, transitionSpeed * Time.unscaledDeltaTime);
        }
        else
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1f, transitionSpeed * Time.unscaledDeltaTime);
        }


        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // normal jump
        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // double jump
        else if ( Input.GetKeyDown(jumpKey) && !grounded && canDoubleJump && DoubleJumpUnlocked == true)
        {
            DoubleJump();
        }

        // when to dash
        if (Input.GetKeyDown(dashKey) && readyToDash && DashUnlocked==true)
        {
            StartCoroutine(Dash());
        }

        // start crouching
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(
                transform.localScale.x,
                crouchYScale,
                transform.localScale.z);

            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // stop crouching
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(
                transform.localScale.x,
                startYScale,
                transform.localScale.z);
        }

        // sprint lock
        if (Input.GetKeyUp(sprintKey))
        {
            sprintReleasedLock = true;
            canSprint = false;
        }
    }

    private void StateHandler()
    {
        // Mode - Dashing
        if (isDashing)
        {
            state = MovementState.dashing;
            moveSpeed = dashSpeed;
        }

        // Mode - Crouching
        else if (grounded && Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if (grounded &&
                 Input.GetKey(sprintKey) &&
                 canSprint &&
                 currentStamina > 0)
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;
            moveSpeed = walkSpeed;
        }
    }

    private void MovePlayer()
    {
        // stop normal movement during dash
        if (isDashing)
            return;

        // calculate movement direction
        moveDirection =
            orientation.forward * verticalInput +
            orientation.right * horizontalInput;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(
                GetSlopeMoveDirection() * moveSpeed * 20f,
                ForceMode.Force);

            if (rb.linearVelocity.y > 0)
            {
                rb.AddForce(
                    Vector3.down * 80f,
                    ForceMode.Force);
            }
        }

        // on ground
        if (grounded)
        {
            rb.AddForce(
                moveDirection.normalized * moveSpeed * 10f,
                ForceMode.Force);
        }

        // in air
        else
        {
            rb.AddForce(
                moveDirection.normalized *
                moveSpeed *
                10f *
                airMultiplier,
                ForceMode.Force);
        }

        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        // don't limit dash speed
        if (isDashing)
            return;

        // limit speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
            {
                rb.linearVelocity =
                    rb.linearVelocity.normalized * moveSpeed;
            }
        }

        // limit speed on ground or air
        else
        {
            Vector3 flatVel = new Vector3(
                rb.linearVelocity.x,
                0f,
                rb.linearVelocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel =
                    flatVel.normalized * moveSpeed;

                rb.linearVelocity = new Vector3(
                    limitedVel.x,
                    rb.linearVelocity.y,
                    limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z);

        rb.AddForce(
            transform.up * jumpForce,
            ForceMode.Impulse);
    }

    private void DoubleJump()
    {
        canDoubleJump = false;

        // reset y velocity
        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z);

        rb.AddForce(
            transform.up * doubleJumpForce,
            ForceMode.Impulse);
    }

    private IEnumerator Dash()
    {
        readyToDash = false;
        isDashing = true;

        // reset horizontal velocity
        rb.linearVelocity = new Vector3(
            0f,
            rb.linearVelocity.y,
            0f);

        // dash direction
        Vector3 dashDirection = orientation.forward;

        rb.AddForce(
            dashDirection.normalized * dashSpeed,
            ForceMode.Impulse);

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        readyToDash = true;
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(
            transform.position,
            Vector3.down,
            out slopeHit,
            playerHeight * 0.5f + 0.3f))
        {
            float angle =
                Vector3.Angle(Vector3.up, slopeHit.normal);

            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(
            moveDirection,
            slopeHit.normal).normalized;
    }

    private void HandleStamina()
    {
        // Drain stamina while sprinting
        if (state == MovementState.sprinting)
        {
            // Use Time.unscaledDeltaTime so drain speed is constant
            currentStamina -= staminaDrainRate * Time.unscaledDeltaTime;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                canSprint = false;
            }
        }
        // Regenerate stamina
        else if (grounded && state != MovementState.air)
        {
            // Use Time.unscaledDeltaTime so regen speed is constant
            currentStamina += staminaRegenRate * Time.unscaledDeltaTime;

            if (currentStamina >= maxStamina)
            {
                currentStamina = maxStamina;

                // Unlock sprint after full recharge
                if (sprintReleasedLock)
                {
                    canSprint = true;
                    sprintReleasedLock = false;
                }
            }
        }

        // Safety clamp
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }
}