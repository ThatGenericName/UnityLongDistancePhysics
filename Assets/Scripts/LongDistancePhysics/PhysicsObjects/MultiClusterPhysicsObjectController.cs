using System.Collections.Generic;


using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace LongDistancePhysics.PhysicsObjects
{
    public class MultiClusterPhysicsObjectController : MonoBehaviour
    {

        public static MultiClusterPhysicsObjectController Singleton;

        public Dictionary<int, List<MultiClusterPhysicsParent>> Objects;
        
        private void FixedUpdate()
        {

        }

    }
}