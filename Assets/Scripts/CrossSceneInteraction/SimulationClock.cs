using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationClock : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(transform.position, transform.position + offset);
    }


    public Vector3 offset;
    public int period = 100;
    public int ctr = 0;
    private void FixedUpdate()
    {
        if (ctr >= 100)
        {
            ctr = 0;
        }
        var percent = (double)ctr / (double)period;

        var k = percent * Math.PI * 2;

        offset = new Vector3(2 * (float)Math.Cos(k), 2 * (float)Math.Sin(k));

        ctr++;
    }
}
