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

    // [Factory Method Pattern] Delegates construction of solo effects (Fire/Wind/Float/Earth)
    // to RuneEffectFactory instead of Altar knowing the details of each one itself.
    [Header("Effect Factory")]
    [Tooltip("Handles construction of Fire/Wind/Float/Earth solo effects. See RuneEffectFactory.")]
    [SerializeField] private RuneEffectFactory effectFactory; // concrete type, so it's draggable in the Inspector
    [SerializeField] private float spawnDistance = 2f;
    [SerializeField] private float spawnHeight = 1f;

    // All calls to the factory go through this interface reference, not the
    // concrete field above - Altar's logic only ever depends on the contract.
    private IRuneEffectFactory EffectFactory => effectFactory;

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

    [Header("Earth + Smooth Combo")]
    [Tooltip("Same behavior as a solo Earth block, just a smooth-textured variant.")]
    [SerializeField] private GameObject smoothEarthBlockPrefab;

    [Header("Earth + Sticky Combo")]
    [Tooltip("Same behavior as a solo Earth block, just a sticky-textured variant.")]
    [SerializeField] private GameObject stickyEarthBlockPrefab;

    [Header("Fire + Fire Combo")]
    [Tooltip("If true, Fire + Fire combo can melt BigIce objects. Solo Fire projectiles cannot.")]
    [SerializeField] private GameObject bigFireZonePrefab;

    [Header("Fire + Wind Combo")]
    [Tooltip("If true, Fire + Wind combo cant melt BigIce objects.")]
    [SerializeField] private GameObject longFireZonePrefab;

    [Header("Earth + Earth Combo")]
    [Tooltip("Same behavior as a solo Earth block, just a sticky-textured variant.")]
    [SerializeField] private GameObject bigEarthBlockPrefab;

    [Header("Wind + Wind Combo")]
    [SerializeField] private GameObject strongWindPrefab;

    [Header("Float + Float Combo")]
    [SerializeField] private GameObject strongFloatPrefab;

    [Header("Earth + Wind Combo")]
    [SerializeField] private GameObject windBlockPrefab;

    [Header("Earth + Water Combo")]
    [SerializeField] private GameObject waterBlockPrefab;

    [Header("Wind + Water Combo")]
    [SerializeField] private GameObject waterSprayPrefab;

    [Header("Water + Water Combo")]
    [SerializeField] private GameObject compactWaterPrefab;
    // Each of these tracks the single active instance of its COMBO effect
    // per altar (solo effects are now tracked inside RuneEffectFactory).
    private GameObject activeComboFloatZone;
    private GameObject activeComboWindZone;
    private GameObject activeFloatingTerrain;
    private GameObject activeSmoothEarthBlock;
    private GameObject activeStickyEarthBlock;
    private GameObject activeBigFireZone;
    private GameObject activeBigEarthBlock;
    private GameObject activeLongFireZone;
    private GameObject activeStrongWind;
    private GameObject activeStrongFloatZone;
    private GameObject activeWindBlock;
    private GameObject activeWaterBlock;
    private GameObject activeWaterSpray;
    private GameObject activeCompactWater;

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

    // [Factory Method Pattern] Delegates to EffectFactory instead of knowing
    // construction details for each effect type. WATER is intentionally
    // absent - it's a bucket-style item used directly from the inventory,
    // not something that involves the altar at all.
    private void SpawnRuneEffect(RuneType type)
    {
        if (EffectFactory == null)
        {
            Debug.LogWarning("RuneEffectFactory not assigned on Altar.");
            return;
        }

        bool spawned = EffectFactory.SpawnEffect(type, GetSpawnPosition(), Quaternion.LookRotation(transform.forward));

        if (!spawned)
        {
            Debug.Log($"Failed to spawn effect for {type}.");
        }
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

    // Routes valid combos to their specific implementation. Only Wind + Float and
    // Earth + Float are implemented so far, but this is where future combos would be handled.
    private void TriggerComboEffect(RuneCombo runeCombo)
    {
        bool isWindFloat = (runeCombo.runeA == RuneType.WIND && runeCombo.runeB == RuneType.FLOAT) || (runeCombo.runeA == RuneType.FLOAT && runeCombo.runeB == RuneType.WIND);
        bool isEarthFloat = (runeCombo.runeA == RuneType.EARTH && runeCombo.runeB == RuneType.FLOAT) || (runeCombo.runeA == RuneType.FLOAT && runeCombo.runeB == RuneType.EARTH);
        bool isEarthSmooth = (runeCombo.runeA == RuneType.EARTH && runeCombo.runeB == RuneType.SMOOTH) || (runeCombo.runeA == RuneType.SMOOTH && runeCombo.runeB == RuneType.EARTH);
        bool isEarthSticky = (runeCombo.runeA == RuneType.EARTH && runeCombo.runeB == RuneType.STICKY) || (runeCombo.runeA == RuneType.STICKY && runeCombo.runeB == RuneType.EARTH);
        bool isFireFire = (runeCombo.runeA == RuneType.FIRE && runeCombo.runeB == RuneType.FIRE);
        bool isEarthEarth = (runeCombo.runeA == RuneType.EARTH && runeCombo.runeB == RuneType.EARTH);
        bool isWindWind = (runeCombo.runeA == RuneType.WIND && runeCombo.runeB == RuneType.WIND);
        bool isFireWind = (runeCombo.runeA == RuneType.FIRE && runeCombo.runeB == RuneType.WIND || runeCombo.runeA == RuneType.WIND && runeCombo.runeB == RuneType.FIRE);
        bool isFloatFloat = (runeCombo.runeA == RuneType.FLOAT && runeCombo.runeB == RuneType.FLOAT);
        bool isWindEarth = (runeCombo.runeA == RuneType.WIND && runeCombo.runeB == RuneType.EARTH || runeCombo.runeA == RuneType.EARTH && runeCombo.runeB == RuneType.WIND);
        bool isWaterEarth = (runeCombo.runeA == RuneType.WATER && runeCombo.runeB == RuneType.EARTH || runeCombo.runeA == RuneType.EARTH && runeCombo.runeB == RuneType.WATER);
        bool isWaterWind = (runeCombo.runeA == RuneType.WATER && runeCombo.runeB == RuneType.WIND || runeCombo.runeA == RuneType.WIND && runeCombo.runeB == RuneType.WATER);
        bool isWaterWater = (runeCombo.runeA == RuneType.WATER && runeCombo.runeB == RuneType.WATER);

        if (isWindFloat)
        {
            SpawnWindFloatCombo();
        }
        else if (isEarthFloat)
        {
            SpawnFloatingTerrain();
        }
        else if (isEarthSmooth)
        {
            SpawnSmoothEarthBlock();
        }
        else if (isEarthSticky)
        {
            SpawnStickyEarthBlock();
        }
        else if (isFireFire)
        {
            SpawnBigFire();
        }
        else if (isFireWind)
        {
            SpawnLongFire();
        }
        else if (isEarthEarth)
        {
            SpawnBigEarthBlock();
        }
        else if (isWindWind)
        {
            SpawnStrongWind();
        }
        else if (isFloatFloat)
        {
            SpawnStrongFloat();
        }
        else if (isWindEarth)
        {
            SpawnWindBlock();
        }
        else if (isWaterEarth)
        {
            SpawnWaterBlock();
        }
        else if (isWaterWind)
        {
            SpawnWaterSpray();
        }
        else if (isWaterWater)
        {
            SpawnWaterWater();
        }
        else
        {
            Debug.Log($"Combo effect triggered: {runeCombo.runeA} + {runeCombo.runeB}");
        }
    }

    private void SpawnWaterWater()
    {
        if (compactWaterPrefab == null)
        {
            Debug.LogWarning("prefab not assigned");
            return;
        }
        if (activeCompactWater != null)
        {
            Destroy(activeCompactWater);
        }
        activeCompactWater = Instantiate(compactWaterPrefab, GetSpawnPosition(), Quaternion.LookRotation(transform.forward));
        Debug.Log("Altar created compact water.");
    }

    private void SpawnWaterSpray()
    {
        if (waterSprayPrefab == null)
        {
            Debug.LogWarning("prefab not assigned");
            return;
        }
        if (activeWaterSpray != null)
        {
            Destroy(activeWaterSpray);
        }
        activeWaterSpray = Instantiate(waterSprayPrefab, GetSpawnPosition(), Quaternion.LookRotation(transform.forward));
        Debug.Log("Altar created water spray zone.");
    }

    private void SpawnWaterBlock()
    {
        if (waterBlockPrefab == null)
        {
            Debug.LogWarning("prefab not assigned");
            return;
        }
        if (activeWaterBlock != null)
        {
            Destroy(activeWaterBlock);
        }
        activeWaterBlock = Instantiate(waterBlockPrefab, GetSpawnPosition(), Quaternion.LookRotation(transform.forward));
        Debug.Log("Altar created a Mud zone.");
    }

    private void SpawnWindBlock()
    {
        if (windBlockPrefab == null)
        {
            Debug.LogWarning("prefab not assigned");
            return;
        }
        if (activeWindBlock != null)
        {
            Destroy(activeWindBlock);
        }
        activeWindBlock = Instantiate(windBlockPrefab, GetSpawnPosition(), Quaternion.LookRotation(transform.forward));
        Debug.Log("Altar created a moving terrain.");
    }

    private void SpawnStrongFloat()
    {
        if (strongFloatPrefab == null)
        {
            Debug.LogWarning("prefab not assigned");
            return;
        }
        if (activeStrongFloatZone != null)
        {
            Destroy(activeStrongFloatZone);
        }
        activeStrongFloatZone = Instantiate(strongFloatPrefab, GetSpawnPosition(), Quaternion.LookRotation(transform.forward));
        Debug.Log("Altar created a Strong float zone.");
    }

    private void SpawnStrongWind()
    {
        if (strongWindPrefab == null)
        {
            Debug.LogWarning("prefab not assigned");
            return;
        }
        if (activeStrongWind != null)
        {
            Destroy(activeStrongWind);
        }
        activeStrongWind = Instantiate(strongWindPrefab, GetSpawnPosition(), Quaternion.LookRotation(transform.forward));
        Debug.Log("Altar created a Strong Wind zone.");
    }

    private void SpawnBigFire()
    {
        if (bigFireZonePrefab == null)
        {
            Debug.LogWarning("Big Fire Zone prefab not assigned on Altar.");
            return;
        }

        if (activeBigFireZone != null)
        {
            Destroy(activeBigFireZone);
        }

        activeBigFireZone = Instantiate(bigFireZonePrefab, GetSpawnPosition(), Quaternion.LookRotation(transform.forward));
        Debug.Log("Altar created a Big Fire flamethrower zone.");
    }
    private void SpawnLongFire()
    {
        if (longFireZonePrefab == null)
        {
            Debug.LogWarning("Big Fire Zone prefab not assigned on Altar.");
            return;
        }

        if (activeLongFireZone != null)
        {
            Destroy(activeLongFireZone);
        }

        activeLongFireZone = Instantiate(longFireZonePrefab, GetSpawnPosition(), Quaternion.LookRotation(transform.forward));
        Debug.Log("Altar created a Big Fire flamethrower zone.");
    }

    private void SpawnBigEarthBlock()
    {
        if (bigEarthBlockPrefab == null)
        {
            Debug.LogWarning("Big Earth Block prefab not assigned on Altar.");
            return;
        }
        if (activeBigEarthBlock != null)
        {
            Destroy(activeBigEarthBlock);
        }
        activeBigEarthBlock = Instantiate(bigEarthBlockPrefab, GetSpawnPosition(), Quaternion.LookRotation(transform.forward));
        Debug.Log("Altar created a Big Earth terrain block.");
    }

    // Earth + Sticky combo: same solid-terrain block, but slows the player
    // via StickyBlock while standing on top of it.
    private void SpawnStickyEarthBlock()
    {
        if (stickyEarthBlockPrefab == null)
        {
            Debug.LogWarning("Sticky Earth Block prefab not assigned on Altar.");
            return;
        }

        if (activeStickyEarthBlock != null)
        {
            Destroy(activeStickyEarthBlock);
        }

        activeStickyEarthBlock = Instantiate(stickyEarthBlockPrefab, GetSpawnPosition(), Quaternion.LookRotation(transform.forward));
        Debug.Log("Altar created a sticky terrain block.");
    }

    // Earth + Smooth combo: functionally identical to a solo Earth block -
    // same spawn logic, just a visually smooth-textured variant prefab.
    private void SpawnSmoothEarthBlock()
    {
        if (smoothEarthBlockPrefab == null)
        {
            Debug.LogWarning("Smooth Earth Block prefab not assigned on Altar.");
            return;
        }

        if (activeSmoothEarthBlock != null)
        {
            Destroy(activeSmoothEarthBlock);
        }

        activeSmoothEarthBlock = Instantiate(smoothEarthBlockPrefab, GetSpawnPosition(), Quaternion.LookRotation(transform.forward));
        Debug.Log("Altar created a smooth terrain block.");
    }

    // Earth + Float combo: a rising platform the player can stand on and
    // ride up to a fixed height, like a stairstep.
    private void SpawnFloatingTerrain()
    {
        if (floatingTerrainPrefab == null)
        {
            Debug.LogWarning("Floating Terrain prefab not assigned on Altar.");
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

        // Bottom of the L: float zone lifts the player straight up.
        Vector3 floatSpawnPos = GetSpawnPosition();
        activeComboFloatZone = Instantiate(comboFloatZonePrefab, floatSpawnPos, Quaternion.LookRotation(transform.forward));

        // Top of the L: wind zone catches the player at the top of the lift and pushes them forward from there.
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