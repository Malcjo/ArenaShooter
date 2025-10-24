using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum FireMode { Hitscan, Projectile }

[System.Serializable]
public struct WeaponStats
{
    public float damage;
    public float fireRate;        // shots/sec
    public float spread;          // degrees
    public float recoil;
    public int ammo;

    // Projectile params (used only when FireMode.Projectile)
    public float projectileSpeed;     // m/s
    public float projectileGravity;   // m/s^2 (negative to fall)
    public float projectileLifetime;  // seconds
    public int pellets;               // 1 = single, >1 = shotgun style

    public void ApplyMul(float dmg, float rate, float spr, float rec, float amm)
    {
        damage *= dmg;
        fireRate *= rate;
        spread *= spr;
        recoil *= rec;
        ammo = Mathf.RoundToInt(ammo * amm);
    }
}

public class WeaponRuntime : MonoBehaviour
{
    [Header("Refs")]
    public Camera aimCamera;                // drag your player camera here
    public LayerMask hitMask = ~0;

    [Header("Sockets on the gun")]
    public Transform muzzle;
    public Transform barrelSocket, receiverSocket, magazineSocket, stockSocket, gripSocket, sightSocket, foregripSocket;

    [Header("Firing Mode")]
    public FireMode fireMode = FireMode.Projectile;

    [Header("Projectile (when FireMode = Projectile)")]
    public GameObject projectilePrefab;

    [Header("Base Stats")]
    public WeaponStats baseStats = new WeaponStats
    {
        damage = 10,
        fireRate = 5,
        spread = 1.5f,
        recoil = 0.5f,
        ammo = 999,
        projectileSpeed = 60f,
        projectileGravity = -9.81f,
        projectileLifetime = 5f,
        pellets = 1
    };

    [Header("Equipped Parts")]
    public WeaponPart barrel, receiver, magazine, stock, grip, sight, foregrip;

    [SerializeField] WeaponStats _stats;
    public WeaponStats Stats => _stats; // read-only outside

    float _cooldown;
    readonly Dictionary<PartType, GameObject> _visuals = new Dictionary<PartType, GameObject>();

    void Start() => Rebuild();

    // --- helper: destroy correctly in edit or play ---

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying) return;
        // Defer Rebuild to after validation so we can safely do Undo.DestroyObjectImmediate
        EditorApplication.delayCall += () =>
        {
            if (this) Rebuild();
        };
    }
