using UnityEngine;

public class BasicEnemy : Entity
{
    public float knockbackScale = 1f;

    protected override void OnDamaged(in DamageInfo info)
    {
        // play hit VFX/SFX
        // if you have a Rigidbody, apply impulse:
        var rb = GetComponent<Rigidbody>();
        if (rb) rb.AddForce(info.Impulse * knockbackScale, ForceMode.Impulse);
    }

    protected override void Die(in DamageInfo finalBlow)
    {
        base.Die(finalBlow);
        // drop loot, despawn with animation, etc.
        Destroy(gameObject, 2f);
    }
}
