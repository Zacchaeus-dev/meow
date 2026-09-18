using UnityEngine;

[System.Serializable]
public class Rune
{
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
    
    // Triggers the rune's solo effect
    public void RuneEffect(Transform caster)
    {
        switch (runeType)
        {
            case RuneType.FIRE:
                Debug.Log("Fire rune solo effect triggered.");
                break;
            case RuneType.WATER:
                Debug.Log("Water rune solo effect triggered.");
                break;
            case RuneType.EARTH:
                Debug.Log("Earth rune solo effect triggered.");
                break;
            case RuneType.WIND:
                Debug.Log("Wind rune solo effect triggered.");
                break;
            case RuneType.FLOAT:
                Debug.Log("Float rune solo effect triggered.");
                //FloatCaster(caster);
                break;
            case RuneType.STICKY:
            case RuneType.SMOOTH:
                Debug.Log($"{runeType} rune has no solo effect - combo only.");
                break;
        }
    }
    //private void FloatCaster(Transform caster)
    //{
    //    Rigidbody rb = caster.GetComponent<Rigidbody>();
    //    if (rb != null)
    //    {
    //        rb.AddForce(Vector3.up * 6f, ForceMode.Impulse);
    //        Debug.Log("Float rune lifted the player.");
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Float rune couldn't find a Rigidbody on the caster.");
    //    }
    //}
}