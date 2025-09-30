using UnityEngine;

[RequireComponent(typeof(Collider))]
public class UpgradeZone : MonoBehaviour
{
    public WeaponPart[] options;

    [Tooltip("If true, destroy zone after giving an upgrade")]
    public bool oneTime = true;

    void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // Find player's weapon
        var weapon = other.GetComponentInParent<WeaponRuntime>();
        if (!weapon)
        {
            var ctx = other.GetComponentInParent<PlayerContext>();
            if (ctx) weapon = ctx.GetComponentInChildren<WeaponRuntime>();
        }
        if (!weapon || options == null || options.Length == 0) return;

        var part = options[Random.Range(0, options.Length)];
        weapon.ReplacePart(part);
        Debug.Log($"[UpgradeZone] Granted {part.type}: {part.name}");

        if (oneTime) Destroy(gameObject);
    }



    Vector3 GetBoundsSize()
    {
        var col = GetComponent<Collider>();
        if (col is BoxCollider b) return Vector3.Scale(b.size, transform.lossyScale);
        if (col is SphereCollider s) return Vector3.one * s.radius * 2f;
        return Vector3.one;
    }
}
