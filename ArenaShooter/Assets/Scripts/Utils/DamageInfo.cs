using UnityEngine;

public enum DamageType { Bullet, Explosive, Fire, Melee, Energy }

public struct DamageInfo
{
    public float Amount;
    public DamageType Type;
    public Vector3 HitPoint;
    public Vector3 HitNormal;
    public Vector3 Impulse;   // for knockback/RB forces
    public GameObject Instigator; // who dealt damage
    public GameObject Source;     // projectile/weapon ref

    public DamageInfo(float amount, DamageType type, Vector3 point, Vector3 normal,
                      Vector3 impulse, GameObject instigator, GameObject source)
    {
        Amount = amount; Type = type; HitPoint = point; HitNormal = normal;
        Impulse = impulse; Instigator = instigator; Source = source;
    }
}
