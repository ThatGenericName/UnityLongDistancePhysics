using UnityEngine;
using System.Collections.Generic;


namespace ObjectZoning2
{
        public class StaticBoundingVolumeNode
        {
            public (int, int, int) ChunkId;

            public HashSet<TrackedObject> objects = new();
            public HashSet<DynamicBoundingVolumeNode> dbvNodes = new();

            public StaticBoundingVolumeController SBVManager;

            private LineRenderer BoundaryMarker1;
            
            private static Queue<StaticBoundingVolumeNode> inactiveNodes = new();

            public Vector3 Min;
            public Vector3 Max;
            
            
            private StaticBoundingVolumeNode()
            {
                
            }

            public static void AllocateNodes(StaticBoundingVolumeController manager, int count)
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
                    ChunkId.Item1 * TrackedObjectController.HalfZoneOffset,
                    ChunkId.Item2 * TrackedObjectController.HalfZoneOffset,
                    ChunkId.Item3 * TrackedObjectController.HalfZoneOffset
                );
                
                Max = new Vector3(
                    (ChunkId.Item1 + 1) * TrackedObjectController.HalfZoneOffset,
                    (ChunkId.Item2 + 1) * TrackedObjectController.HalfZoneOffset,
                    (ChunkId.Item3 + 1) * TrackedObjectController.HalfZoneOffset
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
                BoundaryMarker1.positionCount = corners.Length;
                BoundaryMarker1.SetPositions(corners);
            }

            public void HideBoundaryMarkers()
            {
                BoundaryMarker1.positionCount = 0;
            }

            public static StaticBoundingVolumeNode InitializeNewNode(StaticBoundingVolumeController manager, (int, int, int) xyz = default((int, int, int)))
            {
                StaticBoundingVolumeNode returnNode = new StaticBoundingVolumeNode();
                returnNode.BoundaryMarker1 = Object.Instantiate(manager.boundaryMarkerPrefab).GetComponent<LineRenderer>();
   
                returnNode.SBVManager = manager;
                returnNode.ChunkId = xyz;
                returnNode.CalculateBounds();
                returnNode.BoundaryMarker1.transform.parent = manager.transform;
                return returnNode;
            }
            public static StaticBoundingVolumeNode InitializeNode(
                StaticBoundingVolumeController manager, (int, int, int) xyz = default((int, int, int)))
            {
                if (inactiveNodes.Count == 0)
                {
                    StaticBoundingVolumeNode returnNode = new StaticBoundingVolumeNode
                    {
                        BoundaryMarker1 = Object.Instantiate(manager.boundaryMarkerPrefab).GetComponent<LineRenderer>(),
                        SBVManager = manager,
                        ChunkId = xyz
                    };

                    returnNode.CalculateBounds();
                    return returnNode;
                }
                else
                {
                    StaticBoundingVolumeNode returnNode = inactiveNodes.Dequeue();
                    returnNode.SBVManager = manager;
                    returnNode.ChunkId = xyz;
                    returnNode.CalculateBounds();
                    return returnNode;
                }
            }
            
            public Vector3[] Corners => new Vector3[]
            {
                Min,                        // 0
                new(Min.x, Min.y, Max.z),   // 1
                new(Max.x, Min.y, Max.z),   // 2
                new(Max.x, Min.y, Min.z),   // 3
                new(Min.x, Max.y, Min.z),   // 4
                new(Min.x, Max.y, Max.z),   // 5
                Max,                        // 6
                new(Max.x, Max.y, Min.z)    // 7
            };

            public Vector3[]GetCornerSets()
            {
                var corners = Corners;
                Vector3[] set1 = new[]
                {
                    corners[4],
                    corners[0],
                    corners[1],
                    corners[4],
                    corners[5],
                    corners[1],
                    corners[2],
                    corners[5],
                    corners[6],
                    corners[2],
                    corners[3],
                    corners[6],
                    corners[7],
                    corners[3],
                    corners[0],
                    corners[7],
                    corners[4],
                };
                return set1;
            }
        }
}