using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FloatingOrigin
{
    public struct HighPrecisionNumber
    {

        private readonly int upper;
        public int Upper => upper;
        private readonly double lower;
        public double Lower => lower;
        
        private const double LowerMax = 16384.0;
        private const decimal LowerMaxm = 16384.0m;
        private const double LowerMin = 0.0d;

        public bool IsOverflow
        {
            get;
            private set;
        }

        public static HighPrecisionNumber MaxValue => new HighPrecisionNumber(Int32.MaxValue, LowerMax);

        public const float MaxValuef = (float)LowerMax * Int32.MaxValue + (float)LowerMax;
        public const double MaxValued = LowerMax * Int32.MaxValue + LowerMax;
        public const decimal MaxValuem = (decimal)LowerMax * Int32.MaxValue + (decimal)LowerMax;

        public static HighPrecisionNumber MinValue => new HighPrecisionNumber(Int32.MinValue, LowerMin);


        public HighPrecisionNumber(int value)
        {
            upper = (int)Math.Floor(value / LowerMax);
            lower = MathHP.mod(value, LowerMax);
            IsOverflow = false;
        }

        public HighPrecisionNumber(float value)
        {
            upper = (int)Math.Floor(value / LowerMax);
            lower = MathHP.mod(value, LowerMax);
            IsOverflow = false;
        }
        
        public HighPrecisionNumber(double value)
        {
            upper = (int)Math.Floor(value / LowerMax);
            lower = MathHP.mod(value, LowerMax);
            IsOverflow = false;
        }

        public HighPrecisionNumber(decimal value)
        {
            upper = (int)Math.Floor(value / LowerMaxm);
            lower = (double)MathHP.modm(value, LowerMaxm);
            IsOverflow = false;
        }

        public HighPrecisionNumber(int upper, double lower)
        {
            this.upper = upper;
            this.lower = lower;
            IsOverflow = false;
            //TODO: make some asserts to ensure that lowerMin <= lower <= lowerMax;
        }

        // type casts
        public float ToFloat()
        {
            return (float)(upper * LowerMax) + (float)lower;
        }
        
        public double ToDouble()
        {
            return (double)(upper * LowerMax) + (double)lower;
        }

        public decimal ToDecimal()
        {
            return (decimal)upper * (decimal)LowerMax + (decimal)lower;
        }
        
        // math overloads
        
        // Addition Operators
        public static HighPrecisionNumber operator +(HighPrecisionNumber a)
        {
            return Add(a);
        }
        
        public static HighPrecisionNumber operator +(HighPrecisionNumber a, HighPrecisionNumber b)
        {
            return Add(a, b);
        }

        public static HighPrecisionNumber operator +(HighPrecisionNumber a, int b)
        {
            return Add(a, new HighPrecisionNumber(b));
        }
        
        public static HighPrecisionNumber operator +(HighPrecisionNumber a, float b)
        {
            return Add(a, new HighPrecisionNumber(b));
        }
        
        public static HighPrecisionNumber operator +(HighPrecisionNumber a, double b)
        {
            return Add(a, new HighPrecisionNumber(b));
        }
        
        public static HighPrecisionNumber operator +(HighPrecisionNumber a, decimal b)
        {
            return Add(a, new HighPrecisionNumber(b));
        }
        
        // Subtraction Operators
        
        public static HighPrecisionNumber operator -(HighPrecisionNumber a)
        {
            return Sub(a);
        }
        
        public static HighPrecisionNumber operator -(HighPrecisionNumber a, HighPrecisionNumber b)
        {
            return Sub(a, b);
        }

        public static HighPrecisionNumber operator -(HighPrecisionNumber a, int b)
        {
            return Sub(a, new HighPrecisionNumber(b));
        }
        
        public static HighPrecisionNumber operator -(HighPrecisionNumber a, float b)
        {
            return Sub(a, new HighPrecisionNumber(b));
        }
        
        public static HighPrecisionNumber operator -(HighPrecisionNumber a, double b)
        {
            return Sub(a, new HighPrecisionNumber(b));
        }
        
        public static HighPrecisionNumber operator -(HighPrecisionNumber a, decimal b)
        {
            return Sub(a, new HighPrecisionNumber(b));
        }
        
        
        // Division Operators

        public static HighPrecisionNumber operator /(HighPrecisionNumber a, HighPrecisionNumber b)
        {
            return Div(a, b);
        }

        public static HighPrecisionNumber operator /(HighPrecisionNumber a, int b)
        {
            return Div(a, new HighPrecisionNumber(b));
        }
        
        public static HighPrecisionNumber operator /(HighPrecisionNumber a, float b)
        {
            return Div(a, new HighPrecisionNumber(b));
        }
        
        public static HighPrecisionNumber operator /(HighPrecisionNumber a, double b)
        {
            return Div(a, new HighPrecisionNumber(b));
        }
        
        public static HighPrecisionNumber operator /(HighPrecisionNumber a, decimal b)
        {
            return Div(a, new HighPrecisionNumber(b));
        }
        
        // Math Operators

        public static HighPrecisionNumber operator *(HighPrecisionNumber a, HighPrecisionNumber b)
        {
            return Mult(a, b);
        }

        public static HighPrecisionNumber operator *(HighPrecisionNumber a, int b)
        {
            return Mult(a, b);
        }
        
        public static HighPrecisionNumber operator *(HighPrecisionNumber a, float b)
        {
            return Mult(a, b);
        }
        
        public static HighPrecisionNumber operator *(HighPrecisionNumber a, double b)
        {
            return Mult(a, b);
        }
        
        public static HighPrecisionNumber operator *(HighPrecisionNumber a, decimal b)
        {
            return Mult(a, b);
        }
        
        
        // math
        
        private static HighPrecisionNumber Add(HighPrecisionNumber a)
        {
            return new(a.upper, a.lower);
        }
        
        private static HighPrecisionNumber Add(HighPrecisionNumber a, HighPrecisionNumber b)
        {
            double lowerSum = a.lower + b.lower;

            int lowerOverflow = (int)(lowerSum / LowerMax);
            double lower = lowerSum % LowerMax;

            int upper = a.upper + b.upper + lowerOverflow;
            return new(upper, lower);
        }

        private static HighPrecisionNumber Sub(HighPrecisionNumber a)
        {
            double lowerSum = - a.lower;

            int lowerOverflow = (int)(lowerSum / LowerMax);
            double lower = lowerSum % LowerMax;

            int upper = - a.upper + lowerOverflow;
            return new(upper, lower);
        }
        
        private static HighPrecisionNumber Sub(HighPrecisionNumber a, HighPrecisionNumber b)
        {
            double lowerSum = a.lower - b.lower;

            int lowerOverflow = (int)(lowerSum / LowerMax);
            double lower = lowerSum % LowerMax;

            int upper = a.upper - b.upper + lowerOverflow;
            return new(upper, lower);
        }

        private static HighPrecisionNumber Div(HighPrecisionNumber a, HighPrecisionNumber b)
        {
            decimal bDec = b.ToDecimal();
            decimal upperDiv = a.upper / bDec;
            decimal upperDivDec = upperDiv % 1;

            int newUpper = (int)upperDiv;
            double lowerDiv = a.lower / (double)bDec;
            double newLower = lowerDiv + (double)upperDivDec * LowerMax;

            return new(newUpper, newLower);
        }
        
        private const double LowerMaxSqr = LowerMax * LowerMax;
        
        private static HighPrecisionNumber Mult(HighPrecisionNumber a, HighPrecisionNumber b)
        {
            double m1 = a.upper * b.upper * LowerMaxSqr;
            double m2 = (a.upper * b.lower * LowerMax);
            double m3 = (a.lower * b.upper * LowerMax);
            double m4 = (a.lower * b.lower);

            return new((decimal)m1 + (decimal)m2 + (decimal)m3 + (decimal)m4);
        }

        private static HighPrecisionNumber Mult(HighPrecisionNumber a, int b)
        {
            double m1 = a.upper * b * LowerMax;
            double m2 = a.lower * b;
            return new(m1 + m2);
        }
        
        private static HighPrecisionNumber Mult(HighPrecisionNumber a, double b)
        {
            double m1 = a.upper * b * LowerMax;
            double m2 = a.lower * b;
            return new((decimal)m1 + (decimal)m2);
        }
        
        private static HighPrecisionNumber Mult(HighPrecisionNumber a, decimal b)
        {
            decimal m1 = a.upper * b * (decimal)LowerMax;
            decimal m2 = (decimal)a.lower * b;
            return new(m1 + m2);
        }
    }
}

