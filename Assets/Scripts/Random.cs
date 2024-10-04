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