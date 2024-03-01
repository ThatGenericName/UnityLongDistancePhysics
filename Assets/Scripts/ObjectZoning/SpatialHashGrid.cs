using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LongDistancePhysics;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace ObjectZoning
{
    public class SpatialHashGrid
    {
        public const float ChunkSize = 20f;
        
        private ConcurrentDictionary<Vector3Int, int> elements = new ConcurrentDictionary<Vector3Int, int>();
        public int ObjectCount = 0;

        public float NodeOccupanceDensity => (float)ObjectCount / NodeAllocator.ActiveCount;

        public SHGNodeQR[] AddObject(ZObject zObject)
        {
            Vector3 c1 = zObject.ObjectBounds.min;
            Vector3 c2 = zObject.ObjectBounds.max;

            Vector3Int min = MathLDP.CalculateXYZToInt(c1, ChunkSize);
            int minX = min.x;
            int minY = min.y;
            int minZ = min.z;

            Vector3Int max = MathLDP.CalculateXYZToInt(c2, ChunkSize) + Vector3Int.one;
            int maxX = max.x;
            int maxY = max.y;
            int maxZ = max.z;
            
            // estimate how many nodes will be used.
            var diff = max - min;
            int volume = diff.x * diff.y * diff.z;
            
            List<SHGNodeQR> nodeQRs = new(volume);
            
            for (int z = minZ; z < maxZ; z++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    for (int x = minX; x < maxX; x++)
                    {
                        var qr = InsertObjectIntoXYZ(zObject, x, y, z);
                        
                        nodeQRs.Add(qr);
                        zObject.GridNodes.Add(qr);
                    }
                }
            }

            ObjectCount++;
            return nodeQRs.ToArray();
        }

        public SHGNodeQR[][] AddObjects(IList<ZObject> zObjects)
        {
            SHGNodeQR[][] returnArray = new SHGNodeQR[zObjects.Count][];
            for (int i = 0; i < zObjects.Count; i++)
            {
                returnArray[i] = AddObject(zObjects[i]);
            }

            ObjectCount += zObjects.Count;
            return returnArray;
        }

        /// <summary>
        /// Revalidates the location of the object.
        /// </summary>
        /// <param name="zObj"></param>
        public SHGNodeQR[] RevalidateObject(ZObject zObject)
        {
            if (zObject.ID == 28)
            {
                ;
            }
            Vector3 c1 = zObject.ObjectBounds.min;
            Vector3 c2 = zObject.ObjectBounds.max;

            Vector3Int min = MathLDP.CalculateXYZToInt(c1, ChunkSize);
            int minX = min.x;
            int minY = min.y;
            int minZ = min.z;

            Vector3Int max = MathLDP.CalculateXYZToInt(c2, ChunkSize) + Vector3Int.one;
            int maxX = max.x;
            int maxY = max.y;
            int maxZ = max.z;
            
            // estimate how many nodes will be used.
            var diff = max - min;
            int volume = diff.x * diff.y * diff.z;
            
            List<SHGNodeQR> nodeQRs = new(volume);
            HashSet<SHGNodeQR> existingNodes = new(zObject.GridNodes, new SHGNodeQRLightCompare());
            
            for (int z = minZ; z < maxZ; z++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    for (int x = minX; x < maxX; x++)
                    {
                        var qr = new SHGNodeQR()
                        {
                            Position = new Vector3Int(x, y, z)
                        };
                        
                        if (existingNodes.Contains(qr))
                        {
                            existingNodes.Remove(qr);
                        }
                        else
                        {
                            var qr2 = InsertObjectIntoXYZ(zObject, x, y, z);
                            zObject.GridNodes.Add(qr2);
                            nodeQRs.Add(qr2);
                        }
                    }
                }
            }

            int objId = zObject.ID;
            foreach (var id in existingNodes)
            {
                ref SHGNode node = ref GetSHGNode(id);
                node.ObjectIDs.Remove(objId);
                zObject.GridNodes.Remove(id);
            }
            
            return nodeQRs.ToArray();
        }

        private SHGNodeQR InsertObjectIntoXYZ(ZObject zObject, int x, int y, int z)
        {
            Vector3Int pos = new Vector3Int(x, y, z);
            SHGNodeQR nodeQr;
            int id;
            if (!elements.TryGetValue(pos, out id))
            {
                ref SHGNode node = ref AllocateSHGNode();
                node.ObjectIDs.Add(zObject.ID);
                id = node.StructID;
                elements[pos] = id;
                node.Position = pos;
                Vector3 boundMin = new Vector3(x, y, z) * ChunkSize;
                Vector3 boundMax = new Vector3(x + 1, y + 1, z + 1) * ChunkSize;
                Vector3 center = boundMin + Vector3.one * (ChunkSize / 2);
                Vector3 size = boundMax - boundMin;
                node.NodeBounds = new Bounds(center, size);
                nodeQr = node.GetQR();
            }
            else
            {
                ref SHGNode node = ref GetSHGNode(id);
                node.ObjectIDs.Add(zObject.ID);
                nodeQr = node.GetQR();
            }

            return nodeQr;
        }


        #region FST Node Operations

        public int[] InsertFSTNodeIntoGrid(FloatingSpatialTree.FSTNode fstNode)
        {
            Vector3 c1 = fstNode.NodeBounds.min;
            Vector3 c2 = fstNode.NodeBounds.max;

            Vector3Int min = MathLDP.CalculateXYZToInt(c1, ChunkSize);
            int minX = min.x;
            int minY = min.y;
            int minZ = min.z;

            Vector3Int max = MathLDP.CalculateXYZToInt(c2, ChunkSize) + Vector3Int.one;
            int maxX = max.x;
            int maxY = max.y;
            int maxZ = max.z;
            
            // estimate how many nodes will be used.
            var diff = max - min;
            int volume = diff.x * diff.y * diff.z;
            
            List<int> nodeIDs = new(volume);
            HashSet<SHGNodeQR> existingNodes = new(fstNode.GridNodes, new SHGNodeQRLightCompare());
            
            for (int y = minY; y < maxY; y++)
            {
                for (int z = minZ; z < maxZ; z++)
                {
                    for (int x = minX; x < maxX; x++)
                    {
                        Vector3Int pos = new(x, y, z);
                        var qr = new SHGNodeQR()
                        {
                            ElementID = -1,
                            Position = new(x, y, z)
                        };
                        
                        if (existingNodes.Contains(qr))
                        {
                            existingNodes.Remove(qr);
                            continue;
                        }
                        
                        int id = InsertFSTNIntoXYZ(fstNode, x, y, z);
                        qr.ElementID = id;
                        nodeIDs.Add(id);
                        fstNode.GridNodes.Add(qr);
                    }
                }
            }

            int fstNodeID = fstNode.StructID;
            foreach (var id in existingNodes)
            {
                ref SHGNode shgNode = ref GetSHGNode(id);
                shgNode.FSTNodeIDs.Remove(fstNodeID);
                fstNode.GridNodes.Remove(id);
            }
            
            return nodeIDs.ToArray();
        }

        private int InsertFSTNIntoXYZ(FloatingSpatialTree.FSTNode fstNode, int x, int y, int z)
        {
            int id;
            Vector3Int pos = new Vector3Int(x, y, z);
            if (!elements.TryGetValue(pos, out id))
            {
                ref SHGNode node = ref AllocateSHGNode();
                node.FSTNodeIDs.Add(fstNode.StructID);
                id = node.StructID;
                elements[pos] = id;
                node.Position = pos;
                Vector3 boundMin = new Vector3(x, y, z) * ChunkSize;
                Vector3 boundMax = new Vector3(x + 1, y + 1, z + 1) * ChunkSize;
                Vector3 center = boundMin + Vector3.one * (ChunkSize / 2);
                Vector3 size = boundMax - boundMin;
                node.NodeBounds = new Bounds(center, size);
            }
            else
            {
                ref SHGNode node = ref GetSHGNode(id);
                node.FSTNodeIDs.Add(fstNode.StructID);
            }

            return id;
        }

        #endregion

        public void DeallocUnusedNodes()
        {
            NodeAllocator.DeallocIf(ShouldDealloc, CleanupBeforeDealloc);
        }

        private bool ShouldDealloc(SHGNode node)
        {
            
            return node.ObjectIDs.Count == 0 && node.FSTNodeIDs.Count == 0;
        }

        private bool CleanupBeforeDealloc(SHGNode node)
        {
            elements.Remove(node.Position, out _);
            return true;
        }

        public void GetObjectsAtNode(int x, int y, int z)
        {
            int id = elements[new(x, y, z)];
            
        }

        public SHGNode this[int x, int y, int z]
        {
            get
            {
                int id = elements[new(x, y, z)];
                return GetSHGNode(id);
            }
        }

        public SHGNode[] GetNodes()
        {
            return NodeAllocator.GetActiveStructs();
        }

        public SHGNode[] GetNodes(int[] ids)
        {
            SHGNode[] returnArray = new SHGNode[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                returnArray[i] = NodeAllocator[ids[i]];
            }
            return returnArray;
        }
        
        public SHGNode[] GetNodesFromQRs(SHGNodeQR[] ids)
        {
            SHGNode[] returnArray = new SHGNode[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                returnArray[i] = NodeAllocator[ids[i].ElementID];
            }
            return returnArray;
        }
        
        public ZObject[] GetNearbyObjects(Vector3 point, float distance)
        {
            //int offset = Mathf.FloorToInt(distance / ChunkSize);
            var objectsIDSet = GetNearbyChunksZObjects(point, distance);

            List<ZObject> filteredObjects = new(objectsIDSet.Count);
            var nc = NodeController.Singleton;
            
            float threshSqr = distance * distance;
            foreach (var zobjID in objectsIDSet)
            {
                ZObject zobj = nc.GetZObject(zobjID);
                Vector3 diff = zobj.transform.position - point;
                if (diff.sqrMagnitude < threshSqr)
                {
                    filteredObjects.Add(zobj);
                }
            }
            return filteredObjects.ToArray();
        }
        
        public int[] GetNearbyObjectIDs(Vector3 point, float distance)
        {
            //int offset = Mathf.FloorToInt(distance / ChunkSize);
            var objectsIDSet = GetNearbyChunksZObjects(point, distance);

            List<int> filteredObjects = new(objectsIDSet.Count);
            var nc = NodeController.Singleton;
            
            float threshSqr = distance * distance;
            foreach (var zobjID in objectsIDSet)
            {
                ZObject zobj = nc.GetZObject(zobjID);
                Vector3 diff = zobj.Position - point;
                if (diff.sqrMagnitude < threshSqr)
                {
                    filteredObjects.Add(zobjID);
                }
            }
            return filteredObjects.ToArray();
        }

        public ZObject[] GetNearbyLinearDistance(Vector3 point, float distance)
        {
            //int offset = Mathf.FloorToInt(distance / ChunkSize);
            var objectsIDSet = GetNearbyChunksZObjects(point, distance);

            List<ZObject> filteredObjects = new(objectsIDSet.Count);
            var nc = NodeController.Singleton;
            foreach (var zobjID in objectsIDSet)
            {
                ZObject zobj = nc.GetZObject(zobjID);
                if (MathLDP.WithinLinearDistance(point, zobj.transform.position, distance))
                {
                    filteredObjects.Add(zobj);
                }
            }
            return filteredObjects.ToArray();
        }
        
        public (ZObject, float)[] GetNearbyLinearDistance2(Vector3 point, float distance)
        {
            //int offset = Mathf.FloorToInt(distance / ChunkSize);
            var objectsIDSet = GetNearbyChunksZObjects(point, distance);

            List<(ZObject, float)> filteredObjects = new(objectsIDSet.Count);
            var nc = NodeController.Singleton;
            foreach (var zobjID in objectsIDSet)
            {
                ZObject zobj = nc.GetZObject(zobjID);
                if (MathLDP.WithinLinearDistance(point, zobj.transform.position, distance, out var linDist))
                {
                    filteredObjects.Add((zobj, linDist));
                }
            }
            return filteredObjects.ToArray();
        }
        
        public ZObject[] GetNearbyLinearDistanceConditional(Vector3 point, float distance, Func<ZObject, bool> condition)
        {
            var objectsIDSet = GetNearbyChunksZObjects(point, distance);

            List<ZObject> filteredObjects = new(objectsIDSet.Count);
            var nc = NodeController.Singleton;
            foreach (var zobjID in objectsIDSet)
            {
                ZObject zobj = nc.GetZObject(zobjID);
                if (!condition(zobj))
                {
                    continue;
                }
                
                if (MathLDP.WithinLinearDistance(point, zobj.transform.position, distance))
                {
                    filteredObjects.Add(zobj);
                }
                
            }
            return filteredObjects.ToArray();
        }
        
        public (ZObject, float)[] GetNearbyLinearDistanceConditional2(Vector3 point, float distance, Func<ZObject, bool> condition)
        {
            var objectsIDSet = GetNearbyChunksZObjects(point, distance);

            List<(ZObject, float)> filteredObjects = new(objectsIDSet.Count);
            var nc = NodeController.Singleton;
            foreach (var zobjID in objectsIDSet)
            {
                ZObject zobj = nc.GetZObject(zobjID);
                if (!condition(zobj))
                {
                    continue;
                }

                float linDist;
                if (MathLDP.WithinLinearDistance(point, zobj.Position, distance, out linDist))
                {
                    float sqrtDistance = (zobj.Position - point).sqrMagnitude;
                    filteredObjects.Add((zobj, linDist));
                }
                
            }
            return filteredObjects.ToArray();
        }

        private HashSet<int> GetNearbyChunksZObjects(Vector3 point, float distance)
        {

            int offset = (int)Math.Max(distance / ChunkSize, 1);
            Vector3Int offsetInt = Vector3Int.one * offset;
            Vector3Int ptInt = Vector3Int.FloorToInt(point / ChunkSize);
            Vector3Int min = ptInt - offsetInt;
            Vector3Int max = ptInt + offsetInt + Vector3Int.one;

            int minX = min.x;
            int minY = min.y;
            int minZ = min.z;

            int maxX = max.x;
            int maxY = max.y;
            int maxZ = max.z;

            int volume = (maxX - minX) * (maxY - minY) * (maxZ - minZ);
            int estInitCount = (int)(volume * 0.75f * NodeOccupanceDensity);

            HashSet<int> objectsIDSet = new(estInitCount);
            
            for (int y = minY; y < maxY; y++)
            {
                for (int z = minZ; z < maxZ; z++)
                {
                    for (int x = minX; x < maxX; x++)
                    {
                        int id;
                        if (elements.TryGetValue(new(x, y, z), out id))
                        {
                            var node = NodeAllocator.GetStruct(id);
                            objectsIDSet.UnionWith(node.ObjectIDs);
                        }
                    }
                }
            }

            return objectsIDSet;
        }

        public int[] GetIntersectingFSTNodes(Bounds bounds)
        {
            Vector3 min = bounds.min;
            Vector3Int minCI = Vector3Int.FloorToInt(min / ChunkSize);
            int minX = minCI.x;
            int minY = minCI.y;
            int minZ = minCI.z;

            Vector3 max = bounds.max;
            Vector3Int maxCI = Vector3Int.FloorToInt(max / ChunkSize);
            int maxX = Mathf.Max(maxCI.x + 1, minX + 1);
            int maxY = Mathf.Max(maxCI.y + 1, minY + 1);
            int maxZ = Mathf.Max(maxCI.z + 1, minZ + 1);
            
            
            var nc = NodeController.Singleton;

            HashSet<int> FSTNodeIDs = new();
            
            for (int y = minY; y < maxY; y++)
            {
                for (int z = minZ; z < maxZ; z++)
                {
                    for (int x = minX; x < maxX; x++)
                    {
                        Vector3Int coord = new(x, y, z);
                        if (elements.TryGetValue(coord, out var nodeID))
                        {
                            var node = NodeAllocator.GetStruct(nodeID);
                            foreach (var fstNodeID in node.FSTNodeIDs)
                            {
                                if (FSTNodeIDs.Contains(fstNodeID))
                                {
                                    continue;
                                }
                                
                                var fstNode = nc.GetFSTNode(fstNodeID);
                                if (fstNode.NodeBounds.Intersects(bounds))
                                {
                                    FSTNodeIDs.Add(fstNodeID);
                                }
                            }
                        }
                    }
                }
            }

            return FSTNodeIDs.ToArray();
        }

        public (int, float)[] GetIntersectingFSTNodesDist(Bounds bounds)
        {
            Vector3Int minCI = Vector3Int.FloorToInt(bounds.min / ChunkSize);
            int minX = minCI.x;
            int minY = minCI.y;
            int minZ = minCI.z;
            
            Vector3Int maxCI = Vector3Int.FloorToInt(bounds.max / ChunkSize) + Vector3Int.one;
            int maxX = maxCI.x;
            int maxY = maxCI.y;
            int maxZ = maxCI.z;
            
            var nc = NodeController.Singleton;

            HashSet<int> FSTNodeIDs = new();
            
            for (int y = minY; y < maxY; y++)
            {
                for (int z = minZ; z < maxZ; z++)
                {
                    for (int x = minX; x < maxX; x++)
                    {
                        Vector3Int coord = new(x, y, z);
                        if (elements.TryGetValue(coord, out var nodeID))
                        {
                            var node = NodeAllocator.GetStruct(nodeID);
                            foreach (var fstNodeID in node.FSTNodeIDs)
                            {
                                if (FSTNodeIDs.Contains(fstNodeID))
                                {
                                    continue;
                                }
                                
                                var fstNode = nc.GetFSTNode(fstNodeID);
                                if (fstNode.NodeBounds.Intersects(bounds))
                                {
                                    FSTNodeIDs.Add(fstNodeID);
                                }
                            }
                        }
                    }
                }
            }

            List<(int, float)> resultArray = new List<(int, float)>(FSTNodeIDs.Count);

            foreach (var nodeID in FSTNodeIDs)
            {
                var node = nc.GetFSTNode(nodeID);
                var nodeCenter = node.NodeCenter;
                float linDist = MathLDP.GetLinearDistance(nodeCenter, bounds.center);
                resultArray.Add((nodeID, linDist));
            }
            
            return resultArray.ToArray();
        }
        

        #region Node Allocator

        private StructAllocator<SHGNode> NodeAllocator = new();
        
        public ref SHGNode GetSHGNode(int id)
        {
            return ref NodeAllocator.GetStruct(id);
        }

        public ref SHGNode GetSHGNode(SHGNodeQR qr)
        {
            return ref NodeAllocator.GetStruct(qr.ElementID);
        }
        
        public ref SHGNode AllocateSHGNode()
        {
            return ref NodeAllocator.AllocateStruct();
        }

        #endregion
        


        #region Supporting Structs

        public struct SHGNode : IStructAllocatorObject
        {
            public HashSet<int> ObjectIDs;
            public HashSet<int> FSTNodeIDs;

            public Bounds NodeBounds;
            
            public Vector3Int Position;

            public int StructID { get; set; }
            
            public void Init()
            {
                ObjectIDs = new HashSet<int>();
                FSTNodeIDs = new HashSet<int>();
            }
            
            public void Destroy()
            {
                ObjectIDs.Clear();
                ObjectIDs = null;
                FSTNodeIDs.Clear();
                FSTNodeIDs = null;
            }

            public void AddObject(ZObject zObject)
            {
                ObjectIDs.Add(zObject.ID);
            }

            public SHGNodeQR GetQR()
            {
                return new SHGNodeQR()
                {
                    ElementID = StructID,
                    Position = Position
                };
            }
        }

        /// <summary>
        /// Quick reference version of a SGHNode meant to reduce duplicate computations
        /// </summary>
        public struct SHGNodeQR
        {
            public int ElementID;
            public Vector3Int Position;
        }
        
        public class SHGNodeQRLightCompare : IEqualityComparer<SHGNodeQR>
        {
            public bool Equals(SHGNodeQR x, SHGNodeQR y)
            {
                return x.Position == y.Position;
            }

            public int GetHashCode(SHGNodeQR obj)
            {
                return obj.Position.GetHashCode();
            }
        }

        #endregion

    }
}