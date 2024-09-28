using System;
using System.IO;
using System.Linq;
using static System.Math;

using URLG.CellularAutomata;
using UnityEngine;

namespace URLG.GNB
{
    public static class Math
    {
        public static double Mean(double[] values)
        {
            return values.Average();
        }

        public static double Variance(double[] values)
        {
            double mean = Mean(values);
            double sumSqrd = values.Sum(num => Pow(num - mean, 2));
            return sumSqrd / (values.Length - 1);
        }

        public static double StandardDeviation(double[] values)
        {
            return Sqrt(Variance(values));
        }
    }

    public struct CalculatePosteriorParameters
    {
        public float Evidence { get; set; }
    }

    public struct Posterior
    {

    }

    public class Evaluator
    {
        /// <summary>
        /// Returns a value between -1 and 1, representing the similarity of two range of values.
        /// </summary>
        /// <returns></returns>
        public static double CosineSimilarity(double[] a, double[] b)
        {
            if (a == null || b == null) throw new ArgumentException("Input arrays cannot be null.");
            if (a.Length != b.Length) throw new ArgumentException("Input arrays must have the same length.");

            double dotProduct = 0;
            double magnitudeA = 0;
            double magnitudeB = 0;

            for (int i = 0; i < a.Length; i++)
            {
                dotProduct += a[i] * b[i];
                magnitudeA += Pow(a[i], 2);
                magnitudeB += Pow(b[i], 2);
            }

            if (magnitudeA == 0 || magnitudeB == 0) throw new ArgumentException("Input vectors must not be zero-vectors.");

            return dotProduct / (Sqrt(magnitudeA) * Sqrt(magnitudeB));
        }

        // public static double WeaponEfficiency<T>() where T : Enum
        // {

        // }
    }
    
    public class GaussianNaiveBayes
    {
        public static (int, int, int) RandomIntTotaled(int total)
        {
            int cut1 = UnityEngine.Random.Range(0, total + 1);
            int cut2 = UnityEngine.Random.Range(0, total + 1);
            int first = Mathf.Min(cut1, cut2);
            int second = Mathf.Max(cut1, cut2);

            return (first, second - first, total - second);
        }

        public static FeatureParameters GenerateFeatureOptionsRandom(FeatureParametersSettings settings)
        {
            var features = new FeatureParameters();

            (int, int, int) enemyCount = RandomIntTotaled(settings.MaxEnemyCount);            
            features.EnemyCountFire = enemyCount.Item1;
            features.EnemyCountBeam = enemyCount.Item2;
            features.EnemyCountWave = enemyCount.Item3;

            (int, int, int) obsCount = RandomIntTotaled(settings.MaxObstacleCount);   
            features.ObstacleCountFire = obsCount.Item1;
            features.ObstacleCountBeam = obsCount.Item2;
            features.ObstacleCountWave = obsCount.Item3;

            return features;
        }

        void CalculateDependencies()
        {
            
        }

        static void DistributionEquation()
        {
            // float delta = 0f;
            // return 1 / Math.Sqrt(2 * Math.PI * Math.Pow(delta, 2));
        }

        public void ReadCSVFromFile(string filepath)
        {
            string contents = File.ReadAllText(filepath);
        }

        // Posterior CalculatePosterior(CalculatePosteriorParameters parameters)
        // {

        // }
    }
}