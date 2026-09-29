using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WindZone : MonoBehaviour
{
    [Header("Horizontal Push Settings")]
    [SerializeField] private float targetSpeed = 15f;
    [SerializeField] private float pushStrength = 20f;

    [Header("Levitation / Pit Protection")]
    [SerializeField] private float verticalSpringStrength = 40f;
    [SerializeField] private float verticalDamping = 6f;

    [Header("Moveable Object Push")]
    [Tooltip("Wind strength value checked against each MoveableObject;s Heavy Wind Threshold. Keep this low for a 'weak wind' zone.")]
    [SerializeField] private float objectPushStrength = 1f;

    [Header("Moveable Object Elevation")]
    [Tooltip("How strongly objects are pulled to the zone's centre height while carried.")]
    [SerializeField] private float objectHoverSpring = 40f;
    [SerializeField] private float objectHoverDamping = 6f;

    [Header("Cover / Blocking")]
    [Tooltip("Layer used by walls or cover that should block this wind's push.")]
    [SerializeField] private LayerMask obstacleLayer;

    private Collider zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();
        zoneCollider.isTrigger = true;
    }

    // Called every physics tick while an object is inside the WindZone
    private void OnTriggerStay(Collider other)
    {
        Vector3 targetCenter = other.bounds.center;
        if (IsBlockedByCover(other.bounds.center)) return;

        Vector3 windDirection = transform.forward;

        if (other.CompareTag("Player")) // [Strategy Pattern] (Type-Based Dispatch)
        {
            Rigidbody playerRb = other.GetComponent<Rigidbody>(); // [Component Pattern] (Decoupling Patterns)
            if (playerRb != null)
            {
                ApplyHorizontalWind(playerRb, windDirection);
                ApplyVerticalLevitation(playerRb);
            }
            return;
        }

        MoveableObject movable = other.GetComponent<MoveableObject>(); // [Strategy Pattern] (Type-Based Dispatch)
        if (movable != null)
        {
            movable.PushContinuous(windDirection, objectPushStrength);
            movable.HoverContinuous(zoneCollider.bounds.center.y, objectPushStrength, objectHoverSpring, objectHoverDamping);
        }
    }

    // Casts from the wind zone toward the target. A wall on obstacleLayer anywhere along that line mean the target is in cover.
    private bool IsBlockedByCover(Vector3 targetCenter)
    {
        //// Flatten wind direction onto the horizontal XZ plane so rays don't shoot up/down
        //Vector3 windDir = transform.forward;
        //windDir.y = 0f; // Force horizontal raycast parallel to the ground

        //Vector3 upstreamDirection = -windDir.normalized;

        //// Set a fixed max distance across the wind zone rather than a huge diagonal range
        //float maxRayDistance = zoneCollider.bounds.size.z;

        //if (Physics.Raycast(targetCenter, upstreamDirection, out RaycastHit hit, maxRayDistance, obstacleLayer))
        //{
        //    Debug.DrawLine(targetCenter, hit.point, Color.green);
        //    return true; // Blocked by wall
        //}

        //Debug.DrawRay(targetCenter, upstreamDirection * maxRayDistance, Color.red);
        //return false; // Path clear

        Vector3 windDir = transform.forward;
        windDir.y = 0f;
        windDir.Normalize();

        Vector3 upstreamDirection = -windDir;

        // Long enough to always reach past the zone's origin, regardless of
        // the zone's rotation (bounds.size isn't reliable for a rotated object).
        float maxRayDistance = zoneCollider.bounds.size.magnitude;

        if (Physics.Raycast(targetCenter, upstreamDirection, out RaycastHit hit, maxRayDistance, obstacleLayer))
        {
            Debug.DrawLine(targetCenter, hit.point, Color.green);
            return true;
        }

        Debug.DrawRay(targetCenter, upstreamDirection * maxRayDistance, Color.red);
        return false;
    }

    // Apply horizontal wind force to the player's Rigidbody
    private void ApplyHorizontalWind(Rigidbody rb, Vector3 direction)
    {
        // Measure current speed along the wind direction
        float currentSpeed = Vector3.Dot(rb.linearVelocity, direction);
        float speedError = targetSpeed - currentSpeed;

        // Apply force to reach and maintain target speed
        Vector3 force = direction * (speedError * pushStrength);
        rb.AddForce(force, ForceMode.Force);
    }

    // Apply vertical levitation to counteract gravity and keep the player at a stable height
    private void ApplyVerticalLevitation(Rigidbody rb)
    {
        // Target Y position is the vertical center of this WindZone box
        float targetY = zoneCollider.bounds.center.y;
        float heightError = targetY - rb.position.y;

        // Counteract gravity + apply spring force to lock Y height over the pit
        float gravityCounterForce = -Physics.gravity.y * rb.mass;
        float springForce = heightError * verticalSpringStrength;
        float dampingForce = -rb.linearVelocity.y * verticalDamping;

        float netVerticalForce = gravityCounterForce + springForce + dampingForce;

        rb.AddForce(Vector3.up * netVerticalForce, ForceMode.Force);
    }
}