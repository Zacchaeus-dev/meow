using UnityEngine;

public class PlayerAbilityCaster : MonoBehaviour
{
    [Header("Projectile Prefabs")]
    [SerializeField] private GameObject windProjectilePrefab;
    [SerializeField] private GameObject fireProjectilePrefab;


    [Header("Ability Strength")]
    [SerializeField] private float windStrength = 1f;

    public GameObject WindProjectilePrefab => windProjectilePrefab;
    public GameObject FireProjectilePrefab => fireProjectilePrefab;
    public float WindStrength => windStrength;
}
