using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

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
    public Transform projectileOrigin;  // player head/camera pivot
    public Transform muzzleVisual;      // viewmodel muzzle (on Viewmodel layer)
    public float maxAimDistance = 1000f;



    [Header("Viewmodel VFX")]
    public GameObject tracerPrefab;     // viewmodel-only tracer (Viewmodel layer)

    [Header("Sockets on the gun")]
    public Transform muzzle;
    public Transform barrelSocket, receiverSocket, magazineSocket, stockSocket, gripSocket, sightSocket, foregripSocket, weaponFrameSocket;

    [Header("Firing Mode")]
    public FireMode fireMode = FireMode.Projectile;

    [Header("Projectile (when FireMode = Projectile)")]
    public GameObject projectilePrefab;
    [Header("Viewmodel VFX")]
    public VisualBullet visualBulletPrefab;  // put this prefab on the Viewmodel layer

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
    public WeaponPart barrel, receiver, magazine, stock, grip, sight, foregrip, weaponFrame;



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
        ApplyPart(weaponFrame);
        

        // Build visuals (simple cubes)
        BuildVisual(receiverSocket, receiver);
        BuildVisual(barrelSocket, barrel);
        BuildVisual(magazineSocket, magazine);
        BuildVisual(stockSocket, stock);
        BuildVisual(gripSocket, grip);
        BuildVisual(sightSocket, sight);
        BuildVisual(foregripSocket, foregrip);
        BuildVisual(weaponFrameSocket, weaponFrame);

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
        var partModel = Instantiate(p.PartModel, socket);
        //var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
#if UNITY_EDITOR
        if (!Application.isPlaying) Undo.RegisterCreatedObjectUndo(partModel, "Create Weapon Part Visual");
#endif
        
        partModel.transform.localPosition = p.localPosition;
        partModel.transform.localRotation = Quaternion.identity;
        //partModel.transform.localScale = p.localScale;
        partModel.transform.SetParent(socket, false);

        _visuals[p.type] = partModel;
        
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

    public Camera worldCam, weaponCam;
    void LateUpdate()
    {
        if (!worldCam || !weaponCam) return;
        weaponCam.transform.SetPositionAndRotation(worldCam.transform.position, worldCam.transform.rotation);
        // Optional: weaponCam.fieldOfView = worldCam.fieldOfView;
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
        if (!projectilePrefab || !projectileOrigin)
        {
            Debug.LogWarning("Missing projectilePrefab or projectileOrigin.");
            return;
        }

        Vector3 aim = GetAimPoint(out var _);
        int pellets = Mathf.Max(1, Stats.pellets);

        for (int i = 0; i < pellets; i++)
        {
            // 1) REAL PHYSICS projectile from the camera/head
            Vector3 dirWorld = DirFrom(projectileOrigin, aim, Stats.spread);

            var go = Instantiate(projectilePrefab, projectileOrigin.position, Quaternion.LookRotation(dirWorld));
            var proj = go.GetComponent<Projectile>();
            if (!proj) { Debug.LogError("Projectile prefab missing Projectile component."); Destroy(go); return; }

            proj.Init(
                velocity: dirWorld * Mathf.Max(0, Stats.projectileSpeed),
                dmg: Stats.damage,
                grav: Stats.projectileGravity,
                life: Stats.projectileLifetime,
                mask: hitMask,
                ignore: new[] { "Player", "Projectile" }
            );

            // 2) VIEWMODEL-ONLY visual bullet from the muzzle
            if (visualBulletPrefab && muzzleVisual)
            {
                // build a direction from the muzzle to the same aim point, with the same spread
                Vector3 dirView = DirFrom(muzzleVisual, aim, Stats.spread);

                // life: match time-to-aim so it disappears around impact time
                float distanceToAimFromCamera = Vector3.Distance(projectileOrigin.position, aim);
                float timeToHit = Stats.projectileSpeed > 0f ? distanceToAimFromCamera / Stats.projectileSpeed : 0.1f;

                var vb = Instantiate(visualBulletPrefab, muzzleVisual.position, Quaternion.LookRotation(dirView));
                // ensure the whole instance is on the Viewmodel layer so only the WeaponCamera sees it
                SetLayerRecursively(vb.gameObject, LayerMask.NameToLayer("Viewmodel"));
                vb.Init(muzzleVisual.position, dirView, Mathf.Max(0, Stats.projectileSpeed), timeToHit);
            }
        }
    }

    // helper
    static void SetLayerRecursively(GameObject go, int layer)
    {
        if (!go) return;
        go.layer = layer;
        foreach (Transform c in go.transform) SetLayerRecursively(c.gameObject, layer);
    }


    // helper to put the whole VFX object on the Viewmodel layer




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
            case PartType.WeaponFrame: weaponFrame = part; break;
        }
        Rebuild();
    }

    Vector3 GetAimPoint(out bool hit)
    {
        hit = false;
        Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (Physics.Raycast(ray, out var h, maxAimDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            hit = true;
            return h.point;
        }
        return ray.origin + ray.direction * maxAimDistance;
    }

    Vector3 DirFrom(Transform origin, Vector3 aimPoint, float spreadDeg)
    {
        Vector3 dir = (aimPoint - origin.position).normalized;
        if (spreadDeg <= 0f) return dir;

        // random small cone around dir
        Vector3 axis = Random.onUnitSphere;
        return (Quaternion.AngleAxis(Random.Range(-spreadDeg, spreadDeg), axis) * dir).normalized;
    }

}
