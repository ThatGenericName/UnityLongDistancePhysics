using System;
using UnityEngine;

namespace FloatingOrigin
{
    /// <summary>
    /// Partial Vector3 implementation with the HighPrecisionNumber.
    ///
    /// A significant limit of this class is that HighPrecisionNumber is
    /// essentially a fixed point precision value, and thus has relatively
    /// low maximum value.
    ///
    /// It is for this reason that if you need to do dot or cross products,
    /// you perform the normalized dot product instead.
    /// </summary>
    public struct Vector3HP
    {
        public HighPrecisionNumber x;

        public HighPrecisionNumber y;

        public HighPrecisionNumber z;
        
        // Constructors

        public Vector3HP(double x, double y)
        {
            this.x = new(x);
            this.y = new(y);
            this.z = new(0);
        }
        
        public Vector3HP(HighPrecisionNumber x, HighPrecisionNumber y)
        {
            this.x = x;
            this.y = y;
            this.z = new(0);
        }

        public Vector3HP(double x, double y, double z)
        {
            this.x = new(x);
            this.y = new(y);
            this.z = new(z);
        }

        public Vector3HP(HighPrecisionNumber x, HighPrecisionNumber y, HighPrecisionNumber z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public void Set(HighPrecisionNumber x, HighPrecisionNumber y, HighPrecisionNumber z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        
        
        // Casts to related types.

        public Vector3 ToVector3()
        {
            return new Vector3(x.ToFloat(), y.ToFloat(), z.ToFloat());
        }

        public Vector3 ToVector3Normed()
        {
            var normed = Normalized();
            return normed.ToVector3();
        }
        
        // Vector maths
        
        // magnitude


        public HighPrecisionNumber Magnitude()
        {
            decimal res1 = MathHP.Sqrt(SqrMagnitudem());

            return new(res1);
        }

        public HighPrecisionNumber SqrMagnitude()
        {
            decimal xm = x.ToDecimal();
            decimal ym = y.ToDecimal();
            decimal zm = z.ToDecimal();

            return new(xm + ym + zm);
        }
        
        public decimal SqrMagnitudem()
        {
            decimal xm = x.ToDecimal();
            decimal ym = y.ToDecimal();
            decimal zm = z.ToDecimal();

            return xm + ym + zm;
        }
        
        
        /// <summary>
        /// Returns the normalized vector
        /// </summary>
        /// <returns></returns>
        public Vector3HP Normalized()
        {
            decimal x = this.x.ToDecimal();
            decimal y = this.y.ToDecimal();
            decimal z = this.z.ToDecimal();

            decimal length = MathHP.Sqrt(x * x + y * y + z * z);

            HighPrecisionNumber newX = this.x / length;
            HighPrecisionNumber newY = this.y / length;
            HighPrecisionNumber newZ = this.z / length;

            return new(newX, newY, newZ);
        }
    
        /// <summary>
        /// Returns a normalized dot product of the 2 numbers
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static HighPrecisionNumber DotNormhp(Vector3HP lhs, Vector3HP rhs)
        {
            return Dot(lhs.Normalized(), rhs.Normalized());
        }

        public static double DotNormd(Vector3HP lhs, Vector3HP rhs)
        {
            var lhsNorm = lhs.Normalized();
            var rhsNorm = rhs.Normalized();

            double xComp = lhsNorm.x.ToDouble() * rhsNorm.x.ToDouble();
            double yComp = lhsNorm.y.ToDouble() * rhsNorm.y.ToDouble();
            double zComp = lhsNorm.z.ToDouble() * rhsNorm.z.ToDouble();

            return xComp + yComp + zComp;
        }

        public static HighPrecisionNumber Dot(Vector3HP lhs, Vector3HP rhs)
        {
            HighPrecisionNumber xComp = lhs.x * rhs.x;
            HighPrecisionNumber yComp = lhs.y * rhs.y;
            HighPrecisionNumber zComp = lhs.z * rhs.z;

            return xComp + yComp + zComp;
        }

        // cross product

        public static Vector3HP CrossHP(Vector3HP lhs, Vector3HP rhs)
        {
            HighPrecisionNumber newX = lhs.y * rhs.z - lhs.z * rhs.y;
            HighPrecisionNumber newY = lhs.z * rhs.x - lhs.x * rhs.z;
            HighPrecisionNumber newZ = lhs.x * rhs.y - lhs.y * rhs.x;

            return new Vector3HP(newX, newY, newZ);
        }
        
        
        public static Vector3HP CrossNormHP(Vector3HP lhs, Vector3HP rhs)
        {
            return CrossHP(lhs.Normalized(), rhs.Normalized());
        }

        public static Vector3 CrossNormf(Vector3HP lhs, Vector3HP rhs)
        {
            return Vector3.Cross(lhs.Normalized().ToVector3(), rhs.Normalized().ToVector3());
        }
        
        // get angle between 2 vectors.

        public static float Angle(Vector3HP from, Vector3HP to)
        {
            var formNorm = from.Normalized();
            var toNormed = to.Normalized();

            return Vector3.Angle(formNorm.ToVector3(), toNormed.ToVector3());
        }
        
    }
}