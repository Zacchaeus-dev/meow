using UnityEngine;

// The contract: anything the player can interact with must be able to Activate() and Deactivate().
// This doesn't care HOW - just that it can.
public interface IInteractable
{
    void Activate();
    void Deactivate();
}
