using System.Collections;
using System.Collections.Generic;
using LongDistancePhysics;
using UnityEditor;
using UnityEngine;


public struct Conef
{
    public Vector3 origin;
    public Vector3 end;
    /// <summary>
    /// Angle of the cone in degrees.
    /// </summary>
    public float angle;

#if UNITY_EDITOR
    
    public int segments;
    public void DebugVisualize(Color color)
    {
        // get angle ratio

        float ratio = Mathf.Atan(Mathf.Deg2Rad * angle);
        Vector3 dir = (end - origin).normalized;
        Vector3 segment = (end - origin) / segments;
        float segmentLength = segment.magnitude;
        
        // draw primary line
        Debug.DrawLine(origin, end, color, Time.deltaTime);
        
        // drawing cross points
        // get some orthogonal vectors
        Vector3 testV = SceneView.lastActiveSceneView.camera.transform.position;
        
        Vector3 ortho1 = Vector3.Cross(dir, testV).normalized;
        Vector3 ortho2 = Vector3.Cross(dir, ortho1).normalized;

        for (int i = 1; i < segments; i++)
        {
            var crossWidth = i * segmentLength * ratio;
            Vector3 segmentCenter = origin + (segment * i);
            
            Vector3 b0 = ortho1 * crossWidth;
            Vector3 b1 = segmentCenter + b0;
            Vector3 b2 = segmentCenter - b0;

            Vector3 b3 = ortho2 * crossWidth;
            Vector3 b4 = segmentCenter + b3;
            Vector3 b5 = segmentCenter - b3;
            
            Debug.DrawLine(b1, b2, color, Time.deltaTime);
            Debug.DrawLine(b4, b5, color, Time.deltaTime);
            
            DebugLDP.DrawCircle(segmentCenter, crossWidth, dir, color, Time.deltaTime);
        }
        
        var crossWidthO = segments * segmentLength * ratio;
        Vector3 segmentCenterO = origin + (segment * segments);
            
        Vector3 c0 = ortho1 * crossWidthO;
        Vector3 c1 = segmentCenterO + c0;
        Vector3 c2 = segmentCenterO - c0;

        Vector3 c3 = ortho2 * crossWidthO;
        Vector3 c4 = segmentCenterO + c3;
        Vector3 c5 = segmentCenterO - c3;
            
        Debug.DrawLine(c1, c2, color, Time.deltaTime);
        Debug.DrawLine(c4, c5, color, Time.deltaTime);
        
        Debug.DrawLine(origin, c1, color, Time.deltaTime);
        Debug.DrawLine(origin, c2, color, Time.deltaTime);
        Debug.DrawLine(origin, c4, color, Time.deltaTime);
        Debug.DrawLine(origin, c5, color, Time.deltaTime);
            
        DebugLDP.DrawCircle(segmentCenterO, crossWidthO, dir, color, Time.deltaTime);
        
    }
#endif
}
