using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// [Factory Method Pattern] Mirrors RuneEffectFactory exactly, but for combo
/// effects instead of solo ones. Owns every combo prefab, every "single
/// active instance per altar" tracking variable, and all spawn logic -
/// Altar no longer knows any of these details, just delegates through
/// IComboEffectFactory.
/// </summary>
public class ComboEffectFactory : MonoBehaviour, IComboEffectFactory
{
    [Header("Wind + Float Combo (L-shaped)")]
    [Tooltip("Vertical float zone at the base of the L.")]
    [SerializeField] private GameObject comboFloatZonePrefab;
    [Tooltip("Horizontal wind zone at the top of the L, catching the player after they float up.")]
    [SerializeField] private GameObject comboWindZonePrefab;
    [Tooltip("How high the float zone lifts before the wind zone takes over.")]
    [SerializeField] private float comboLiftHeight = 4f;

    [Header("Earth + Float Combo")]
    [SerializeField] private GameObject floatingTerrainPrefab;

    [Header("Earth + Smooth Combo")]
    [SerializeField] private GameObject smoothEarthBlockPrefab;

    [Header("Earth + Sticky Combo")]
    [SerializeField] private GameObject stickyEarthBlockPrefab;

    [Header("Fire + Fire Combo")]
    [SerializeField] private GameObject bigFireZonePrefab;

    [Header("Fire + Wind Combo")]
    [SerializeField] private GameObject longFireZonePrefab;

    [Header("Earth + Earth Combo")]
    [SerializeField] private GameObject bigEarthBlockPrefab;

    [Header("Wind + Wind Combo")]
    [SerializeField] private GameObject strongWindPrefab;

    [Header("Float + Float Combo")]
    [SerializeField] private GameObject strongFloatPrefab;

    [Header("Wind + Earth Combo")]
    [SerializeField] private GameObject windBlockPrefab;

    [Header("Water + Earth Combo")]
    [SerializeField] private GameObject waterBlockPrefab;

    [Header("Water + Wind Combo")]
    [SerializeField] private GameObject waterSprayPrefab;

    [Header("Water + Water Combo")]
    [SerializeField] private GameObject compactWaterPrefab;

    [Header("Fire + Water Combo")]
    [Tooltip("Spawns centered on the altar (centerPosition), not the front spawn point like every other combo.")]
    [SerializeField] private GameObject steamZonePrefab;

    // Tracks the single active instance per combo, so triggering the same
    // combo twice replaces the old one instead of stacking duplicates.
    private Dictionary<string, GameObject> activeInstances = new Dictionary<string, GameObject>();

    public bool SpawnCombo(Altar.RuneCombo combo, Vector3 frontPosition, Vector3 centerPosition, Quaternion rotation)
    {
        if (Matches(combo, RuneType.WIND, RuneType.FLOAT)) return SpawnWindFloat(frontPosition, rotation);
        if (Matches(combo, RuneType.EARTH, RuneType.FLOAT)) return SpawnPersistent("EarthFloat", floatingTerrainPrefab, frontPosition, Quaternion.identity);
        if (Matches(combo, RuneType.EARTH, RuneType.SMOOTH)) return SpawnPersistent("EarthSmooth", smoothEarthBlockPrefab, frontPosition, rotation);
        if (Matches(combo, RuneType.EARTH, RuneType.STICKY)) return SpawnPersistent("EarthSticky", stickyEarthBlockPrefab, frontPosition, rotation);
        if (Matches(combo, RuneType.FIRE, RuneType.FIRE)) return SpawnPersistent("FireFire", bigFireZonePrefab, frontPosition, rotation);
        if (Matches(combo, RuneType.FIRE, RuneType.WIND)) return SpawnPersistent("FireWind", longFireZonePrefab, frontPosition, rotation);
        if (Matches(combo, RuneType.EARTH, RuneType.EARTH)) return SpawnPersistent("EarthEarth", bigEarthBlockPrefab, frontPosition, rotation);
        if (Matches(combo, RuneType.WIND, RuneType.WIND)) return SpawnPersistent("WindWind", strongWindPrefab, frontPosition, rotation);
        if (Matches(combo, RuneType.FLOAT, RuneType.FLOAT)) return SpawnPersistent("FloatFloat", strongFloatPrefab, frontPosition, rotation);
        if (Matches(combo, RuneType.WIND, RuneType.EARTH)) return SpawnPersistent("WindEarth", windBlockPrefab, frontPosition, rotation);
        if (Matches(combo, RuneType.WATER, RuneType.EARTH)) return SpawnPersistent("WaterEarth", waterBlockPrefab, frontPosition, rotation);
        if (Matches(combo, RuneType.WATER, RuneType.WIND)) return SpawnPersistent("WaterWind", waterSprayPrefab, frontPosition, rotation);
        if (Matches(combo, RuneType.WATER, RuneType.WATER)) return SpawnPersistent("WaterWater", compactWaterPrefab, frontPosition, rotation);
        if (Matches(combo, RuneType.FIRE, RuneType.WATER)) return SpawnPersistent("FireWater", steamZonePrefab, centerPosition, Quaternion.identity);

        Debug.Log($"Combo effect for {combo.runeA} + {combo.runeB} not yet implemented.");
        return false;
    }

    // Order-independent match check (WIND+FLOAT == FLOAT+WIND).
    private bool Matches(Altar.RuneCombo combo, RuneType a, RuneType b)
    {
        return (combo.runeA == a && combo.runeB == b) || (combo.runeA == b && combo.runeB == a);
    }

    // Shared logic for any combo that's a single spawned object.
    private bool SpawnPersistent(string key, GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"{key} prefab not assigned on ComboEffectFactory.");
            return false;
        }

        if (activeInstances.TryGetValue(key, out GameObject existing) && existing != null)
        {
            Destroy(existing);
        }

        activeInstances[key] = Instantiate(prefab, position, rotation);
        Debug.Log($"Altar created a {key} combo effect.");
        return true;
    }

    // Wind + Float is the one combo that spawns TWO objects (an L-shape),
    // so it can't reuse the single-object SpawnPersistent helper as-is.
    private bool SpawnWindFloat(Vector3 frontPosition, Quaternion rotation)
    {
        if (comboFloatZonePrefab == null || comboWindZonePrefab == null)
        {
            Debug.LogWarning("Combo Float/Wind Zone prefabs not assigned on ComboEffectFactory.");
            return false;
        }

        if (activeInstances.TryGetValue("WindFloat_Float", out GameObject existingFloat) && existingFloat != null)
            Destroy(existingFloat);
        if (activeInstances.TryGetValue("WindFloat_Wind", out GameObject existingWind) && existingWind != null)
            Destroy(existingWind);

        // Bottom of the L: float zone lifts the player straight up.
        activeInstances["WindFloat_Float"] = Instantiate(comboFloatZonePrefab, frontPosition, rotation);

        // Top of the L: wind zone catches the player at the top of the lift.
        Vector3 windSpawnPos = frontPosition + Vector3.up * comboLiftHeight;
        activeInstances["WindFloat_Wind"] = Instantiate(comboWindZonePrefab, windSpawnPos, rotation);

        Debug.Log("Altar created an L-shaped wind + float current.");
        return true;
    }
}