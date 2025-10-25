using UnityEngine;

public class DestructibleBarrel : Entity
{
    public float explosionRadius = 5f;
    public float explosionDamage = 60f;

    protected override void Die(in DamageInfo finalBlow)
    {
        base.Die(finalBlow);

        var pos = transform.position;
        var cols = Physics.OverlapSphere(pos, explosionRadius, ~0, QueryTriggerInteraction.Ignore);
        foreach (var c in cols)
        {
            var ent = c.GetComponentInParent<Entity>();
            if (ent == null || !ent.CanTakeDamage) continue;

            float d01 = Mathf.Clamp01(Vector3.Distance(pos, ent.transform.position) / explosionRadius);
            float dmg = Mathf.Lerp(explosionDamage, 0f, d01);

            Vector3 dir = (ent.transform.position - pos).normalized;
            var info = new DamageInfo(dmg, DamageType.Explosive, pos, -dir, dir * 50f, instigator: gameObject, source: gameObject);
            ent.ApplyDamage(info);
        }

        // spawn explosion VFX/audio, then destroy root
        Destroy(gameObject, 0.05f);
    }
}
