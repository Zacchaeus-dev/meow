using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterPlant : MonoBehaviour
{


    [Header("Growth Settings")]
    [Tooltip("Target scale multiplier when fully grown (e.g., 2 = double size).")]
    [SerializeField] private Vector3 targetScale = new Vector3(1f, 3f, 1f);

    [Tooltip("How fast the plant grows to its target scale.")]
    [SerializeField] private float growSpeed = 2f;

    [Header("Detection Settings")]
    [Tooltip("Tag used by water projectiles or water zones.")]
    [SerializeField] private string waterTag = "Water";

    private Vector3 initialScale;
    private bool isGrowing;


    private void Awake()
    {
        initialScale = transform.localScale;
    }

    private void Update()
    {
        if (isGrowing)
        {
            // Smoothly scale towards the target grown scale
            transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, growSpeed * Time.deltaTime);

            // Stop growing once fully scaled
            if (Vector3.Distance(transform.localScale, targetScale) < 0.01f)
            {
                transform.localScale = targetScale;
                isGrowing = false;
            }
        }
    }

    // Works with Trigger volumes (e.g. Water Zone or Trigger Water Projectile)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(waterTag))
        {
            TriggerGrowth();
        }
    }

    // Works with Physical Colliders (e.g. Solid Water Projectile)
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(waterTag))
        {
            TriggerGrowth();
        }
    }

    /// <summary>
    /// Call this manually or via events to force plant growth.
    /// </summary>
    public void TriggerGrowth()
    {
        isGrowing = true;
        Debug.Log($"[WaterPlant] Water hit {gameObject.name}! Plant is growing.");
    }

}
