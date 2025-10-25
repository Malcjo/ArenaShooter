using UnityEngine;

public class Grenade : Entity
{
    [Header("Grenade")]
    public float fuseSeconds = 2.0f;
    public float radius = 6f;
    public float maxDamage = 80f;
    public AnimationCurve falloff = AnimationCurve.EaseInOut(0, 1, 1, 0);

    float _timer;

    protected override void Awake()
    {
        base.Awake();
        canBeDamaged = false; // shooting does nothing (or make it blow early if you want)
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= fuseSeconds)
        {
            Explode();
            // kill/grave the grenade entity
            onDeath?.Invoke();
            Destroy(gameObject);
        }
    }

    void Explode()
    {
        var pos = transform.position;
        var cols = Physics.OverlapSphere(pos, radius, ~0, QueryTriggerInteraction.Ignore);
        foreach (var c in cols)
        {
            var ent = c.GetComponentInParent<Entity>();
            if (ent == null || !ent.CanTakeDamage) continue;

            float d01 = Mathf.Clamp01(Vector3.Distance(pos, ent.transform.position) / radius);
            float dmg = maxDamage * falloff.Evaluate(1f - d01);

            Vector3 dir = (ent.transform.position - pos).normalized;
            var info = new DamageInfo(dmg, DamageType.Explosive, pos, -dir, dir * 40f, instigator: this.gameObject, source: this.gameObject);
            ent.ApplyDamage(info);
        }

        // FX: particle, sound, camera shake, etc.
    }
}
