using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class WindTerrain : MonoBehaviour
{


    [Header("Movement Settings")]
    [Tooltip("How far forward along its facing direction the block moves.")]
    [SerializeField] private float moveDistance = 6f;

    [Tooltip("Linear movement speed.")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Automation Settings")]
    [Tooltip("Delay in seconds before the block automatically begins moving after spawning.")]
    [SerializeField] private float startDelay = 2f;

    [Header("Detection Settings")]
    [Tooltip("Layer mask assigned to the Player GameObject.")]
    [SerializeField] private LayerMask playerLayer;

    [Tooltip("How far above the top surface to scan for the player.")]
    [SerializeField] private float detectionHeight = 0.3f;

    private Rigidbody rb;
    private Collider blockCollider;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 lastPosition;
    private float delayTimer;
    private bool hasStarted;
    private bool hasReachedDestination;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Moved manually via MovePosition
        blockCollider = GetComponent<Collider>();
        startPosition = transform.position;
        lastPosition = transform.position;
    }

    private void Start()
    {
        targetPosition = startPosition + transform.forward * moveDistance;
        delayTimer = startDelay;
    }

    private void FixedUpdate()
    {
        if (hasReachedDestination) return;

        // 1. Wait out initial start delay before beginning movement
        if (!hasStarted)
        {
            delayTimer -= Time.fixedDeltaTime;
            if (delayTimer <= 0f)
            {
                hasStarted = true;
            }
            lastPosition = rb.position;
            return;
        }

        // 2. Move platform towards target position
        Vector3 newPosition = Vector3.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        // 3. Calculate platform delta movement for this frame
        Vector3 platformDelta = newPosition - lastPosition;

        // 4. If player is standing on top, push their Rigidbody by platformDelta
        Collider playerCollider = GetPlayerColliderOnTop();
        if (playerCollider != null)
        {
            Rigidbody playerRb = playerCollider.attachedRigidbody;
            if (playerRb != null)
            {
                playerRb.position += platformDelta;
            }
        }

        lastPosition = newPosition;

        // 5. Stop permanently once target location is reached
        if (Vector3.Distance(rb.position, targetPosition) < 0.01f)
        {
            rb.MovePosition(targetPosition);
            hasReachedDestination = true;
        }
    }

    private Collider GetPlayerColliderOnTop()
    {
        Bounds bounds = blockCollider.bounds;
        Vector3 center = new Vector3(bounds.center.x, bounds.max.y + (detectionHeight * 0.5f), bounds.center.z);
        Vector3 halfExtents = new Vector3(bounds.extents.x * 0.9f, detectionHeight * 0.5f, bounds.extents.z * 0.9f);

        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, Quaternion.identity, playerLayer, QueryTriggerInteraction.Ignore);
        return hitColliders.Length > 0 ? hitColliders[0] : null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player") || collision.collider.CompareTag("Moveable"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Player") || collision.collider.CompareTag("Moveable"))
        {
            collision.transform.SetParent(null);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Bounds bounds = col.bounds;
        Vector3 center = new Vector3(bounds.center.x, bounds.max.y + (detectionHeight * 0.5f), bounds.center.z);
        Vector3 size = new Vector3(bounds.size.x * 0.9f, detectionHeight, bounds.size.z * 0.9f);

        Gizmos.color = GetPlayerColliderOnTop() != null ? Color.green : Color.red;
        Gizmos.DrawWireCube(center, size);

        Vector3 origin = Application.isPlaying ? startPosition : transform.position;
        Vector3 destination = origin + transform.forward * moveDistance;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, destination);
        Gizmos.DrawWireSphere(destination, 0.3f);
    }
}