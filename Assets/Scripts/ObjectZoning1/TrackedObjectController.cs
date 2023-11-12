using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ObjectZoning1
{
    [RequireComponent(typeof(StaticBoundingVolume))]
    [RequireComponent(typeof(DynamicBoundingVolume))]
    public class TrackedObjectController : MonoBehaviour
    {
        public List<TrackedObject> TrackedObjects = new();

        public StaticBoundingVolume StaticBoundingVolume;

        public DynamicBoundingVolume DynamicBoundingVolume;

        public List<TrackedObject> PriorityZeroObjs = new();
        public List<TrackedObject> PriorityOneObjs = new();
        public List<TrackedObject> PriorityTwoObjs = new();
        
        public const float BaseZoneWidth = 10;
        public const float BaseZoneWidthSqr = BaseZoneWidth * BaseZoneWidth;
        
        public const float HalfZoneWidth = BaseZoneWidth / 2;
        public const float HalfZoneWidthSqr = HalfZoneWidth * HalfZoneWidth;
        
        public const float QuarterZoneWidth = BaseZoneWidth / 4;
        public const float QuarterZoneWidthSqr = QuarterZoneWidth * QuarterZoneWidth;

        public const float MultiZoneAngThresh = 60f;
        public const float MultZoneAngThreshLwr = 30f;
        
        private void Awake()
        {
            StaticBoundingVolume = GetComponent<StaticBoundingVolume>();
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
            
            StaticBoundingVolume.SetupStaticBoundingVolume(trackedObjects);
            DynamicBoundingVolume.SetupDynamicBoundingVolume(insertArr);
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
            
                StaticBoundingVolume.AddObjects(objectsToUpdate);
            }
        }
    }
}