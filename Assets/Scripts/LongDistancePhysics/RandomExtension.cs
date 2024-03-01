using System;

namespace LongDistancePhysics
{
    public static class RandomExtension
    {
        public static Random GlobalRandom { get; private set; } = new Random();

        public static int GlobalRandomSeed;

        public static void ResetGlobalRandom()
        {
            GlobalRandom = new Random();
        }

        public static void ResetGlobalRandom(int seed)
        {
            GlobalRandomSeed = seed;
            GlobalRandom = new Random(seed);
        }

        public static float Range(this Random rand, float min, float max)
        {
            float init = (float)rand.NextDouble();

            float range = max - min;
            return min + init * range;
        }

        public static float NextFloat(this Random rand)
        {
            return (float)rand.NextDouble();
        }
    }
}