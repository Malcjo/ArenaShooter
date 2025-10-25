using UnityEngine;
using UnityEngine.Events;

public abstract class Entity : MonoBehaviour, IDamageable , IInteractable
{
    [Header("Entity Flags")]
    public bool canBeDamaged = true;
    public bool canInteract = false;

    [Header("Health")]
    public float maxHP = 100f;
    [SerializeField] private float _hp = 100f;

    [Header("FX Hooks (optional)")]
    public UnityEvent onDamaged;
    public UnityEvent onDeath;
    public UnityEvent onInteracted;

    public float HP => _hp;
    public bool IsDead => _hp <= 0f;

    protected virtual void Awake()
    {
        _hp = Mathf.Clamp(maxHP, 1f, Mathf.Max(1f, maxHP));
    }

    // ---------- IDamageable ----------
    public bool CanTakeDamage => canBeDamaged && !IsDead;

    public virtual void ApplyDamage(DamageInfo info)
    {
        if (!CanTakeDamage) return;
        _hp -= info.Amount;
        onDamaged?.Invoke();
        OnDamaged(info);
        if (_hp <= 0f) { _hp = 0f; Die(info); }
    }

    protected virtual void OnDamaged(in DamageInfo info) { /* override for hit FX */ }

    protected virtual void Die(in DamageInfo finalBlow)
    {
        onDeath?.Invoke();
        // default: disable collisions & visuals
        var col = GetComponent<Collider>(); if (col) col.enabled = false;
        var rend = GetComponentInChildren<Renderer>(); if (rend) rend.enabled = false;
        // override to explode / drop loot / etc.
    }

    // ---------- IInteractable ----------
    public virtual bool CanInteract(PlayerContext ctx) => canInteract && !IsDead;
    public virtual void Interact(PlayerContext ctx)
    {
        if (!CanInteract(ctx)) return;
        onInteracted?.Invoke();
        // override for custom logic
    }

    public virtual string Prompt => canInteract ? "Interact" : "";
}
