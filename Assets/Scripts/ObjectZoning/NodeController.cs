using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LongDistancePhysics;
using ObjectZoning;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Serialization;

public class NodeController : MonoBehaviour
{

    public static NodeController Singleton;
    
    private SpatialHashGrid SpatialGrid = new SpatialHashGrid();

    private FloatingSpatialTree SpatialTree = new FloatingSpatialTree();
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
    }

    public bool DrawSGBorders = false;
    public bool DrawSTLeaves = false;
    public bool DrawSTBranches = false;

    public int SGYLayer = 0;
    public bool restrictToLayer = false;
    
    // Update is called once per frame
    void Update()
    {
        if (DrawSGBorders)
        {
            DebugDrawSHG(SGYLayer, restrictToLayer);
        }

        if (DrawSTLeaves)
        {
            DebugDrawFSTLeaves();
            
            // DebugDrawAllBranch();
        }

        if (DrawSTBranches)
        {
            SpatialTree.DebugDrawDepth(minDepth, width);
        }
        
    }

    public bool InitSpatialTree = false;
    private bool STInit = false;
    private int deallocCounter = 0;
    private const int DeallocThreshold = 100;

    private static readonly ProfilerMarker zObjUpdateProfiler = new("NodeController.UpdateZObjectData");
    private static readonly ProfilerMarker TreeBuildProfiler = new("SpatialTree.SetupTreeSynchronous");
    private static readonly ProfilerMarker LeafUpdateProfiler = new("SpatialTree.UpdateLeafNodes");
    private static readonly ProfilerMarker NodePositionUpdateProfiler = new("SpatialTree.UpdateHostLeafNodePositions");
    
    private void FixedUpdate()
    {

        zObjUpdateProfiler.Begin();
        UpdateZObjectData();
        zObjUpdateProfiler.End();
        

        if (InitSpatialTree && !STInit)
        {
            InitSpatialTree = false;
            STInit = true;
            InitializeSpatialTree();
        }
        else if (STInit)
        {
            NodePositionUpdateProfiler.Begin();
            SpatialTree.UpdateHostLeafNodePositions();
            NodePositionUpdateProfiler.End();
            

            LeafUpdateProfiler.Begin();
            SpatialTree.UpdateLeafNodes(ZObjectsList);
            LeafUpdateProfiler.End();

            TreeBuildProfiler.Begin();
            SpatialTree.ResetBranches();
            SpatialTree.SetupTreeSynchronous();
            TreeBuildProfiler.End();
        }
    }

    private void UpdateZObjectData()
    {
        foreach (var zObject in ZObjectsList)
        {
            zObject.UpdatePosition();
            zObject.ObjectBounds.center = zObject.transform.position;
            SpatialGrid.RevalidateObject(zObject);
        }

        if (deallocCounter == DeallocThreshold)
        {
            SpatialGrid.DeallocUnusedNodes();
            deallocCounter = 0;
        }
        deallocCounter++;
    }


    private List<ZObject> ZObjectsList = new List<ZObject>();

    public ZObject GetZObject(int id)
    {
        return ZObjectsList[id];
    }

    public ZObject[] GetZObjects(int[] ids)
    {
        ZObject[] returnArray = new ZObject[ids.Length];
        for (int i = 0; i < ids.Length; i++)
        {
            returnArray[i] = ZObjectsList[ids[i]];
        }

        return returnArray;
    }

    public void AddZObject(ZObject zObject)
    {
        zObject.ID = ZObjectsList.Count;
        SpatialGrid.AddObject(zObject);
        ZObjectsList.Add(zObject);
    }

    public void AddZObjects(IList<ZObject> zObjects)
    {
        foreach (var zObj in zObjects)
        {
            zObj.ID = ZObjectsList.Count;
            ZObjectsList.Add(zObj);
        }
        SpatialGrid.AddObjects(zObjects);
    }



    #region Spatial Tree Operations


    public void InitializeSpatialTree()
    {
        SpatialTree.SetupLeafNodesSynchronous(ZObjectsList);
        SpatialTree.SetupTreeSynchronous();
    }

    public void AddFSTNodesIntoSHT(List<int> fstNodeIDs)
    {
        foreach (var fstNodeID in fstNodeIDs)
        {
            SpatialGrid.InsertFSTNodeIntoGrid(SpatialTree.GetNode(fstNodeID));
        }
    }
    
    public void AddFSTNodesIntoSHT(int[] fstNodeIDs)
    {
        foreach (var fstNodeID in fstNodeIDs)
        {
            SpatialGrid.InsertFSTNodeIntoGrid(SpatialTree.GetNode(fstNodeID));
        }
    }

    public void AddFSTNodeIntoSHT(int fstNodeID)
    {
        SpatialGrid.InsertFSTNodeIntoGrid(SpatialTree.GetNode(fstNodeID));
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref FloatingSpatialTree.FSTNode GetFSTNode(int id)
    {
        return ref SpatialTree.GetNode(id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref FloatingSpatialTree.FSTNode GetFSTNode(FloatingSpatialTree.FSTNodeQR qr)
    {
        return ref SpatialTree.GetNode(qr);
    }
    
    private void DebugDrawFSTLeaves()
    {
        if (SpatialTree.HostedLeafNodes == null)
        {
            return;
        }
        var nodes = SpatialTree.GetNodes(SpatialTree.HostedLeafNodes);

        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].DebugDrawBoundaries();
        }
    }

    [Range(0, 20)]
    public int minDepth = 1;
    [Range(1, 10)]
    public int width = 5;
    
    

    #endregion
    
    
    #region Spatial Grid Operations

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SpatialHashGrid.SHGNode[] GetSHGNodes(int[] ids)
    {
        return SpatialGrid.GetNodes(ids);
    }

    public SpatialHashGrid.SHGNode[] GetSHGNodesFromQR(SpatialHashGrid.SHGNodeQR[] qrs)
    {
        return SpatialGrid.GetNodesFromQRs(qrs);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref SpatialHashGrid.SHGNode GetSHGNode(int id)
    {
        return ref SpatialGrid.GetSHGNode(id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ZObject[] GetNearbyObjects(Vector3 point, float distance)
    {
        return SpatialGrid.GetNearbyObjects(point, distance);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int[] GetNearbyObjectIDs(Vector3 point, float distance)
    {
        return SpatialGrid.GetNearbyObjectIDs(point, distance);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ZObject[] GetNearbyLinearDist(Vector3 point, float distance)
    {
        return SpatialGrid.GetNearbyLinearDistance(point, distance);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (ZObject, float)[] GetNearbyLinearDist2(Vector3 point, float distance)
    {
        return SpatialGrid.GetNearbyLinearDistance2(point, distance);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ZObject[] GetNearbyLinearDistConditional(Vector3 point, float distance, Func<ZObject, bool> conditional)
    {
        return SpatialGrid.GetNearbyLinearDistanceConditional(point, distance, conditional);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (ZObject, float)[] GetNearbyLinearDistConditional2(Vector3 point, float distance, Func<ZObject, bool> conditional)
    {
        return SpatialGrid.GetNearbyLinearDistanceConditional2(point, distance, conditional);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int[] GetIntersectingFSTNodes(Bounds bounds)
    {
        return SpatialGrid.GetIntersectingFSTNodes(bounds);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (int, float)[] GetIntersectingFSTNodesDist(Bounds bounds)
    {
        return SpatialGrid.GetIntersectingFSTNodesDist(bounds);
    }
    
    private void DebugDrawSHG(int layer, bool restrictToLayer = false)
    {
        SpatialHashGrid.SHGNode[] nodes = SpatialGrid.GetNodes();

        if (restrictToLayer)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                var k = nodes[i];
                if (k.Position.y == layer)
                {
                    DebugLDP.DrawBox(nodes[i].NodeBounds, Color.blue, 0.5f);
                }            
            }
        }
        else
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                DebugLDP.DrawBox(nodes[i].NodeBounds, Color.blue, 0.5f);        
            }
        }

    }
    
    #endregion
    

}
