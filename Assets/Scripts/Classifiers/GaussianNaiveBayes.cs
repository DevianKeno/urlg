/*

Program Title: Gaussian Naive Bayes [Classifier] (Algorithm)

Date written: October 4, 2024
Date revised: October 29, 2024

Programmer/s:
    Gian Paolo Buenconsejo, John Franky Nathaniel V. Batisla-Ong, Edrick L. De Villa, John Paulo A. Dela Cruz

Where the program fits in the general system design:
    Serves as the main classification model for the GNB implementation of the system.

Purpose:
    This is the main algorithm implementation for the Gaussian Naive Bayes model of the system.
    A GNB classifier is a probabilistic machine learning model used for classification tasks.
    This GNB classifier models the likelihood of features using a Gaussian distribution.
    This implementation is designed to classify data entries into two categories: "Accepted" or "Rejected,"
    based on a dataset with a specific format of headers.
    This program is a singleton.

Control:
    If enabled (which is by default), the model is trained upon
    the start of the application using the dataset gathered by the researchers. 
    The dataset is loaded into the model and is trained. On which after,
    generated feature sets can now be classified which yields a result.

Data Structures/Key Variables:
    GNBData: used to represent a data entry from a dataset for GNB.
    GNBResult: used to store the result of a GNB classification.
*/

using System.Collections.Generic;
using static System.Math;

using UnityEngine;

using RL.CellularAutomata;
using RL.Telemetry;
using RL.RD;

namespace RL.Classifiers
{
    /// <summary>
    /// Data structure representing a data entry from a dataset for GNB.
    /// </summary>
    public class GNBData
    {
        public List<ARDataEntry> AcceptedEntries = new();
        public List<ARDataEntry> RejectedEntries = new();
        public int TotalEntryCount => AcceptedEntries.Count + RejectedEntries.Count;
    }

    /// <summary>
    /// Data structure to store the result of a GNB classification.
    /// </summary>
    public struct GNBResult : IResult
    {
        public double PosteriorAccepted { get; set; }
        public double PosteriorRejected { get; set; }
        public Status Status { get; set; }
        public readonly bool IsAccepted => Status == Status.Accepted;
        public readonly bool IsRejected => Status == Status.Rejected;
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

        /// <summary>
        /// Perform a classification for given a feature set (PlayerStatCollection + RoomStatCollection).
        /// </summary>
        /// <returns>The result</returns>
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

        /// <summary>
        /// Train the current model given a testing set and a validation set.
        /// </summary>
        /// <param name="testingSet"></param>
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

        /// <summary>
        /// Calculates the cosine similarity between a PlayerStatCollection and RoomStatCollection.
        /// </summary>
        /// <returns>A value between 0-1, representing the similarity of the feature set</returns>
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

        /// <summary>
        /// Calculates the cosine similarity given a Accept-Reject data entry.
        /// </summary>
        /// <returns>A value between 0-1, representing the similarity of the feature set</returns>
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

        /// <summary>
        /// 
        /// </summary>
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