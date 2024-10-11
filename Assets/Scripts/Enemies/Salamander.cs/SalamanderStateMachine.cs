using RL.Systems;

namespace RL.Entities
{
    public enum SalamanderStates
    {
        Idle, Move, Hop, Charge, Jump, Land
    }
    public class SalamanderStateMachine : StateMachine<SalamanderStates>
    {
    }
}