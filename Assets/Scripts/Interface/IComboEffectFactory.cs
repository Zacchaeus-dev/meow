using UnityEngine;

// The contract: given a combo and where the altar is, spawn the right
// effect and report success/failure. Altar depends on this, not the
// concrete ComboEffectFactory class - mirrors IRuneEffectFactory exactly.
public interface IComboEffectFactory
{
    bool SpawnCombo(Altar.RuneCombo combo, Vector3 frontPosition, Vector3 centerPosition, Quaternion rotation);
}