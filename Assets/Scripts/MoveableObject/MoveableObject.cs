using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoveableObject : MonoBehaviour // [Component Pattern] (Decoupling)
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float weight = 1f;
    [SerializeField] private ObjectType objectType;

    [Tooltip("Minimum wind strength required to move a HEAVY object. LIGHT objects can always be moved.")]
    [SerializeField] private float heavyWindThreshold = 2f;

    public ObjectType ObjectType => objectType;

    // For a single instant hit
    public void Move(Vector3 direction, float windStrength)
    {
        if (!CanBeMoved(windStrength)) return;

        Rigidbody rb = GetComponent<Rigidbody>();
        float appliedForce = (windStrength * moveSpeed) / weight;
        rb.AddForce(direction.normalized * appliedForce, ForceMode.VelocityChange);
    }

    // For continuous zone - call every physics tick while inside.
    public void PushContinuous(Vector3 direction, float windStrength)
    {
        if (!CanBeMoved(windStrength)) return;

        Rigidbody rb = GetComponent<Rigidbody>();
        float appliedForce = (windStrength * moveSpeed) / weight;
        rb.AddForce(direction.normalized * appliedForce, ForceMode.Acceleration);
    }

    // Check if the object can be moved based on its type and the wind strength.
    private bool CanBeMoved(float windStrength)
    {
        if (objectType == ObjectType.HEAVY && windStrength < heavyWindThreshold)
        {
            return false;
        }
        return true;
    }

    public void HoverContinuous(float targetY, float windStrength, float springStrength, float damping)
    {
        if (!CanBeMoved(windStrength)) return;

        Rigidbody rb = GetComponent<Rigidbody>();

        float heightError = targetY - rb.position.y;
        float gravityCounter = -Physics.gravity.y;            // cancels gravity as an acceleration
        float spring = heightError * springStrength;          // pulls toward the hover height
        float dampingAccel = -rb.linearVelocity.y * damping;  // stops it bouncing

        rb.AddForce(Vector3.up * (gravityCounter + spring + dampingAccel), ForceMode.Acceleration);
    }
}