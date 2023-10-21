using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;


namespace CrossSceneInteraction
{
    public class CrossSceneInteractionController : MonoBehaviour
    {
        // Start is called before the first frame update


        public static CrossSceneInteractionController Singleton;
        
        void Start()
        {
            if (Singleton == null)
            {
                Singleton = this;
            }
            else
            {
                throw new ApplicationException("Why Make Multiple Singletons?");
            }
        }

        private void OnDestroy()
        {
            // Singleton = null;
        }

        // Update is called once per frame
        void Update()
        {
            foreach (var physicsScene in PhysicsScenes)
            {
                // physicsScene.Item2.InterpolateBodies();
            }
            // to deal with a weird bug.

            if (recheckCounter++ > 200)
            {
                if (Singleton == null)
                {
                    Singleton = this;
                }

                recheckCounter = 0;
            }
        }

        private int recheckCounter = 0;
        private int recheckMax = 200;
        
        private void FixedUpdate()
        {
            if (LoadScenesBool)
            {
                LoadDemoPhysicsScenes();
                CreateDemoPrefabs();
                LoadScenesBool = false;
            }

            // Update Physics Scenes.
            foreach (var scenePair in PhysicsScenes)
            {
                scenePair.Value.Item2.ResetInterpolationPoses();
                scenePair.Value.Item2.Simulate(Time.fixedDeltaTime);
            }
            
            AddForceToObjects();
            ProcessCollisionSynchronizationRequests();
        }

        public bool LoadScenesBool = false;

        void Awake()
        {
            
        }

        private bool scenesLoaded = false;

        void LoadDemoPhysicsScenes()
        {
            if (scenesLoaded)
            {
                return;
            }
            LoadSceneParameters loadParams = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);
            Scene scene1 = SceneManager.LoadScene("CrossScene_1", loadParams);
            Scene scene2 = SceneManager.LoadScene("CrossScene_2", loadParams);
            
            PhysicsScenes[physicsScenesIDTrack++] = (scene2, scene2.GetPhysicsScene());
            PhysicsScenes[physicsScenesIDTrack++] = (scene1, scene1.GetPhysicsScene());
            scenesLoaded = true;
        }
        

        public GameObject testPhysicsObjectPrefab;

        private int physicsScenesIDTrack = 0;
        public Dictionary<int, (Scene, PhysicsScene)> PhysicsScenes = new Dictionary<int, (Scene, PhysicsScene)>();

        private int physicsObjectTrack = 0;
        public readonly Dictionary<int, Dictionary<int, CSI_PhysicsObject>> PhysicsObjects = new Dictionary<int, Dictionary<int, CSI_PhysicsObject>>();

        public void CreateDemoPrefabs()
        {
            CreateDemoPrefabs(Vector3.zero, Quaternion.identity);
        }
        
        public void CreateDemoPrefabs(Vector3 position, Quaternion rotation)
        {
            var objectList = new Dictionary<int, CSI_PhysicsObject>();
            PhysicsObjects[physicsObjectTrack] = objectList;
            for (int i = 0; i < PhysicsScenes.Count; i++)
            {
                var physicsScenePair = PhysicsScenes[i];
                var newObject = Instantiate(testPhysicsObjectPrefab, position, rotation);
                var newObjectPO = newObject.GetComponent<CSI_PhysicsObject>();
                newObjectPO.CSI_ObjectID = physicsObjectTrack;
                newObjectPO.CSI_ObjectSubID = i;
                objectList.Add(i, newObject.GetComponent<CSI_PhysicsObject>());
                SceneManager.MoveGameObjectToScene(newObject, physicsScenePair.Item1);
            }

            physicsObjectTrack++;
        }

        private Dictionary<int, Dictionary<int, CSI_CollisionSyncData>> collisionSyncRequestData = new();
        // structured as such ObjectID > CollisionOtherID > Data

