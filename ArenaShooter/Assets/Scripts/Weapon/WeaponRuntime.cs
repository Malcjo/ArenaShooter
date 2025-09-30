using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WeaponStats
{
    public float damage;
    public float fireRate;        // shots/sec
    public float spread;          // degrees
    public float recoil;
    public int ammo;

    public void ApplyMul(float dmg, float rate, float spr, float rec, float amm)
    {
        damage *= dmg; fireRate *= rate; spread *= spr; recoil *= rec; ammo = Mathf.RoundToInt(ammo * amm);
    }
}

public class WeaponRuntime : MonoBehaviour
{
    [Header("Refs")]
    public Camera aimCamera;          // drag your player camera here
    public LayerMask hitMask = ~0;

    [Header("Sockets on the gun")]
    public Transform muzzle;
    public Transform barrelSocket, receiverSocket, magazineSocket, stockSocket, gripSocket, sightSocket;

    [Header("Base Stats")]
    public WeaponStats baseStats = new WeaponStats { damage = 10, fireRate = 5, spread = 1.5f, recoil = 0.5f, ammo = 999 };

    [Header("Equipped Parts")]
    public WeaponPart barrel, receiver, magazine, stock, grip, sight;

    [SerializeField] WeaponStats _stats;
    public WeaponStats Stats => _stats; // read-only outside

    float _cooldown;
    readonly Dictionary<PartType, GameObject> _visuals = new Dictionary<PartType, GameObject>();



    void Start() => Rebuild();

    public void Rebuild()
    {
        // Destroy old visuals
        foreach (var kv in _visuals) if (kv.Value) Destroy(kv.Value);
        _visuals.Clear();

        // Recompute stats
        _stats = baseStats;
        ApplyPart(receiver);
        ApplyPart(barrel);
        ApplyPart(magazine);
        ApplyPart(stock);
        ApplyPart(grip);
        ApplyPart(sight);

        // Build visuals (simple cubes)
        BuildVisual(receiverSocket, receiver);
        BuildVisual(barrelSocket, barrel);
        BuildVisual(magazineSocket, magazine);
        BuildVisual(stockSocket, stock);
        BuildVisual(gripSocket, grip);
        BuildVisual(sightSocket, sight);
    }



    void ApplyPart(WeaponPart p)
    {
        if (!p) return;
        _stats.damage += p.damageAdd;
        _stats.fireRate += p.fireRateAdd;
        _stats.spread += p.spreadAdd;
        _stats.recoil += p.recoilAdd;
        _stats.ammo += p.ammoAdd;
        _stats.ApplyMul(p.damageMul, p.fireRateMul, p.spreadMul, p.recoilMul, p.ammoMul);
    }


    void BuildVisual(Transform socket, WeaponPart p)
    {
        if (!socket || !p) return;
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = p.name;
        cube.transform.SetParent(socket, false);
        cube.transform.localPosition = p.localPosition;
        cube.transform.localRotation = Quaternion.identity;
        cube.transform.localScale = p.localScale;
        var r = cube.GetComponent<Renderer>();
        r.sharedMaterial = new Material(Shader.Find("Standard"));
        r.sharedMaterial.color = p.color;
        _visuals[p.type] = cube;
    }

    public void TryFire(bool isHeld = true)
    {
        
        if (_cooldown > 0f) return;
        FireHitscan();
        _cooldown = Stats.fireRate > 0 ? 1f / Stats.fireRate : 0f;
        // optional: recoil/view punch here
    }

    void Update()
    {
        if (_cooldown > 0f) _cooldown -= Time.deltaTime;
    }

    void FireHitscan()
    {
        var origin = aimCamera ? aimCamera.transform.position : muzzle.position;
        var forward = aimCamera ? aimCamera.transform.forward : muzzle.forward;

        // apply spread
        if (Stats.spread > 0f)
        {
            var axis = Random.onUnitSphere;
            forward = Quaternion.AngleAxis(Random.Range(-Stats.spread, Stats.spread), axis) * forward;
        }

        if (Physics.Raycast(origin, forward, out var hit, 200f, hitMask, QueryTriggerInteraction.Ignore))
        {
            // simple: print hit, try to damage
            // IDamageable d = hit.collider.GetComponent<IDamageable>(); d?.Damage(Stats.damage);
            Debug.DrawLine(origin, hit.point, Color.red, 0.1f);
        }
        else
        {
            Debug.DrawRay(origin, forward * 30f, Color.yellow, 0.1f);
        }
    }

    public void ReplacePart(WeaponPart part)
    {
        if (!part) return;
        switch (part.type)
        {
            case PartType.Barrel: barrel = part; break;
            case PartType.Receiver: receiver = part; break;
            case PartType.Magazine: magazine = part; break;
            case PartType.Stock: stock = part; break;
            case PartType.Grip: grip = part; break;
            case PartType.Sight: sight = part; break;
        }
        Rebuild();
    }
}
