using UnityEngine;

namespace LongDistancePhysics.Physics.CrossSceneInteraction
{
    public class CollisionSyncData
    {
        public int ObjectID;
        public int ObjectSubID;
        public int OtherObjectID;

        public Vector3 PreVelocity;
        public Vector3 PostVelocity;
        public Vector3 DeltaVelocity => PostVelocity - PreVelocity;
        
        public Vector3 PreAngularVelocity;
        public Vector3 PostAngularVelocity;
        public Vector3 DeltaAngularVelocity => PostAngularVelocity - PreAngularVelocity;
        
        public Vector3 PrePosition;
        public Vector3 PostPosition;
        public Vector3 DeltaPosition => PostPosition - PrePosition;
    }
}