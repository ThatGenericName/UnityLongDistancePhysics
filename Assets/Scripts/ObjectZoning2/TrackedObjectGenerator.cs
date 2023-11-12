using System;
using System.Collections.Generic;
using LongDistancePhysics;
using Random = System.Random;
using TreeEditor;
using UnityEngine;
using Object = UnityEngine.Object;

using Unity.Profiling;

namespace ObjectZoning2
{
    [RequireComponent(typeof(TrackedObjectController))]
    public class TrackedObjectGenerator : MonoBehaviour
    {
        [Min(0)] 
        public int count = 100;

        public float priority0rate = 0.1f;
        public float priority1rate = 0.3f;

        public TrackedObjectController TrackedObjectController;

        public GameObject trackedObjectPrefab;

        public float spread = 10;

        public void Update()
        {

        }
        public void FixedUpdate()
        {
            if (allocateNewNodes)
            {
                allocateNewNodes = false;
                PreAllocateNodes();
            }

            if (generateObjects)
            {
                generateObjects = false;
                GenerateItems();
                
            }

            if (addObjectsToMain)
            {
                addObjectsToMain = false;
                AddObjectsToMain();;
            }

            if (generateZones)
            {
                generateZones = false;
                TrackedObjectController.StaticBoundingVolumeController.SetupBoundingVolume(GeneratedGameObjects);
                TrackedObjectController.DynamicBoundingVolumeController.SetupBoundingVolumes(GeneratedGameObjects);
            }
        }
        
        public bool allocateNewNodes = false;
        public bool generateObjects = false;
        public bool addObjectsToMain = false;
        public bool generateZones = false;


        public void AddObjectsToMain()
        {
            TrackedObjectController.Initialize(GeneratedGameObjects);
        }
        public void PreAllocateNodes()
        {
            int chunkNodesCount = count;
            
            double h = 1.5 * Math.Log(count, 2);
            double n = Math.Pow(2, h + 1);

            int treeCount = (int)n;
            
            StaticBoundingVolumeNode.AllocateNodes(TrackedObjectController.StaticBoundingVolumeController, chunkNodesCount);
            DynamicBoundingVolumeNode.AllocateNodes(TrackedObjectController.DynamicBoundingVolumeController, treeCount);
        }
        

        public List<TrackedObject> GeneratedGameObjects;

        public int randomSeed = 451373785;
        public void GenerateItems()
        {
            Random rand = new Random(randomSeed);
            float min = -spread * TrackedObjectController.HalfZoneOffset;
            float max = spread * TrackedObjectController.HalfZoneOffset;
            
            List<TrackedObject> newObjects = new();
            
            for (int i = 0; i < count; i++)
            {
                float x = rand.Range(min, max);
                float y = rand.Range(min, max);
                float z = rand.Range(min, max);

                Vector3 pos = new(x, y, z);

                var newObject = Instantiate(trackedObjectPrefab, pos, Quaternion.identity).GetComponent<TrackedObject>();
                newObject.position = pos;

                float chance = rand.NextFloat();
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