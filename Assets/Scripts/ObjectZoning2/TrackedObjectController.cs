using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ObjectZoning2
{
    [RequireComponent(typeof(StaticBoundingVolumeController))]
    public class TrackedObjectController : MonoBehaviour
    {
        public List<TrackedObject> TrackedObjects = new();

        [FormerlySerializedAs("staticBoundingVolumeController")] public StaticBoundingVolumeController StaticBoundingVolumeController;
        public DynamicBoundingVolumeController DynamicBoundingVolumeController;

        public List<TrackedObject> PriorityZeroObjs = new();
        public List<TrackedObject> PriorityOneObjs = new();
        public List<TrackedObject> PriorityTwoObjs = new();
        
        public const float BaseZoneOffset = 10;
        public const float BaseZoneOffsetSqr = BaseZoneOffset * BaseZoneOffset;
        
        public const float HalfZoneOffset = BaseZoneOffset / 2;
        public const float HalfZoneOffsetSqr = HalfZoneOffset * HalfZoneOffset;
        
        public const float QuarterZoneOffset = BaseZoneOffset / 4;
        public const float QuarterZoneOffsetSqr = QuarterZoneOffset * QuarterZoneOffset;

        public const float MultiZoneAngThresh = 60f;
        public const float MultZoneAngThreshLwr = 30f;
        
        private void Awake()
        {
            StaticBoundingVolumeController = GetComponent<StaticBoundingVolumeController>();
            DynamicBoundingVolumeController = GetComponent<DynamicBoundingVolumeController>();
        }


        public void Initialize(List<TrackedObject> trackedObjects)
        {
            TrackedObjects.Clear();
            TrackedObjects.AddRange(trackedObjects);
            
            PriorityZeroObjs.Clear();
            PriorityTwoObjs.Clear();
            PriorityOneObjs.Clear();
            
            List<TrackedObject>[] insertArr = new[] { PriorityZeroObjs, PriorityOneObjs, PriorityTwoObjs };

            foreach (var trackedObject in TrackedObjects)
            {
                insertArr[trackedObject.ObjectZoningPriority].Add(trackedObject);

                trackedObject.recalculateXYZ();
            }
            
            Initialized = true;
        }

        public bool Initialized { get; private set; } = false;
        private void FixedUpdate()
        {
            if (Initialized)
            {
                List<TrackedObject> objectsToUpdate = new();
            
                foreach (var trackedObject in TrackedObjects)
                {
                    // update xyz position
                    (int, int, int) lastPos;
                    (int, int, int) newPos = trackedObject.recalculateXYZ(out lastPos);

                    if (lastPos != newPos)
                    {
                        objectsToUpdate.Add(trackedObject);
                        trackedObject.staticZone = null;
                    }
                    
                    // cache object transform cuz extern calls are slow as fuck.
                    trackedObject.position = trackedObject.transform.position;
                }
            
                StaticBoundingVolumeController.AddObjects(objectsToUpdate);
            }
        }
    }
}