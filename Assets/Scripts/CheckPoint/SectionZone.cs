using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SectionZone : MonoBehaviour
{
    [SerializeField] Transform sectionSpawn; // required: where the player respawns
    [SerializeField] int sectionIndex;       // 1, 2, 3... set per zone
    bool reached;

    void Reset() => GetComponent<Collider>().isTrigger = true;

    void OnTriggerEnter(Collider other)
    {
        if (reached || !other.CompareTag("Player")) return;

        if (sectionSpawn == null)
        {
            Debug.LogWarning($"{name}: no sectionSpawn assigned!", this);
            return;
        }

        reached = true; // only saves the first time
        CheckpointManager.Instance.SetCheckpoint(sectionIndex, sectionSpawn.position, sectionSpawn.rotation);
    }
}
