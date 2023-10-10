using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Chunk : MonoBehaviour
{
    public int x;
    public int z;

    public List<WanderingObject> Objects = new List<WanderingObject>();

    public HashSet<WanderingObject> ZonedObjects = new HashSet<WanderingObject>();

    public HashSet<WanderingObject> PossibleZoneObjects = new HashSet<WanderingObject>();
    
    public HashSet<Zone> Zones = new HashSet<Zone>();

    public Vector3 TLC;
    public Vector3 BRC;

    public bool Active = false;


    private void Awake()
    {
        DrawChunkBorders();
    }
    
    public LineRenderer ChunkBorderDrawer;

    public void DrawChunkBorders()
    {
        TLC.y = -1;
        BRC.y = -1;
        
        Vector3[] points;
        if (Active)
        {
            points = new[]
            {
                TLC,
                new Vector3(BRC.x, -1, TLC.z),
                BRC,
                new Vector3(TLC.x, -1, BRC.z)
            };
            

        }
        else
        {
            points = new[]
            {
                Vector3.zero,
                Vector3.zero,
                Vector3.zero,
                Vector3.zero
            };
        }
        
        ChunkBorderDrawer.SetPositions(points);
        return;
    }

    public void Add(WanderingObject wanderingObject)
    {
        Objects.Add(wanderingObject);
    }
}