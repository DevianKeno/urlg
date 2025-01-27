namespace RL.Entities
{
    /// <summary>
    /// Represents objects that can be burned.
    /// </summary>
    public interface IBurnable
    {
        public virtual void Burn(float duration){}
    }
}
    