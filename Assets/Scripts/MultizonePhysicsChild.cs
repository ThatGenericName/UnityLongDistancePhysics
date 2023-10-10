using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultizonePhysicsChild : MonoBehaviour
{
    // Start is called before the first frame update
    
    public Rigidbody rb;

    public MultizonePhysicsParent ParentController;
    
    void Start()
    {
        
    }
    
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
}
