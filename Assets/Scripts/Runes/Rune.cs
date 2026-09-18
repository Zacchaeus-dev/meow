using UnityEngine;

[System.Serializable]
public class Rune
{
    // [Object Pattern]
    [SerializeField] private RuneType runeType;
    private bool isFilled;
    public RuneType RuneType => runeType;
    public bool IsFilled => isFilled;

    // Constructor
    public Rune(RuneType type)
    {
        runeType = type;
    }

    // Marks the rune as filled
    public void Fill()
    {
        isFilled = true;
    }

    // Marks the rune as empty
    public void Empty()
    {
        isFilled = false;
    }

    // Called when the rune is inserted into an altar
    public void AltarInsert()
    {
        Debug.Log($"{runeType} rune inserted into altar.");
    }

    // Factory Pattern will be ideal for 
}