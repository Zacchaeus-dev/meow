using UnityEngine;

// [Component Pattern] (Decoupling) - FireProjectile is a component that can be attached to any GameObject to give it projectile behavior.
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class FireProjectile : MonoBehaviour
{
    [SerializeField] private float travelSpeed = 8f;
    [SerializeField] private float lifeTime = 3f;

    [Tooltip("If true, this fireball can also melt objects tagged 'BigIce' (Fire + Fire combo). A solo Fire projectile leaves this false.")]
    [SerializeField] private bool canMeltBigIce = false;

    private Rigidbody rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        GetComponent<Collider>().isTrigger = true;
    }
    // Launch the projectile in a specified direction, optionally ignoring a specific collider 
    public void Launch(Vector3 direction, Collider ignoreCollider = null)
    {
        rb.linearVelocity = direction.normalized * travelSpeed; 
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Ice"))
        {
            Destroy(other.gameObject);
            Debug.Log("Fire projectile melted ice.");
        }
        else if (canMeltBigIce && other.CompareTag("BigIce"))
        {
            Destroy(other.gameObject);
            Debug.Log("Fire + Fire melted big ice.");
        }

        Destroy(gameObject);
    }
}
