using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LongDistancePhysics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ObjectZoning2
{
    public class StaticBoundingVolumeController : MonoBehaviour
    {

        public Dictionary<(int, int, int), StaticBoundingVolumeNode> Chunks = new();

        public GameObject boundaryMarkerPrefab;

        public void AddObjects(List<TrackedObject> objects)
        {
            foreach (var trackedObject in objects)
            {
                var xyz = trackedObject.XYZPos;

                StaticBoundingVolumeNode targetNode;
                if (!Chunks.TryGetValue(xyz, out targetNode))
                {
                    targetNode = StaticBoundingVolumeNode.InitializeNode(this);
                    Chunks[xyz] = targetNode;
                    targetNode.ChunkId = xyz;
                }

                targetNode.objects.Add(trackedObject);
                trackedObject.staticZone = targetNode;
            }
        }
        
        public StaticBoundingVolumeNode this[int x, int y, int z] => getXYZ(x, y, z);

        public StaticBoundingVolumeNode this[(int, int, int) xyz] => getXYZ(xyz.Item1, xyz.Item2, xyz.Item3);

        public StaticBoundingVolumeNode getXYZ(int x, int y, int z)
        {
            StaticBoundingVolumeNode targetNode;

            var xyz = (x, y, z);
            
            if (!Chunks.TryGetValue(xyz, out targetNode))
            {
                targetNode = StaticBoundingVolumeNode.InitializeNode(this, xyz);
            }
            return targetNode;
        }
        
        public void Reset()
        {
            foreach (var chunkPair in Chunks)
            {
                chunkPair.Value.Clear();
            }
            
            Chunks.Clear();
        }

        public void SetupBoundingVolume(List<TrackedObject> objects)
        {
            foreach (var trackedObject in objects)
            {
                var xyz = trackedObject.XYZPos;

                StaticBoundingVolumeNode targetNode;
                if (!Chunks.TryGetValue(xyz, out targetNode))
                {
                    targetNode = StaticBoundingVolumeNode.InitializeNode(this);
                    Chunks[xyz] = targetNode;
                    targetNode.CalculateBounds();
                    targetNode.ChunkId = xyz;
                }

                targetNode.objects.Add(trackedObject);
                trackedObject.staticZone = targetNode;
            }
        }

        public void AddDBVNode(DynamicBoundingVolumeNode dbvNode)
        {
            var minXYZ = MathLDP.CalculateXYZ(dbvNode.Min, TrackedObjectController.HalfZoneOffset);
            var maxXYZ = MathLDP.CalculateXYZ(dbvNode.Max, TrackedObjectController.HalfZoneOffset);

            for (int x = minXYZ.Item1; x < maxXYZ.Item1 + 1; x++)
            {
                for (int y = minXYZ.Item2; y < maxXYZ.Item2 + 1; y++)
                {
                    for (int z = minXYZ.Item3; z < maxXYZ.Item3 + 1; z++)
                    {
                        var xyz = (x, y, z);
                        var targetNode = this[x, y, z];

                        targetNode.dbvNodes.Add(dbvNode);
                    }
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<TrackedObject> GetObjectsInProximity((int, int, int) xyz, int prox)
        {
            return GetObjectsInProximity(xyz.Item1, xyz.Item2, xyz.Item3, prox);
        }
        
        public List<TrackedObject> GetObjectsInProximity(int x, int y, int z, int prox)
        {
            List<TrackedObject> returnList = new();
            
            for (int xInd = x-prox; xInd < x+prox+1; xInd++)
            {
                for (int yInd = y-prox; yInd < y+prox+1; yInd++)
                {
                    for (int zInd = z-prox; zInd < z+prox+1; zInd++)
                    {
                        StaticBoundingVolumeNode node;
                        if (Chunks.TryGetValue((xInd, yInd, zInd), out node))
                        {
                            returnList.AddRange(node.objects);
                        }
                    }
                }
            }

            return returnList;
        }
        
        public HashSet<DynamicBoundingVolumeNode> GetDynamicZonesInProximity((int, int, int) xyz, int prox)
        {
            return GetDynamicZonesInProximity(xyz.Item1, xyz.Item2, xyz.Item3, prox);
        }

        public HashSet<DynamicBoundingVolumeNode> GetDynamicZonesInProximity(int x, int y, int z, int prox)
        {
            HashSet<DynamicBoundingVolumeNode> returnList = new();
            
            for (int xInd = x-prox; xInd < x+prox; xInd++)
            {
                for (int yInd = y-prox; yInd < y+prox; yInd++)
                {
                    for (int zInd = z-prox; zInd < z+prox; zInd++)
                    {
                        StaticBoundingVolumeNode node;
                        if (Chunks.TryGetValue((xInd, yInd, zInd), out node))
                        {
                            returnList.UnionWith(node.dbvNodes);
                        }
                    }
                }
            }

            return returnList;
        }
    }
}