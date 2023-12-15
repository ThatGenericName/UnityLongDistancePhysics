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
        
        public static void DrawBox(Bounds bounds, Color color)
        {
            var edgePairs = MathLDP.GetEdgePairs(bounds.min, bounds.max);

            foreach (var edgePair in edgePairs)
            {
                Debug.DrawLine(edgePair.Item1, edgePair.Item2, color);
            }
        }
    }
}