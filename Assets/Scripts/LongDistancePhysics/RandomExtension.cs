using System;

namespace LongDistancePhysics
{
    public static class RandomExtension
    {

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