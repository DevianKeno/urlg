/*

Program Title: Math
Date written: September 28, 2024
Date revised: October 11, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    A static class that contains helper methods for performing various
    mathematical calculations needed for the system.

Data Structures/Key Variables:
    N/A
*/

using System;
using System.Linq;
using static System.Math;

namespace RL
{
    /// <summary>
    /// Helper methods for math calculations.
    /// </summary>
    public static class Math
    {
        /// <summary>
        /// Calculates the mean of a <c>double</c> array.
        /// </summary>
        public static double Mean(double[] values)
        {
            return values.Average();
        }

        /// <summary>
        /// Calculates the mean of a <c>int</c> array.
        /// </summary>
        public static double Mean(int[] values)
        {
            return values.Average();
        }

        /// <summary>
        /// Calculates the variance given a <c>double</c> array.
        /// </summary>
        public static double Variance(double[] values)
        {
            if (values.Length == 0) throw new InvalidOperationException("Cannot calculate variance of an empty array.");
            if (values.Length == 1) return 0;

            double mean = Mean(values);
            double sumSqrd = values.Sum(num => Pow(num - mean, 2));

            return sumSqrd / (values.Length - 1);
        }

        /// <summary>
        /// Calculates the variance given a <c>int</c> array.
        /// </summary>
        public static double Variance(int[] values)
        {
            if (values.Length == 0) throw new InvalidOperationException("Cannot calculate variance of an empty array.");
            if (values.Length == 1) return 0;

            double mean = Mean(values);
            double sumSqrd = values.Sum(num => Pow(num - mean, 2));

            return sumSqrd / (values.Length - 1);
        }

        /// <summary>
        /// Calculates the standard deviation of a <c>double</c> array.
        /// The standard deviation is just the square root of variance.
        /// </summary>
        public static double StandardDeviation(double[] values)
        {
            return Sqrt(Variance(values));
        }
        
        /// <summary>
        /// Perform a cosine similarity calculation given two range of values.
        /// The cosine similarity is the measurement on how similar two vectors are, by calculating
        /// the cosine of the angle between two vectors in a multi-dimensional space,
        /// </summary>
        /// <returns>A value between -1 and 1, representing the similarity of two range of values.</returns>
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
        /// Normalization function that maps values to a 0-1 range based on some known max range
        /// </summary>
        public static double Normalize(double value, double minValue = 0, double maxValue = 1)
        {
            return (value - minValue) / (maxValue - minValue);
        }

        /// <summary>
        /// Normalization function that maps values to a 0-1 range based on some known max range
        /// </summary>
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