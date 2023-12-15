using System.Collections.Generic;
using LongDistancePhysics;
using UnityEngine;

namespace ObjectZoning3
{
    public class TrackedObject : MonoBehaviour
    {
        public float bufferSize = 5;

        private StaticBVNode _staticBvNode;

        private HashSet<DynamicBVNode> _dynamicBvNodes;

        public Bounds Extent = new Bounds(Vector3.zero, 2 * Vector3.one);

        public Bounds Bounds => Extent.Translate(Position);

        public Vector3 Position;

        public int PriorityType;
    }
}