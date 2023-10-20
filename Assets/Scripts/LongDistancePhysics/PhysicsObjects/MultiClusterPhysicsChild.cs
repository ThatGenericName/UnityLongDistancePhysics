

using UnityEngine;

namespace LongDistancePhysics.PhysicsObjects
{
    public class MultiClusterPhysicsChild : MonoBehaviour
    {
        public Rigidbody rb;

        public MultiClusterPhysicsParent parent;

        private void Awake()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }
        }
        
        
        
    }
}