/*

Program Title: Evaluate
Date written: September 28, 2024
Date revised: October 16, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    A static class that contains helper methods for evaluating and performing mathematical calculations
    on the various custom-implemented data structures created for the system.

Data Structures/Key Variables:
    N/A
*/

using System;

using RL.Telemetry;

namespace RL
{
    /// <summary>
    /// Evaluation function helper methods.
    /// </summary>
    public static class Evaluate
    {
        // public static double WeaponPreference(double )
        // {
        //     Math.CosineSimilarity();
        // }
        
        // public static double SkillPreference(double )
        // {
        //     Math.CosineSimilarity();
        // }

        /// <summary>
        /// Evaluation functions for Player preferences.
        /// </summary>
        public static class Player
        {
            /// <summary>
            /// A value between 0 and 1, representing the efficiency of a player given a weapon type.
            /// </summary>
            /// <returns></returns>
            public static double WeaponEfficiency(int hit, int use)
            {
                try
                {
                    return (double) hit / (double) UnityEngine.Mathf.Clamp(use, 1, use);
                }
                catch (Exception ex)
                {
                    return 0d;
                }
            }
            
            /// <summary>
            /// A value between 0 and 1, representing the efficiency of a player given a weapon type.
            /// </summary>
            /// <returns></returns>
            public static double WeaponPreference(double weaponEfficiency, StatKey hitCountKey, PlayerStatCollection stats)
            {
                try
                {
                    if (stats == null) throw new NullReferenceException("Stats cannot be null.");

                    return weaponEfficiency * ((double) stats[hitCountKey].Value / (double) stats.TotalHitCount);
                }
                catch (Exception ex)
                {
                    return 0d;
                }
            }

            /// <summary>
            /// A value between 0 and 1, representing the efficiency of a player given a weapon type.
            /// </summary>
            /// <returns></returns>
            public static double WeaponPreference(StatKey hitCountKey, StatKey useCountKey, PlayerStatCollection stats)
            {
                try
                {
                    if (stats == null) throw new NullReferenceException("Stats cannot be null.");

                    return WeaponEfficiency(stats[hitCountKey].Value, stats[useCountKey].Value) * ((double) stats[hitCountKey].Value / (double) stats.TotalHitCount);
                }
                catch (Exception ex)
                {
                    return 0d;
                }
            }
            
            /// <summary>
            /// A value between 0 and 1, representing the efficiency of a player given a weapon type.
            /// </summary>
            /// <returns></returns>
            public static float DodgeRating(int hitsTaken, int enemyAttackCount)
            {
                try
                {
                    return (float) hitsTaken / enemyAttackCount;
                }
                catch (Exception ex)
                {
                    return 0f;
                }
            }
        }

        /// <summary>
        /// Evaluation functions for Room preferences.
        /// </summary>
        public static class Room
        {
            /// <summary>
            /// A value between 0 and 1, representing the preference of a room for a particular Enemy Type.
            /// </summary>
            /// <returns></returns>
            public static double WeaponPreference(StatKey enemyCountKey, StatKey obstacleCountKey, RoomStatCollection stats)
            {
                try
                {
                    if (stats == null) throw new NullReferenceException("Stats cannot be null.");

                    double numerator = (double) stats[enemyCountKey].Value +  (double) stats[obstacleCountKey].Value;
                    return numerator / ((double) stats.TotalEnemyCount + (double) stats.TotalObstacleCount);
                }
                catch (Exception ex)
                {
                    return 0d;
                }
            }
                        
            /// <summary>
            /// A value between 0 and 1, representing the efficiency of a player given a weapon type.
            /// </summary>
            /// <returns></returns>
            public static float Difficulty(RoomStatCollection stats, int maxFeatureCount)
            {
                try
                {
                    if (stats == null) throw new NullReferenceException("Stats cannot be null.");

                    return (float) stats.TotalEnemyCount / maxFeatureCount;
                }
                catch (Exception ex)
                {
                    return 0f;
                }
            }
        }
    }
}