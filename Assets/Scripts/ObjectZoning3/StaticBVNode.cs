using System.Collections.Generic;
using LongDistancePhysics;
using UnityEngine;

namespace ObjectZoning3
{
    public class StaticBVNode
    {
        private readonly HashSet<TrackedObject> _objects = new();

        private readonly HashSet<DynamicBVNode> _dynamicZones = new();

        private Bounds _bounds;


        private Vector3Int _xyz;
        public Vector3Int xyz
        {
            get => _xyz;
            private set
            {
                
            }
        }

        public Vector3Int NodePosition
        {
            get => _xyz;
            set
            {
                _xyz = value;
                RecalculateBounds();
            }
        }
        
        // Experimental Node Traversal Stuff.
        /*
         * element indices
         * z = -1:
         * y 1  6 7 8
         *   0  3 4 5
         *  -1  0 1 2
         *     -1 0 1 x
         * 
         * z = 0:
         * y 1  15 16 17
         *   0  12 13 14
         *  -1   9 10 11
         *      -1  0  1 x
         *
         * z = 1:
         * y 1 24 25 26
         *   0 21 22 23
         *  -1 18 19 20
         *     -1  0  1 x
         */
        
        private StaticBVNode[] _neighbours = new StaticBVNode[26];
        
        // to be tested in the future.
        
        public void AddObject(TrackedObject trackedObject)
        {
            _objects.Add(trackedObject);
        }
        
        public void RemoveDynamicZone(DynamicBVNode node)
        {
            _dynamicZones.Remove(node);
        }

        public void AddDynamicZone(DynamicBVNode node)
        {
            _dynamicZones.Add(node);
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
            Vector3 min = new Vector3(
                NodePosition.x * StaticBoundingVolumeController.NodeWidth,
                NodePosition.y * StaticBoundingVolumeController.NodeWidth,
                NodePosition.z * StaticBoundingVolumeController.NodeWidth
                );
            
            Vector3 max = new Vector3(
                (NodePosition.x + 1) * StaticBoundingVolumeController.NodeWidth,
                (NodePosition.y + 1) * StaticBoundingVolumeController.NodeWidth,
                (NodePosition.z + 1) * StaticBoundingVolumeController.NodeWidth
            );

            _bounds = BoundsExtension.CreateBounds(min, max);
        }

        
        /// <summary>
        /// Clears all information in this node
        /// </summary>
        private void ClearNode()
        {
            _objects.Clear();
            _dynamicZones.Clear();
            _bounds = default;
            NodePosition = default;
        }
        
        /*
         * Here's some static methods for node allocation and deallocation.
         * More experimentation is needed to test performance vs just nulling and
         * and allocating new Node instances.
         *
         * For this reason the Constructor will be private.
         */

        private StaticBVNode()
        {
            
        }
        
        private StaticBVNode(Vector3Int xyz)
        {
            NodePosition = xyz;
            RecalculateBounds();
        }
        
        private static Queue<StaticBVNode> InactiveNodeQueue = new();

        public static StaticBVNode AllocateNode()
        {
            if (InactiveNodeQueue.Count == 0)
            {
                return new StaticBVNode();
            }

            return InactiveNodeQueue.Dequeue();
        }
        
        public static StaticBVNode AllocateNode(Vector3Int xyz)
        {
            if (InactiveNodeQueue.Count == 0)
            {
                return new StaticBVNode(xyz);
            }

            var returnNode = InactiveNodeQueue.Dequeue();
            returnNode.NodePosition = xyz;
            return returnNode;
        }

        public static void DeallocateNode(StaticBVNode staticBVNode)
        {
            staticBVNode.ClearNode();
            InactiveNodeQueue.Enqueue(staticBVNode);
        }
    }
}