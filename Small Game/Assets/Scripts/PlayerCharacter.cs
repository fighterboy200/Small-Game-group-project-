using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{

    //momevment varriables
    public float moveSpeed = 5.0f;
    public float jumpHeight = 2.0f;
    public float gravity = -9.81f;

    private CharacterController controller;

    private Vector3 velocity;
    private bool isGrounded;
    private int maxJumps = 2;
    private int jumpcount = 0;

    // Dash variables
    public float dashSpeed = 30f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private bool isDashing = false;
    private bool canDash = true;
    private Vector3 dashDirection;

    // Grappling Hook Variables
    public float maxGrappleDistance = 30f;
    public float grappleSpeed = 20f;
    public LayerMask grappleLayerMask;
    public Transform cameraTransform;

    private bool isGrappling = false;
    private Vector3 grapplePoint;

    private LineRenderer lineRenderer;
    public Transform ropeOrigin;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    void CheckIfGroundedAndAddDownwardVelocity()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
            jumpcount = 0;
        }
    }
    void HandleMovement()
    {
        if (isDashing) return; // Don't allow normal movement while dashing

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Store last movement direction for dash
        if (move != Vector3.zero)
        {
            dashDirection = move.normalized;
        }
    }

    void HandleJumping()
    {
        if (Input.GetButtonDown("Jump") && jumpcount < maxJumps)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravity);
            jumpcount++;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleDash()
    {
        if (Input.GetButtonDown("Fire3") && canDash && dashDirection != Vector3.zero)
        { 
           StartCoroutine(Dash());
        }
    }
    System.Collections.IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;

        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            yield return null;
        }

        isDashing = false;

        // Wait for cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void HandleGrappling()
    {
        if (isGrappling)
        {
            Vector3 direction = (grapplePoint - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, grapplePoint);

            if (distance < 2f)
            {
                isGrappling = false;
                velocity = Vector3.zero;
                lineRenderer.positionCount = 0; // Hide rope
                return;
            }

            controller.Move(direction * grappleSpeed * Time.deltaTime);

            // Update rope visual
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, ropeOrigin.position);
            lineRenderer.SetPosition(1, grapplePoint);

            return;
        }

        if (Input.GetButtonDown("Fire2"))
        {
            
           RaycastHit hit;
           if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxGrappleDistance, grappleLayerMask))
           {
                grapplePoint = hit.point;
                isGrappling = true;
                velocity = Vector3.zero;

                // Initialize rope
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, ropeOrigin.position);
                lineRenderer.SetPosition(1, grapplePoint);
           }
        }
    }

    // Update is called once per frame
    void Update()
    {

        CheckIfGroundedAndAddDownwardVelocity();

        if (!isGrappling && !isDashing)
        {
            HandleMovement();
            HandleJumping();
            HandleDash();
        }

        HandleGrappling();
    }
}
