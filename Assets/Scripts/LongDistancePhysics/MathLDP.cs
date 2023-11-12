using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LongDistancePhysics
{
    public static class MathLDP
    {
        /// <summary>
        /// returns the shortest distance to the box IF <code>query</code> is located
        /// outside of the box. If <code>query</code> is located within the box, returns 0
        ///
        /// uses the lower precision sdecimal.sqrt
        /// </summary>
        /// <param name="query"></param>
        /// <param name="boxMin"></param>
        /// <param name="boxMax"></param>
        /// <returns></returns>
        public static sdecimal PointDistanceToBox(Vector3m query, Vector3m boxMin, Vector3m boxMax)
        {
            return sdecimal.sqrt(PointDistanceToBoxSqr(query, boxMin, boxMax));
        }
        
        /// <summary>
        /// returns the shortest distance to the box IF <code>query</code> is located
        /// outside of the box. If <code>query</code> is located within the box, returns 0
        ///
        /// Currently only uses the low precision sdecimal.sqrt, as sdecimal.sqrtHP has not
        /// been implemented.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="boxMin"></param>
        /// <param name="boxMax"></param>
        /// <returns></returns>
        public static sdecimal PointDistanceToBoxHP(Vector3m query, Vector3m boxMin, Vector3m boxMax)
        {
            return sdecimal.sqrt(PointDistanceToBoxSqr(query, boxMin, boxMax));
        }

        public static sdecimal PointDistanceToBoxSqr(Vector3m query, Vector3m boxMin, Vector3m boxMax)
        {
            sdecimal x = Math.Max(Math.Max(boxMin.x - query.x, 0), query.x - boxMax.x);
            sdecimal y = Math.Max(Math.Max(boxMin.y - query.y, 0), query.y - boxMax.y);
            sdecimal z = Math.Max(Math.Max(boxMin.z - query.z, 0), query.z - boxMax.z);
            return x * x + y * y + z * z;
        }
        
        public static Vector3m PointDistanceToBoxVec(Vector3m query, Vector3m boxMin, Vector3m boxMax)
        {
            sdecimal x = Math.Max(Math.Max(boxMin.x - query.x, 0), query.x - boxMax.x);
            sdecimal y = Math.Max(Math.Max(boxMin.y - query.y, 0), query.y - boxMax.y);
            sdecimal z = Math.Max(Math.Max(boxMin.z - query.z, 0), query.z - boxMax.z);
            return new(x, y, z);
        }
        
        
        /// <summary>
        /// returns the shortest distance to the box IF <code>query</code> is located
        /// outside of the box. If <code>query</code> is located within the box, returns 0
        ///
        /// uses the lower precision sdecimal.sqrt
        /// </summary>
        /// <param name="query"></param>
        /// <param name="boxMin"></param>
        /// <param name="boxMax"></param>
        /// <returns></returns>
        public static float PointDistanceToBox(Vector3 query, Vector3 boxMin, Vector3 boxMax)
        {
            return Mathf.Sqrt(PointDistanceToBoxSqr(query, boxMin, boxMax));
        }

        public static float PointDistanceToBoxSqr(Vector3 query, Vector3 boxMin, Vector3 boxMax)
        {
            float x = Math.Max(Math.Max(boxMin.x - query.x, 0), query.x - boxMax.x);
            float y = Math.Max(Math.Max(boxMin.y - query.y, 0), query.y - boxMax.y);
            float z = Math.Max(Math.Max(boxMin.z - query.z, 0), query.z - boxMax.z);
            return x * x + y * y + z * z;
        }
        
        public static Vector3 PointDistanceToBoxVec(Vector3 query, Vector3 boxMin, Vector3 boxMax)
        {
            float x = Math.Max(Math.Max(boxMin.x - query.x, 0), query.x - boxMax.x);
            float y = Math.Max(Math.Max(boxMin.y - query.y, 0), query.y - boxMax.y);
            float z = Math.Max(Math.Max(boxMin.z - query.z, 0), query.z - boxMax.z);
            return new(x, y, z);
        }

        public static bool WithinLinearDistance(Vector3 a, Vector3 b, float limit)
        {
            float diffX = Math.Abs(b.x - a.x);
            float diffY = Math.Abs(a.y - b.y);
            float diffZ = Math.Abs(a.z - b.z);
            return diffX <= limit && diffY <= limit && diffZ < limit;
        }

        public static bool WithinLinearDistance(Vector3 a, Vector3 b, Vector3 limits)
        {
            float diffX = Math.Abs(b.x - a.x);
            float diffY = Math.Abs(a.y - b.y);
            float diffZ = Math.Abs(a.z - b.z);
            return diffX <= limits.x && diffY <= limits.y && diffZ < limits.z;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WithinLinearDistance(Vector3 delta, float limit)
        {
            return Math.Abs(delta.x) <= limit && Math.Abs(delta.y) <= limit && Math.Abs(delta.z) <= limit;
        }
        
        // TODO: Implement WithinLinearDistance with Vector3m

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlap1D(float aMin, float aMax, float bMin, float bMax)
        {
            return aMax >= bMin && bMax >= aMin;
        }
        
        // TODO: Implement Overlap1D with sdecimal
        
        public static bool BoxBoxIntersect(Vector3 aMin, Vector3 aMax, Vector3 bMin, Vector3 bMax)
        {
            return Overlap1D(aMin.x, aMax.x, bMin.x, bMax.x)
                   && Overlap1D(aMin.y, aMax.y, bMin.y, bMax.y)
                   && Overlap1D(aMin.z, aMax.z, bMin.z, bMax.z);
        }
        
        // TODO: Implement BoxBoxIntersect with Vector3m

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int, int, int) CalculateXYZ(Vector3 position, float division)
        {
            var pos = position;
            int x = Mathf.FloorToInt(pos.x / division);
            int y = Mathf.FloorToInt(pos.y / division);
            int z = Mathf.FloorToInt(pos.z / division);
            return (x, y, z);
        }
    }
}