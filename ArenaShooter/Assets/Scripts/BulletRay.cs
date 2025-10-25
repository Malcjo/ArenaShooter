using UnityEngine;

public class BulletRay : MonoBehaviour
{
    public float damage = 15f;
    public DamageType type = DamageType.Bullet;
    public float range = 150f;
    public LayerMask hitMask = ~0;

    public void Fire(Camera cam, GameObject instigator)
    {
        Ray r = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(r, out var hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            float mult = 1f;
            if (hit.collider.TryGetComponent<Hitbox>(out var hb) && hb.owner) mult = hb.damageMultiplier;

            var ent = hit.collider.GetComponentInParent<Entity>();
            if (ent != null && ent.CanTakeDamage)
            {
                Vector3 impulse = cam.transform.forward * 20f; // tweak
                var info = new DamageInfo(damage * mult, type, hit.point, hit.normal, impulse, instigator, this.gameObject);
                ent.ApplyDamage(info);
            }

            // optional: spawn decal/FX at hit.point
        }
    }
}
