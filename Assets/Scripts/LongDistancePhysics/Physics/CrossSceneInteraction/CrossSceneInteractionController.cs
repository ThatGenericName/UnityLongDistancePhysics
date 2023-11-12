using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


namespace LongDistancePhysics.Physics.CrossSceneInteraction
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
            foreach (var physicsScenePair in PhysicsScenes)
            {
                physicsScenePair.Value.Item2.InterpolateBodies();
            }
            // to deal with a weird bug.

            if (recheckCounter++ > recheckMax)
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

            // Update Physics Scenes.
            foreach (var scenePair in PhysicsScenes)
            {
                scenePair.Value.Item2.ResetInterpolationPoses();
                scenePair.Value.Item2.Simulate(Time.fixedDeltaTime);
            }
            
            AddForceToObjects();
            ProcessCollisionSynchronizationRequests();
        }

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


        public void CreatePhysicsScene()
        {
            int nextId = physicsScenesIDTrack++;
            LoadSceneParameters loadParams = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);
            Scene scene = SceneManager.LoadScene("CrossSceneEmpty", loadParams);
            PhysicsScenes[nextId] = (scene, scene.GetPhysicsScene());

            foreach (var pObjDataKV in PhysicsObjects)
            {
                CopyPhysicsObjectToScene(pObjDataKV.Key, nextId);
            }
        }
        

        public GameObject testPhysicsObjectPrefab;

        private int physicsScenesIDTrack = 0;
        public Dictionary<int, (Scene, PhysicsScene)> PhysicsScenes = new Dictionary<int, (Scene, PhysicsScene)>();

        private int physicsObjectTrack = 0;
        public readonly Dictionary<int, Dictionary<int, PhysicsObject>> PhysicsObjects = new Dictionary<int, Dictionary<int, PhysicsObject>>();
        

        public void CopyPhysicsObjectToScene(int csObjectID, int sceneID)
        {
            if (!PhysicsObjects.ContainsKey(csObjectID) || !PhysicsScenes.ContainsKey(sceneID))
            {
                return;
            }

            var objectList = PhysicsObjects[csObjectID];
            int newSubID = objectList.Keys.Max() + 1;
            
            var physicsScenePair = PhysicsScenes[sceneID];

            var gameObjectToClone = objectList[newSubID - 1];
            var baseObjectTransform = gameObjectToClone.transform;
            var newObject = Instantiate(gameObjectToClone.gameObject, baseObjectTransform.position, baseObjectTransform.rotation);
            var newObjectPO = newObject.GetComponent<PhysicsObject>();
            newObjectPO.ObjectID = csObjectID;
            newObjectPO.ObjectSubID = newSubID;
            
            SceneManager.MoveGameObjectToScene(newObject, physicsScenePair.Item1);
            
        }
        
        
        public void InstantiatePrefab(GameObject gameObject, Vector3 position, Quaternion rotation)
        {
            int newObjectID = physicsObjectTrack++;
            var objectList = new Dictionary<int, PhysicsObject>();
            PhysicsObjects[newObjectID] = objectList;

            int subObjectID = 0;

            foreach (var sceneID in PhysicsScenes.Keys)
            {
                InstantiatePrefabSingleScene(gameObject, position, rotation, sceneID, newObjectID, subObjectID++);
            }
        }

        public bool InstantiatePrefabSingleScene(GameObject gameObject, Vector3 position, Quaternion rotation, 
            int sceneID, int physicsObjectID = -1, int subObjectID = 0)
        {
            if (!PhysicsScenes.ContainsKey(sceneID))
            {
                return false;
            }

            if (physicsObjectID == -1)
            {
                physicsObjectID = physicsObjectTrack++;
            }
            var objectList = new Dictionary<int, PhysicsObject>();
            PhysicsObjects[physicsObjectID] = objectList;
            var physicsScenePair = PhysicsScenes[sceneID];
            var newObject = Instantiate(gameObject, position, rotation);
            var newObjectPO = newObject.GetComponent<PhysicsObject>();
            newObjectPO.ObjectID = physicsObjectID;
            newObjectPO.ObjectSubID = subObjectID;
            objectList.Add(subObjectID, newObject.GetComponent<PhysicsObject>());
            SceneManager.MoveGameObjectToScene(newObject, physicsScenePair.Item1);

            return true;
        }
        

        private Dictionary<int, Dictionary<int, CollisionSyncData>> collisionSyncRequestData = new();
        // structured as such ObjectID > CollisionOtherID > Data

        public void AddCollisionSynchronizationRequest(CollisionSyncData csiCollisionSyncData)
        {
            int objectID = csiCollisionSyncData.ObjectID;
            int otherID = csiCollisionSyncData.OtherObjectID;
            
            Dictionary<int, CollisionSyncData> objectSyncData;
            if (!collisionSyncRequestData.ContainsKey(objectID))
            {
                objectSyncData = new Dictionary<int, CollisionSyncData>();
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

            foreach (var subObjectPair in subObjects)
            {
                if (subObjectPair.Key == HostSubID)
                {
                    continue;
                }
                var subObject = subObjectPair.Value;
                
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

