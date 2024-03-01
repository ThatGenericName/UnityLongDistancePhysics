using Unity.Mathematics;
using UnityEngine;


namespace LongDistancePhysics
{
    public static class DebugLDP
    {
        public static void DrawBox(Vector3 min, Vector3 max, Color color)
        {
            var edgePairs = MathLDP.GetEdgePairs(min, max);

            foreach (var edgePair in edgePairs)
            {
                Debug.DrawLine(edgePair.Item1, edgePair.Item2, color);
            }
        }
        
        public static void DrawBox(Bounds bounds, Color color, float duration = 0.0f, bool occlusion=false)
        {
            var edgePairs = MathLDP.GetEdgePairs(bounds.min, bounds.max);

            foreach (var edgePair in edgePairs)
            {
                Debug.DrawLine(edgePair.Item1, edgePair.Item2, color, duration, occlusion);
            }
        }


        public static void DrawCircle(Vector3 center, float radius, Vector3 dir, Color color, float duration = 0.0f, bool occlusion=false, int points = 32)
        {
            /*
             * This method works by first creating a circle of the specified size, rotating it to face the direction,
             * and then moving it to where it should be centered.
             *
             */
            
            var rotationQuat = Quaternion.FromToRotation(Vector3.up, dir);
            Vector3[] pointsArray = new Vector3[points];
            float arc = 2 * math.PI / points;
            for (int i = 0; i < points; i++)
            {
                float x = radius * Mathf.Cos(arc * i);
                float z = radius * Mathf.Sin(arc * i);
                pointsArray[i].x = x;
                pointsArray[i].z = z;
                pointsArray[i].y = 0f;
                pointsArray[i] = rotationQuat * pointsArray[i] + center;
            }
            

            for (int i = 0; i < points; i++)
            {
                
                Debug.DrawLine(pointsArray[i], pointsArray[(i+1) % points], color, duration, occlusion);
            }
        }

        public static void DrawTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Color color, float duration = 0.0f, bool occlusion=false)
        {
            Debug.DrawLine(v0, v1, color, duration, occlusion);
            Debug.DrawLine(v1, v2, color, duration, occlusion);
            Debug.DrawLine(v2, v0, color, duration, occlusion);
        }
    }
}