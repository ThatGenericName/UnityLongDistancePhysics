using System;
using LongDistancePhysics;
using UnityEngine;

namespace Cone
{
    public class BoxTest : MonoBehaviour
    {

        public Vector3 extent;
        public Bounds bounds;
        public void Update()
        {
            bounds = new Bounds();
            bounds.extents = extent;
            bounds.center = transform.position;

            DebugLDP.DrawBox(bounds, Color.green, Time.fixedDeltaTime);
        }
    }
}