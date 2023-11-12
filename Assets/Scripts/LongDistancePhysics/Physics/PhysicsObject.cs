using System;
using System.Collections;
using System.Collections.Generic;
using LongDistancePhysics.Physics.CrossSceneInteraction;
using UnityEngine;


namespace LongDistancePhysics.Physics
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(LDPTransform))]
    public class PhysicsObject : MonoBehaviour
    {
        // Start is called before the first frame update

        public int ObjectID;
        public int ObjectSubID;
        
        public LDPTransform LDPTransform { get; private set; }
        void Start()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }

            LDPTransform = GetComponent<LDPTransform>();
        }

        // Update is called once per frame
        
        void Update()
        {
            
        }
        
        private Vector3m LastVelocity;
        private Vector3 LastAngularVelocity;
        private Vector3m LastPosition;

        private void FixedUpdate()
        {

        }

        public void UpdateLastState()
        {

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
            var otherCPO = other.gameObject.GetComponent<PhysicsObject>();
            int otherCID = -1;
            if (otherCPO != null)
            {
                otherCID = otherCPO.ObjectID;
            }
            
            CollisionSyncData data = new CollisionSyncData()
            {
                ObjectID = this.ObjectID,
                ObjectSubID = this.ObjectSubID,
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
            CrossSceneInteractionController.Singleton.AddForce(ObjectID, force);
        }
    }
}

