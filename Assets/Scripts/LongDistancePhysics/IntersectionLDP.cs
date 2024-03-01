using UnityEngine;

namespace LongDistancePhysics
{
    public static class IntersectionLDP
    {
        public static bool ConeBoxIntersect(Conef cone, Bounds bounds, out Vector3 lineToBox, out Vector3 v2)
        {
            var shortDist = MathLDP.PointDistanceToBoxVec(cone.origin, bounds.min, bounds.max, bounds.center);

            lineToBox = shortDist + cone.origin;

            // construct some orthogonal vectors

            var coneDir = cone.end - cone.origin;

            var dirToBox = shortDist;
            var ortho1 = Vector3.Cross(shortDist, coneDir);
            var ortho2 = Vector3.Cross(coneDir, ortho1).normalized;
            var ratio = Mathf.Atan(Mathf.Deg2Rad * cone.angle);
            var mag = coneDir.magnitude * ratio;

            v2 = mag * ortho2 + cone.end;
            return AABBTriIntersect(cone.origin, cone.end, v2, bounds);
        }


        /*
         * The below comes from
         * https://gist.github.com/zvonicek/fe73ba9903f49d57314cf7e8e0f05dcf?permalink_comment_id=4269670#gistcomment-4269670
         * Which should be based on
         * https://gdbooks.gitbooks.io/3dcollisions/content/Chapter4/aabb-triangle.html
         *
         * The original code was written in C# for Godot, which I have made very minor modifications
         * for Unity.
         *
         */
        public static bool AABBTriIntersect(Vector3 v0, Vector3 v1, Vector3 v2, Bounds aabb)
        {
            var aabbCentre = aabb.center;
            var aabbExtents = aabb.extents;

            v0 -= aabbCentre;
            v1 -= aabbCentre;
            v2 -= aabbCentre;

            var ab = (v1 - v0).normalized;
            var bc = (v2 - v1).normalized;
            var ca = (v0 - v2).normalized;

            //Cross ab, bc, and ca with (1, 0, 0)
            Vector3 a00 = new(0.0f, -ab.z, ab.y);
            Vector3 a01 = new(0.0f, -bc.z, bc.y);
            Vector3 a02 = new(0.0f, -ca.z, ca.y);

            //Cross ab, bc, and ca with (0, 1, 0)
            Vector3 a10 = new(ab.z, 0.0f, -ab.x);
            Vector3 a11 = new(bc.z, 0.0f, -bc.x);
            Vector3 a12 = new(ca.z, 0.0f, -ca.x);

            //Cross ab, bc, and ca with (0, 0, 1)
            Vector3 a20 = new(-ab.y, ab.x, 0.0f);
            Vector3 a21 = new(-bc.y, bc.x, 0.0f);
            Vector3 a22 = new(-ca.y, ca.x, 0.0f);

            if (!AABB_Tri_SAT(v0, v1, v2, aabbExtents, a00) ||
                !AABB_Tri_SAT(v0, v1, v2, aabbExtents, a01) ||
                !AABB_Tri_SAT(v0, v1, v2, aabbExtents, a02) ||
                !AABB_Tri_SAT(v0, v1, v2, aabbExtents, a10) ||
                !AABB_Tri_SAT(v0, v1, v2, aabbExtents, a11) ||
                !AABB_Tri_SAT(v0, v1, v2, aabbExtents, a12) ||
                !AABB_Tri_SAT(v0, v1, v2, aabbExtents, a20) ||
                !AABB_Tri_SAT(v0, v1, v2, aabbExtents, a21) ||
                !AABB_Tri_SAT(v0, v1, v2, aabbExtents, a22) ||
                !AABB_Tri_SAT(v0, v1, v2, aabbExtents, new Vector3(1, 0, 0)) ||
                !AABB_Tri_SAT(v0, v1, v2, aabbExtents, new Vector3(0, 1, 0)) ||
                !AABB_Tri_SAT(v0, v1, v2, aabbExtents, new Vector3(0, 0, 1)) ||
                !AABB_Tri_SAT(v0, v1, v2, aabbExtents, Vector3.Cross(ab, bc))
               )
                return false;

            return true;
        }

        public static bool AABB_Tri_SAT(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 aabbExtents, Vector3 axis)
        {
            var p0 = Vector3.Dot(v0, axis);
            var p1 = Vector3.Dot(v1, axis);
            var p2 = Vector3.Dot(v2, axis);

            var r = aabbExtents.x * Mathf.Abs(Vector3.Dot(new Vector3(1, 0, 0), axis)) +
                    aabbExtents.y * Mathf.Abs(Vector3.Dot(new Vector3(0, 1, 0), axis)) +
                    aabbExtents.z * Mathf.Abs(Vector3.Dot(new Vector3(0, 0, 1), axis));

            var maxP = Mathf.Max(p0, Mathf.Max(p1, p2));
            var minP = Mathf.Min(p0, Mathf.Min(p1, p2));

            return !(Mathf.Max(-maxP, minP) > r);
        }
    }
}