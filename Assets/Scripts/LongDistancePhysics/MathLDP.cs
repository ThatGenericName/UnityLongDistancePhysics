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
        
        public static Vector3 PointDistanceToBoxVec(Vector3 query, Vector3 boxMin, Vector3 boxMax, Vector3 boxMid)
        {
            Vector3 q1 = boxMin - query;
            Vector3 q2 = query - boxMax;
            Vector3 diff = boxMid - query;
            
            
            float x = Math.Max(Math.Max(q1.x, 0), q2.x) * Mathf.Sign(diff.x);
            float y = Math.Max(Math.Max(q1.y, 0), q2.y) * Mathf.Sign(diff.y);
            float z = Math.Max(Math.Max(q1.z, 0), q2.z) * Mathf.Sign(diff.z);;
            return new(x, y, z);
        }

        public static bool WithinLinearDistance(Vector3 a, Vector3 b, float limit)
        {
            float diffX = Math.Abs(b.x - a.x);
            float diffY = Math.Abs(a.y - b.y);
            float diffZ = Math.Abs(a.z - b.z);
            return diffX <= limit && diffY <= limit && diffZ < limit;
        }
        
        public static bool WithinLinearDistance(Vector3 a, Vector3 b, float limit, out float linDist)
        {
            float diffX = Math.Abs(b.x - a.x);
            float diffY = Math.Abs(a.y - b.y);
            float diffZ = Math.Abs(a.z - b.z);
            linDist = Math.Max(Math.Max(diffX, diffY), diffZ);
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
    
        public static float GetLinearDistance(Vector3 a, Vector3 b)
        {
            var diff = a - b;
            float diffX = Math.Abs(diff.x);
            float diffY = Math.Abs(diff.y);
            float diffZ = Math.Abs(diff.z);
            return Math.Max(Math.Max(diffX, diffY), diffZ);
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlap1D(float aMin, float aMax, float bMin, float bMax)
        {
            return aMax >= bMin && bMax >= aMin;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlap1D(float aMin, float aMax, float bMin, float bMax, out float newMin, out float newMax)
        {
            newMin = Math.Max(aMin, bMin);
            newMax = Math.Min(aMax, bMax);

            return newMax >= newMin;
        }
        
        // TODO: Implement Overlap1D with sdecimal
        
        public static bool BoxBoxIntersect(Vector3 aMin, Vector3 aMax, Vector3 bMin, Vector3 bMax)
        {
            return Overlap1D(aMin.x, aMax.x, bMin.x, bMax.x)
                   && Overlap1D(aMin.y, aMax.y, bMin.y, bMax.y)
                   && Overlap1D(aMin.z, aMax.z, bMin.z, bMax.z);
        }
        
        public static bool BoxBoxIntersect(Vector3 aMin, Vector3 aMax, Vector3 bMin, Vector3 bMax, out Vector3 newMin, out Vector3 newMax)
        {
            float minX;
            float maxX;
            float minY;
            float maxY;
            float minZ;
            float maxZ;
            if (!Overlap1D(aMin.x, aMax.x, bMin.x, bMax.x, out minX, out maxX))
            {
                newMax = Vector3.zero;
                newMin = Vector3.zero;
                return false;
            }
            if (!Overlap1D(aMin.y, aMax.y, bMin.y, bMax.y, out minY, out maxY))
            {
                newMax = Vector3.zero;
                newMin = Vector3.zero;
                return false;
            }
            if (!Overlap1D(aMin.z, aMax.z, bMin.z, bMax.z, out minZ, out maxZ))
            {
                newMax = Vector3.zero;
                newMin = Vector3.zero;
                return false;
            }

            newMin = new Vector3(minX, minY, minZ);
            newMax = new Vector3(maxX, maxY, maxZ);
            return true;
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int CalculateXYZToInt(Vector3 position, float division)
        {
            var pos = position;
            int x = Mathf.FloorToInt(pos.x / division);
            int y = Mathf.FloorToInt(pos.y / division);
            int z = Mathf.FloorToInt(pos.z / division);
            return new (x, y, z);
        }
        
        // Some Vector Extensions
        public static bool LessThan(this Vector3 a, Vector3 b)
        {
            return a.x < b.x && a.y < b.y && a.z < b.z;
        }
        public static bool Leq(this Vector3 a, Vector3 b)
        {
            return a.x <= b.x && a.y <= b.y && a.z <= b.z;
        }
        
        public static bool GreaterThan(this Vector3 a, Vector3 b)
        {
            return a.x > b.x && a.y > b.y && a.z > b.z;
        }
        
        public static bool Geq(this Vector3 a, Vector3 b)
        {
            return a.x >= b.x && a.y >= b.y && a.z >= b.z;
        }
        
        
        // corners
        // TODO: Move this to a different class
        
        public static Vector3[] GetCorners(Vector3 min, Vector3 max)
        {
            return new[]
            {
                min, // 0
                new(min.x, min.y, max.z), // 1
                new(max.x, min.y, max.z), // 2
                new(max.x, min.y, min.z), // 3
                new(min.x, max.y, min.z), // 4
                new(min.x, max.y, max.z), // 5
                max, // 6
                new(max.x, max.y, min.z) // 7
            };
        }
        
        public static (int, int)[] Edges()
        {
            return new[]
            {
                (0, 1),
                (1, 2),
                (2, 3),
                (3, 0),
                (0, 4),
                (1, 5),
                (2, 6),
                (3, 7),
                (4, 5),
                (5, 6),
                (6, 7),
                (7, 4)
            };
        }

        public static (Vector3, Vector3)[] GetEdgePairs(Vector3 min, Vector3 max)
        {
            Vector3[] corners = GetCorners(min, max);

            return new[]
            {
                (corners[0], corners[1]),
                (corners[1], corners[2]),
                (corners[2], corners[3]),
                (corners[3], corners[0]),
                (corners[0], corners[4]),
                (corners[1], corners[5]),
                (corners[2], corners[6]),
                (corners[3], corners[7]),
                (corners[4], corners[5]),
                (corners[5], corners[6]),
                (corners[6], corners[7]),
                (corners[7], corners[4])
            };
        }


        /// <summary>
        /// Determines from the perspective of <c>a</c> whether <c>c</c> is
        /// behind <c>b</c> or not. 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsBehind(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 planeNorm = a - b;
            // center everything such that b is at the origin, this will simplify our math
            c =- b;
            var dp = Vector3.Dot(planeNorm, c);
            return (dp < 0);
        }
        
        /// <summary>
        /// Determines from the perspective of <c>a</c> whether <c>c</c> is
        /// behind <c>b</c> or not with an angular check.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsBehindAngle(Vector3 a, Vector3 b, Vector3 c, float angleThreshold)
        {
            Vector3 bToa = a - b;
            Vector3 bToc = c - b;
            Vector3 aToC = c - a;
            float ang = Vector3.Angle(aToC, -bToa);

            if (ang > angleThreshold)
            {
                return false;
            }

            return Vector3.Dot(bToa, bToc) < 0;
        }
    }
}