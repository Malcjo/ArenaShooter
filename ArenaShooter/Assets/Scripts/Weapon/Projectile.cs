using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Physics")]
    public float mass = 0.02f;
    public float drag = 0f;
    public float gravityScale = 1f;
    public float lifeTime = 5f;

    [Header("Damage")]
    public float damage = 10f;
    public LayerMask hitMask = ~0;
    public string[] ignoreTags = { "Player"};

    Rigidbody _rb;
    bool _initialized;
    float _age;

    public void Init(Vector3 velocity, float dmg, float grav, float life, LayerMask mask, string[] ignore = null)
    {
        _rb = GetComponent<Rigidbody>();
        _rb.mass = mass;
        _rb.drag = drag;
        _rb.useGravity = false;                 // we'll apply scaled gravity manually
        _rb.velocity = velocity;

        damage = dmg;
        gravityScale = grav / -9.81f;           // convert your gravity to a multiplier
        lifeTime = Mathf.Max(0.05f, life);
        hitMask = mask;
        if (ignore != null) ignoreTags = ignore;

        _initialized = true;
    }

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb) _rb.useGravity = false;
    }

    void FixedUpdate()
    {
        if (!_initialized) return;

        _age += Time.fixedDeltaTime;
        if (_age >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        // apply scaled gravity
        _rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
    }

    public float GetLife()
    {
        return lifeTime;
    }
    void OnCollisionEnter(Collision col)
    {
        if (ShouldIgnore(col.collider)) return;

        // Damageable interface optional
        col.collider.GetComponent<IDamageable>()?.Damage(damage);

        // Debug line
        Debug.DrawRay(col.contacts[0].point, col.contacts[0].normal * 0.3f, Color.red, 1f);

        Destroy(gameObject);
    }

    bool ShouldIgnore(Collider c)
    {
        if (!c) return false;
        if (ignoreTags == null || ignoreTags.Length == 0) return false;

        string otherTag = c.gameObject.tag; // always returns some string (e.g. "Untagged")
        for (int i = 0; i < ignoreTags.Length; i++)
        {
            var t = ignoreTags[i];
            if (!string.IsNullOrEmpty(t) && otherTag == t)
                return true;
        }
        return false;
    }
}
