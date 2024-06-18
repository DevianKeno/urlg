using RL.Systems;

namespace RL.Enemies
{
    public enum SalamanderStates
    {
        Idle, Move, Hop, Charge, Jump, Land
    }
    public class SalamanderStateMachine : StateMachine<SalamanderStates>
    {
    }
}