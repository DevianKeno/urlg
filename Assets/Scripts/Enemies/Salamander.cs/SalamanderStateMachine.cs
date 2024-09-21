using URLG.Systems;

namespace URLG.Enemies
{
    public enum SalamanderStates
    {
        Idle, Move, Hop, Charge, Jump, Land
    }
    public class SalamanderStateMachine : StateMachine<SalamanderStates>
    {
    }
}