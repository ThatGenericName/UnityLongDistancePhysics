using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace ObjectZoning2
{
    [RequireComponent(typeof(TrackedObjectController))]
    [RequireComponent(typeof(DynamicBoundingVolumeController))]
    [RequireComponent(typeof(StaticBoundingVolumeController))]
    public class BoundingVolumeVisualizer : MonoBehaviour
    {
        [Range(0, 25)]
        public int minDepth;
        [Range(0, 3)]
        public int depthCount;

        public bool DrawStaticBounds = false;
        public bool DrawDynamicBounds = false;
        
        public StaticBoundingVolumeController staticBoundingVolumeController;
        public DynamicBoundingVolumeController DynamicBoundingVolumeController;
        public TrackedObjectController TrackedObjectController;
        
        private void Awake()
        {
            TrackedObjectController toc = GetComponent<TrackedObjectController>();
            staticBoundingVolumeController = toc.StaticBoundingVolumeController;
            DynamicBoundingVolumeController = toc.DynamicBoundingVolumeController;
        }

        // Update is called once per frame
        void Update()
        {
            if (TrackedObjectController.Initialized)
            {
                if (DrawStaticBounds)
                {
                    ShowSBVBoundsRenderer();
                }
                else
                {
                    HideSBVBoundsRenderer();
                }

                if (DrawDynamicBounds)
                {
                    ShowDBVBoundsRenderer();
                }
                else
                {
                    HideDBVBoundsRenderer();
                }
                
                UpdatePriorityVisibility();
            }
        }

        private void FixedUpdate()
        {

        }

        public bool ShowPriority0 = true;
        public bool ShowPriority1 = true;
        public bool ShowPriority2 = true;
        
        private void UpdatePriorityVisibility()
        {
            foreach (var priorityObj in TrackedObjectController.PriorityZeroObjs)
            {
                priorityObj.mr.enabled = ShowPriority0;
            }
            foreach (var priorityObj in TrackedObjectController.PriorityOneObjs)
            {
                priorityObj.mr.enabled = ShowPriority1;
            }
            foreach (var priorityObj in TrackedObjectController.PriorityTwoObjs)
            {
                priorityObj.mr.enabled = ShowPriority2;
            }
        }

        private void ShowSBVBoundsRenderer()
        {
            foreach (var nodePair in staticBoundingVolumeController.Chunks)
            {
                nodePair.Value.DrawBoundaryMarkers();
            }
        }

        private void HideSBVBoundsRenderer()
        {
            foreach (var nodePair in staticBoundingVolumeController.Chunks)
            {
                nodePair.Value.HideBoundaryMarkers();
            }
        }

        public void ShowDBVBoundsRenderer()
        {
            foreach (var node in DynamicBoundingVolumeController.Nodes)
            {
                node.DrawBoundaryMarkers();
            }
        }
        
        public void HideDBVBoundsRenderer()
        {
            foreach (var node in DynamicBoundingVolumeController.Nodes)
            {
                node.HideBoundaryMarkers();
            }
        }
    }
}


