using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FloatZone : MonoBehaviour
{
    [SerializeField] private float targetHeightOffset = 1.0f; // Height relative to zone center
    [SerializeField] private float springStrength = 50f;
    [SerializeField] private float damping = 5f;

    private Collider zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();
        zoneCollider.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Rigidbody rb = other.GetComponent<Rigidbody>(); // [Component Pattern] (Decoupling)
        if (rb == null) return;

        // Calculate target Y position inside the zone
        float targetY = zoneCollider.bounds.max.y - targetHeightOffset;
        float heightError = targetY - rb.position.y;

        // Calculate spring force: stronger force when lower, less force when high
        float springForce = heightError * springStrength;

        // Damping resists current velocity to prevent bouncy float/soft fall loops
        float dampingForce = -rb.linearVelocity.y * damping;

        // Apply net buoyancy force
        float finalYForce = springForce + dampingForce;
        rb.AddForce(Vector3.up * finalYForce, ForceMode.Force);
    }
}