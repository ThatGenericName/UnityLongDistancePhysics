using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace ObjectZoning1
{
    [RequireComponent(typeof(TrackedObjectController))]
    public class BoundingVolumeVisualizer : MonoBehaviour
    {
        [Range(0, 25)]
        public int minDepth;
        [Range(0, 3)]
        public int depthCount;

        public bool DrawStaticBounds = false;
        public bool DrawDynamicBounds = false;

        public DynamicBoundingVolume DynamicBoundingVolume;
        public StaticBoundingVolume StaticBoundingVolume;
        public TrackedObjectController TrackedObjectController;
        
        private void Awake()
        {
            TrackedObjectController toc = GetComponent<TrackedObjectController>();
            DynamicBoundingVolume = toc.DynamicBoundingVolume;
            StaticBoundingVolume = toc.StaticBoundingVolume;
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
                    ShowDBVBoundRenderer();
                }
                else
                {
                    HideDBVBoundRenderer();
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
            foreach (var nodePair in StaticBoundingVolume.Chunks)
            {
                nodePair.Value.DrawBoundaryMarkers();
            }
        }

        private void HideSBVBoundsRenderer()
        {
            foreach (var nodePair in StaticBoundingVolume.Chunks)
            {
                nodePair.Value.HideBoundaryMarkers();
            }
        }

        private void ShowDBVBoundRenderer()
        {
            var root = DynamicBoundingVolume.GetRoot();

            Queue<(int, DynamicBoundingVolume.DBVNode)> nodeQueue = new();
            nodeQueue.Enqueue((0, root));
            
            while (nodeQueue.Count != 0)
            {
                var depthPair = nodeQueue.Dequeue();

                if (depthPair.Item1 >= minDepth && depthPair.Item1 <= minDepth + depthCount)
                {
                    depthPair.Item2.ShowBoundaryMarkers();
                }
                else
                {
                    depthPair.Item2.HideBoundaryMarkers();;
                }
                if (depthPair.Item2.Left != null)
                {
                    nodeQueue.Enqueue((depthPair.Item1 + 1, depthPair.Item2.Left));
                }
                if (depthPair.Item2.Right != null)
                {
                    nodeQueue.Enqueue((depthPair.Item1 + 1, depthPair.Item2.Right));
                }
            }
        }

        private void HideDBVBoundRenderer()
        {
            var root = DynamicBoundingVolume.GetRoot();

            Queue<(int, DynamicBoundingVolume.DBVNode)> nodeQueue = new();
            nodeQueue.Enqueue((0, root));
            
            while (nodeQueue.Count != 0)
            {
                var depthPair = nodeQueue.Dequeue();
                
                depthPair.Item2.HideBoundaryMarkers();;
                
                if (depthPair.Item2.Left != null)
                {
                    nodeQueue.Enqueue((depthPair.Item1 + 1, depthPair.Item2.Left));
                }
                if (depthPair.Item2.Right != null)
                {
                    nodeQueue.Enqueue((depthPair.Item1 + 1, depthPair.Item2.Right));
                }
            }
        }
    }
}


