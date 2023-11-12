

using System.Collections.Generic;
using LongDistancePhysics.Physics;
using UnityEngine;

namespace LongDistancePhysics.FloatingOrigin
{
    public class LocalFloatingOriginController : MonoBehaviour
    {

        public List<PhysicsObject> PhysicsObjects;

        public Vector3m ZoneCenter { get; private set; }
        public Vector3m ZoneVelocity { get; private set; }
    }
}