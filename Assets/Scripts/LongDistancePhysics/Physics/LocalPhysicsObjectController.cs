using System;
using System.Collections.Generic;
using LongDistancePhysics.FloatingOrigin;
using UnityEngine;

namespace LongDistancePhysics.Physics
{
    [RequireComponent(typeof(LocalFloatingOriginController))]
    public class LocalPhysicsObjectController : MonoBehaviour
    {
        public Dictionary<int, PhysicsObject> LocalObjects = new();

        private LocalFloatingOriginController _localFloatingOriginController;

        private void Awake()
        {
            _localFloatingOriginController = GetComponent<LocalFloatingOriginController>();
        }

        private void FixedUpdate()
        {
            
        }

        /// <summary>
        /// Updates the transform of all local objects.
        /// </summary>
        private void UpdateObjectPositions()
        {
            Vector3m velocityFactor = Time.fixedDeltaTime * _localFloatingOriginController.ZoneVelocity;
            foreach (var pObjPair in LocalObjects)
            {
                var objTransform = pObjPair.Value.transform;
                var ldpTransform = pObjPair.Value.LDPTransform;

                var lastPos = ldpTransform.lastPosition;
                ldpTransform.lastPosition = objTransform.position;
                var delta = objTransform.position - lastPos;
                ldpTransform.Position += delta;
            }
        }
    }
}