using UnityEngine;

// Earth + Sticky combo: identical solid-terrain behavior to a normal Earth
// block, but slows the player's movement speed while they're standing on
// top of it. Opposite effect of the Earth + Smooth combo.
[RequireComponent(typeof(Collider))]
public class StickyBlock : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float slowMultiplier = 0.4f; // 1 = normal speed, 0 = fully stuck

    [Tooltip("Layer mask assigned to the Player GameObject.")]
    [SerializeField] private LayerMask playerLayer;

    [Tooltip("How far above the top surface to scan for the player.")]
    [SerializeField] private float detectionHeight = 0.3f;

    private Collider blockCollider;
    private PlayerMovement currentPlayer;
    private bool playerOnTop;

    private void Awake()
    {
        blockCollider = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        bool wasOnTop = playerOnTop;
        playerOnTop = CheckIfPlayerIsOnTop();

        if (playerOnTop && !wasOnTop)
        {
            ApplySlow();
        }
        else if (!playerOnTop && wasOnTop)
        {
            RemoveSlow();
        }
    }

    private bool CheckIfPlayerIsOnTop()
    {
        Bounds bounds = blockCollider.bounds;
        Vector3 center = new Vector3(bounds.center.x, bounds.max.y + (detectionHeight * 0.5f), bounds.center.z);
        Vector3 halfExtents = new Vector3(bounds.extents.x * 0.9f, detectionHeight * 0.5f, bounds.extents.z * 0.9f);

        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, playerLayer, QueryTriggerInteraction.Ignore);

        if (hits.Length > 0)
        {
            currentPlayer = hits[0].GetComponent<PlayerMovement>();
            return currentPlayer != null;
        }

        return false;
    }

    private void ApplySlow()
    {
        currentPlayer?.SetSpeedMultiplier(slowMultiplier);
        Debug.Log("Player stuck on sticky terrain - slowed down.");
    }

    private void RemoveSlow()
    {
        currentPlayer?.SetSpeedMultiplier(1f);
        currentPlayer = null;
    }

    private void OnDestroy()
    {
        // Safety: if this block is destroyed while the player is still on
        // it (e.g. replaced by a new one), make sure their speed resets.
        RemoveSlow();
    }

    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Bounds bounds = col.bounds;
        Vector3 center = new Vector3(bounds.center.x, bounds.max.y + (detectionHeight * 0.5f), bounds.center.z);
        Vector3 size = new Vector3(bounds.size.x * 0.9f, detectionHeight, bounds.size.z * 0.9f);

        Gizmos.color = playerOnTop ? Color.yellow : Color.gray;
        Gizmos.DrawWireCube(center, size);
    }
}