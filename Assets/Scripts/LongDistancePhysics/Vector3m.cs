using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LongDistancePhysics
{
    
    /// <summary>
    /// A 3D vector using the decimal type.
    ///
    /// Unity's Vector3 will always perform better, so when possible use that instead.
    /// For that reason many methods that exist in Vector3 is not cloned to here.
    /// 
    /// </summary>
    public struct Vector3m : IEquatable<Vector3m>, IFormattable
    {
        public sdecimal x;
        public sdecimal y;
        public sdecimal z;

        // constructors
        
        public Vector3m(sdecimal x, sdecimal y, sdecimal z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3m(decimal x, decimal y, decimal z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        
        // type casting

        public static implicit operator Vector3(Vector3m value)
        {
            return new Vector3((float)value.x, (float)value.y, (float)value.z);
        }

        // inherited members that I need to implement

        public override bool Equals(object other) => other is Vector3m other1 && this.Equals(other1);
        public bool Equals(Vector3m other)
        {
            return (sdecimal) this.x == (sdecimal) other.x && (sdecimal) this.y == (sdecimal) other.y && (sdecimal) this.z == (sdecimal) other.z;
        }
        public string ToString(string format) => this.ToString(format, (IFormatProvider)null);
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                format = "F2";
            if (formatProvider == null)
                formatProvider = (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat;

            return String.Format(
                CultureInfo.InvariantCulture.NumberFormat,
                "({0}, {1}, {2})",
                x.ToString(format, formatProvider),
                y.ToString(format, formatProvider),
                z.ToString(format, formatProvider)
            );
        }
        
        // properties that I found I needed to use
        private static readonly Vector3m zeroVector = new Vector3m(0.0m, 0.0m, 0.0m);
        private static readonly Vector3m oneVector = new Vector3m(1m, 1m, 1m);
        private static readonly Vector3m upVector = new Vector3m(0.0m, 1m, 0.0m);
        private static readonly Vector3m downVector = new Vector3m(0.0m, -1m, 0.0m);
        private static readonly Vector3m leftVector = new Vector3m(-1m, 0.0m, 0.0m);
        private static readonly Vector3m rightVector = new Vector3m(1m, 0.0m, 0.0m);
        private static readonly Vector3m forwardVector = new Vector3m(0.0m, 0.0m, 1m);
        private static readonly Vector3m backVector = new Vector3m(0.0m, 0.0m, -1m);
        
        
        /// <summary>
        ///   <para>Shorthand for writing Vector3(0, 0, 0).</para>
        /// </summary>
        public static Vector3m zero
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Vector3m.zeroVector;
        }
        
        
        // methods that I found I needed to use.
        
        /// <summary>
        ///   <para>Cross Product of two vectors.</para>
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3m Cross(Vector3m lhs, Vector3m rhs) => new Vector3m((lhs.y * rhs.z - lhs.z * rhs.y), ( lhs.z * rhs.x - lhs.x * rhs.z), (lhs.x * rhs.y - lhs.y * rhs.x));
        /// <summary>
        ///   <para>Dot Product of two vectors.</para>
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sdecimal Dot(Vector3m lhs, Vector3m rhs) => (lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z);
        
        // might not need this.
        /// <summary>
        ///   <para>Projects a vector onto another vector.</para>
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="onNormal"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3m Project(Vector3m vector, Vector3m onNormal)
        {
            sdecimal num1 = Vector3m.Dot(onNormal, onNormal);
            if (num1 == 0m) // zero check
                return Vector3m.zero;
            sdecimal num2 = Vector3m.Dot(vector, onNormal);
            return new Vector3m(onNormal.x * num2 / num1, onNormal.y * num2 / num1, onNormal.z * num2 / num1);
        }
        
        // Operator Overloads

        public static Vector3m operator +(Vector3m a, Vector3m b) => new Vector3m(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3m operator +(Vector3m a, Vector3 b) => new Vector3m(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3m operator -(Vector3m a, Vector3m b) => new Vector3m(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3m operator -(Vector3m a, Vector3 b) => new Vector3m(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3m operator -(Vector3m a) => new Vector3m(-a.x, -a.y, -a.z);
        public static Vector3m operator *(Vector3m a, float d) => new Vector3m(a.x * d, a.y * d, a.z * d);
        
        public static Vector3m operator *(float d, Vector3m a) => new Vector3m(a.x * d, a.y * d, a.z * d);
    }
}