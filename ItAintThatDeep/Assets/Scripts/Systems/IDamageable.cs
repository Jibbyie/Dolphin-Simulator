public interface IDamageable
{
    /// <summary>
    /// Apply the given amount of damage. Return true if the entity died as a result.
    /// </summary>
    bool TakeDamage(float amount);
}
