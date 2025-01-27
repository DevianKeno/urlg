/*

Program Title: Random
Date written: September 28, 2024
Date revised: October 4, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    A static class that contains helper methods for generating randomness
    primarily used for testing different aspects of the system.

Data Structures/Key Variables:
    N/A
*/

namespace RL.Random
{
    public static class Random
    {
        public static (int, int, int) RandomTripleIntTotaled(int total)
        {
            int cut1 = UnityEngine.Random.Range(0, total + 1);
            int cut2 = UnityEngine.Random.Range(0, total + 1);
            int first = System.Math.Min(cut1, cut2);
            int second = System.Math.Max(cut1, cut2);

            return (first, second - first, total - second);
        }
    }
}