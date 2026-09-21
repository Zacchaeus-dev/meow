using UnityEngine;

// The contract: anything that spawns rune effects must be able to
// SpawnEffect(type, position, rotation) and report success/failure.
// Altar depends on THIS, not on RuneEffectFactory directly - so a
// different implementation could be swapped in without changing Altar.
public interface IRuneEffectFactory
{
    bool SpawnEffect(RuneType type, Vector3 position, Quaternion rotation);
}