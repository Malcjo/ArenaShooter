using UnityEngine;

public enum PartType { Barrel, Receiver, Magazine, Stock, Grip, Sight, Foregrip }

[CreateAssetMenu(menuName = "FPS/Weapon Part")]
public class WeaponPart : ScriptableObject
{
    public PartType type;

    [Header("Visual (simple cube)")]
    public Color color = Color.white;
    public Vector3 localPosition = Vector3.zero;
    public Vector3 localScale = new Vector3(0.12f, 0.12f, 0.12f);

    [Header("Stat adds")]
    public float damageAdd;
    public float fireRateAdd;        // shots/sec
    public float spreadAdd;          // degrees
    public float projectileSpeedAdd; // unused for hitscan now, future use
    public float recoilAdd;
    public int ammoAdd;

    [Header("Stat multipliers (1.0 = no change)")]
    public float damageMul = 1f;
    public float fireRateMul = 1f;
    public float spreadMul = 1f;
    public float projectileSpeedMul = 1f;
    public float recoilMul = 1f;
    public float ammoMul = 1f;
}
