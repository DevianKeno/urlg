using UnityEngine;
using RL.Enemies;

namespace RL.Levels
{
    public class RoomStatsManager : MonoBehaviour
    {
        [SerializeField] GameObject enemiesContainer;

        void Start()
        {
            foreach (Transform c in enemiesContainer.transform)
            {
                if (c.TryGetComponent(out Enemy enemy))
                {
                    if (enemy is FireWeak)
                    {
                        // Game.Telemetry.RoomStats["enemyCountFire"].Increment();
                    }
                    else if (enemy is BeamWeak)
                    {
                        // Game.Telemetry.RoomStats["enemyCountBeam"].Increment();
                    }
                    else if (enemy is WaveWeak)
                    {
                        Game.Telemetry.RoomStats["enemyCountWave"].Increment();
                    }
                }
            }
        }
    }
}