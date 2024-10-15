using RL.Systems;

namespace RL.Enemies
{
    public enum ArmadilloStates
    {
        Idle, Move, Windup, Ball, Lunge
    }
    public class ArmadilloStateMachine : StateMachine<ArmadilloStates>
    {
    }
}