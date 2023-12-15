using System;
using System.Collections.Generic;
using UnityEngine;

namespace ObjectZoning3
{
    public class TrackedObjectController : MonoBehaviour
    {

        public List<TrackedObject> TrackedObjects = new List<TrackedObject>();


        public DynamicBoundingVolumeController DynamicBoundingVolumeController = new();
        public StaticBoundingVolumeController StaticBoundingVolumeController = new();

        
        private void FixedUpdate()
        {
            UpdateObjectPositions();
        }

        private void UpdateObjectPositions()
        {
            foreach (var trackedObject in TrackedObjects)
            {
                trackedObject.Position = trackedObject.transform.position;
            }
        }
    }
}