        public void AddCollisionSynchronizationRequest(CSI_CollisionSyncData csiCollisionSyncData)
        {
            int objectID = csiCollisionSyncData.ObjectID;
            int otherID = csiCollisionSyncData.OtherObjectID;
            
            Dictionary<int, CSI_CollisionSyncData> objectSyncData;
            if (!collisionSyncRequestData.ContainsKey(objectID))
            {
                objectSyncData = new Dictionary<int, CSI_CollisionSyncData>();
                collisionSyncRequestData[objectID] = objectSyncData;
            }
            else
            {
                objectSyncData = collisionSyncRequestData[objectID];
            }
            
            if (!objectSyncData.ContainsKey(otherID))
            {
                objectSyncData[otherID] = csiCollisionSyncData;
            }
        }

        private void ProcessCollisionSynchronizationRequests()
        {
            foreach (var objectRequest in collisionSyncRequestData)
            {
                var dataPairs = objectRequest.Value.ToArray();
                
                if (dataPairs.Length == 1)
                {
                    var syncData = objectRequest.Value.Values.Single();
                    int objectID = syncData.ObjectID;
                    int subObjectID = syncData.ObjectSubID;
                    
                    SynchronizeObjectsToSubObject(objectID, subObjectID);
                }
                else
                {
                    Vector3 deltaVelocityResult = Vector3.zero;
                    Vector3 deltaAngularVelocityResult = Vector3.zero;
                    Vector3 deltaPositionResult = Vector3.zero;

                    int count = 0;

                    var tempObj = dataPairs[0].Value;
                    
                    Vector3 originalVelocity = tempObj.PreVelocity;
                    Vector3 originalAngularVelocity = tempObj.PreAngularVelocity;
                    Vector3 originalPosition = tempObj.PrePosition;
                    
                    foreach (var collisionSyncData in dataPairs)
                    {
                        count++;
                        var data = collisionSyncData.Value;
                        deltaVelocityResult += data.DeltaVelocity;
                        deltaAngularVelocityResult += data.DeltaAngularVelocity;
                        deltaPositionResult += data.DeltaPosition;
                    }

                    deltaPositionResult /= count;
                    deltaVelocityResult /= count;
                    deltaAngularVelocityResult /= count;

                    Vector3 newPosition = originalPosition + deltaPositionResult;
                    Vector3 newVelocity = originalVelocity + deltaVelocityResult;
                    Vector3 newAngularVelocity = originalAngularVelocity + deltaAngularVelocityResult;
                    
                    var subObjects = PhysicsObjects[objectRequest.Key];
                    for (int i = 0; i < subObjects.Count; i++)
                    {
                        var subObject = subObjects[i];
                        subObject.transform.position = newPosition;
                        subObject.rb.velocity = newVelocity;
                        subObject.rb.angularVelocity = newAngularVelocity;
                    }
                }
            }
            
            collisionSyncRequestData.Clear();
        }

        private void SynchronizeObjectsToSubObject(int ObjId, int HostSubID)
        {
            var subObjects = PhysicsObjects[ObjId];
            var subObjectSyncHost = subObjects[HostSubID];
            for (int i = 0; i < subObjects.Count; i++)
            {
                if (HostSubID == i)
                {
                    continue;
                }
                
                var subObject = subObjects[i];
                subObject.transform.position = subObjectSyncHost.transform.position;
                subObject.rb.velocity = subObjectSyncHost.rb.velocity;
                subObject.rb.angularVelocity = subObjectSyncHost.rb.angularVelocity;
            }
        }
        
        private Dictionary<int, Vector3> ObjectForceQueue = new Dictionary<int, Vector3>();
        
        public void AddForce(int ObjID, Vector3 force)
        {
            if (!ObjectForceQueue.ContainsKey(ObjID))
            {
                ObjectForceQueue[ObjID] = force;
            }
            else
            {
                ObjectForceQueue[ObjID] += force;
            }
        }

        private void AddForceToObjects()
        {
            foreach (var forceDataPair in ObjectForceQueue)
            {
                var physicsObjectList = PhysicsObjects[forceDataPair.Key];

                foreach (var subObject in physicsObjectList)
                {
                    subObject.Value.rb.AddForce(forceDataPair.Value, ForceMode.Impulse);
                }
            }
            
            ObjectForceQueue.Clear();
        }
    }
}

