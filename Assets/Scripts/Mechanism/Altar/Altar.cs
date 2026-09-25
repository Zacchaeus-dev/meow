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

    // [Factory Method Pattern] Solo effects (Fire/Wind/Float/Earth) are built by
    // RuneEffectFactory; combo effects are built by ComboEffectFactory. Altar
    // knows the construction details of neither - just delegates through
    // their interfaces.
    [Header("Effect Factories")]
    [SerializeField] private RuneEffectFactory effectFactory;
    [SerializeField] private ComboEffectFactory comboEffectFactory;
    [SerializeField] private float spawnDistance = 2f;
    [SerializeField] private float spawnHeight = 1f;

    private IRuneEffectFactory EffectFactory => effectFactory;
    private IComboEffectFactory ComboFactory => comboEffectFactory;

    [System.Serializable]
    public struct RuneCombo : System.IEquatable<RuneCombo>
    {
        public RuneType runeA;
        public RuneType runeB;

        public RuneCombo(RuneType a, RuneType b)
        {
            runeA = a;
            runeB = b;
        }

        // Treats WIND+FIRE the same as FIRE+WIND.
        public bool Equals(RuneCombo other)
        {
            return (runeA == other.runeA && runeB == other.runeB) ||
                   (runeA == other.runeB && runeB == other.runeA);
        }

        public override bool Equals(object obj) => obj is RuneCombo other && Equals(other);

        public override int GetHashCode()
        {
            int a = (int)runeA;
            int b = (int)runeB;
            int min = Mathf.Min(a, b);
            int max = Mathf.Max(a, b);
            return min * 31 + max;
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

    // Common spawn point for every solo effect and most combos: a fixed
    // distance and height in front of the altar, along its own facing direction.
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

    private bool IsValidCombo(RuneCombo runeCombo) => validCombos.Contains(runeCombo);

    // [Factory Method Pattern] Delegates to ComboFactory instead of knowing
    // construction details for each combo. ComboFactory needs both the
    // front spawn point (most combos) and the altar's own center
    // (Fire + Water's steam spawns centered on the altar).
    private void TriggerComboEffect(RuneCombo runeCombo)
    {
        if (ComboFactory == null)
        {
            Debug.LogWarning("ComboEffectFactory not assigned on Altar.");
            return;
        }

        bool spawned = ComboFactory.SpawnCombo(runeCombo, GetSpawnPosition(), transform.position, Quaternion.LookRotation(transform.forward));

        if (!spawned)
        {
            Debug.Log($"Failed to spawn combo effect for {runeCombo.runeA} + {runeCombo.runeB}.");
        }
    }

    private void ReturnRuneToInventory(Rune rune)
    {
        currentInventory?.AddRune(rune);
        Debug.Log($"{rune.RuneType} rune returned to inventory.");
    }
}