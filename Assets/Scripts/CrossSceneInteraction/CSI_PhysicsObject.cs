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
        
        private Vector3 LastVelocity;
        private Vector3 LastAngularVelocity;
        private Vector3 LastPosition;

        private void FixedUpdate()
        {
            LastPosition = transform.position;
            LastVelocity = rb.velocity;
            LastAngularVelocity = rb.angularVelocity;
        }


        private void OnCollisionEnter(Collision other)
        {
            RequestCollisionSync(other);
        }

        private void OnCollisionExit(Collision other)
        {
            RequestCollisionSync(other);
        }

        private void OnCollisionStay(Collision other)
        {

            RequestCollisionSync(other);
        }

        public bool TempObject = false; //TODO: Remove this flag.
        private void RequestCollisionSync(Collision other)
        {
            if (TempObject)
            {
                return;
            }
            var otherCPO = other.gameObject.GetComponent<CSI_PhysicsObject>();
            int otherCID = -1;
            if (otherCPO != null)
            {
                otherCID = otherCPO.CSI_ObjectID;
            }
            
            CSI_CollisionSyncData data = new CSI_CollisionSyncData()
            {
                ObjectID = this.CSI_ObjectID,
                ObjectSubID = this.CSI_ObjectSubID,
                OtherObjectID = otherCID,
                PrePosition = LastPosition,
                PostPosition = transform.position,
                PreAngularVelocity = LastAngularVelocity,
                PostAngularVelocity = rb.angularVelocity,
                PreVelocity = LastVelocity,
                PostVelocity = rb.velocity
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

