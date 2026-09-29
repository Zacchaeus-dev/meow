using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class BlizzardCurrent : MonoBehaviour
{
    [Tooltip("Tag on the compact water prefab (Water + Water combo).")]
    [SerializeField] private string compactWaterTag = "CompactWater";

    [Tooltip("What the compact water turns into. Tag this prefab 'Ice' (or 'BigIce') so Fire can melt it.")]
    [SerializeField] private GameObject frozenIcePrefab;

    private void Awake()
    {
        // Trigger events need at least one Rigidbody involved, and the
        // compact water is probably static, so give this zone a kinematic one.
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(compactWaterTag)) return;

        if (frozenIcePrefab == null)
        {
            Debug.LogWarning("Frozen Ice prefab not assigned on BlizzardFreezer.");
            return;
        }

        Instantiate(frozenIcePrefab, other.transform.position, other.transform.rotation);

        // Destroy() is deferred to the end of the frame, so deactivate first
        // to stop this water triggering a second freeze on the next physics step.
        other.gameObject.SetActive(false);
        Destroy(other.gameObject);
        Debug.Log("Blizzard froze compact water into ice.");
    }
}
