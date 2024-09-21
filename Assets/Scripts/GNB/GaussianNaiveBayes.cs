using System;
using System.IO;
using System.Linq;

namespace URLG.GNB
{
    public struct CalculatePosteriorParameters
    {
        public float Evidence { get; set; }
    }

    public struct Posterior
    {

    }

    public static class Helper
    {
        public static double Mean(double[] values)
        {
            return values.Average();
        }

        public static double Variance(double[] values)
        {
            double mean = Mean(values);
            double sumOfSquares = values.Sum(num => Math.Pow(num - mean, 2));
            return sumOfSquares / (values.Length - 1);
        }

        public static double StandardDeviation(double[] values)
        {
            return Math.Sqrt(Variance(values));
        }
    }

    public enum WeaponType {
        Fireball, Beam, Wave
    }

    public struct HitCount<T> where T : Enum
    {

    }

    public struct Efficiency<T>
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
            if (a == null || b == null)
            {
                throw new ArgumentException("Input arrays cannot be null.");
            }
            if (a.Length != b.Length)
            {
                throw new ArgumentException("Input arrays must have the same length.");
            }

            double dotProduct = 0;
            double magnitudeA = 0;
            double magnitudeB = 0;

            for (int i = 0; i < a.Length; i++)
            {
                dotProduct += a[i] * b[i];
                magnitudeA += Math.Pow(a[i], 2);
                magnitudeB += Math.Pow(b[i], 2);
            }

            if (magnitudeA == 0 || magnitudeB == 0)
            {
                throw new ArgumentException("Input vectors must not be zero-vectors.");
            }

            return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
        }

        // public static double WeaponEfficiency<T>() where T : Enum
        // {

        // }
    }
    
    public class GaussianNaiveBayes
    {
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