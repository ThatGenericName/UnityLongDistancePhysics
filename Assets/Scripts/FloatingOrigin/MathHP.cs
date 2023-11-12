using System;
using System.Runtime.CompilerServices;

namespace FloatingOrigin
{
    public static class MathHP
    {
        public static decimal modm(decimal k, decimal n) { return ((k %= n) < 0) ? k + n : k; }
        public static int modi(int k, int n) { return ((k %= n) < 0) ? k + n : k; }
        public static double mod(double k, double n) { return ((k %= n) < 0) ? k + n : k; }
        public static float modf(float k, float n){ return ((k %= n) < 0) ? k + n : k; }
        
        public static decimal SqrtHighPrecision(decimal x, decimal? guess = null)
        {
            var ourGuess = guess.GetValueOrDefault(x / 2m);
            var result = x / ourGuess;
            var average = (ourGuess + result) / 2m;

            if (average == ourGuess) // This checks for the maximum precision possible with a decimal.
                return average;
            else
                return SqrtHighPrecision(x, average);
        }
        /// <summary>
        /// A lower precision version of SQRT for decimal
        /// Simply casts to double before performing the standard
        /// Math.Sqrt on it. Based on my tests, it has an average
        /// % error of 2.7e-13
        ///
        /// So as long as you don't need to accumulate sqrt results
        /// you should be fine.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Sqrt(decimal x)
        {
            return (decimal)Math.Sqrt((double)x);
        }
    }
}