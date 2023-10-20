using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class WanderingObject : MonoBehaviour
{
    // Start is called before the first frame update


    public float maxSpeed = 0.0f;
    public float minSpeed = 0.0f;
    public float maxDeflection = 0.0f;
    
    public int movementMode = 0; // 0 = straight line, 1 = full random, 2 = periodic random

    public float speedPeriod = 0.0f;
    private float speedCurrentCycle = 0.0f;
    public float directionPeriod = 0.0f;
    private float directionCurrentCycle = 0.0f;

    public Rigidbody rb = null;
    
    public bool AllowCentering = false;
    public bool ForcedCentering = false;

    public List<Zone> CurrentZones = new List<Zone>();
    public List<LineRenderer> ZoneLineRenderers = new List<LineRenderer>();

    public (int, int) CurrentChunk = (0, 0);
    public List<(int, int)> AssignedChunks;
    
    public Vector3 NextCyclePosition;
    
    void Start()
    {
        
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
    }


    // Update is called once per frame
    void Update()
    {
        DrawLineToHCP();
        DrawLineToCenter();
        SetTypeColour();
    }

    public LineRenderer LineToHCP = null;
    public LineRenderer LineToObjType = null;
    public LineRenderer LineToRing = null;
    
    void DrawLineToHCP()
    {
        LineToHCP.SetPosition(0, transform.position);
        LineToHCP.SetPosition(1, NextCyclePosition);
    }

    
    public GameObject LineToZoneCenterPrefab = null;
    
    void DrawLineToCenter()
    {
        for (int i = 0; i < ZoneLineRenderers.Count; i++)
        {
            ZoneLineRenderers[i].gameObject.SetActive(false);
        }

        List<LineRenderer> lineRenderersList = new List<LineRenderer>(ZoneLineRenderers);
        ZoneLineRenderers.Clear();
        
        for (int i = 0; i < CurrentZones.Count; i++)
        {
            var zone = CurrentZones[i];
            
            if (lineRenderersList.Count <= i)
            {
                GameObject lRenderObject = (GameObject)Instantiate(LineToZoneCenterPrefab, transform, instantiateInWorldSpace:false);
                
                lineRenderersList.Add(lRenderObject.GetComponent<LineRenderer>());
                lRenderObject.SetActive(false);
            }
            
            LineRenderer lineRenderer = lineRenderersList[i];
            
            if (zone.Center != this)
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, zone.Center.transform.position);
                lineRenderer.gameObject.SetActive(true);
                continue;
            }
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position);
        }
        
        ZoneLineRenderers.AddRange(lineRenderersList);
    }

    public void SetTypeColour()
    {
        if (ForcedCentering)
        {
            LineToObjType.startColor = Color.red;
            LineToObjType.endColor = Color.red;
        }
        else if (AllowCentering)
        {
            LineToObjType.startColor = Color.blue;
            LineToObjType.endColor = Color.blue;
        }
        else
        {
            LineToObjType.startColor = Color.white;
            LineToObjType.endColor = Color.white;
        }
    }

    public void SetupDistanceRing()
    {
        int pts = 16;

        int sep = 360 / 16;

        Vector3[] ptVector = new Vector3[16];
        
        for (int i = 0; i < pts; i++)
        {
            float ang = sep * i;

            float x = Mathf.Cos(Mathf.Deg2Rad * ang) * ZoningController.BaseZoneDistance;
            float z = Mathf.Sin(Mathf.Deg2Rad * ang) * ZoningController.BaseZoneDistance;

            ptVector[i] = new Vector3(x, 0, z);
        }

        LineToRing.SetPositions(ptVector);

    }

    private void FixedUpdate()
    {
        
    }
    
    public void ResetVelocity()
    {
        float randDir = Random.Range(0, 360);
        var movementDir = Quaternion.AngleAxis(randDir, Vector3.up) * Vector3.forward;
        movementDir = Random.Range(minSpeed, maxSpeed) * movementDir;
        rb.velocity = movementDir;
    }

    public void ResetPosition(float maxP)
    {
        float xPos = Random.Range(-maxP, maxP);
        float zPos = Random.Range(-maxP, maxP);

        Vector3 pos = new Vector3(xPos, 0, zPos);
        transform.position = pos;
    }
}
