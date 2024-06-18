using UnityEngine;

namespace RL.Player
{
    public class PlayerStatsManager : MonoBehaviour
    {
        public PlayerStats Stats;

        void Start()
        {
            Stats = new();
        }
    }
}