using System.Collections.Generic;
using LongDistancePhysics;
using UnityEngine;

namespace ObjectZoning3
{
    public class DynamicBVNode
    {
        private readonly HashSet<TrackedObject> _objects = new();

        private TrackedObject _centerObject;

        private HashSet<StaticBVNode> _intersectingStaticNodes;

        public Bounds Bounds { get; private set; }
        

        public void AddObject(TrackedObject trackedObject)
        {
            _objects.Add(trackedObject);
        }

        public void RemoveObject(TrackedObject trackedObject)
        {
            _objects.Remove(trackedObject);
        }
        
        /// <summary>
        /// Since this is a static bound, for the sake of performance this should
        /// only be called by StaticBoundingVolumeController during FixedUpdate
        /// </summary>
        public void RecalculateBounds()
        {
            Vector3 center = _centerObject.Position;
            float offset = DynamicBoundingVolumeController.BaseOffset; // More calculations in the future.
            Vector3 extent = Vector3.one * offset;

            Bounds = BoundsExtension.CreateBounds(center, extent);
        }

        
        /// <summary>
        /// Clears all information in this node
        /// </summary>
        private void ClearNode()
        {
            _objects.Clear();
            _intersectingStaticNodes.Clear();
            Bounds = default;
            _centerObject = null;
        }
        
        /*
         * Here's some static methods for node allocation and deallocation.
         * More experimentation is needed to test performance vs just nulling and
         * and allocating new Node instances.
         *
         * For this reason the Constructor will be private.
         */

        private DynamicBVNode()
        {
            
        }
        
        private DynamicBVNode(TrackedObject centerObject)
        {
            _centerObject = centerObject;
            RecalculateBounds();
        }
        
        private static Queue<DynamicBVNode> InactiveNodeQueue = new();

        public static DynamicBVNode AllocateNode()
        {
            if (InactiveNodeQueue.Count == 0)
            {
                return new DynamicBVNode();
            }

            return InactiveNodeQueue.Dequeue();
        }
        
        public static DynamicBVNode AllocateNode(TrackedObject centerObject)
        {
            if (InactiveNodeQueue.Count == 0)
            {
                return new DynamicBVNode(centerObject);
            }

            var returnNode = InactiveNodeQueue.Dequeue();
            returnNode._centerObject = centerObject;
            return returnNode;
        }

        public static void DeallocateNode(DynamicBVNode staticBVNode)
        {
            staticBVNode.ClearNode();
            InactiveNodeQueue.Enqueue(staticBVNode);
        }
    }
}