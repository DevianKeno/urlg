namespace RL.Entities
{
    /// <summary>
    /// Represents objects that can be damaged.
    /// </summary>
    public interface IDamageable
    {
        public void TakeDamage(float damage);
    }
}
    