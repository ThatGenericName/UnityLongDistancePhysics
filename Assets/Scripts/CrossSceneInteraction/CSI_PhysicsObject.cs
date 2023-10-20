using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CrossSceneInteraction
{
    public class CSI_PhysicsObject : MonoBehaviour
    {
        // Start is called before the first frame update

        public int CSI_ObjectID;
        public int CSI_ObjectSubID;
        void Start()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnCollisionEnter(Collision other)
        {
            RequestCollisionSync();
        }

        private void OnCollisionExit(Collision other)
        {
            RequestCollisionSync();
        }

        private void OnCollisionStay(Collision other)
        {
            RequestCollisionSync();
        }

        private void RequestCollisionSync()
        {
            CSI_CollisionSyncData data = new CSI_CollisionSyncData()
            {
                ObjectID = this.CSI_ObjectID,
                ObjectSubID = this.CSI_ObjectSubID,
                OtherObjectID = 0
            };
            
            CrossSceneInteractionController.Singleton.AddCollisionSynchronizationRequest(data);
        }

        public Rigidbody rb;

        public void AddForce(Vector3 force)
        {
            CrossSceneInteractionController.Singleton.AddForce(CSI_ObjectID, force);
        }
    }
} 

