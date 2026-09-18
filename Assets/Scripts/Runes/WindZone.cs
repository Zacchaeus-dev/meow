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

    private Collider zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();
        zoneCollider.isTrigger = true;
    }

    // Called every physics tick while an object is inside the WindZone
    private void OnTriggerStay(Collider other)
    {
        Vector3 windDirection = transform.forward;

        if (other.CompareTag("Player"))
        {
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                ApplyHorizontalWind(playerRb, windDirection);
                ApplyVerticalLevitation(playerRb);
            }
            return;
        }

        MoveableObject movable = other.GetComponent<MoveableObject>();
        if (movable != null)
        {
            movable.PushContinuous(windDirection, objectPushStrength);
        }
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