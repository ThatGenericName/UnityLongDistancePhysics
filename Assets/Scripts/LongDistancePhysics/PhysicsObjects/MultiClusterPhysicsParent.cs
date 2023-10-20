using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LongDistancePhysics.PhysicsObjects
{
    public class MultiClusterPhysicsParent : MonoBehaviour
    {

        public Rigidbody RigidBody;

        public List<MultiClusterPhysicsChild> ChildObjects;
        private void Awake()
        {
            if (RigidBody == null)
            {
                RigidBody = GetComponent<Rigidbody>();
            }
        }
        
        public float angularDrag
        {
            get => RigidBody.angularDrag;
            set
            {
                RigidBody.angularDrag = value;
                foreach (var childObject in ChildObjects)
                {
                    childObject.rb.angularDrag = value;
                }
            }
        }
        
        public Vector3 angularVelocity
        {
            get => RigidBody.angularVelocity;
            set
            {
                RigidBody.angularVelocity = value;
                foreach (var childObject in ChildObjects)
                {
                    childObject.rb.angularVelocity = value;
                }
            }
        }
        
        public Vector3 centerOfMass
        {
            get => RigidBody.centerOfMass;
            set
            {
                RigidBody.centerOfMass = value;
                foreach (var childObject in ChildObjects)
                {
                    childObject.rb.angularVelocity = value;
                }
            }
        }

        public bool detectCollisions
        {
            get => RigidBody.detectCollisions;
            set
            {
                RigidBody.detectCollisions = value;
                foreach (var childObject in ChildObjects)
                {
                    childObject.rb.detectCollisions = value;
                }
            }
        }

        public bool isKinematic
        {
            get => RigidBody.isKinematic;
            set
            {
                RigidBody.isKinematic = value;
                foreach (var childObject in ChildObjects)
                {
                    childObject.rb.isKinematic = value;
                }
            }
        }

        public float mass
        {
            get => RigidBody.mass;
            set
            {
                RigidBody.mass = value;
                for (int i = 0; i < ChildObjects.Count; i++)
                {
                    ChildObjects[i].rb.mass = value;
                }
            }
        }
        
        public Vector3 velocity
        {
            get => RigidBody.velocity;
            set
            {
                RigidBody.velocity = value;
                for (int i = 0; i < ChildObjects.Count; i++)
                {
                    ChildObjects[i].rb.velocity = value;
                }
            }
        }
        
        private SortedDictionary<int, (float, Vector3, float, float, ForceMode)> incomingExplosionForces =
            new SortedDictionary<int, (float, Vector3, float, float, ForceMode)>();

        public void EnqueueExplosionForce(
            int source,
            float explosionForce, 
            Vector3 explosionPosition, 
            float explosionRadius, 
            float upwardsModifier = 0.0f, 
            ForceMode mode = ForceMode.Force)
        {
            incomingExplosionForces[source] = (
                explosionForce,
                explosionPosition,
                explosionRadius,
                upwardsModifier,
                mode
            );
        }

        private void ProcessQueuedExplosionForce()
        {
            foreach (var dataTuple in incomingExplosionForces)
            {
                (
                    var explosionForce,
                    var explosionPosition,
                    var explosionRadius,
                    var upwardsModifier,
                    var mode
                ) = dataTuple.Value;
                
                RigidBody.AddExplosionForce(
                    explosionForce, 
                    explosionPosition, 
                    explosionRadius, 
                    upwardsModifier, 
                    mode
                );

                foreach (var childObject in ChildObjects)
                {
                    childObject.rb.AddExplosionForce(
                        explosionForce, 
                        explosionPosition, 
                        explosionRadius, 
                        upwardsModifier, 
                        mode
                    );
                }
            }
            incomingExplosionForces.Clear();
        }


        private SortedDictionary<int, (Vector3, ForceMode)> incomingForces =
            new SortedDictionary<int, (Vector3, ForceMode)>();
        
        // We will be operating under the assumption that a single source
        // will only ever apply one type of force per tick.
        public void EnqueueForce(int source, Vector3 force, ForceMode mode = ForceMode.Force)
        {
            incomingForces[source] = (force, mode);
        }

        private void ProcessQueuedForce()
        {
            foreach (var incomingForceData in incomingForces)
            {
                int id = incomingForceData.Key;
                (Vector3 force, ForceMode mode) = incomingForceData.Value;
                
                RigidBody.AddForce(force, mode);

                foreach (var childObject in ChildObjects)
                {
                    childObject.rb.AddForce(force, mode);
                }
            }
            incomingForces.Clear();
        }

        private SortedDictionary<int, (Vector3, ForceMode)> incomingRelativeForces =
            new SortedDictionary<int, (Vector3, ForceMode)>();
        
        public void EnqueueRelativeForce(int source, Vector3 force, ForceMode mode = ForceMode.Force)
        {
            incomingRelativeForces[source] = (force, mode);
        }
        
        private void ProcessQueuedRelativeForce()
        {
            foreach (var incomingForceData in incomingRelativeForces)
            {
                int id = incomingForceData.Key;
                (Vector3 force, ForceMode mode) = incomingForceData.Value;
                
                RigidBody.AddRelativeForce(force, mode);

                foreach (var childObject in ChildObjects)
                {
                    childObject.rb.AddRelativeForce(force, mode);
                }
            }
            incomingRelativeForces.Clear();
        }
        
        private SortedDictionary<int, (Vector3, Vector3, ForceMode)> incomingForceAtRelPos =
            new SortedDictionary<int, (Vector3, Vector3, ForceMode)>();

        public void EnqueueForceAtRelativePosition(int source, Vector3 force, Vector3 position,
            ForceMode mode = ForceMode.Force)
        {
            incomingForceAtRelPos[source] = (force, position, mode);
        }

        private void ProcessQueuedForceAtRelativePosition()
        {
            foreach (var incomingForceData in incomingForceAtRelPos)
            {
                int id = incomingForceData.Key;
                (Vector3 force, Vector3 position, ForceMode mode) = incomingForceData.Value;

                Vector3 worldPosToParent = transform.InverseTransformPoint(position);
                RigidBody.AddForceAtPosition(force, worldPosToParent, mode);
                
                foreach (var childObject in ChildObjects)
                {
                    var worldPosToChild = childObject.transform.InverseTransformPoint(position);
                    childObject.rb.AddForceAtPosition(force, worldPosToChild, mode);
                }
            }
            incomingForceAtRelPos.Clear();
        }
    }
}