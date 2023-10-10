using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultizonePhysicsParent : MonoBehaviour
{
    // Start is called before the first frame update

    public Rigidbody rb;

    public List<MultizonePhysicsChild> LinkedPhysicsObjects = new List<MultizonePhysicsChild>();
    
    void Start()
    {
        
    }

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Collision collision in ContactPoints)
        {
            ContactPoint[] contactPts = new ContactPoint[collision.contacts.Length];
            var contacts = collision.GetContacts(contactPts);

            foreach (ContactPoint contact in contactPts)
            {
                Debug.DrawRay(contact.point, contact.normal, Color.red);
                Debug.DrawRay(contact.point, collision.impulse, Color.blue);
            }
        }
    }

    private List<Collision> ContactPoints = new List<Collision>();

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint[] contactPts = new ContactPoint[collision.contacts.Length];
        var contactCount = collision.GetContacts(contactPts);
        ContactPoints.Add(collision);

        Vector3 pt = Vector3.zero;
        Vector3 norm = Vector3.zero;
        
        foreach (ContactPoint contact in contactPts)
        {
            // compute the average contact point, and then
            // compute the normal of these contact points.

            pt += contact.point;
            norm += contact.normal;

        }
        
        
        pt /= contactCount;
        norm.Normalize();
        
        float magnitude = collision.impulse.magnitude;
        
        Vector3 newPos = transform.InverseTransformPoint(pt);
        
        foreach (var child in LinkedPhysicsObjects)
        {
            
            Vector3 forceWithDir = magnitude * norm ;
            
            child.ApplyForce(forceWithDir, newPos);
        }
        
        int k = 45;
    }
}
