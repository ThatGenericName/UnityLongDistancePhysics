using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Zone
{
    public HashSet<WanderingObject> objects = new HashSet<WanderingObject>();

    public Vector3 WeightedCenter;
    public Vector3 PositionalSum;
    public Vector3 Corner1;
    public Vector3 Corner2;

    public WanderingObject Center;
    // the above is that for each value in the two vectors, Corner1 < Corner2

    public void ClearZone()
    {
        objects = null;
        WeightedCenter = Vector3.zero;
        PositionalSum = Vector3.zero;
        Corner1 = Vector3.zero;
        Corner2 = Vector3.zero;
    }
    public void SetCenter(WanderingObject obj)
    {
        Center = obj;
        objects.Add(obj);
    }

    public void RemoveObject(WanderingObject obj)
    {
        objects.Remove(obj);

        if (Center == obj)
        {
            Center = GetObjectClosestToCenter();
        }

        Vector3 objPos = obj.transform.position;

        if (objects.Count != 0)
        {
            PositionalSum -= objPos;
            WeightedCenter = PositionalSum / objects.Count;
        }

    }
    
    public void AddObject(WanderingObject obj)
    {
        if (objects.Contains(obj))
        {
            return;
        }
        
        Vector3 objPos = obj.transform.position;
        
        if (objects.Count == 0)
        {
            WeightedCenter = objPos;
            Corner1 = objPos;
            Corner2 = objPos;
            PositionalSum = objPos;
        }
        else
        {
            objects.Add(obj);
            if (objPos.x < Corner1.x)
            {
                Corner1.x = objPos.x;
            }
            else if (objPos.x > Corner2.x)
            {
                Corner2.x = objPos.x;
            }

            if (objPos.z < Corner1.z)
            {
                Corner1.z = objPos.z;
            }
            else if (objPos.z > Corner2.z)
            {
                Corner2.z = objPos.z;
            }

            PositionalSum += objPos;
            WeightedCenter = (PositionalSum) / (objects.Count);
        }
    }

    
    /// <summary>
    /// Gets the closest centerable object to the weighted center. If no centerable object exists, then returns the closest object;
    /// </summary>
    /// <returns></returns>
    public WanderingObject GetObjectClosestToCenter()
    {
        Dictionary<WanderingObject, Vector3> relDiff = GetObjectDistancesToCenter();

        var objectsArray = objects.ToArray();
        
        Array.Sort(objectsArray, (a, b) => (relDiff[a].sqrMagnitude.CompareTo(relDiff[b].sqrMagnitude)));

        foreach (var closest in objectsArray)
        {
            if (closest.AllowCentering)
            {
                return closest;
            }
        }

        return objectsArray[0];
    }

    private Dictionary<WanderingObject, Vector3> GetObjectDistancesToCenter()
    {
        Dictionary<WanderingObject, Vector3> results = GetObjectDistancesToCenter();
        
        foreach (var wanderingObject in objects)
        {
            Vector3 diff = WeightedCenter - wanderingObject.transform.position;

            results[wanderingObject] = diff;
        }

        return results;
    }
}