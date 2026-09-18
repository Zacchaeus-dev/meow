using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Altar is the interactable object players use to insert Runes and trigger their effects.
/// It inherits from Mechanism, sharing a common interaction (Activate / Deactivate / Effect) with other interactables in the game.
/// </summary>

public class Altar : Mechanism
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    private bool playerInRange;
    private bool menuOpen;
    private PlayerInventory currentInventory;

    [Header("Slots")]
    private Rune slot1;
    private Rune slot2;

    [Header("Valid Combos")]
    [Tooltip("Define which pairs of RuneTypes combine successfully. Order doesn't matter (WIND+FIRE == FIRE+WIND).")]
    [SerializeField] private List<RuneCombo> validCombos = new List<RuneCombo>();

    [Header("Effect Prefabs")]
    [Tooltip("Spawned in front of the altar (transform.forward) when the matching rune is used solo.")]
    [SerializeField] private GameObject fireProjectilePrefab;
    [SerializeField] private GameObject windZonePrefab;
    [SerializeField] private GameObject floatZonePrefab;
    [SerializeField] private GameObject earthBlockPrefab;
    [SerializeField] private float spawnDistance = 2f;
    [SerializeField] private float spawnHeight = 1f;
  
    // Wind + Float combo effect is a special case where the two runes work together to create an L-shaped current.
    [Header("Wind + Float Combo (L-shaped)")]
    [Tooltip("Vertical float zone at the base of the L.")]
    [SerializeField] private GameObject comboFloatZonePrefab;
    [Tooltip("Horizontal wind zone at the top of the L, catching the player after they float up.")]

    [SerializeField] private GameObject comboWindZonePrefab;
    [Tooltip("How high the float zone lifts before the wind zone takes over.")]
    [SerializeField] private float comboLiftHeight = 4f;

    [Header("Earth + Float Combo")]
    [SerializeField] private GameObject floatingTerrainPrefab;
    // Each of these tracks the single active instance of its effect per altar,
    // so triggering the same effect twice replaces the old one instead of stacking.
    private GameObject activeFloatZone; // only one float zone per altar at a time
    private GameObject activeWindZone; // only one wind zone per altar at a time
    private GameObject activeEarthBlock; // only one earth block per altar at a time
    private GameObject activeComboFloatZone;
    private GameObject activeComboWindZone;
    private GameObject activeFloatingTerrain;

    [System.Serializable]
    public struct RuneCombo
    {
        public RuneType runeA;
        public RuneType runeB;

        public RuneCombo(RuneType a, RuneType b)
        {
            runeA = a;
            runeB = b;
        }
    }

    // [Observer Pattern] (Decoupling) Events to notify the UI when the menu opens/closes and when slots change.
    public event System.Action<PlayerInventory> OnMenuOpened;
    public event System.Action OnMenuClosed;
    public event System.Action OnSlotsChanged;

    public Rune Slot1 => slot1;
    public Rune Slot2 => slot2;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // [Component Pattern] (Decoupling) Get the PlayerInventory component from the player.
            currentInventory = other.GetComponent<PlayerInventory>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (menuOpen)
            {
                CloseMenu();
            }
        }
    }

    private void Update()
    {
        if (playerInRange && !menuOpen && Input.GetKeyDown(interactKey))
        {
            Activate();
        }

        if (menuOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }
    }
    // Template Method override: this is what "Activate" specifically means for 
    // an Altar - open the rune menu. base.Activate() keeps Mechanism's shared ActivationStatus flag in sync.
    // [Template Method Pattern] (Gang of Four/ GoF)
    public override void Activate()
    {
        base.Activate();
        menuOpen = true;
        slot1 = null;
        slot2 = null;
        OnMenuOpened?.Invoke(currentInventory);
    }
    // This is called by the UI when the player clicks a rune to place it.
    // Fills whichever slot is empty first. then returns false if both are full.
    public bool TryPlaceRune(Rune rune)
    {
        if (!menuOpen) return false;

        if (slot1 == null)
        {
            slot1 = rune;
            slot1.AltarInsert();
        }
        else if (slot2 == null)
        {
            slot2 = rune;
            slot2.AltarInsert();
        }
        else
        {
            return false;
        }

        currentInventory?.RemoveRune(rune);
        OnSlotsChanged?.Invoke();
        return true;
    }
    // This closes the menu is the trigger for resolution, not just a UI dismiss action.
    public void CloseMenu()
    {
        if (!menuOpen) return;
        menuOpen = false;
        Effect();
        Deactivate();
        OnMenuClosed?.Invoke();
    }

    // Template Method override: the actual branching logic for 0/1/2 runes.
    // Effect now fires immediately, spawning results in front of the altar instead of crafting a carryable item.
    // [Template Method Pattern] (Gang of Four/ GoF)
    public override void Effect()
    {
        bool hasSlot1 = slot1 != null;
        bool hasSlot2 = slot2 != null;

        if (!hasSlot1 && !hasSlot2)
        {
            Debug.Log("No runes placed - no effect.");
        }
        else if (hasSlot1 && !hasSlot2)
        {
            SpawnRuneEffect(slot1.RuneType);
        }
        else if (!hasSlot1 && hasSlot2)
        {
            SpawnRuneEffect(slot2.RuneType);
        }
        else
        {
            RuneCombo combo = new RuneCombo(slot1.RuneType, slot2.RuneType);
            if (IsValidCombo(combo))
            {
                TriggerComboEffect(combo);
            }
            else
            {
                Debug.Log("Invalid combo - runes rejected and returned to inventory.");
                ReturnRuneToInventory(slot1);
                ReturnRuneToInventory(slot2);
            }
        }

        slot1 = null;
        slot2 = null;
    }

    // Common spawn point for every solo effect: a fixed distance and height in front of the altar, along its own facing direction.
    private Vector3 GetSpawnPosition()
    {
        return transform.position + Vector3.up * spawnHeight + transform.forward * spawnDistance;
    }

    // Routes a single valid rune to its matching effect. WATER is intentionally absent
    // it's a bucket-style item used directly from the inventory, not something that involves the altar at all.
    private void SpawnRuneEffect(RuneType type)
    {
        switch (type)
        {
            case RuneType.FIRE:
                SpawnFire();
                break;
            case RuneType.WIND:
                SpawnWind();
                break;
            case RuneType.FLOAT:
                SpawnFloatZone();
                break;
            case RuneType.EARTH:
                SpawnEarthBlock();
                break;
            default:
                Debug.Log($"{type} has no defined solo effect yet.");
                break;
        }
    }

    private void SpawnFire()
    {
        if (fireProjectilePrefab == null)
        {
            Debug.LogWarning("Fire Projectile prefab not assigned on Altar.");
            return;
        }

        GameObject projectile = Instantiate(fireProjectilePrefab, GetSpawnPosition(), Quaternion.LookRotation(transform.forward));
        FireProjectile fire = projectile.GetComponent<FireProjectile>();
        fire.Launch(transform.forward);
        Debug.Log("Altar launched a fireball.");
    }

    private void SpawnWind()
    {
        if (windZonePrefab == null)
        {
            Debug.LogWarning("Wind Zone prefab not assigned on Altar.");
            return;
        }

        if (activeWindZone != null)
        {
            Destroy(activeWindZone);
        }

        activeWindZone = Instantiate(windZonePrefab, GetSpawnPosition(), Quaternion.LookRotation(transform.forward));
        Debug.Log("Altar created a wind current.");
    }

    private void SpawnFloatZone()
    {
        if (floatZonePrefab == null)
        {
            Debug.LogWarning("Float Zone prefab not assigned on Altar.");
            return;
        }

        // Only one float zone fixture per altar - replace the old one if it exists.
        if (activeFloatZone != null)
        {
            Destroy(activeFloatZone);
        }

        activeFloatZone = Instantiate(floatZonePrefab, GetSpawnPosition(), Quaternion.LookRotation(transform.forward));
        Debug.Log("Altar created a float current.");
    }

    private void SpawnEarthBlock()
    {
        if (earthBlockPrefab == null)
        {
            Debug.LogWarning("Earth Block prefab not assigned on Altar.");
            return;
        }

        // Only one earth block fixture per altar - replace the old one if it exists.
        if (activeEarthBlock != null)
        {
            Destroy(activeEarthBlock);
        }

        activeEarthBlock = Instantiate(earthBlockPrefab, GetSpawnPosition(), Quaternion.LookRotation(transform.forward));
        Debug.Log("Altar created a terrain block.");
    }

    private bool IsValidCombo(RuneCombo runeCombo)
    {
        foreach (var combo in validCombos)
        {
            if ((combo.runeA == runeCombo.runeA && combo.runeB == runeCombo.runeB) || (combo.runeA == runeCombo.runeB && combo.runeB == runeCombo.runeA))
            {
                return true;
            }
        }
        return false;
    }
    // Routes valid combos to their specific implementation. Only Wind + Float is implementd so far, but this is where future combos would be handled.
    private void TriggerComboEffect(RuneCombo runeCombo)
    {
        bool isWindFloat = (runeCombo.runeA == RuneType.WIND && runeCombo.runeB == RuneType.FLOAT) || (runeCombo.runeA == RuneType.FLOAT && runeCombo.runeB == RuneType.WIND);
        bool isEarthFloat = (runeCombo.runeA == RuneType.EARTH && runeCombo.runeB == RuneType.FLOAT) || (runeCombo.runeA == RuneType.FLOAT && runeCombo.runeB == RuneType.EARTH);

        if (isWindFloat)
        {
            SpawnWindFloatCombo();
        }
        else if (isEarthFloat)
        {
            SpawnFloatingTerrain();
        }
        else 
        {
            Debug.Log($"Combo effect triggered: {runeCombo.runeA} + {runeCombo.runeB}");
        }
    }

    // Earth + Float combo: a rising platform the player can stand on and
    // ride up to a fixed height, like a stairstep.
    private void SpawnFloatingTerrain()
    {
        if (floatingTerrainPrefab == null)
        {
            Debug.LogWarning("Floating Platform prefab not assigned on Altar.");
            return;
        }

        if (activeFloatingTerrain != null)
        {
            Destroy(activeFloatingTerrain);
        }

        activeFloatingTerrain = Instantiate(floatingTerrainPrefab, GetSpawnPosition(), Quaternion.identity);
        Debug.Log("Altar created a floating terrain.");
    }


    // Wind + Float combo, built as an "L" shape per the designer's intent:
    // a FloatZone lifts the player straight up, and a WindZone positioned above it catches them at the top of that lift and carries them forward.
    private void SpawnWindFloatCombo()
    {
        if (comboFloatZonePrefab == null || comboWindZonePrefab == null)
        {
            Debug.LogWarning("Combo Float/Wind Zone prefabs not assigned on Altar.");
            return;
        }

        if (activeComboFloatZone != null)
        {
            Destroy(activeComboFloatZone);
        }
        if (activeComboWindZone != null)
        {
            Destroy(activeComboWindZone);
        }

        // Botom of the L: float zone lifts the player straight up.
        Vector3 floatSpawnPos = GetSpawnPosition();
        activeComboFloatZone = Instantiate(comboFloatZonePrefab, floatSpawnPos, Quaternion.LookRotation(transform.forward));

        //Top of the L: wind zone catches the player at the top of the lift.
        // and pushes them forward for there.
        Vector3 windSpawnPos = floatSpawnPos + Vector3.up * comboLiftHeight;
        activeComboWindZone = Instantiate(comboWindZonePrefab, windSpawnPos, Quaternion.LookRotation(transform.forward));

        Debug.Log("Altar created an L-shaped wind + float current.");
    }

    private void ReturnRuneToInventory(Rune rune)
    {
        currentInventory?.AddRune(rune);
        Debug.Log($"{rune.RuneType} rune returned to inventory.");
    }
}