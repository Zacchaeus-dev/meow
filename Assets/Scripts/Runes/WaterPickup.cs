using UnityEngine;
using System.Linq; // For LINQ methods like FirstOrDefault

[RequireComponent(typeof(Collider))]
public class WaterPickup : MonoBehaviour
{
    [SerializeField] private KeyCode scoopKey = KeyCode.E;

    private bool playerInRange;
    private PlayerInventory playerInventory;

    // Ensure the collider is set up as a trigger
    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    // Detect when the player enters the trigger zone for the water pickup
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        playerInventory = other.GetComponent<PlayerInventory>();
    }
    
    // Detect when the player exits the trigger zone for the water pickup
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        playerInventory = null;
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(scoopKey))
        {
            Scoop();
        }
    }
    
    // Attempt to fill an empty Water bucket in the player's inventory
    private void Scoop()
    {
        if (playerInventory == null) return;

        Rune emptyBucket = playerInventory.Runes.FirstOrDefault(r => r.RuneType == RuneType.WATER && !r.IsFilled);

        if (emptyBucket == null)
        {
            Debug.Log("Need an empty Water bucket to scoop water.");
            return;
        }

        emptyBucket.Fill();
        Debug.Log("Filled the bucket with water.");
        Destroy(gameObject);
    }
}