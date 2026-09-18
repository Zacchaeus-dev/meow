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

    // For a single instant hit (e.g. a projectile on contact).
    public void Move(Vector3 direction, float windStrength)
    {
        if (!CanBeMoved(windStrength)) return;

        Rigidbody rb = GetComponent<Rigidbody>();
        float appliedForce = (windStrength * moveSpeed) / weight;
        rb.AddForce(direction.normalized * appliedForce, ForceMode.Impulse);
    }

    // For continuous zones (e.g. a standing wind current) - call every physics tick while inside.
    public void PushContinuous(Vector3 direction, float windStrength)
    {
        if (!CanBeMoved(windStrength)) return;

        Rigidbody rb = GetComponent<Rigidbody>();
        float appliedForce = (windStrength * moveSpeed) / weight;
        rb.AddForce(direction.normalized * appliedForce, ForceMode.Force);
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
}