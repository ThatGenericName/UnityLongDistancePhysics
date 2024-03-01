using System;
using Unity.Mathematics;
using UnityEngine;

namespace ObjectZoning
{
    public class ZObjectMover : MonoBehaviour
    {
        public Vector3 Velocity;

        public Vector3 UpperLimit;
        public Vector3 LowerLimit;

        private Rigidbody rb;
        private ZObject zobj;

        public bool run = true;
        private void Awake()
        {
            zobj = GetComponent<ZObject>();
            if (zobj != null)
            {
                rb = GetComponent<Rigidbody>();
                
                rb.velocity = Velocity;
            }
            else
            {
                run = false;
            }
        }
        
        private void FixedUpdate()
        {
            if (run)
            {
                rb.velocity = Velocity;
                MoveConstraint();
            }
        }

        private void MoveConstraint()
        {
            Vector3 pos = transform.position;
            Vector3 vel = Velocity;
            if (pos.x < LowerLimit.x)
            {
                pos.x += float.Epsilon;
                vel.x = math.abs(vel.x);
            }
            else if (pos.x > UpperLimit.x)
            {
                pos.x -= float.Epsilon;
                vel.x = -math.abs(vel.x);
            }
            
            if (pos.y < LowerLimit.y)
            {
                pos.y += float.Epsilon;
                vel.y = math.abs(vel.y);
            }
            else if (pos.y > UpperLimit.y)
            {
                pos.y -= float.Epsilon;
                vel.y = -math.abs(vel.y);
            }
            
            if (pos.z < LowerLimit.z)
            {
                pos.z += float.Epsilon;
                vel.z = math.abs(vel.z);
            }
            else if (pos.z > UpperLimit.z)
            {
                pos.z -= float.Epsilon;
                vel.z = -math.abs(vel.z);
            }

            // transform.position = pos;
            Velocity = vel;
        }
    }
}