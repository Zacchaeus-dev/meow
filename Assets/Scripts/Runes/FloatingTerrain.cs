using UnityEngine;

/// <summary>
/// A solid platform that rises when the player stands on top of it,
/// and returns to its starting position when the player steps off.
/// Uses Physics.BoxCast for stable top detection.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class FloatingTerrain : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float riseHeight = 4f;
    [SerializeField] private float moveSpeed = 2f;

    [Header("Detection Settings")]
    [Tooltip("Layer mask assigned to the Player GameObject.")]
    [SerializeField] private LayerMask playerLayer;

    [Tooltip("How far above the top surface to scan for the player.")]
    [SerializeField] private float detectionHeight = 0.3f;

    private Rigidbody rb;
    private Collider blockCollider;
    private Vector3 startPosition;
    private bool isPlayerOnTop;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        blockCollider = GetComponent<Collider>();
        startPosition = transform.position;
    }

    private void FixedUpdate()
    {
        isPlayerOnTop = CheckIfPlayerIsOnTop(); // Check if player is on top of the block using BoxCast
        Vector3 targetPosition = isPlayerOnTop ? startPosition + Vector3.up * riseHeight : startPosition; // Determine target position
        Vector3 newPosition = Vector3.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime); // Smoothly move toward target position
        rb.MovePosition(newPosition);
    }

    // Performs a BoxCast right above the block's top face to detect the player.
    private bool CheckIfPlayerIsOnTop()
    {
        Bounds bounds = blockCollider.bounds;
        Vector3 center = new Vector3(bounds.center.x, bounds.max.y + (detectionHeight * 0.5f), bounds.center.z);  // Position the box center slightly above the top face of the collider
        Vector3 halfExtents = new Vector3(bounds.extents.x * 0.9f, detectionHeight * 0.5f, bounds.extents.z * 0.9f); // Make the detection box slightly narrower than the platform to ignore side-brushing
        return Physics.CheckBox(center, halfExtents, Quaternion.identity, playerLayer, QueryTriggerInteraction.Ignore);  // Perform box overlap check for player
    }

    private void OnDrawGizmosSelected()
    {
        // Visual aid in Scene View to debug detection zone
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Bounds bounds = col.bounds;
        Vector3 center = new Vector3(bounds.center.x, bounds.max.y + (detectionHeight * 0.5f), bounds.center.z);
        Vector3 size = new Vector3(bounds.size.x * 0.9f, detectionHeight, bounds.size.z * 0.9f);

        Gizmos.color = isPlayerOnTop ? Color.green : Color.red;
        Gizmos.DrawWireCube(center, size);
    }
}