using System;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

using Unity.Profiling;

namespace ObjectZoning1
{
    [RequireComponent(typeof(TrackedObjectController))]
    public class TrackedObjectGenerator : MonoBehaviour
    {
        [Min(0)] 
        public int count = 100;

        public float priority0rate = 0.1f;
        public float priority1rate = 0.3f;

        public bool generate = false;

        public bool resetBVs = false;

        public TrackedObjectController TrackedObjectController;

        public GameObject trackedObjectPrefab;

        public void Update()
        {

        }

        private bool resetInitiated = false;
        public void FixedUpdate()
        {
            if (resetInitiated)
            {
                resetInitiated = false;
            }

            if (allocateNewNodes)
            {
                allocateNewNodes = false;
                PreAllocateNodes();
            }
            if (generate)
            {
                generate = false;
                GenerateItems();
            }

            if (BVSetup)
            {
                BVSetup = false;
                CallBVSetup();
            }
            else if (resetBVs)
            {
                resetBVs = false;
                if (TrackedObjectController.Initialized)
                {
                    TrackedObjectController.DynamicBoundingVolume.Reset();
                    TrackedObjectController.StaticBoundingVolume.Reset();
                    TrackedObjectController.Initialize(GeneratedGameObjects);
                    resetInitiated = true;
                }
            }
        }

        public int preAllocateFactor = 3;
        public bool allocateNewNodes = false;
        
        public void PreAllocateNodes()
        {
            int treeNodesCount = preAllocateFactor * count;
            int chunkNodesCount = count;


            double h = 1.5 * Math.Log(count, 2);
            double n = Math.Pow(2, h + 1);

            int treeCount = (int)n;
            
            DynamicBoundingVolume.DBVNode.AllocateNodes(TrackedObjectController.DynamicBoundingVolume, treeCount);
            StaticBoundingVolume.SBVNode.AllocateNodes(TrackedObjectController.StaticBoundingVolume, chunkNodesCount);
            
        }

        public bool BVSetup = false;
        public void CallBVSetup()
        {
            TrackedObjectController.Initialize(GeneratedGameObjects);
        }

        private List<TrackedObject> GeneratedGameObjects;
        public void GenerateItems()
        {
            float min = -10 * TrackedObjectController.HalfZoneWidth;
            float max = 10 * TrackedObjectController.HalfZoneWidth;
            
            List<TrackedObject> newObjects = new();
            
            for (int i = 0; i < count; i++)
            {
                float x = Random.Range(min, max);
                float y = Random.Range(min, max);
                float z = Random.Range(min, max);

                Vector3 pos = new(x, y, z);

                var newObject = Instantiate(trackedObjectPrefab, pos, Quaternion.identity).GetComponent<TrackedObject>();
                newObject.position = pos;

                float chance = Random.value;
                if (chance < priority0rate)
                {
                    newObject.ObjectZoningPriority = 0;
                }
                else if (chance < priority0rate + priority1rate)
                {
                    newObject.ObjectZoningPriority = 1;
                }
                else
                {
                    newObject.ObjectZoningPriority = 2;
                }
                
                newObjects.Add(newObject);
            }
            GeneratedGameObjects = newObjects;
        }
    }
}