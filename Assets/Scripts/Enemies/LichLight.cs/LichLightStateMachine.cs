using URLG.Systems;

namespace URLG.Enemies
{
    public enum LichLightStates
    {
        Idle, Move, Barrier, Tank
    }
    public class LichLightStateMachine : StateMachine<LichLightStates>
    {
    }
}