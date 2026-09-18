using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    private List<Rune> runes = new List<Rune>();
    public IReadOnlyList<Rune> Runes => runes;

    public event Action OnInventoryChanged;

    // Adds a rune to the player's inventory and triggers the OnInventoryChanged event
    public void AddRune(Rune rune)
    {
        runes.Add(rune);
        OnInventoryChanged?.Invoke();
    }
    
    // Removes a rune from the player's inventory and triggers the OnInventoryChanged event
    public void RemoveRune(Rune rune)
    {
        runes.Remove(rune);
        OnInventoryChanged?.Invoke();
    }
}