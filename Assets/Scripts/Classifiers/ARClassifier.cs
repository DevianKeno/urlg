using RL.CellularAutomata;
using RL.Telemetry;

namespace RL.Classifiers
{
    public enum Result { Accepted, Rejected }

    public struct AREntry
    {
        public int x { get; set; }
        public int y { get; set; }
    }
    
    public enum Status {
        Rejected, Accepted, None, 
    }

    public enum ConfusionMatrixStatus { 
        TruePositive, TrueNegative, FalsePositive, FalseNegative,
    }

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

            /// Normalize the values between 0 and 1 (you can adjust normalization logic as per the actual data ranges)
            double playerFireNorm = Normalize(playerFirePref);
            double playerBeamNorm = Normalize(playerBeamPref);
            double playerWaveNorm = Normalize(playerWavePref);
            
            double roomFireNorm = Normalize(roomFirePref);
            double roomBeamNorm = Normalize(roomBeamPref);
            double roomWaveNorm = Normalize(roomWavePref);

            double fireDeviation = System.Math.Abs(playerFireNorm - roomFireNorm);
            double beamDeviation = System.Math.Abs(playerBeamNorm - roomBeamNorm);
            double waveDeviation = System.Math.Abs(playerWaveNorm - roomWaveNorm);

            if (fireDeviation > acceptanceThreshold
             || beamDeviation > acceptanceThreshold
             || waveDeviation > acceptanceThreshold)
            {
                result.Status = Status.Rejected;
            }
            else
            {
                result.Status = Status.Accepted;
            }

            return result;
        }

        /// <summary>
        /// Normalization function that maps values to a [0, 1] scale based on some known max range
        /// </summary>
        public static double Normalize(double value, double minValue = 0, double maxValue = 1)
        {
            return (value - minValue) / (maxValue - minValue);
        }
    }
}