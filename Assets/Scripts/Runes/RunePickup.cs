using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RunePickup : MonoBehaviour
{
    [SerializeField] private RuneType runeType;
    [SerializeField] private KeyCode pickupKey = KeyCode.E;

    private bool playerInRange;
    private PlayerInventory playerInventory;

    private void Reset()
    {
        // Make sure the collider is set up as a trigger by default
        GetComponent<Collider>().isTrigger = true;
    }

    // Ensure the collider is a trigger
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        playerInventory = other.GetComponent<PlayerInventory>(); // [Component Pattern] (Decoupling)
    }

    // Ensure the collider is a trigger
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        playerInventory = null;
    }

    // Check for player input to pick up the rune
    private void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(pickupKey))
        {
            TryPickup();
        }
    }
    // Attempt to add the rune to the player's inventory
    private void TryPickup()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("Player has no PlayerInventory component - rune not collected.");
            return;
        }

        playerInventory.AddRune(new Rune(runeType)); // [Factory / Creational Instantiation]
        Debug.Log($"{runeType} rune picked up.");

        Destroy(gameObject);
    }
}