using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    private List<Rune> runes = new List<Rune>(); // [Encapsulated Collection]
    public IReadOnlyList<Rune> Runes => runes; // [Encapsulated Collection]

    public event Action OnInventoryChanged;

    // Adds a rune to the player's inventory and triggers the OnInventoryChanged event
    public void AddRune(Rune rune)
    {
        // [Observer Pattern]
        runes.Add(rune);
        OnInventoryChanged?.Invoke();
    }
    
    // Removes a rune from the player's inventory and triggers the OnInventoryChanged event
    public void RemoveRune(Rune rune)
    {
        // [Observer Pattern]
        runes.Remove(rune);
        OnInventoryChanged?.Invoke();
    }
}