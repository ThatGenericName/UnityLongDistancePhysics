using System;
using LongDistancePhysics.FloatingOrigin;
using UnityEngine;

namespace LongDistancePhysics.Physics
{
    public class LDPTransform : MonoBehaviour
    {
        public Transform objectTransform;

        public LDPTransform parentTransform;

        public LocalPhysicsObjectController localController;
        private LocalFloatingOriginController localFloatingOriginController;
        
        public Vector3m Position;

        public Vector3m LocalPosition
        {
            get => getLocalPosition();
        }

        public Vector3 lastPosition;
        
        private Vector3m getLocalPosition()
        {
            if (parentTransform == null)
            {
                return Vector3m.zero;
            }

            Vector3m offset = parentTransform.Position;
            return Position - offset;
        }

        /// <summary>
        /// Moves the object via it's global position.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="relative"></param>
        private void Move(Vector3m value, bool relative)
        {
            if (relative)
            {
                Vector3 delta = value;
                Position += value;
                transform.position += delta;
                lastPosition += delta;
            }
            else
            {
                Vector3 zoneOffset = localFloatingOriginController.ZoneCenter - value;
                Vector3 delta = zoneOffset - lastPosition;
                transform.position = zoneOffset;
                lastPosition += delta;
            }
        }

        /// <summary>
        /// Moves the object's base transform without moving the LDP Transform
        /// for functions such as Floating Origin.
        /// 
        /// if <c>relative</c> is true, value is added to the object's position,
        /// otherwise the object's transform is set to the value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="relative"></param>
        public void MoveExtern(Vector3 value, bool relative)
        {
            if (relative)
            {
                transform.position += value;
                lastPosition += value;
            }
            else
            {
                Vector3 delta = value - transform.position;
                lastPosition += delta;
            }
        }
    }
}