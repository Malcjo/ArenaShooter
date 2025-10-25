public interface IDamageable
{
    bool CanTakeDamage { get; }
    void ApplyDamage(DamageInfo info);

}
