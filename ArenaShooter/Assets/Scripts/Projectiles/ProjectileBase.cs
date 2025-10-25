using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    [Header("Damage")]
    public float damage = 10f;
    public DamageType damageType = DamageType.Bullet;
    public LayerMask hitMask = ~0;
    public string[] ignoreTags = { "Player" };
    public GameObject instigator;  // who fired
    public GameObject source;      // this projectile/weapon

    [Header("Lifetime")]
    public float lifeTime = 5f;
    float _age;

    protected virtual void Update()
    {
        _age += Time.deltaTime;
        if (_age >= lifeTime) { OnExpire(); }
    }

    protected virtual void OnExpire()
    {
        Destroy(gameObject);
    }

    protected bool ShouldIgnore(Collider c)
    {
        if (!c || ignoreTags == null) return false;
        string otherTag = c.gameObject.tag;
        for (int i = 0; i < ignoreTags.Length; i++)
        {
            var t = ignoreTags[i];
            if (!string.IsNullOrEmpty(t) && otherTag == t)
                return true;
        }
        return false;
    }

    protected void TryDamage(RaycastHit hit, Vector3 impulse)
    {
        if (ShouldIgnore(hit.collider)) return;

        float mult = 1f;
        if (hit.collider.TryGetComponent<Hitbox>(out var hb) && hb.owner)
            mult = hb.damageMultiplier;

        var ent = hit.collider.GetComponentInParent<Entity>();
        if (ent != null && ent.CanTakeDamage)
        {
            var info = new DamageInfo(
                amount: damage * mult,
                type: damageType,
                point: hit.point,
                normal: hit.normal,
                impulse: impulse,
                instigator: instigator ? instigator : this.gameObject,
                source: source ? source : this.gameObject
            );
            ent.ApplyDamage(info);
        }
    }
}
