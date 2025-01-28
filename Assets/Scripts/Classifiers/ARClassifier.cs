/*

Program Title: Accept-Reject Classifier (Algorithm)

Date written: October 3, 2024
Date revised: October 29, 2024

Programmer/s:
    Gian Paolo Buenconsejo, John Franky Nathaniel V. Batisla-Ong, Edrick L. De Villa, John Paulo A. Dela Cruz

Where the program fits in the general system design:
    Serves as the main classification model for the accept-reject implementation of the system.
    
Purpose:
    This is the main algorithm implemention for the classification model
    utilizing Accept-Reject sampling algorithm for the system.
    The model evaluates similarity between feature sets (player preferences and room characteristics),
    or data entries, then classifies it to two categories: "Accepted" or "Rejected."

Control:
    The Classify() method is called whenever an AR classification is needed to be performed.
    It is static and can be called anywhere, but correct arguments must also be observed.

Data Structures/Key Variables:
    - Status (Enum): represents the classification status
    - ConfusionMatrixStatus (Enum): used for evaluating classification performance
    - ARResult: stores the result of an AR classification, including the predicted status and
        helper properties to check if the result is accepted or rejected
*/

using RL.Telemetry;

namespace RL.Classifiers
{
    public enum Status {
        Rejected, Accepted, None, 
    }

    public enum ConfusionMatrixStatus { 
        TruePositive, TrueNegative, FalsePositive, FalseNegative, Invalid
    }

    /// <summary>
    /// Data structure to store the result of an AR classification.
    /// </summary>
    public struct ARResult : IResult
    {
        public Status Status { get; set; }
        public readonly bool IsAccepted => Status == Status.Accepted;
        public readonly bool IsRejected => Status == Status.Rejected;
    }

    /// <summary>
    /// Accept-reject based classification model.
    /// </summary>
    public class ARClassifier
    {
        /// <summary>
        /// Perform a classification for given a feature set (PlayerStatCollection + RoomStatCollection).
        /// </summary>
        /// <returns>The result</returns>
        public static ARResult Classify(PlayerStatCollection playerStats, RoomStatCollection roomStats, float acceptanceThreshold = 0f, bool normalized = false)
        {
            var result = new ARResult();

            #region Player
            /// Weapon Preference
            var playerFirePref = Evaluate.Player.WeaponPreference(StatKey.HitCountFire, StatKey.UseCountFire, playerStats);
            var playerBeamPref = Evaluate.Player.WeaponPreference(StatKey.HitCountBeam, StatKey.UseCountBeam, playerStats);
            var playerWavePref = Evaluate.Player.WeaponPreference(StatKey.HitCountWave, StatKey.UseCountWave, playerStats);
            #endregion
            
            Math.NormalizeMaxed(ref playerFirePref, ref playerBeamPref, ref playerWavePref);
            
            #region Room
            var roomFirePref = Evaluate.Room.WeaponPreference(StatKey.EnemyCountFire, StatKey.ObstacleCountFire, roomStats);
            var roomBeamPref = Evaluate.Room.WeaponPreference(StatKey.EnemyCountBeam, StatKey.ObstacleCountBeam, roomStats);
            var roomWavePref = Evaluate.Room.WeaponPreference(StatKey.EnemyCountWave, StatKey.ObstacleCountWave, roomStats);
            #endregion
            
            Math.NormalizeMaxed(ref roomFirePref, ref roomBeamPref, ref roomWavePref);

            /// Normalize the values between 0 and 1
            double playerFireNorm = Math.Normalize(playerFirePref);
            double playerBeamNorm = Math.Normalize(playerBeamPref);
            double playerWaveNorm = Math.Normalize(playerWavePref);
            
            double roomFireNorm = Math.Normalize(roomFirePref);
            double roomBeamNorm = Math.Normalize(roomBeamPref);
            double roomWaveNorm = Math.Normalize(roomWavePref);

            double fireDeviation = System.Math.Abs(playerFireNorm - roomFireNorm);
            double beamDeviation = System.Math.Abs(playerBeamNorm - roomBeamNorm);
            double waveDeviation = System.Math.Abs(playerWaveNorm - roomWaveNorm);

            if (fireDeviation > acceptanceThreshold ||
                beamDeviation > acceptanceThreshold ||
                waveDeviation > acceptanceThreshold)
            {
                result.Status = Status.Rejected;
            }
            else
            {
                result.Status = Status.Accepted;
            }

            return result;
        }
    }
}