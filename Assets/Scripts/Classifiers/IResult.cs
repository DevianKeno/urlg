namespace RL.Classifiers
{
    /// <summary>
    /// Interface to represent structs with results.
    /// </summary>
    public interface IResult
    {
        public Status Status { get; }
    }
}