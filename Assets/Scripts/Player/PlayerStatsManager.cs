using UnityEngine;

namespace URLG.Player
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