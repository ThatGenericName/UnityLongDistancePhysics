using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LongDistancePhysics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ObjectZoning1
{
    public class StaticBoundingVolume : MonoBehaviour
    {

        public Dictionary<(int, int, int), SBVNode> Chunks = new();

        public GameObject boundaryMarkerPrefab;

        public void AddObjects(List<TrackedObject> objects)
        {
            foreach (var trackedObject in objects)
            {
                var xyz = trackedObject.XYZPos;

                SBVNode targetNode;
                if (!Chunks.TryGetValue(xyz, out targetNode))
                {
                    targetNode = SBVNode.InitializeNode(this);
                    Chunks[xyz] = targetNode;
                    targetNode.ChunkId = xyz;
                }

                targetNode.objects.Add(trackedObject);
                trackedObject.staticZone = targetNode;
            }
        }

        public void Reset()
        {
            foreach (var chunkPair in Chunks)
            {
                chunkPair.Value.Clear();
            }
            
            Chunks.Clear();
        }

        public void SetupStaticBoundingVolume(List<TrackedObject> objects)
        {
            foreach (var trackedObject in objects)
            {
                var xyz = trackedObject.XYZPos;

                SBVNode targetNode;
                if (!Chunks.TryGetValue(xyz, out targetNode))
                {
                    targetNode = SBVNode.InitializeNode(this);
                    Chunks[xyz] = targetNode;
                    targetNode.CalculateBounds();
                    targetNode.ChunkId = xyz;
                }

                targetNode.objects.Add(trackedObject);
                trackedObject.staticZone = targetNode;
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
            
            for (int xInd = x-prox; xInd < x+prox; xInd++)
            {
                for (int yInd = y-prox; yInd < y+prox; yInd++)
                {
                    for (int zInd = z-prox; zInd < z+prox; zInd++)
                    {
                        SBVNode node;
                        if (Chunks.TryGetValue((xInd, yInd, zInd), out node))
                        {
                            returnList.AddRange(node.objects);
                        }
                    }
                }
            }

            return returnList;
        }

        public void InsertDBVNode(DynamicBoundingVolume.DBVNode dbvNode)
        {
            var minXYZ = MathLDP.CalculateXYZ(dbvNode.Min, TrackedObjectController.HalfZoneWidth);
            var maxXYZ = MathLDP.CalculateXYZ(dbvNode.Max, TrackedObjectController.HalfZoneWidth);

            for (int x = minXYZ.Item1; x < maxXYZ.Item1; x++)
            {
                for (int y = minXYZ.Item2; y < maxXYZ.Item2; y++)
                {
                    for (int z = minXYZ.Item3; z < maxXYZ.Item3; z++)
                    {
                        SBVNode targetNode;

                        var xyz = (x, y, z);
                        
                        if (!Chunks.TryGetValue(xyz, out targetNode))
                        {
                            targetNode = SBVNode.InitializeNode(this);
                            Chunks[xyz] = targetNode;
                            targetNode.ChunkId = xyz;
                        }

                        if (MathLDP.BoxBoxIntersect(dbvNode.Min, dbvNode.Max, targetNode.Min, targetNode.Max))
                        {
                            targetNode.DBVNodes.Add(dbvNode);
                        }
                    }
                }
            }
        }

        public class SBVNode
        {
            public (int, int, int) ChunkId;

            public HashSet<TrackedObject> objects = new();
            public HashSet<DynamicBoundingVolume.DBVNode> DBVNodes = new();

            public StaticBoundingVolume SBVManager;

            private LineRenderer BoundaryMarker1;
            private LineRenderer BoundaryMarker2;
            
            private static Queue<SBVNode> inactiveNodes = new();

            public Vector3 Min;
            public Vector3 Max;

            private SBVNode()
            {
                
            }

            public static void AllocateNodes(StaticBoundingVolume manager, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    var newNode = InitializeNewNode(manager);
                    newNode.SBVManager = null;
                    inactiveNodes.Enqueue(newNode);
                }
            }

            public void Clear()
            {
                foreach (var trackedObject in objects)
                {
                    trackedObject.staticZone = null;
                }

                DBVNodes.Clear();;
                SBVManager = null;
                
                objects.Clear();
                HideBoundaryMarkers();
                Min = Vector3.positiveInfinity;
                Max = Vector3.negativeInfinity;
                inactiveNodes.Enqueue(this);
                ChunkId = default;
            }
            
            public void CalculateBounds()
            {
                Min = new Vector3(
                    ChunkId.Item1 * TrackedObjectController.HalfZoneWidth,
                    ChunkId.Item2 * TrackedObjectController.HalfZoneWidth,
                    ChunkId.Item3 * TrackedObjectController.HalfZoneWidth
                );
                
                Max = new Vector3(
                    (ChunkId.Item1 + 1) * TrackedObjectController.HalfZoneWidth,
                    (ChunkId.Item2 + 1) * TrackedObjectController.HalfZoneWidth,
                    (ChunkId.Item3 + 1) * TrackedObjectController.HalfZoneWidth
                );
            }

            public void DrawBoundaryMarkers()
            {
                if (objects.Count == 0)
                {
                    return;
                }
                
                CalculateBounds();
                var corners = GetCornerSets();
                
                BoundaryMarker1.startColor = Color.red;
                BoundaryMarker1.endColor = Color.red;
                BoundaryMarker1.positionCount = corners.Item1.Length;
                BoundaryMarker1.SetPositions(corners.Item1);
                
                BoundaryMarker2.startColor = Color.red;
                BoundaryMarker2.endColor = Color.red;
                BoundaryMarker2.positionCount = corners.Item2.Length;
                BoundaryMarker2.SetPositions(corners.Item2);
            }

            public void HideBoundaryMarkers()
            {
                BoundaryMarker1.positionCount = 0;
                
                BoundaryMarker2.positionCount = 0;
            }

            public static SBVNode InitializeNewNode(StaticBoundingVolume manager, (int, int, int) xyz = default((int, int, int)))
            {
                SBVNode returnNode = new SBVNode();
                returnNode.BoundaryMarker1 = Instantiate(manager.boundaryMarkerPrefab).GetComponent<LineRenderer>();
                returnNode.BoundaryMarker2 = Instantiate(manager.boundaryMarkerPrefab).GetComponent<LineRenderer>();
                    
                returnNode.SBVManager = manager;
                returnNode.ChunkId = xyz;
                returnNode.CalculateBounds();
                return returnNode;
            }
            public static SBVNode InitializeNode(
                StaticBoundingVolume manager, (int, int, int) xyz = default((int, int, int)))
            {
                if (inactiveNodes.Count == 0)
                {
                    SBVNode returnNode = new SBVNode();
                    returnNode.BoundaryMarker1 = Instantiate(manager.boundaryMarkerPrefab).GetComponent<LineRenderer>();
                    returnNode.BoundaryMarker2 = Instantiate(manager.boundaryMarkerPrefab).GetComponent<LineRenderer>();
                    
                    returnNode.SBVManager = manager;
                    returnNode.ChunkId = xyz;
                    returnNode.CalculateBounds();
                    return returnNode;
                }
                else
                {
                    SBVNode returnNode = inactiveNodes.Dequeue();
                    returnNode.SBVManager = manager;
                    returnNode.ChunkId = xyz;
                    returnNode.CalculateBounds();
                    return returnNode;
                }
            }
            
            public Vector3[] Corners => new Vector3[]
            {
                Min,                        // 1
                new(Min.x, Min.y, Max.z),   // 2
                new(Max.x, Min.y, Max.z),   // 3
                new(Max.x, Min.y, Min.z),   // 4
                new(Min.x, Max.y, Min.z),   // 5
                new(Min.x, Max.y, Max.z),   // 6
                Max,                        // 7
                new(Max.x, Max.y, Min.z)    // 8
            };

            public (Vector3[], Vector3[]) GetCornerSets()
            {
                var corners = Corners;
                Vector3[] set1 = new[]
                {
                    corners[5],
                    corners[4],
                    corners[0],
                    corners[1],
                    corners[5],
                    corners[6],
                    corners[2],
                    corners[1]
                };
                Vector3[] set2 = new[]
                {
                    corners[7],
                    corners[3],
                    corners[2],
                    corners[6],
                    corners[7],
                    corners[4],
                    corners[0],
                    corners[3]
                };
                return (set1, set2);
            }
        }
    }
}