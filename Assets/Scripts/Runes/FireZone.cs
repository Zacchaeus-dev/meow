using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FireZone : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;

    [Tooltip("If true, this fireball can also melt objects tagged 'BigIce' (Fire + Fire combo). A solo Fire projectile leaves this false.")]
    [SerializeField] private bool canMeltBigIce = false;

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
        CheckAndMelt(other);
        DestoryBlight(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // Re-check objects staying inside the zone (e.g., if ice enters or moves into the fire stream)
        CheckAndMelt(other);
        DestoryBlight(other);
    }

    private void CheckAndMelt(Collider other)
    {
        if (other.CompareTag("Ice"))
        {
            Destroy(other.gameObject);
            Debug.Log("Flamethrower zone melted standard ice.");
        }
        else if (canMeltBigIce && other.CompareTag("BigIce"))
        {
            Destroy(other.gameObject);
            Debug.Log("Upgraded (Fire + Fire) flamethrower zone melted BigIce.");
        }
    }

    private void DestoryBlight(Collider other)
    {
        if (other.CompareTag("Blight"))
        {
            Destroy(other.gameObject);
            
        }
    }
}
