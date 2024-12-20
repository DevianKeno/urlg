using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using static System.Math;

using UnityEngine;

using RL.CellularAutomata;
using RL.Telemetry;
using RL.RD;

namespace RL.Classifiers
{
    public class GNBData
    {
        public List<ARDataEntry> AcceptedEntries = new();
        public List<ARDataEntry> RejectedEntries = new();
        public int TotalEntryCount => AcceptedEntries.Count + RejectedEntries.Count;
    }

    public struct GNBResult : IResult
    {
        public double PosteriorAccepted { get; set; }
        public double PosteriorRejected { get; set; }
        public Status Status { get; set; }
        public readonly bool IsAccepted => Status == Status.Accepted;
        public readonly bool IsRejected => Status == Status.Rejected;
    }
    
    public struct GNBTestResult
    {
        
    }

    public class GaussianNaiveBayes : MonoBehaviour
    {
        public static GaussianNaiveBayes Instance { get; private set;}

        GNBData testingSet;
        public GNBData TestingSet => testingSet;
        GNBData validationSet;
        public GNBData ValidationSet => validationSet;

        float acceptedProbability;
        float rejectedProbability;
        List<double> acceptedCosines = new();
        List<double> rejectedCosines = new();
        double acceptedMean = 0;
        double rejectedMean = 0;
        double acceptedVariance = 0;
        double rejectedVariance = 0;

        void Awake()
        {
            DontDestroyOnLoad(this);

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public GNBResult ClassifyRoom(PlayerStatCollection playerStats, RoomStatCollection roomStats)
        {
            var result = new GNBResult();
            var x = CalculateCosine(playerStats, roomStats);
            var acceptedProb = ProbabilityDistributionFunction(x, acceptedMean, acceptedVariance);
            var rejectedProb = ProbabilityDistributionFunction(x, rejectedMean, rejectedVariance);

            var posteriorAccepted = CalculatePosterior(acceptedProb, acceptedProbability);
            var posteriorRejected = CalculatePosterior(rejectedProb, rejectedProbability);

            if (posteriorAccepted > posteriorRejected)
                result.Status = Status.Accepted;
            else if (posteriorRejected > posteriorAccepted)
                result.Status = Status.Rejected;
            
            result.PosteriorAccepted = posteriorAccepted;
            result.PosteriorRejected = posteriorRejected;
            
            return result;
        }

        double CalculatePosterior(double featureProbability, double prior)
        {
            return featureProbability * prior;
        }

        public void Train(GNBData testingSet, GNBData validationSet = null)
        {
            this.testingSet = testingSet;
            this.validationSet = validationSet;

            acceptedCosines = new();
            rejectedCosines = new();
            acceptedMean = 0;
            rejectedMean = 0; 
            acceptedVariance = 0;
            rejectedVariance = 0;

            acceptedProbability = (float) testingSet.AcceptedEntries.Count / testingSet.TotalEntryCount;
            rejectedProbability = (float) testingSet.RejectedEntries.Count / testingSet.TotalEntryCount;

            foreach (var entry in testingSet.AcceptedEntries)
                acceptedCosines.Add(CalculateCosine(entry));
            foreach (var entry in testingSet.RejectedEntries)
                rejectedCosines.Add(CalculateCosine(entry));
            
            if (acceptedCosines.Count > 1) acceptedMean = Math.Mean(acceptedCosines.ToArray());
            if (rejectedCosines.Count > 1) rejectedMean = Math.Mean(rejectedCosines.ToArray());
            if (acceptedCosines.Count > 1) acceptedVariance = Math.Variance(acceptedCosines.ToArray());
            if (rejectedCosines.Count > 1) rejectedVariance = Math.Variance(rejectedCosines.ToArray());

            Debug.Log("Model trained");
        }

        public void Test(GNBData data)
        {

        }

        double CalculateCosine(PlayerStatCollection playerStats, RoomStatCollection roomStats)
        {
            var playerPrefs = new double[]{
                Evaluate.Player.WeaponPreference(StatKey.HitCountFire, StatKey.UseCountFire, playerStats),
                Evaluate.Player.WeaponPreference(StatKey.HitCountBeam, StatKey.UseCountBeam, playerStats),
                Evaluate.Player.WeaponPreference(StatKey.HitCountWave, StatKey.UseCountWave, playerStats),
            };
            var roomPrefs = new double[]{
                Evaluate.Room.WeaponPreference(StatKey.EnemyCountFire, StatKey.ObstacleCountFire, roomStats),
                Evaluate.Room.WeaponPreference(StatKey.EnemyCountBeam, StatKey.ObstacleCountBeam, roomStats),
                Evaluate.Room.WeaponPreference(StatKey.EnemyCountWave, StatKey.ObstacleCountWave, roomStats),
            };

            return Math.CosineSimilarity(playerPrefs, roomPrefs);
        }

        double CalculateCosine(ARDataEntry entry)
        {
            var playerStats = PlayerStatCollection.FromAREntry(entry);
            var roomStats = RoomStatCollection.FromAREntry(entry);
            var playerPrefs = new double[]{
                Evaluate.Player.WeaponPreference(StatKey.HitCountFire, StatKey.UseCountFire, playerStats),
                Evaluate.Player.WeaponPreference(StatKey.HitCountBeam, StatKey.UseCountBeam, playerStats),
                Evaluate.Player.WeaponPreference(StatKey.HitCountWave, StatKey.UseCountWave, playerStats),
            };
            var roomPrefs = new double[] {
                Evaluate.Room.WeaponPreference(StatKey.EnemyCountFire, StatKey.ObstacleCountFire, roomStats),
                Evaluate.Room.WeaponPreference(StatKey.EnemyCountBeam, StatKey.ObstacleCountBeam, roomStats),
                Evaluate.Room.WeaponPreference(StatKey.EnemyCountWave, StatKey.ObstacleCountWave, roomStats),
            };

            return Math.CosineSimilarity(playerPrefs, roomPrefs);
        }

        public static FeatureParameters GenerateFeatureOptionsRandom(FeatureParametersSettings settings, int seed)
        {
            UnityEngine.Random.InitState(seed);
            var features = new FeatureParameters();
            features.Seed = seed;
            (int, int, int) enemyCount = Random.Random.RandomTripleIntTotaled(settings.MaxEnemyCount);            
            features.EnemyCountFire = enemyCount.Item1;
            features.EnemyCountBeam = enemyCount.Item2;
            features.EnemyCountWave = enemyCount.Item3;

            (int, int, int) obsCount = Random.Random.RandomTripleIntTotaled(settings.MaxObstacleCount);   
            features.ObstacleCountFire = obsCount.Item1;
            features.ObstacleCountBeam = obsCount.Item2;
            features.ObstacleCountWave = obsCount.Item3;

            return features;
        }

        public static double ProbabilityDistributionFunction(double x, double mean, double variance)
        {
            if (variance == 0) variance = 1e-8; /// Small constant to prevent division by zero
            return 1 / Sqrt(2 * PI * variance) * Exp(-Pow(x - mean, 2) / (2 * variance));
        }

        public static double CalculatePosterior(Dictionary<StatKey, double> means, Dictionary<StatKey, double> variances, ARDataEntry entry)
        {
            double posterior = 1.0;
            
            foreach (var statKey in entry.Values.Keys)
            {
                double mean = means[statKey];
                double variance = variances[statKey];
                double value = entry.Values[statKey];

                posterior *= ProbabilityDistributionFunction(value, mean, variance);
            }

            return posterior;
        }
    }
}