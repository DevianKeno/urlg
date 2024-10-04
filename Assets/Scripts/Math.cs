using System;
using System.Linq;
using static System.Math;

namespace RL
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

        /// <summary>
        /// Normalization function that maps values to a [0, 1] scale based on some known max range
        /// </summary>
        public static double Normalize(double value, double minValue = 0, double maxValue = 1)
        {
            return (value - minValue) / (maxValue - minValue);
        }

        public static void NormalizeMaxed(ref double v1, ref double v2, ref double v3)
        {
            float maxVal = UnityEngine.Mathf.Max((float) v1, (float) v2, (float) v3);

            if (maxVal == 0) 
            {
                v1 = v2 = v3 = 0;
                return;
            } 
            v1 /= maxVal;
            v2 /= maxVal;
            v3 /= maxVal;
        }
    }
}