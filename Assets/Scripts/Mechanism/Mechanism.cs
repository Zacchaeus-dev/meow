using UnityEngine;

// Base class for any trigger-activated mechanism in the game
// (Altars, Gates, Pressure Plates, Wind Current, etc.)
public abstract class Mechanism : MonoBehaviour
{
    [SerializeField] protected bool activationStatus;
    public bool ActivationStatus => activationStatus;

    // Called when the mechanism is triggered (collision/trigger enter, etc.)
    // Override to add mechanism-specific activation behavior,
    // but call base.Activate() to keep ActivationStatus in sync.
    public virtual void Activate()
    {
        activationStatus = true;
    }

    // Called when the mechanism should deactivate
    // (e.g. player walks away, lever reset, etc.)
    public virtual void Deactivate()
    {
        activationStatus = false;
    }

    // Each mechanism defines its own outcome when activated.
    public abstract void Effect();
}