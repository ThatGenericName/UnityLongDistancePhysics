using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LongDistancePhysics;
using UnityEngine;

namespace ObjectZoning2
{
    public class DynamicBoundingVolumeNode
    {
        public TrackedObject CenterObject;
        public HashSet<TrackedObject> TrackedObjects = new();

        public Vector3 Min
        {
            get => _min;
        }

        public Vector3 Max
        {
            get => _max;
        }

        public Vector3 Mid
        {
            get => _mid;
        }
        
        private Vector3 _min;
        private Vector3 _max;
        private Vector3 _mid;
        
        private LineRenderer BoundaryMarker1;

        private DynamicBoundingVolumeNode()
        {
            
        }

        public void DeactivateNode()
        {
            CenterObject = null;
            
        }

        public bool CheckCollision(TrackedObject trackedObject)
        {
            float threshold;
            switch (trackedObject.ObjectZoningPriority)
            {
                case 1:
                    threshold = TrackedObjectController.QuarterZoneOffset;
                    break;
                case 2:
                    threshold = TrackedObjectController.BaseZoneOffset;
                    break;
                default:
                    threshold = 0;
                    return false;
            }
            
            return MathLDP.WithinLinearDistance(trackedObject.position, CenterObject.position, threshold);
        }


        public void AddObject(TrackedObject trackedObject)
        {
            trackedObject.dynamicZones.Add(this);
            TrackedObjects.Add(trackedObject);
        }

        public void RemoveObject(TrackedObject trackedObject)
        {
            trackedObject.dynamicZones.Remove(this);
            TrackedObjects.Remove(trackedObject);
        }
        
        
        private static Queue<DynamicBoundingVolumeNode> inactiveNodeQueue = new();

        public static void AllocateNodes(DynamicBoundingVolumeController manager, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var k = InitializeNewNode(manager);
                inactiveNodeQueue.Enqueue(k);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DynamicBoundingVolumeNode InitializeNewNode(
            DynamicBoundingVolumeController manager)
        {
            DynamicBoundingVolumeNode returnNode = new()
            {
                BoundaryMarker1 = Object.Instantiate(manager.boundaryMarkerPrefab).GetComponent<LineRenderer>()
            };
            
            returnNode.BoundaryMarker1.transform.parent = manager.transform;
            returnNode.UpdateBounds();
            return returnNode;
        }
        
        public static DynamicBoundingVolumeNode InitializeNode(
            DynamicBoundingVolumeController manager)
        {
            if (inactiveNodeQueue.Count == 0)
            {
                return InitializeNewNode(manager);
            }
            else
            {
                DynamicBoundingVolumeNode returnNode = inactiveNodeQueue.Dequeue();
                returnNode.UpdateBounds();
                return returnNode;
            }
        }

        public static DynamicBoundingVolumeNode InitializeNode(
            DynamicBoundingVolumeController manager,
            TrackedObject trackedObject
            )
        {
            var newNode = InitializeNode(manager);
            newNode.CenterObject = trackedObject;
            newNode.TrackedObjects.Add(trackedObject);
            newNode.UpdateBounds();
            return newNode;
        }

        public void UpdateBounds()
        {
            if (CenterObject == null)
            {
                _mid = Vector3.zero;
                _min = Vector3.zero;
                _max = Vector3.zero;
                return;
            }

            Vector3 offset;
            switch (CenterObject.ObjectZoningPriority)
            {
                case 0:
                case 1:
                    offset = TrackedObjectController.BaseZoneOffset * Vector3.one;
                    break;
                case 2:
                    if (TrackedObjects.Count == 0)
                    {
                        offset = CenterObject.objectSelfBuffer * Vector3.one;
                    }
                    else
                    {
                        offset = TrackedObjectController.HalfZoneOffset * Vector3.one;
                    }

                    break;
                default:
                    offset = Vector3.one;
                    break;
            }
            _mid = CenterObject.position;
            _min = _mid - offset;
            _max = _mid + offset;
        }
        
        public void DrawBoundaryMarkers()
        {
            UpdateBounds();
            var corners = GetCornerSets();
                
            BoundaryMarker1.startColor = Color.yellow;
            BoundaryMarker1.endColor = Color.yellow;
            BoundaryMarker1.positionCount = corners.Length;
            BoundaryMarker1.SetPositions(corners);
        }

        public void HideBoundaryMarkers()
        {
            BoundaryMarker1.positionCount = 0;
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