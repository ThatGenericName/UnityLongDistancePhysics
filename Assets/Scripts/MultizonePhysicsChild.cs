using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultizonePhysicsChild : MonoBehaviour
{
    // Start is called before the first frame update
    
    public Rigidbody rb;

    public MultizonePhysicsParent ParentController;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var (pos, impulse) in AppliedForces)
        {
            Debug.DrawRay(pos, impulse, Color.blue);
        }
    }

    private List<(Vector3, Vector3)> AppliedForces = new List<(Vector3, Vector3)>();

    public void ApplyForce(Vector3 impulse, Vector3 localPos)
    {
        Vector3 pos = transform.TransformPoint(localPos);

        rb.AddForceAtPosition(impulse, pos, ForceMode.Impulse);
        AppliedForces.Add((pos, impulse));
    }

    public Vector3 velocity
    {
        get => rb.velocity;
        set => rb.velocity = value;
    }


    public Vector3 angularVelocity
    {
        get => rb.angularVelocity;
        set => rb.angularVelocity = value;
    }
    
    public void AddForceAtRelPosition(Vector3 force, Vector3 position)
    {
        var worldPos = transform.TransformPoint(position);
        var worldDir = transform.TransformDirection(force);
        
        rb.AddForceAtPosition(worldDir, worldPos);
    }
}
