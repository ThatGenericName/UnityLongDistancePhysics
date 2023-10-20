using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


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
                LoadPhysicsScenes();
                CreatePrefab();
                LoadScenesBool = false;
            }
            
            
            foreach (var physicsScene in PhysicsScenes)
            {
                physicsScene.Item2.ResetInterpolationPoses();
                physicsScene.Item2.Simulate(Time.fixedDeltaTime);
            }
            AddForceToObjects();
            ProcessCollisionSynchronizationRequests();
        }

        public bool LoadScenesBool = false;

        void Awake()
        {
            
        }

        private bool scenesLoaded = false;

        void LoadPhysicsScenes()
        {
            if (scenesLoaded)
            {
                return;
            }
            LoadSceneParameters loadParams = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);
            Scene scene1 = SceneManager.LoadScene("CrossScene_1", loadParams);
            Scene scene2 = SceneManager.LoadScene("CrossScene_2", loadParams);
            
            PhysicsScenes.Add((scene2, scene2.GetPhysicsScene()));
            PhysicsScenes.Add((scene1, scene1.GetPhysicsScene()));
            scenesLoaded = true;
        }
        

        public GameObject testPhysicsObjectPrefab;

        public List<(Scene, PhysicsScene)> PhysicsScenes = new List<(Scene, PhysicsScene)>();

        private int physicsObjectTrack = 0;
        public Dictionary<int, List<CSI_PhysicsObject>> PhysicsObjects = new Dictionary<int, List<CSI_PhysicsObject>>();

        public void CreatePrefab()
        {
            CreatePrefab(Vector3.zero, Quaternion.identity);
        }
        
        public void CreatePrefab(Vector3 position, Quaternion rotation)
        {
            var objectList = new List<CSI_PhysicsObject>();
            PhysicsObjects[physicsObjectTrack] = objectList;
            for (int i = 0; i < PhysicsScenes.Count; i++)
            {
                var physicsScenePair = PhysicsScenes[i];
                var newObject = Instantiate(testPhysicsObjectPrefab, position, rotation);
                var newObjectPO = newObject.GetComponent<CSI_PhysicsObject>();
                newObjectPO.CSI_ObjectID = physicsObjectTrack;
                newObjectPO.CSI_ObjectSubID = i;
                objectList.Add(newObject.GetComponent<CSI_PhysicsObject>());
                SceneManager.MoveGameObjectToScene(newObject, physicsScenePair.Item1);
            }

            physicsObjectTrack++;
        }

        private Dictionary<int, CSI_CollisionSyncData[]> collisionData = new Dictionary<int, CSI_CollisionSyncData[]>();

        public void AddCollisionSynchronizationRequest(CSI_CollisionSyncData csiCollisionSyncData)
        {
            int objectId = csiCollisionSyncData.ObjectID;
            
            CSI_CollisionSyncData[] dataArray;
            if (!collisionData.ContainsKey(objectId))
            {
                dataArray = new CSI_CollisionSyncData[PhysicsObjects[objectId].Count];
                collisionData[objectId] = dataArray;
            }
            else
            {
                dataArray = collisionData[objectId];
            }

            dataArray[csiCollisionSyncData.ObjectSubID] = csiCollisionSyncData;
        }

        private void ProcessCollisionSynchronizationRequests()
        {
            foreach (var objectCollisionDataPair in collisionData)
            {
                int ObjectID = objectCollisionDataPair.Key;
                var collisionSyncDataArray = objectCollisionDataPair.Value;

                for (int subID = 0; subID < objectCollisionDataPair.Value.Length; subID++)
                {
                    var collisionSyncData = collisionSyncDataArray[subID];

                    if (collisionSyncData != null)
                    {
                        SynchronizeObject(ObjectID, subID);
                        break;
                    }
                }
            }
            collisionData.Clear();
        }

        private void SynchronizeObject(int ObjId, int ObjSubID)
        {
            var subObjects = PhysicsObjects[ObjId];
            var subObjectSyncHost = subObjects[ObjSubID];
            for (int i = 0; i < subObjects.Count; i++)
            {
                if (ObjSubID == i)
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
                    subObject.rb.AddForce(forceDataPair.Value, ForceMode.Impulse);
                }
            }
            
            ObjectForceQueue.Clear();
        }
    }
}

