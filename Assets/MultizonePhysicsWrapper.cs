using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultizonePhysicsWrapper : MonoBehaviour
{
    private MultizonePhysicsParent parent;
    
    public Vector3 velocity
    {
        get => parent.rb.velocity;
        set => parent.velocity = value;
    }
    
    
    public void Awake()
    {
        if (parent == null)
        {
            parent = GetComponent<MultizonePhysicsParent>();
            if (parent == null)
            {
                var physicsLink = GetComponent<MultizonePhysicsChild>();
                parent = physicsLink.ParentController;
            }
        }
    }

    private void SetPhysics()
    {
        
    }

    public void AddForceAtPosition(Vector3 force, Vector3 position)
    {
        var relPos = transform.InverseTransformPoint(position);
        var relForce = transform.InverseTransformDirection(force);

        parent.AddForceAtRelPosition(relPos, relForce);
    }
}
