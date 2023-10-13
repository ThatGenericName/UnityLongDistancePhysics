using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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

        SychronizePhysicsState();
    }

    private void OnCollisionStay(Collision other)
    {
        SychronizePhysicsState();
    }

    private void OnCollisionExit(Collision other)
    {
        SychronizePhysicsState();
    }

    public void SychronizePhysicsState()
    {
        var currentPos = transform.position;
        var currentRot = transform.rotation;
        var currentAngVel = rb.angularVelocity;
        var currentVel = rb.velocity;
        
        foreach (var linkedPhysicsObject in LinkedPhysicsObjects)
        {
            linkedPhysicsObject.velocity = currentVel;
            linkedPhysicsObject.angularVelocity = currentAngVel;
            // test code, we get rid of z values.


            var newZ = linkedPhysicsObject.transform.position.z;
            var k = currentPos;
            k.z = newZ;

            linkedPhysicsObject.transform.position = k;
            linkedPhysicsObject.transform.rotation = currentRot;

        }
    }
    
    
    public void AddForceAtRelPosition(Vector3 force, Vector3 position)
    {
        var worldPos = transform.TransformPoint(position);
        var worldDir = transform.TransformDirection(force);
        
        rb.AddForceAtPosition(worldDir, worldPos);

        foreach (var linkedPhysicsObject in LinkedPhysicsObjects)
        {
            linkedPhysicsObject.AddForceAtRelPosition(force, position);
        }
    }

    public Vector3 velocity
    {
        get => rb.velocity;
        set => SetVelocity(value);
    }
    
    private void SetVelocity(Vector3 velocity)
    {
        rb.velocity = velocity;

        foreach (var linkedObject in LinkedPhysicsObjects)
        {
            linkedObject.velocity = velocity;
        }
    }
}
