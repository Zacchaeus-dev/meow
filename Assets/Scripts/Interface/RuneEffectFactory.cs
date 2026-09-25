using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Factory Method pattern: centralizes HOW each rune's solo effect gets
/// created, so Altar doesn't need to know construction details for each
/// effect type. Implements IRuneEffectFactory so Altar depends on the
/// contract, not this concrete class - a different factory (e.g. for
/// testing, or a "weakened altar" variant) could be swapped in later
/// without touching Altar at all.
/// </summary>
public class RuneEffectFactory : MonoBehaviour, IRuneEffectFactory
{
    [Header("Solo Effect Prefabs")]
    [SerializeField] private GameObject fireZonePrefab;
    [SerializeField] private GameObject windZonePrefab;
    [SerializeField] private GameObject floatZonePrefab;
    [SerializeField] private GameObject earthBlockPrefab;

    // Tracks the single active instance per persistent effect type, so
    // triggering the same effect twice replaces the old one instead of
    // stacking duplicates. Fire is transient (self-destructs) so it's
    // never stored here.
    private Dictionary<RuneType, GameObject> activeInstances = new Dictionary<RuneType, GameObject>();

    public bool SpawnEffect(RuneType type, Vector3 position, Quaternion rotation)
    {
        switch (type)
        {
            case RuneType.FIRE:
                return SpawnPersistent(type, fireZonePrefab, position, rotation, "fire zone");
            case RuneType.WIND:
                return SpawnPersistent(type, windZonePrefab, position, rotation, "wind current");
            case RuneType.FLOAT:
                return SpawnPersistent(type, floatZonePrefab, position, rotation, "float current");
            case RuneType.EARTH:
                return SpawnPersistent(type, earthBlockPrefab, position, rotation, "terrain block");
            default:
                Debug.Log($"{type} has no defined solo effect yet.");
                return false;
        }
    }

    //private bool SpawnFire(Vector3 position, Quaternion rotation)
    //{
    //    if (fireProjectilePrefab == null)
    //    {
    //        Debug.LogWarning("Fire Projectile prefab not assigned on RuneEffectFactory.");
    //        return false;
    //    }

    //    GameObject projectile = Instantiate(fireProjectilePrefab, position, rotation);
    //    FireProjectile fire = projectile.GetComponent<FireProjectile>();
    //    fire.Launch(rotation * Vector3.forward);
    //    Debug.Log("Spawned a fireball.");
    //    return true;
    //}

    // Shared logic for any effect that stays in the world as a single
    // persistent instance per rune type, replacing itself if triggered again.
    private bool SpawnPersistent(RuneType type, GameObject prefab, Vector3 position, Quaternion rotation, string label)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"{type} prefab not assigned on RuneEffectFactory.");
            return false;
        }

        if (activeInstances.TryGetValue(type, out GameObject existing) && existing != null)
        {
            Destroy(existing);
        }

        GameObject instance = Instantiate(prefab, position, rotation);
        activeInstances[type] = instance;
        Debug.Log($"Created a {label}.");
        return true;
    }
}