using System.Collections;
using System.Collections.Generic;
using LongDistancePhysics;
using UnityEngine;

public class CircleDrawer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 center = transform.position;
        Vector3 dir = (target.transform.position - center).normalized;
        
        DebugLDP.DrawCircle(center, size, dir, Color.black);
        
    }

    public GameObject target;
    public float size = 5.0f;
}
