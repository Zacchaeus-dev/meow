using UnityEngine;

// [Component Pattern] (Decoupling) - PlayerMovement is a component that can be attached to any GameObject to give it player movement behavior.
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 12f;

    //[Header("Jump")]
    //[SerializeField] private float jumpForce = 8f;
    //[SerializeField] private Transform groundCheck;
    //[SerializeField] private float groundCheckRadius = 0.2f;
    //[SerializeField] private LayerMask groundLayer;

    [Header("Jump Tuning")]
    [SerializeField] private float jumpForce = 12f; // Increased default force
    [SerializeField] private float fallMultiplier = 2.5f; // Pulls down faster when falling
    [SerializeField] private float lowJumpMultiplier = 2f; // Tapping jump gives a short hop
    [SerializeField] private float gravityScale = 1.5f; // Extra base gravity
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Camera-Relative Movement")]
    [Tooltip("Assign the main camera here so WASD move relative to the camera's facing direction.")]
    [SerializeField] private Transform cameraTransform;

    private Rigidbody rb;
    private Vector2 inputVector;
    private Vector3 moveDirection;
    private bool isGrounded;
    private bool jumpRequested;

    private float speedMultiplier = 1f; // This can be modified by external factors like StickyBlock
    public void SetSpeedMultiplier (float multiplier) => speedMultiplier = multiplier;


    private void FixedUpdate()
    {
        CheckGrounded();
        CalculateMoveDirection();
        MoveCharacter();
        RotateTowardsMovement();
        HandleJump();
        ApplyCustomGravity(); // Applied every physics tick
    }

    private void ApplyCustomGravity()
    {
        if (isGrounded) return;

        // Falling down -> Apply fallMultiplier
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        // Rising up without holding jump -> Apply lowJumpMultiplier for quick release
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
        // Normal upward arc -> Apply base gravityScale
        else if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (gravityScale - 1) * Time.fixedDeltaTime;
        }
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // This is to prevent the Rigidbody from rotating due to physics interactions so we rotate it manually

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    // Update is called once per frame
    void Update() // [Update Method / Input Separation] (Sequencing Patterns)
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        inputVector = new Vector2(h, v).normalized;

        //CheckGrounded();

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }
    }

    //private void FixedUpdate() // [Update Method / Input Separation] (Sequencing Patterns)
    //{
    //    CheckGrounded();
    //    CalculateMoveDirection();
    //    MoveCharacter();
    //    RotateTowardsMovement();
    //    HandleJump();
    //}

    private void CheckGrounded()
    {
        if (groundCheck == null)
        {
            isGrounded = true; // If no ground check is assigned, assume the player is always grounded
            return;
        }
        //isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer); /// this
        // Check sphere synchronized with physics tick
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);

    }

    private void CalculateMoveDirection()
    {
        if (inputVector.sqrMagnitude < 0.01f)
        {
            moveDirection = Vector3.zero;
            return;
        }

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        moveDirection = (camForward * inputVector.y + camRight * inputVector.x).normalized;
    }

    private void MoveCharacter()
    {
        Vector3 targetVelocity = moveDirection * moveSpeed * speedMultiplier;
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 velocityChange = new Vector3(targetVelocity.x - currentVelocity.x, 0f, targetVelocity.z - currentVelocity.z);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void RotateTowardsMovement()
    {
        if (moveDirection.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    private void HandleJump()
    {
        if (!jumpRequested) return;
        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;

        jumpRequested = false;
    }

    //private void OnDrawGizmosSelected()
    //{
    //    if (groundCheck == null) return;
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    //}
}
