using URLG.Systems;

namespace URLG.Enemies
{
    public enum ArmadilloStates
    {
        Idle, Move, Windup, Ball, Lunge
    }
    public class ArmadilloStateMachine : StateMachine<ArmadilloStates>
    {
    }
}