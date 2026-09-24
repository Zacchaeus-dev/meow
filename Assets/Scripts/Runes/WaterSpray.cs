using UnityEngine;

public class WaterSpray : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;

    [Tooltip("If true, this fireball can also melt objects tagged 'BigIce' (Fire + Fire combo). A solo Fire projectile leaves this false.")]
   // [SerializeField] private bool canMeltBigIce = false;

    private Collider zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();
        zoneCollider.isTrigger = true;
    }

    private void Start()
    {
        // Automatically destroy the flamethrower zone once its active duration expires
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckPlants(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // Re-check objects staying inside the zone (e.g., if ice enters or moves into the fire stream)
        CheckPlants(other);
    }

    private void CheckPlants(Collider other)
    {
        if (other.CompareTag("Plant"))
        {
            Debug.Log("Plants watered");
        }
    }
}