#endif
    static void SafeDestroy(GameObject go)
    {
        if (!go) return;
#if UNITY_EDITOR
        if (!Application.isPlaying) Undo.DestroyObjectImmediate(go);

        else Debug.Log("clean " + go); Destroy(go);

#else
        Destroy(go);
#endif
    }

    public void Rebuild()
    {
        // Destroy old visuals
        foreach (var kv in _visuals) SafeDestroy(kv.Value);
        _visuals.Clear();
        foreach (var kv in _visuals)Debug.Log("clean " + kv);

        // Recompute stats
        _stats = baseStats;
        ApplyPart(receiver);
        ApplyPart(barrel);
        ApplyPart(magazine);
        ApplyPart(stock);
        ApplyPart(grip);
        ApplyPart(sight);
        ApplyPart(foregrip);

        // Build visuals (simple cubes)
        BuildVisual(receiverSocket, receiver);
        BuildVisual(barrelSocket, barrel);
        BuildVisual(magazineSocket, magazine);
        BuildVisual(stockSocket, stock);
        BuildVisual(gripSocket, grip);
        BuildVisual(sightSocket, sight);
        BuildVisual(foregripSocket, foregrip);

        foreach (var kv in _visuals) Debug.Log("killed " + kv); ;
        
    }

    void ApplyPart(WeaponPart p)
    {
        if (!p) return;
        _stats.damage += p.damageAdd;
        _stats.fireRate += p.fireRateAdd;
        _stats.spread += p.spreadAdd;
        _stats.recoil += p.recoilAdd;
        _stats.ammo += p.ammoAdd;

        // If your WeaponPart has projectile fields, you can extend here, e.g.:
        // _stats.projectileSpeed += p.projectileSpeedAdd;
        // _stats.projectileSpeed *= p.projectileSpeedMul;

        _stats.ApplyMul(p.damageMul, p.fireRateMul, p.spreadMul, p.recoilMul, p.ammoMul);
    }

    void BuildVisual(Transform socket, WeaponPart p)
    {
        if (!socket || !p) return;
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
#if UNITY_EDITOR
        if (!Application.isPlaying) Undo.RegisterCreatedObjectUndo(cube, "Create Weapon Part Visual");
#endif
        cube.name = p.name;
        cube.transform.SetParent(socket, false);
        cube.transform.localPosition = p.localPosition;
        cube.transform.localRotation = Quaternion.identity;
        cube.transform.localScale = p.localScale;
        DestroyImmediate(cube.GetComponent<BoxCollider>());

        var r = cube.GetComponent<Renderer>();
        r.sharedMaterial = new Material(Shader.Find("Standard"));
        r.sharedMaterial.color = p.color;
        _visuals[p.type] = cube;
        
    }

    public void TryFire(bool isHeld = true)
    {
        if (_cooldown > 0f) return;
        if (Stats.ammo <= 0) return;

        switch (fireMode)
        {
            case FireMode.Projectile:
                FireProjectile();
                break;
            case FireMode.Hitscan:
                FireHitscan();
                break;
        }

        _cooldown = Stats.fireRate > 0 ? 1f / Stats.fireRate : 0f;

        // Ammo: 1 per shot; if you want pellets to consume more, change to:
        // int ammoCost = Mathf.Max(1, Stats.pellets);
        int ammoCost = 1;
        _stats.ammo = Mathf.Max(0, _stats.ammo - ammoCost);

        // TODO: apply recoil/kick here if desired
    }

    void Update()
    {
        if (_cooldown > 0f) _cooldown -= Time.deltaTime;
    }

    // ----------------------
    // H I T S C A N  (with pellets)
    // ----------------------
    void FireHitscan()
    {
        var origin = aimCamera ? aimCamera.transform.position : (muzzle ? muzzle.position : transform.position);
        var forward = aimCamera ? aimCamera.transform.forward : (muzzle ? muzzle.forward : transform.forward);

        int pelletCount = Mathf.Max(1, Stats.pellets);

        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 dir = ApplySpread(forward, Stats.spread);
            if (Physics.Raycast(origin, dir, out var hit, 200f, hitMask, QueryTriggerInteraction.Ignore))
            {
                // Try to damage (uncomment if you have IDamageable in project)
                // hit.collider.GetComponent<IDamageable>()?.Damage(Stats.damage);

                Debug.DrawLine(origin, hit.point, Color.red, 0.1f);
            }
            else
            {
                Debug.DrawRay(origin, dir * 30f, Color.yellow, 0.1f);
            }
        }
    }

    // ------------------------
    // P R O J E C T I L E (with pellets)
    // ------------------------
    void FireProjectile()
    {
        if (!projectilePrefab || !muzzle)
        {
            Debug.LogWarning("WeaponRuntime: Missing projectilePrefab or muzzle.");
            return;
        }

        var origin = muzzle.position;
        var forward = aimCamera ? aimCamera.transform.forward : muzzle.forward;
        int pelletCount = Mathf.Max(1, Stats.pellets);

        // Define which tags to ignore (Player, Weapon, etc.)
        string[] ignoreTags = new string[] { "Player", "Weapon" };

        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 dir = ApplySpread(forward, Stats.spread);
            var proj = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(dir));
            var initVel = dir * Mathf.Max(0f, Stats.projectileSpeed);
            var lifeTime = (Stats.projectileLifetime + proj.GetComponent<Projectile>().GetLife());

            proj.GetComponent<Projectile>().Init(
                velocity: initVel,
                dmg: Stats.damage,
                grav: Stats.projectileGravity,
                life: lifeTime,
                mask: hitMask,
                ignore: new[] { "Player", "Weapon" }
            );

            Debug.Log(proj.GetComponent<Projectile>().GetLife());
        }
    }


    // Utility: spread around a direction
    static Vector3 ApplySpread(Vector3 forward, float spreadDeg)
    {
        if (spreadDeg <= 0f) return forward.normalized;
        var axis = Random.onUnitSphere;
        var dir = Quaternion.AngleAxis(Random.Range(-spreadDeg, spreadDeg), axis) * forward;
        return dir.normalized;
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
            case PartType.Foregrip: foregrip = part; break;
        }
        Rebuild();
    }
}
