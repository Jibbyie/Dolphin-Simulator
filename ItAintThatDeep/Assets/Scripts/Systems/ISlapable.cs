public interface ISlapable
{
    /// <summary>
    /// Apply slap damage. Return true if the entity died.
    /// </summary>
    bool TakeDamage(float amount);
}