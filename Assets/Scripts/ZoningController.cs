using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class ZoningController : MonoBehaviour
{
    // Start is called before the first frame update


    public List<WanderingObject> WanderingObjects = new List<WanderingObject>();
    public List<WanderingObject> CenterableObjects = new List<WanderingObject>();
    [FormerlySerializedAs("ForcedCenterables")] public List<WanderingObject> ForcedCenterableObjects = new List<WanderingObject>();
    

    private const float pMax = 80;
    private const float pMaxN = -pMax;

    public GameObject WanderingPrefab;

    public int objCount;

    public const int Splits = 16;
    public const float ChunkSize = pMax * 2 / Splits;
    
    // Here's some physics zone related properties

    public const float SubZoneDist = pMax * 2 / Splits;
    public const float SubZoneDistSqr = SubZoneDist * SubZoneDist;
    public const float BaseZoneDistance = SubZoneDist * 2;
    public const float BaseZoneDistanceSqr = BaseZoneDistance * BaseZoneDistance;
    public const float MaxZoneDistance = SubZoneDist * 4;
    public const float MaxZoneDistanceSqr = MaxZoneDistance * MaxZoneDistance;

    public const int TicksPerUpdate = 5;
    
    void Start()
    {
        
        WanderingObject[] k = (WanderingObject[])GameObject.FindObjectsOfType(typeof(WanderingObject));
        
        WanderingObjects.AddRange(k);

        if (WanderingPrefab != null)
        {
            for (int i = WanderingObjects.Count; i < objCount; i++)
            {
                var k1 = Instantiate(WanderingPrefab, Vector3.zero, Quaternion.identity);
                var k2 = k1.GetComponent<WanderingObject>();

                if (i == 0)
                {
                    k2.ForcedCentering = true;
                }

                if (i < 7)
                {
                    k2.AllowCentering = true;
                    k2.SetupDistanceRing();
                }
                
                k2.ResetPosition(pMax - 5.0f);
                k2.ResetVelocity();
                k2.SetTypeColour();
                
                
                WanderingObjects.Add(k2);
            }
        }

        if (ChunkController == null)
        {
            ChunkController = gameObject.AddComponent<ChunkController>();
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    private int tickCount = 0;
    private void FixedUpdate()
    {
        CheckObjectBounds();

        if (tickCount++ > TicksPerUpdate)
        {
            GroupObjectsIntoZones();
            tickCount = 0;
        }
    }
    
    private void CheckObjectBounds()
    {
        foreach (var wanderingObj in WanderingObjects)
        {
            Vector3 pos = wanderingObj.transform.position;
            var woRB = wanderingObj.rb;

            bool shifted = RolloverVector3(ref pos);

            if (shifted)
            {
                wanderingObj.transform.position = pos;
            }
        }
    }

    public static bool RolloverVector3(ref Vector3 pos)
    {
        bool shifted = false;
        if (pos.x > pMax)
        {
            pos.x = pMaxN + 1.0f;
            shifted = true;
        }
        else if (pos.x < pMaxN)
        {
            pos.x = pMax - 1.0f;
            shifted = true;
        }
            
        if (pos.z > pMax)
        {
            pos.z = pMaxN + 1.0f;
            shifted = true;
        }
        else if (pos.z < pMaxN)
        {
            pos.z = pMax - 1.0f;
            shifted = true;
        }

        return shifted;
    }

    /*
     * Outlining the process
     *
     * First a half cycle linear approximation of an object's position is calculated. This value is used to
     * separate objects into separate physics chunks.
     *
     * Then using these chunks, ForcedCenterables are placed into their own zones. Objects in the immediate neighbouring
     * chunks are automatically added unless they are within another zone, in which case they are added only if they are
     * closer in proximity to this zone.
     * 
     * 
     *
     * 
     */

    public ChunkController ChunkController;


    private void ClearZonesFromObjects()
    {
        ForcedCenterableObjects.Clear();
        CenterableObjects.Clear();
        
        for (int i = 0; i < WanderingObjects.Count; i++)
        {
            var obj = WanderingObjects[i];
            obj.CurrentZones.Clear();
            if (obj.ForcedCentering)
            {
                ForcedCenterableObjects.Add(obj);
            }
            else if (obj.AllowCentering)
            {
                CenterableObjects.Add(obj);
            }
        }
    }
    
    private void GroupObjectsIntoZones()
    {
        ChunkController.UpdateChunks(WanderingObjects);
        
        ClearZonesFromObjects();
        
        // First setup the zone for the ForcedCenterable

        foreach (var forcedCenterable in ForcedCenterableObjects)
        {
            (int chunkX, int chunkZ) = forcedCenterable.CurrentChunk;

            Zone zone = new Zone();
            zone.SetCenter(forcedCenterable);
            
            SearchNeighbours(chunkX, chunkZ, forcedCenterable.transform.position, zone);
        }
        
        
        // Next setup zones for other Centerables

        foreach (var centerable in CenterableObjects)
        {
            (int chunkX, int chunkZ) = centerable.CurrentChunk;

            Zone zone = new Zone();
            zone.SetCenter(centerable);
            
            SearchNeighbours(chunkX, chunkZ, centerable.transform.position, zone);
        }
        
        
        // Setup zones for stray objects.
        
        
    }

    private void SearchNeighbours(int chunkX, int chunkZ, Vector3 center, Zone zone)
    {

        var chunks = ChunkController.ChunkDict;
        
        for (int x = -2; x < 2; x++)
        {
            for (int z = -2; z < 2; z++)
            {
                int cX = chunkX + x;
                int cZ = chunkZ + z;
                if (!chunks.ContainsKey((cX, cZ)))
                {
                    continue;
                }

                var chunk = chunks[(cX, cZ)];

                if (Math.Abs(x) <= 1 && Math.Abs(z) <= 1)
                {
                    AddImmediateNeighbourToCenterable(zone, chunk);
                }
                else
                {
                    AddDistantNeighbourToCenterable(zone, chunk);
                }
            }
        }
    }

    public float AngleThreshold = 40f;
    private void AddImmediateNeighbourToCenterable(Zone zone, Chunk chunk)
    {
        
        foreach (var wObject in chunk.Objects)
        {
            
            // for now, ignore centerables.

            if (wObject.AllowCentering)
            {
                continue;
            }


            
            if (wObject.CurrentZones.Count != 0)
            {
                List<Zone> zonesToAdd = new List<Zone>();
                List<Zone> zonesToRemove = new List<Zone>();
                // perform comparison
                foreach (var otherZone in wObject.CurrentZones)
                {
                    if (otherZone == zone)
                    {
                        continue;
                    }
                    
                    Vector3 diffToOther = otherZone.Center.transform.position - wObject.transform.position;
                    Vector3 diffToCurrent = zone.Center.transform.position - wObject.transform.position;
                    float angBetween = Vector3.Angle(diffToOther, diffToCurrent);
                    
                    if (angBetween < AngleThreshold) // completely arbitrary angle choice
                    {
                        if (diffToOther.sqrMagnitude > diffToCurrent.sqrMagnitude)
                        {
                            otherZone.RemoveObject(wObject);
                            zone.AddObject(wObject);
                            chunk.ZonedObjects.Add(wObject);
                            zonesToAdd.Add(zone);
                            zonesToRemove.Add(otherZone);
                        }
                    }
                    else
                    {
                        zone.AddObject(wObject);
                        chunk.ZonedObjects.Add(wObject);
                        zonesToAdd.Add(zone);
                    }
                }

                foreach (var zoneToRemove in zonesToRemove)
                {
                    wObject.CurrentZones.Remove(zoneToRemove);
                }
                wObject.CurrentZones.AddRange(zonesToAdd);
            }
            else
            {
                zone.AddObject(wObject);
                wObject.CurrentZones.Add(zone);
                chunk.ZonedObjects.Add(wObject);
            }
        }
    }

    private void AddDistantNeighbourToCenterable(Zone zone, Chunk chunk)
    {
        
        foreach (var wObject in chunk.Objects)
        {
            
            // for now, ignore centerables.

            if (wObject.AllowCentering)
            {
                continue;
            }
            
            
            
            if (wObject.CurrentZones.Count != 0)
            {
                // perform comparison
                List<Zone> zonesToAdd = new List<Zone>();
                List<Zone> zonesToRemove = new List<Zone>();
                // perform comparison
                foreach (var otherZone in wObject.CurrentZones)
                {
                    if (otherZone == zone)
                    {
                        continue;
                    }
                    
                    Vector3 diffToOther = otherZone.Center.transform.position - wObject.transform.position;
                    Vector3 diffToCurrent = zone.Center.transform.position - wObject.transform.position;
                    float angBetween = Vector3.Angle(diffToOther, diffToCurrent);
                    
                    if (angBetween < AngleThreshold) // completely arbitrary angle choice
                    {
                        if (diffToOther.sqrMagnitude > diffToCurrent.sqrMagnitude)
                        {
                            otherZone.RemoveObject(wObject);
                            zone.AddObject(wObject);
                            chunk.ZonedObjects.Add(wObject);
                            zonesToAdd.Add(zone);
                            zonesToRemove.Add(otherZone);
                        }
                    }
                    else
                    {
                        zone.AddObject(wObject);
                        chunk.ZonedObjects.Add(wObject);
                        zonesToAdd.Add(zone);
                    }
                }

                foreach (var zoneToRemove in zonesToRemove)
                {
                    wObject.CurrentZones.Remove(zoneToRemove);
                }
                wObject.CurrentZones.AddRange(zonesToAdd);
            }
            else
            {
                zone.AddObject(wObject);
                chunk.Zones.Add(zone);
                chunk.ZonedObjects.Add(wObject);
            }
        }
    }
}
