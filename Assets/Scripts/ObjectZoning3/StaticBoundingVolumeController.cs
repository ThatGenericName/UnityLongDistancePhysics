using System.Collections.Generic;
using LongDistancePhysics;
using UnityEngine;

namespace ObjectZoning3
{
    public class StaticBoundingVolumeController
    {
        
        public const float NodeWidth = 5;
        public const float HalfNodeWidth = NodeWidth / 2;

        private Dictionary<Vector3Int, StaticBVNode> _nodes = new();
        
        public void Insert(TrackedObject trackedObject)
        {
            var xyz = MathLDP.CalculateXYZVector3Int(trackedObject.Position, NodeWidth);
            StaticBVNode targetNode;
            if (!_nodes.TryGetValue(xyz, out targetNode))
            {
                targetNode = StaticBVNode.AllocateNode(xyz);
                _nodes[xyz] = targetNode;
            }
            
            targetNode.AddObject(trackedObject);
        }

        public void InsertDBVNode(DynamicBVNode dynamicBvNode)
        {
            var min = MathLDP.CalculateXYZVector3Int(dynamicBvNode.Bounds.min, NodeWidth);
            var max = MathLDP.CalculateXYZVector3Int(dynamicBvNode.Bounds.max, NodeWidth);

            for (int x = min.x; x < max.x; x++)
            {
                for (int y = min.y; y < max.y; y++)
                {
                    for (int z = min.z; z < max.z; z++)
                    {
                        
                        
                    }
                }
            }
        }
    }
}