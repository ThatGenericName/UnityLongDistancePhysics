using System.Collections.Generic;
using LongDistancePhysics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ObjectZoning
{
    public class OZTestController : MonoBehaviour
    {

        public int ObjectCount = 100;
        public int MovingObjCount = 15;
        public Vector3 Limits = new Vector3(200, 0, 200);
        public Vector3 VelLimits = new Vector3(2, 2, 2);
        public Vector3 Offset = new();
        public int RNGSeed = 35;
        public float Level2Ratio = 0.4f;

        public GameObject ZObjectPrefab;
        private List<ZObject> GeneratedGameObjects = new();
        public void Awake()
        {
            RandomExtension.ResetGlobalRandom(RNGSeed);
            zom = GetComponent<ZObjectMover>();
        }

        private ZObjectMover zom;

        public void FixedUpdate()
        {
            if (InstantiateObjects)
            {
                InstantiateObjects = false;
                GenerateObjects();
            }

            if (InstantiateMovingObjects)
            {
                InstantiateMovingObjects = false;
                GenerateMovingObjects();
            }

            if (UpdateNearbyGrabbers)
            {
                NearbyGrabberUpdate();
            }
            
        }

        private bool ObjectsInstantiated = false;
        public bool InstantiateObjects = false;
        public void GenerateObjects()
        {
            if (ObjectsInstantiated)
            {
                return;
            }

            ObjectsInstantiated = true;
            Vector3 min = -Limits;
            for (int i = 0; i < ObjectCount; i++)
            {
                float x = RandomExtension.GlobalRandom.Range(min.x, Limits.x);
                float y = RandomExtension.GlobalRandom.Range(min.y, Limits.y);
                float z = RandomExtension.GlobalRandom.Range(min.z, Limits.z);
                Vector3 position = new Vector3(x, y, z) + Offset;



                var newGO = Instantiate(ZObjectPrefab, position, Quaternion.identity);
                var zObj = newGO.GetComponent<ZObject>();
                zObj.DrawDebugLines = true;
                if (i == 0)
                {
                    zObj.TreeObjectType = FloatingSpatialTree.TreeObjectType.Type0;
                }
                else
                {
                    float chance = RandomExtension.GlobalRandom.NextFloat();
                    zObj.TreeObjectType = chance < Level2Ratio 
                        ? FloatingSpatialTree.TreeObjectType.Type1 
                        : FloatingSpatialTree.TreeObjectType.Type2;
                }
                
                var zObjMvr = newGO.GetComponent<ZObjectMover>();
                zObjMvr.UpperLimit = zom.UpperLimit;
                zObjMvr.LowerLimit = zom.LowerLimit;
                
                GeneratedGameObjects.Add(zObj);
            }
            
            var nc = NodeController.Singleton;
            nc.AddZObjects(GeneratedGameObjects);
        }

        private bool MovingObjectsInstantiated = false;
        public bool InstantiateMovingObjects = false;
        public void GenerateMovingObjects()
        {
            if (MovingObjectsInstantiated)
            {
                return;
            }

            MovingObjectsInstantiated = true;
            Vector3 min = -Limits;
            Vector3 minV = -VelLimits;
            for (int i = 0; i < MovingObjCount; i++)
            {
                float x = RandomExtension.GlobalRandom.Range(min.x, Limits.x);
                float y = RandomExtension.GlobalRandom.Range(min.y, Limits.y);
                float z = RandomExtension.GlobalRandom.Range(min.z, Limits.z);
                Vector3 position = new Vector3(x, y, z) + Offset;
                
                float vx = RandomExtension.GlobalRandom.Range(minV.x, VelLimits.x);
                float vy = RandomExtension.GlobalRandom.Range(minV.y, VelLimits.y);
                float vz = RandomExtension.GlobalRandom.Range(minV.z, VelLimits.z);
                Vector3 velocity = new Vector3(vx, vy, vz);

                var newGO = Instantiate(ZObjectPrefab, position, Quaternion.identity);
                var zObj = newGO.GetComponent<ZObject>();
                var zObjMvr = newGO.GetComponent<ZObjectMover>();
                var zObjGrabber = newGO.GetComponent<ZObjectNearbyGrabber>();
                zObjGrabber.LookForNearby = true;
                zObjMvr.Velocity = velocity;
                zObjMvr.UpperLimit = zom.UpperLimit;
                zObjMvr.LowerLimit = zom.LowerLimit;

                if (i == 0)
                {
                    zObj.TreeObjectType = FloatingSpatialTree.TreeObjectType.Type0;
                }
                else
                {
                    float chance = RandomExtension.GlobalRandom.NextFloat();
                    zObj.TreeObjectType = chance < Level2Ratio 
                        ? FloatingSpatialTree.TreeObjectType.Type1 
                        : FloatingSpatialTree.TreeObjectType.Type2;
                }
                
                
                zObj.DrawDebugLines = true;
                GeneratedGameObjects.Add(zObj);
                NearbyGrabbers.Add(zObjGrabber);
            }

            var nc = NodeController.Singleton;
            nc.AddZObjects(GeneratedGameObjects);
        }

        private List<ZObjectNearbyGrabber> NearbyGrabbers = new();
        public bool UpdateNearbyGrabbers = true;
        public bool UseJobs = false;
        public void NearbyGrabberUpdate()
        {
            if (UseJobs)
            {
                NativeList<JobHandle> jhList = new NativeList<JobHandle>(Allocator.Temp);
                List<NearbyGrabberJob> gnjList = new(NearbyGrabbers.Count);
                for (int i = 0; i < NearbyGrabbers.Count; i++)
                {
                    var NG = NearbyGrabbers[i];
                    if (NG.LookForNearby)
                    {
                        NearbyGrabberJob ngj = new();
                        ngj.nearby = new NativeList<int>(Allocator.TempJob);
                        ngj.distance = NG.threshold;
                        ngj.point = NG.transform.position;
                        ngj.id = i;
                        var jh = ngj.Schedule();
                        jhList.Add(jh);
                        gnjList.Add(ngj);
                    }
                }
                JobHandle.CompleteAll(jhList);

                foreach (var jh in gnjList)
                {
                    int id = jh.id;
                    int[] results = new int[jh.nearby.Length];
                    for (int i = 0; i < results.Length; i++)
                    {
                        results[i] = jh.nearby[i];
                    }
                    
                    jh.nearby.Dispose();
                    NearbyGrabbers[id].DrawToNearby(results);
                }
                jhList.Dispose();
            }
            else
            {
                for (int i = 0; i < NearbyGrabbers.Count; i++)
                {
                    if (NearbyGrabbers[i].LookForNearby)
                    {
                        NearbyGrabbers[i].DrawLinesToNearby();
                    }
                }
            }
        }
    }
    
    
    public struct NearbyGrabberJob : IJob
    {
        public int id;
        public Vector3 point;
        public float distance;
        public NativeList<int> nearby;
        public void Execute()
        {
            var nc = NodeController.Singleton;
            var result = nc.GetNearbyObjectIDs(point, distance);
            foreach (var id in result)
            {
                nearby.Add(id);
            }
        }
    }
}