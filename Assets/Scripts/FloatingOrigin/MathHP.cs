namespace FloatingOrigin
{
    public static class MathHP
    {
        public static decimal modm(decimal k, decimal n) { return ((k %= n) < 0) ? k + n : k; }
        public static int modi(int k, int n) { return ((k %= n) < 0) ? k + n : k; }
        public static double mod(double k, double n) { return ((k %= n) < 0) ? k + n : k; }
        public static float modf(float k, float n){ return ((k %= n) < 0) ? k + n : k; }
        
        public static decimal Sqrt(decimal x, decimal? guess = null)
        {
            var ourGuess = guess.GetValueOrDefault(x / 2m);
            var result = x / ourGuess;
            var average = (ourGuess + result) / 2m;

            if (average == ourGuess) // This checks for the maximum precision possible with a decimal.
                return average;
            else
                return Sqrt(x, average);
        }
    }
}