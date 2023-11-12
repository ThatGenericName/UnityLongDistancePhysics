using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LongDistancePhysics
{
    [System.Serializable]
    public struct sdecimal : ISerializationCallbackReceiver, IEquatable<sdecimal>, IFormattable
    {
        public decimal value;
        [SerializeField]
        private int[] data;

        public void OnBeforeSerialize ()
        {
            data = decimal.GetBits(value);
        }
        public void OnAfterDeserialize ()
        {
            if (data != null && data.Length == 4)
            {
                value = new decimal(data);
            }
        }

        public sdecimal(decimal value)
        {
            this.value = value;
            data = null;
        }
        
        // casts to this type
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator sdecimal(decimal value)
        {
            return new sdecimal(value);
        }

        public static explicit operator sdecimal(double value)
        {
            return new sdecimal((decimal)value);
        }

        public static explicit operator sdecimal(float value)
        {
            return new sdecimal((decimal)value);
        }

        public static implicit operator sdecimal(int value)
        {
            return new sdecimal(value);
        }
        
        // casts to other types
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator decimal(sdecimal value)
        {
            return value.value;
        }

        public static explicit operator double(sdecimal value)
        {
            return (double)value.value;
        }

        public static explicit operator float(sdecimal value)
        {
            return (float)value.value;
        }

        public static explicit operator int(sdecimal value)
        {
            return (int)value.value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(sdecimal other)
        {
            return value == other.value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return value.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return value.ToString(format, formatProvider);
        }
        
        // Certain Operators
        
        // add
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sdecimal operator +(sdecimal a, sdecimal b) => new(a.value + b.value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sdecimal operator +(sdecimal a, float b) => new(a.value + (decimal)b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sdecimal operator +(float b, sdecimal a) => new(a.value + (decimal)b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // sub
        public static sdecimal operator -(sdecimal a, sdecimal b) => new(a.value - b.value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sdecimal operator -(sdecimal a, float b) => new(a.value - (decimal)b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sdecimal operator -(float b, sdecimal a) => new(a.value - (decimal)b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sdecimal operator -(sdecimal a) => new(-a.value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // mult
        public static sdecimal operator *(sdecimal a, sdecimal b) => new(a.value * b.value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sdecimal operator *(sdecimal a, float b) => new(a.value * (decimal)b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sdecimal operator *(float b, sdecimal a) => new(a.value * (decimal)b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sdecimal operator *(sdecimal a, double b) => new(a.value * (decimal)b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sdecimal operator *(double b, sdecimal a) => new(a.value * (decimal)b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sdecimal operator *(sdecimal a, int b) => new(a.value * b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sdecimal operator *(int b, sdecimal a) => new(a.value * b);
        // division
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sdecimal operator /(sdecimal a, sdecimal b) => new(a.value / b.value);
        // modulo
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sdecimal operator %(sdecimal a, sdecimal b) => new(a.value % b.value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(sdecimal a, sdecimal b) => a.value == b.value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(sdecimal a, sdecimal b) => a.value != b.value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(sdecimal a, sdecimal b) => a.value < b.value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(sdecimal a, sdecimal b) => a.value > b.value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(sdecimal a, sdecimal b) => a.value <= b.value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(sdecimal a, sdecimal b) => a.value >= b.value;

        public override bool Equals(object obj)
        {
            if (!(obj is sdecimal sdCast))
            {
                return false;
            }

            return (this == sdCast);
        }

        // new operators
        public static readonly sdecimal Zero = new(0);
        public static sdecimal modm(sdecimal k, sdecimal n) => ((k %= n) < Zero) ? k + n : k;
        public static int modi(sdecimal k, sdecimal n) => ((k %= n) < Zero) ? (int)(k + n) : (int)k;

        public static sdecimal sqrt(sdecimal value)
        {
            return (sdecimal)Math.Sqrt((double)value);
        }
    }
}


