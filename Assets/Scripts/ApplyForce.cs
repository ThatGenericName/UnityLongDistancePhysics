using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ApplyForce : MonoBehaviour
{
    // Start is called before the first frame update

    public Vector3 velocityDirection;
    public bool sendForce = false;
    private Rigidbody rb;
    private MultizonePhysicsWrapper mpw;
    
    void Start()
    {
        
    }

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (mpw == null)
        {
            mpw = GetComponent<MultizonePhysicsWrapper>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position, transform.position + velocityDirection);
    }

    private void FixedUpdate()
    {
        if (sendForce)
        {
            sendForce = false;
            mpw.velocity += velocityDirection;
        }
    }
}
