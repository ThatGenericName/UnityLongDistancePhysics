

using UnityEngine;

namespace LongDistancePhysics
{
    public static class BoundsExtension
    {

        public static Bounds CreateBounds(Vector3 min, Vector3 max)
        {
            return new((max + min) / 2, (max - min) / 2);
        }
        

        /// <summary>
        /// Returns a new bound consisting of the old bounds shifted an offset.
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Bounds Translate(this Bounds bounds, Vector3 offset)
        {
            return new Bounds(bounds.center + offset, bounds.extents);
        }

        public static Vector3[] GetCorners(this Bounds bounds)
        {
            Vector3 Min = bounds.min;
            Vector3 Max = bounds.max;
            return new[]
            {
                Min, // 0
                new(Min.x, Min.y, Max.z), // 1
                new(Max.x, Min.y, Max.z), // 2
                new(Max.x, Min.y, Min.z), // 3
                new(Min.x, Max.y, Min.z), // 4
                new(Min.x, Max.y, Max.z), // 5
                Max, // 6
                new(Max.x, Max.y, Min.z) // 7
            };
        }

        public static (int, int)[] Edges(this Bounds bounds)
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
    }
}