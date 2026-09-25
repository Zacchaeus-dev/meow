using UnityEngine;

public class WaterSpray : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;

    private Collider zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();
        zoneCollider.isTrigger = true;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckPlants(other);
    }

    private void OnTriggerStay(Collider other)
    {
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
