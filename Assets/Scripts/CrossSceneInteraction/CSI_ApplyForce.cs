using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


namespace CrossSceneInteraction
{
    public class CSI_ApplyForce : MonoBehaviour
    {
        // Start is called before the first frame update

        [FormerlySerializedAs("velocityDirection")] public Vector3 force;
        public bool sendForce = false;
        private Rigidbody rb;
    
        void Start()
        {
            
        }

        private void Awake()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }

            if (cpo == null)
            {
                cpo = GetComponent<CSI_PhysicsObject>();
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnDrawGizmos()
        {
            Debug.DrawLine(transform.position, transform.position + force);
        }

        private CSI_PhysicsObject cpo;

        private void FixedUpdate()
        {
            if (sendForce)
            {
                sendForce = false;
                cpo.AddForce(force);
            }
        }
    }
}


