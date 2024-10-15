using RL.Systems;

namespace RL.Enemies
{
    public enum LichLightStates
    {
        Idle, Move, Barrier, Tank
    }
    public class LichLightStateMachine : StateMachine<LichLightStates>
    {
    }
}