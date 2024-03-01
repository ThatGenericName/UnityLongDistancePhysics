using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LongDistancePhysics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Profiling;
using Unity.Profiling.Editor;
using UnityEngine;
using UnityEngine.Serialization;

namespace ObjectZoning
{

    #region Overview

    /*
     * Properties of the FloatingSpatialTree.
     *
     * The Tree follows a simple AABB Binary Tree.
     *
     * The chosen axis and distance comparison is only used for tree setup. Any
     * Subsequent insertions after tree setup determines subtree position based
     * on linear proximity to each child.
     *
     * Terminology:
     * Host Object:
     * * The host object are objects that a leaf node is centered on.
     * * There are 3 Host Object Categories:
     * * * Type 0:
     * * * * Type 0 Objects cannot be absorbed by other leaves no matter
     *       close they are to the other leaf. That is they will always
     *       be the host of their own leaf. They can be adopted
     * * * Type 1:
     * * * * Type 1 Objects can be absorbed by other leaves. However
     *       when they are not, they will have their own leaves.
     * * * Type 2:
     * * * * Type 2 objects can be absorbed by other leaves. If they
     *       are not placed into other leaves, they will have their own
     *       smaller leaf.
     * 
     * Absorption vs Adoption
     * * Adoption is simply adding an object to a leaf, if the object is the host
     *   of their own leaf, they maintain that leaf.
     * * Absorption is when a leaf completely takes over the second leaf; the second
     *   leaf is dissolved and objects from the leaf is adopted by the first leaf.
     *
     * Behaviour
     * A leaf node will represent a physics zone instance. There are 2 types of leaf nodes:
     * Primary Leaf nodes
     * * Primary leaf nodes are full sized leaf nodes.
     * Secondary Leaf Nodes
     * * Secondary leaf nodes are small sized leaves at 1/4 width of a Primary leaf node.
     *   These are used by Type 2 objects by default. They expand to the size of a Primary
     *   Leaf node when there are objects in proximity.
     *
     * For primary leaves, Objects that crosses an absorption or adoption threshold must cross
     * a (relative to the center of the leaf) further upper threshold, this is to prevent
     * oscillating behaviour when an object remains at a boundary point.
     *
     * All distances are using linear distances (think of it as max component distance).
     *
     * Any objects within absorption distance are adopted regardless of occlusion.
     * 
     */

    #endregion
    
    /*
     * General TODO:
     *
     * Randomize initial threshold tick counters for load balancing
     * 
     */
    
    public class FloatingSpatialTree
    {

        #region Thresholds
        
        
        public const float AdoptionDistance = 35;
        public const float AdoptionReleaseDistance = 45;
        public const float AbsorptionDistance = 15;
        public const float AbsorptionReleaseDistance = 20;
        
        // MUST BE BASE 2
        public const int UpdateThreshHostAbsorptionDistance = 4;
        public const int UpdateThreshHostAbsorptionReleaseDistance = 4;
        public const int UpdateThreshAdoptionDistance = 4;
        public const int UpdateThreshAdoptionReleaseDistance = 4;

        public const int LookForObjectsToAbsorbFrequency = 4;
        
        public const float OcclusionAngleThreshold = 40;
        

        #endregion
        
        
        void InitSpatialTree(List<ZObject> objects)
        {

        }

        #region Data

        // Stores the structure of the tree
        public List<int> HostedLeafNodes;
        public List<int> BranchNodeIDs;
        public int RootNodeID { get; private set; } = -1;

        #endregion

        #region Leaf Insertions
        
        public int CreateNodeAtLeaf(ZObject zObject, int leafID)
        {
            throw new NotImplementedException("FloatingSpatialTree.CreateNodeAtLeaf");
        }


        /// <summary>
        /// Inserts a node into the tree, returns the ID of new nodes created.
        /// </summary>
        /// <param name="newNode"></param>
        /// <returns></returns>
        public int InsertLeafIntoTree(ref FSTNode newNode)
        {
            /*
             * Starts at the root node and inserts a node into the tree.
             * 
             * 
             */
            Vector3 nodePosition = newNode.NodeCenter;
            ref FSTNode current = ref NodeAllocator.GetStruct(RootNodeID);
            bool currentHasSpace = false;
            while (!current.IsLeaf)
            {
                current.NodeBounds.Encapsulate(newNode.NodeBounds);
                if (current.Left == -1 || current.Right == -1)
                {
                    currentHasSpace = true;
                    break;
                }
                
                // compare left and right leaves, go to the closer one.
                ref var left = ref NodeAllocator.GetStruct(current.Left);
                ref var right = ref NodeAllocator.GetStruct(current.Right);
                Vector3 leftDiff = left.NodeCenter - nodePosition;
                Vector3 rightDiff = right.NodeCenter - nodePosition;
                

                if (leftDiff.sqrMagnitude < rightDiff.sqrMagnitude)
                {
                    current = left;
                }
                else
                {
                    current = right;
                }
            }

            if (currentHasSpace)
            {
                // reached a node that has space, NOTE: this case should be uncommon, usually only
                // if initially 1 leaf node exists in the tree.
                // TODO: tests to see if I can hardcode this edge case check at tree generation
                newNode.Parent = current.StructID;
                if (current.Left == -1)
                {
                    current.Left = newNode.StructID;
                }
                else
                {
                    current.Right = newNode.StructID;
                }

                return -1; // no new nodes were created.
            }
            
            // reached a leaf node, insert into this place.
            // create a new node to adopt these 2 nodes.
            // Although we used "left" and "right" when creating the tree
            // to try to ensure balance, 
            // it matters less since we don't use left and right for any
            // of the intersection tests that this tree would perform.
            ref FSTNode newInPlaceNode = ref NodeAllocator.AllocateStruct();
            newInPlaceNode.Parent = current.Parent;
            
            newInPlaceNode.Left = current.StructID;
            current.Parent = newInPlaceNode.StructID;
            newInPlaceNode.NodeBounds.Encapsulate(current.NodeBounds);
            
            newInPlaceNode.Right = newNode.StructID;
            newNode.Parent = newInPlaceNode.StructID;
            newInPlaceNode.NodeBounds.Encapsulate(newNode.NodeBounds);

            BranchNodeIDs.Add(newInPlaceNode.StructID);
            return newInPlaceNode.StructID;
        }
        
        #endregion

        #region Synchronous Tree Maintainence

        public void UpdateHostLeafNodePositions()
        {
            /*
             * 
             */
            var nc = NodeController.Singleton;
            foreach (var nodeID in HostedLeafNodes)
            {
                ref var node = ref NodeAllocator.GetStruct(nodeID);
                if (node.TickForwardRebound())
                {
                    node.UpdateBounds();
                    nc.AddFSTNodeIntoSHT(nodeID);
                }
            }
        }

        public void ValidateLeafsValid()
        {
            HashSet<int> seenHosts = new();
            for (int i = 0; i < HostedLeafNodes.Count; i++)
            {
                var leaf = NodeAllocator.GetStruct(HostedLeafNodes[i]);
                if (seenHosts.Contains(leaf.HostObject.ID))
                {
                    // validating that all leaves have unique hosts
                    throw new Exception("FUUUUUUUUUUCK");
                }

                if (leaf.HostObject.HostingTreeNode.StructID != HostedLeafNodes[i])
                {
                    // validating that the leaf correctly has a host.
                    throw new Exception("FUUUUUUUUUUCK2");
                }

                seenHosts.Add(leaf.HostObject.ID);
            }
        }

        private int callNum = 0;
        
        private static readonly ProfilerMarker LeafUpdateS1Profiler = new("SpatialTree.UpdateLeafNodes.S1");
        private static readonly ProfilerMarker LeafUpdateS2Profiler = new("SpatialTree.UpdateLeafNodes.S2");
        private static readonly ProfilerMarker LeafUpdateS3Profiler = new("SpatialTree.UpdateLeafNodes.S3");
        public void UpdateLeafNodes(List<ZObject> zObjects)
        {
            // TODO: proper documentation
            /*
             * Process is a bit different here
             * 1) process absorbed objects to see whether they should be "unabsorbed"
             *
             * 2) Iterate through objects to determine if any connections should be updated,
             *    either removed or changed.
             *
             * 3) Iterate through objects to try to add them into new leaves.
             *
             * All the above are determined by their tick time relations.
             */

            var nc = NodeController.Singleton;

            List<ZObject> absorbedObjects = new List<ZObject>(32);

            foreach (var zObject in zObjects)
            {
                if (zObject.AbsorbingTreeNodes.Count != 0)
                {
                    absorbedObjects.Add(zObject);
                }
            }
            
            
            #region Step 1 Process Absorbed Objects

            LeafUpdateS1Profiler.Begin();
            
            List<ZObject> formerlyAbsorbedObjects = new List<ZObject>();
            foreach (var absorbedObject in absorbedObjects)
            {
                if (callNum == 1905 && absorbedObject.ID == 6)
                {
                    ;
                }
                
                Vector3 zObjPos = absorbedObject.Position;
                var absorbingNodesCpy = absorbedObject.AbsorbingTreeNodes.ToArray();
                foreach (var nodeQR in absorbingNodesCpy)
                {
                    if (!nodeQR.TickForward())
                    {
                        absorbedObject.AbsorbingTreeNodes.Add(nodeQR);
                        continue;
                    }
                    absorbedObject.AbsorbingTreeNodes.Add(nodeQR);
                    ref var node = ref GetNode(nodeQR);
                    Vector3 nodePos = node.NodeCenter;
                    if (MathLDP.WithinLinearDistance(nodePos, zObjPos, AbsorptionReleaseDistance,
                            out var linDIst))
                    {
                        continue;
                    }
                    
                    node.ReleaseAbsorbedObject(absorbedObject);
                    node.AdoptObject(absorbedObject);
                }

                bool createNewNode = (!absorbedObject.Absorbed)
                                     && (absorbedObject.TreeObjectType != TreeObjectType.Type2)
                                     && absorbedObject.HostingTreeNode.StructID == -1;
                
                if (createNewNode)
                {
                    // create a new node for this object. 
                    ref var newNode = ref NodeAllocator.AllocateStruct();
                    newNode.InitHostObject(absorbedObject);
                    var nearbyObjects = nc.GetNearbyLinearDist(newNode.NodeCenter, AdoptionDistance);
                    List<int> nodesToRemove = new List<int>();
                    foreach (var nearbyObject in nearbyObjects)
                    {
                        nodesToRemove.Add(TryAddToNearbyNode(nearbyObject, ref newNode, zObjPos));
                    }
                    HostedLeafNodes.Add(newNode.StructID);
                    formerlyAbsorbedObjects.Add(absorbedObject);
                    newNode.UpdateBounds();
                    nc.AddFSTNodeIntoSHT(newNode.StructID);
                }
                
            }
            
            LeafUpdateS1Profiler.End();
            #endregion

            #region Step 2 Connection Updates
            /*
             * Iterate through each object and their connections, update these connections.
             *
             * If after the updates, the object has no connections remaining and they are
             * not already hosting their own node, create a new node.
             */

            
            LeafUpdateS2Profiler.Begin();
            
            List<ZObject> zObjectsToCreateNode = new();
            List<int> nodesToDestroy = new();

            foreach (var zObject in zObjects)
            {
                Vector3 zObjectPosition = zObject.Position;
                var linksCpy = zObject.AdoptingTreeNodes.ToArray();
                var absorbedLinks = zObject.AbsorbingTreeNodes.ToArray();
                for (int i = 0; i < linksCpy.Length; i++)
                {
                    if (linksCpy[i].StructID == zObject.HostingTreeNode.StructID)
                    {
                        continue;
                    }
                    
                    if (!linksCpy[i].TickForward())
                    {
                        zObject.AdoptingTreeNodes.Add(linksCpy[i]);
                        continue;
                    }
                    
                    ref var linkedNode = ref NodeAllocator.GetStruct(linksCpy[i].StructID);
                    Vector3 nodePosition = linkedNode.NodeCenter;
                    
                    // occlusionCheck
                    bool isOccluded = false;
                    for (int j = i+1; j < linksCpy.Length; j++)
                    {
                        var otherNode = NodeAllocator.GetStruct(linksCpy[j].StructID);
                        Vector3 otherNodePos = otherNode.NodeCenter;

                        if (MathLDP.IsBehindAngle(
                                zObjectPosition, otherNodePos, 
                                nodePosition, OcclusionAngleThreshold)
                            )
                        {
                            isOccluded = true;
                            break;
                        }
                    }

                    if (!isOccluded)
                    {
                        for (int j = 0; j < absorbedLinks.Length; j++)
                        {
                            var otherNode = NodeAllocator.GetStruct(absorbedLinks[j].StructID);
                            Vector3 otherNodePos = otherNode.NodeCenter;

                            if (MathLDP.IsBehindAngle(
                                    zObjectPosition, otherNodePos, 
                                    nodePosition, OcclusionAngleThreshold)
                               )
                            {
                                isOccluded = true;
                                break;
                            }
                        }
                    }

                    

                    if (isOccluded)
                    {
                        linkedNode.ReleaseAdoptedObject(zObject);
                        continue;
                    }
                    

                    float linDist = MathLDP.GetLinearDistance(nodePosition, zObjectPosition);
                    if (linDist < AbsorptionDistance)
                    {
                        if (zObject.HostingTreeNode.StructID == -1)
                        {
                            linkedNode.AbsorbObject(zObject);
                        }
                        else
                        {
                            var hostedNode = NodeAllocator.GetStruct(zObject.HostingTreeNode.StructID);
                            linkedNode.AbsorbNode(ref hostedNode);
                            if (zObject.TreeObjectType != TreeObjectType.Type0)
                            {
                                nodesToDestroy.Add(hostedNode.StructID);
                            }
                        }
                    }
                    else if (linDist > AdoptionReleaseDistance)
                    {
                        linkedNode.ReleaseAdoptedObject(zObject);
                    }
                }

                if (zObject.IsFree)
                {
                    zObjectsToCreateNode.Add(zObject);
                }
                
            }

            
            
            foreach (var nodeID in nodesToDestroy)
            {
                ref var node = ref NodeAllocator.GetStruct(nodeID);
                node.ClearNode();
                HostedLeafNodes.Remove(nodeID);
                NodeAllocator.DeallocateStruct(nodeID);
            }

            int[] newIDs = SetupNodesForZObjects(zObjectsToCreateNode, out int[] deallocatedNodeIDs);
            HostedLeafNodes.AddRange(newIDs);
            nc.AddFSTNodesIntoSHT(newIDs);

            foreach (var deallocNodeId in deallocatedNodeIDs)
            {
                HostedLeafNodes.Remove(deallocNodeId);
            }
            
            LeafUpdateS2Profiler.End();
            #endregion
            
            #region Step 3: New Connections
            LeafUpdateS3Profiler.Begin();
            /*
             * There are some special logic that needs to be done for this.
             *
             * If a Type 2 object is added to a node, if it was the host of a node
             * it needs to be "un-hosted".  
             * 
             */

            foreach (var zObject in zObjects)
            {
                if (!zObject.FSTUpdateTickForward())
                {
                    continue;
                }
                
                var nearbyNodes = nc.GetIntersectingFSTNodesDist(zObject.ObjectBounds);
                int hostedNodeID = zObject.HostingTreeNode.StructID;
                Vector3 zObjectPosition = zObject.Position;
                if (zObject.TreeObjectType == TreeObjectType.Type2 && hostedNodeID != -1)
                {
                    // A Type 2 object that is hosting it's own node has a slightly different rule to follow
                    bool clearHostObjectNode = false;
                    
                    for (int i = 0; i < nearbyNodes.Length; i++)
                    {
                        if (hostedNodeID == nearbyNodes[i].Item1)
                        {
                            continue;
                        }

                        if (nearbyNodes[i].Item2 > AdoptionDistance)
                        {
                            continue;
                        }
                        
                        ref var nearbyNode = ref NodeAllocator.GetStruct(nearbyNodes[i].Item1);
                        
                        var zObjNode = NodeAllocator.GetStruct(hostedNodeID);
                        nearbyNode.AdoptNode(zObjNode);
                        clearHostObjectNode = true;
                    }
                    
                    if (clearHostObjectNode)
                    {
                        ref var zObjNode = ref NodeAllocator.GetStruct(hostedNodeID);
                        zObjNode.ClearNode();
                        HostedLeafNodes.Remove(hostedNodeID);
                        NodeAllocator.DeallocateStruct(hostedNodeID);
                    }
                }
                else
                {
                    for (int i = 0; i < nearbyNodes.Length; i++)
                    {
                        FSTNodeQR qr = new FSTNodeQR()
                        {
                            StructID = nearbyNodes[i].Item1
                        };

                        if (zObject.AdoptingTreeNodes.Contains(qr) || zObject.AbsorbingTreeNodes.Contains(qr))
                        {
                            continue;
                        }
                        if (nearbyNodes[i].Item2 > AdoptionDistance)
                        {
                            continue;
                        }
                        
                        ref var nearbyNode = ref NodeAllocator.GetStruct(nearbyNodes[i].Item1);
                        Vector3 nearbyNodePos = nearbyNode.NodeCenter;
                        
                        // occlusion check with other nodes.
                        List<FSTNodeQR> existingNodeLinks = new List<FSTNodeQR>(zObject.AdoptingTreeNodes);
                        existingNodeLinks.AddRange(zObject.AbsorbingTreeNodes);
                        
                        bool isOccluded = false;
                        for (int j = 0; j < existingNodeLinks.Count; j++)
                        {
                            var otherNode = NodeAllocator.GetStruct(existingNodeLinks[j].StructID);
                            Vector3 otherNodePos = otherNode.NodeCenter;

                            if (MathLDP.IsBehindAngle(
                                    zObjectPosition, otherNodePos, 
                                    nearbyNodePos, OcclusionAngleThreshold)
                               )
                            {
                                isOccluded = true;
                                break;
                            }
                        }

                        if (isOccluded)
                        {
                            continue;
                        }
                        

                        nearbyNode.AdoptObject(zObject);
                    }
                }
            }
            
            LeafUpdateS3Profiler.End();
            
            #endregion
            
            callNum++;

        }
        #endregion
        


        
        public static int GetThreshold(float linDist){
            if (linDist < AbsorptionDistance)
            {
                return UpdateThreshHostAbsorptionDistance;
            }
            if (linDist < AbsorptionReleaseDistance)
            {
                return UpdateThreshHostAbsorptionReleaseDistance;
            }
            if (linDist < AdoptionDistance)
            {
                return UpdateThreshAdoptionDistance;
            }
            else
            {
                return UpdateThreshAdoptionReleaseDistance;
            }
        }
        
        #region Synchronous Tree Setup
        public void SetupLeafNodesSynchronous(List<ZObject> zObjects)
        {
            /*
             * Step 1): setup leaves for primary "seed" objects.
             *      For the final product, seed objects are objects that should always be "centered"
             *      in a floating origin environment, typically the player and/or the camera.
             *      there should only ever be 1 primary seed.
             * 
             * Step 2) setup leaves for secondary objects.
             *      Similar reasoning to step 1 but lower priority, so for example with my planned game,
             *      other/AI controlled ships.
             * 
             * Steps 1 and 2 will "absorb" other secondary objects, that is they are close enough to the
             * absorber that they won't be affected by floating point problems despite not being "centered".
             * 
             * Step 3) setup leaves for tertiary objects not adopted by any nodes
             *
             * Step 4) iterate through EVERY object to search for nodes to insert to.
             *
             * Step 4b) iterate through type 3 objects again, removing their hosted node if they are
             *          already part of another node.
             */
            var nc = NodeController.Singleton;
            RootNodeID = -1;
            // first we need to group objects
            List<ZObject> typeTwoObjects = new();
            List<ZObject> typeOneObjects = new();
            List<ZObject> typeZeroObjects = new();
            // some access abuse TODO: Profile how good the performance of this is
            List<ZObject>[] accessor = new[] { typeZeroObjects, typeOneObjects, typeTwoObjects };
            foreach (var zobject in zObjects)
            {
                accessor[(int)zobject.TreeObjectType].Add(zobject);
            }

            List<int> LeafNodeIDs = new List<int>(zObjects.Count);
            
            #region Step 1: Type One Object Leaf Setup

            var newNodeIDs = SetupNodesForZObjects(typeZeroObjects, out _);
            LeafNodeIDs.AddRange(newNodeIDs);

            #endregion


            #region Step 2: Type Two Object Leaf Setup

            newNodeIDs = SetupNodesForZObjects(typeOneObjects, out _);
            LeafNodeIDs.AddRange(newNodeIDs);
            
            #endregion
            
            
            #region Step 3: Type Two Object Left setup
            
            newNodeIDs = SetupNodesForZObjects(typeTwoObjects, out _);
            LeafNodeIDs.AddRange(newNodeIDs);
            
            #endregion
            
            nc.AddFSTNodesIntoSHT(LeafNodeIDs);
            
            #region Step 4.

            List<int> nodesToReevaluate = new List<int>();
            
            foreach (var zObject in zObjects)
            {
                InsertIntoNearbyNodes(zObject, nodesToReevaluate);
            }

            #endregion

            #region Step 4.2

            foreach (var nodeID in nodesToReevaluate)
            {
                NodeAllocator.DeallocateStructIfActive(nodeID);
                LeafNodeIDs.Remove(nodeID);
            }

            #endregion
            

            HostedLeafNodes = LeafNodeIDs;
        }

        private void InsertIntoNearbyNodes(ZObject zObject, List<int> nodesToReevaluate)
        {
            NodeController nc = NodeController.Singleton;
            var nearbyFSTNodes = nc.GetIntersectingFSTNodes(zObject.ObjectBounds);
                
            var zObjPos = zObject.Position;
            foreach (var nearbyNodeID in nearbyFSTNodes)
            {
                if (nearbyNodeID == zObject.HostingTreeNode.StructID)
                {
                    // nearby node is the node being hosted by this object.
                    continue;
                }
                
                // check if there's already a relation with this node
                FSTNodeQR check = new FSTNodeQR();
                check.StructID = nearbyNodeID;
                if (zObject.AdoptingTreeNodes.Contains(check))
                {
                    continue;
                }

                int reEvalId = TryAddToNearbyNode(zObject, nearbyNodeID, zObjPos);
                if (reEvalId != -1)
                {
                    nodesToReevaluate.Add(reEvalId);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int TryAddToNearbyNode(ZObject zObject, int nearbyNodeID, Vector3 zObjPos)
        {
            ref FSTNode nearbyNode = ref NodeAllocator.GetStruct(nearbyNodeID);
            return TryAddToNearbyNode(zObject, ref nearbyNode, zObjPos);
        }

        private int TryAddToNearbyNode(ZObject zObject, ref FSTNode nearbyNode, Vector3 zObjPos)
        {
            var adoptingNodes = zObject.AdoptingTreeNodes;
            var nearbyNodePos = nearbyNode.NodeCenter;
            List<int> occludedNodesToRemove = new List<int>();
            bool occlusionTestPassed = true;
            foreach (var existingNodeQR in adoptingNodes)
            {
                if (existingNodeQR.StructID == zObject.HostingTreeNode.StructID)
                {
                    continue;
                }
                var existingNode = NodeAllocator.GetStruct(existingNodeQR.StructID);
                var existingNodePos = existingNode.NodeCenter;

                Vector3 toNearby = nearbyNodePos - zObjPos;
                Vector3 toExisting = existingNodePos - zObjPos;
                if (Vector3.Angle(toNearby, toExisting) > OcclusionAngleThreshold)
                {
                    continue;
                }
                        
                if (MathLDP.IsBehind(zObjPos, nearbyNodePos, existingNodePos))
                {
                    // nearby node occludes the existing one, replace existing with nearby.
                    occludedNodesToRemove.Add(existingNode.StructID);
                }
                else
                {
                    occlusionTestPassed = false;
                    break;
                }
            }

            if (occlusionTestPassed)
            {
                foreach (var nodeID in occludedNodesToRemove)
                {
                    ref var node = ref NodeAllocator.GetStruct(nodeID);
                    node.ReleaseAdoptedObject(zObject);
                }
                        
                float linearDist = MathLDP.GetLinearDistance(nearbyNodePos, zObjPos);
                int threshold = GetThreshold(linearDist);
                nearbyNode.AdoptObject(zObject, threshold, FSTLinkType.Adopted);
                if (zObject.HostingTreeNode.StructID != FSTNodeQR.NULL_VALUE.StructID 
                    && zObject.TreeObjectType == TreeObjectType.Type2 
                    && nearbyNode.IsPrimaryLeaf)
                {
                    return zObject.HostingTreeNode.StructID;
                }
            }

            return -1;
        }

        /// <summary>
        /// Performs a node setup on a provided set of objects, for type 1 and 0 objects
        /// Returns the IDs of leaf nodes created
        /// Does not add nodes into the FST or SHG
        /// </summary>
        /// <param name="zObjectSet"></param>
        /// <returns></returns>
        private int[] SetupNodesForZObjects(List<ZObject> zObjectSet, out int[] deallocatedNodeIDs)
        {
            List<int> returnList = new List<int>(zObjectSet.Count);

            List<int> deallocateNodesList = new(4);
            foreach (var zObject in zObjectSet)
            {
                if (zObject.Absorbed)
                {
                    continue;
                }
                
                int newNodeID = SetupNodeForZObject(zObject, out int[] deallocedNodeIds);
                returnList.Add(newNodeID);
                deallocateNodesList.AddRange(deallocedNodeIds);
            }

            deallocatedNodeIDs = deallocateNodesList.ToArray();
            return returnList.ToArray();
        }

        /// <summary>
        /// Sets up the node for a single ZObject as it's host.
        /// Assumes that the ZObject is not already a host object, or
        /// if it's not a type 0 object that it has not been absorbed.
        /// </summary>
        /// <param name="zObject"></param>
        /// <returns></returns>
        private int SetupNodeForZObject(ZObject zObject, out int[] deallocatedNodes)
        {

            NodeController nc = NodeController.Singleton;
            ref var newNode = ref NodeAllocator.AllocateStruct();
            newNode.InitHostObject(zObject);
            var nodePosition = newNode.NodeCenter;
                
            var nearby = nc.GetNearbyLinearDist2(
                nodePosition,
                AbsorptionDistance
            );

            List<int> possibleDeallocs = new(4);
            
            for (int i = 0; i < nearby.Length; i++)
            {
                if (nearby[i].Item1.ID == zObject.ID)
                {
                    continue;
                }

                var linearDist = nearby[i].Item2;
                int updateThresh = GetThreshold(linearDist);
                if (linearDist < AbsorptionDistance)
                {
                    int otherHostingNodeID = nearby[i].Item1.HostingTreeNode.StructID;
                    if (otherHostingNodeID == -1)
                    {
                        newNode.AbsorbObject(nearby[i].Item1, updateThresh);
                    }
                    else
                    {
                        ref var otherNode = ref NodeAllocator.GetStruct(otherHostingNodeID);
                        newNode.AbsorbNode(ref otherNode);
                        otherNode.ClearNode();
                        NodeAllocator.DeallocateStruct(otherHostingNodeID);
                        possibleDeallocs.Add(otherHostingNodeID);
                    }
                }
            }

            deallocatedNodes = possibleDeallocs.ToArray();
            return newNode.StructID;
        }


        public void ResetBranches()
        {
            foreach (var branchNodeID in BranchNodeIDs)
            {
                NodeAllocator.DeallocateStruct(branchNodeID);
            }
        }
        
        private static readonly ProfilerMarker TreeSetupMainLoop = new("SpatialTree.SetupTreeSynchronous.MainLoop");
        private static readonly ProfilerMarker TreeSetupLoop = new("SpatialTree.SetupTreeSynchronous");

        public void SetupTreeSynchronous()
        {
            TreeSetupLoop.Begin();
            List<int> leafNodeIDs = HostedLeafNodes;
            List<FSTNode> newNodes = new List<FSTNode>(leafNodeIDs.Count);
            if (leafNodeIDs.Count == 0)
            {
                // nothing to setup
                TreeSetupLoop.End();
                return;
            }
            
            if (leafNodeIDs.Count == 1)
            {
                // only 1 node
                ref var newNode = ref NodeAllocator.AllocateStruct();
                ref var targetNode = ref NodeAllocator.GetStruct(leafNodeIDs[0]);
                newNode.Left = targetNode.StructID;
                targetNode.Parent = newNode.StructID;
                newNode.NodeBounds.Encapsulate(targetNode.NodeBounds);
                BranchNodeIDs = new(1);
                BranchNodeIDs.Add(newNode.StructID);
                RootNodeID = newNode.StructID;
                TreeSetupLoop.End();
                return;
            }

            if (leafNodeIDs.Count == 2)
            {
                ref var newNode = ref NodeAllocator.AllocateStruct();
                BranchNodeIDs = new(2);
                Bounds bounds = new Bounds();
                
                ref var targetNode1 = ref NodeAllocator.GetStruct(leafNodeIDs[0]);
                newNode.Left = targetNode1.StructID;
                targetNode1.Parent = newNode.StructID;
                bounds.Encapsulate(targetNode1.NodeBounds);
                
                ref var targetNode2 = ref NodeAllocator.GetStruct(leafNodeIDs[1]);
                newNode.Right = targetNode2.StructID;
                targetNode2.Parent = newNode.StructID;
                bounds.Encapsulate(targetNode2.NodeBounds);
                
                newNode.SetBounds(bounds);
                BranchNodeIDs.Add(newNode.StructID);
                RootNodeID = newNode.StructID;
                TreeSetupLoop.End();
                return;
            }

            Queue<(List<FSTNode>, int, bool)> workQueue = new(16);
            var leafNodes = GetNodes(leafNodeIDs);

            var rootLocalID = SplitNodesIntoBranches(-1,ref newNodes, leafNodes, out var left, out var right);

            workQueue.Enqueue((left, rootLocalID, false));
            workQueue.Enqueue((right, rootLocalID, true));
            
            TreeSetupMainLoop.Begin();
            while (workQueue.Count != 0)
            {
                var (work, parentID, isRight) = workQueue.Dequeue();
                var nodeLocalID = SplitNodesIntoBranches(parentID, ref newNodes, work, out var nLeft, out var nRight);
                if (nodeLocalID == -1)
                {
                    continue;
                }
                var parent = newNodes[parentID];
                if (isRight)
                {
                    parent.Right = nodeLocalID;
                }
                else
                {
                    parent.Left = nodeLocalID;
                }
                newNodes[parentID] = parent;
                if (nLeft.Count > 0)
                {
                    workQueue.Enqueue((nLeft, nodeLocalID, false));
                }

                if (nRight.Count > 0)
                {
                    workQueue.Enqueue((nRight, nodeLocalID, true));
                }
                
            }
            TreeSetupMainLoop.End();
            
            var resultIDs = NodeAllocator.PostAllocateStruct(newNodes.ToArray());
            RootNodeID = resultIDs[0];
            ref var rNode = ref NodeAllocator.GetStruct(resultIDs[0]);
            if (rNode.Left != -1) rNode.Left = resultIDs[rNode.Left];
            if (rNode.Right != -1) rNode.Right = resultIDs[rNode.Right];
            // ignore first node since it should always be the root node.
            for (int i = 1; i < resultIDs.Length; i++)
            {
                ref var node = ref NodeAllocator.GetStruct(resultIDs[i]);
                node.Parent = resultIDs[node.Parent];
                if (node.Left != -1) node.Left = resultIDs[node.Left];
                if (node.Right != -1) node.Right = resultIDs[node.Right];
            }

            for (int i = 0; i < HostedLeafNodes.Count; i++)
            {
                ref var node = ref NodeAllocator.GetStruct(HostedLeafNodes[i]);
                node.Parent = resultIDs[node.Parent];
            }
            BranchNodeIDs = new(resultIDs);
            TreeSetupLoop.End();
        }
        
        private static readonly ProfilerMarker NodeSplitCounter = new("SpatialTree.SplitNodesIntoBranches.SR");
        
        private int SplitNodesIntoBranches(int parentID, ref List<FSTNode> newNodes, List<FSTNode> leafNodes, out List<FSTNode> left, out List<FSTNode> right)
        {
            if (leafNodes.Count == 1)
            {
                // don't do anything, we've reached the leaf. Prepare the node to update parent.
                var nodeID = leafNodes[0].StructID;
                ref var node = ref NodeAllocator.GetStruct(nodeID);
                node.Parent = parentID;
                left = null;
                right = null;
                return -1;
            }

            FSTNode newNode = new FSTNode();
            newNode.InitBlank();
            newNode.Parent = parentID;
            newNode.Left = -1;
            newNode.Right = -1;
            
            newNode.IsRootNode = true;
            Vector3 sum = Vector3.zero;
            Bounds bounds = new Bounds();

            
            foreach (var node in leafNodes)
            {
                bounds.Encapsulate(node.NodeBounds);
                sum += node.NodeCenter;
            }

            newNode.SetBounds(bounds);
            
            if (leafNodes.Count == 2)
            {
                left = new(1);
                left.Add(leafNodes[0]);
                right = new(1);
                right.Add(leafNodes[1]);
            }
            else
            {
                NodeSplitCounter.Begin();
                Vector3 diff = bounds.max - bounds.min;
                int axis = 0;
                //float halfPt;
            
                if (diff.x >= diff.y && diff.x >= diff.z) {
                    axis = 0;
                    //halfPt = sum.x / leafNodes.Count;
                }
                else if (diff.y >= diff.x && diff.y >= diff.z) {
                    axis = 1;
                    //halfPt = sum.y / leafNodes.Count;
                }
                else {
                    axis = 2;
                    //halfPt = sum.z / leafNodes.Count;
                }
                
                // we will sort and use median instead of mean, attempt to force tree balance.
                leafNodes.Sort((a, b) => a.NodeCenter[axis].CompareTo(b.NodeCenter[axis]));

                int count = leafNodes.Count;
                int midPt = count / 2;
                left = new(midPt);
                right = new(count - midPt);
                for (int i = 0; i < midPt; i++)
                {
                    left.Add(leafNodes[i]);
                }
                for (int i = midPt; i < count; i++)
                {
                    right.Add(leafNodes[i]);   
                }
                
            
                // foreach (var node in leafNodes)
                // {
                //     if (node.NodeCenter[axis] < halfPt)
                //     {
                //         left.Add(node);
                //     }
                //     else
                //     {
                //         right.Add(node);
                //     }
                // }
                NodeSplitCounter.End();
            }

            newNodes.Add(newNode);
            int newNodeLID = newNodes.Count - 1;
            return newNodeLID;
        }

        #endregion
        

        #region Struct Accessor and Allocation

        public ref FSTNode GetNode(int id)
        {
            return ref NodeAllocator.GetStruct(id);
        }

        public FSTNode[] GetNodes()
        {
            return NodeAllocator.GetActiveStructs();
        }

        public FSTNode[] GetNodes(int[] ids)
        {
            FSTNode[] nodes = new FSTNode[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                nodes[i] = GetNode(ids[i]);
            }
            return nodes;
        }
        
        public List<FSTNode> GetNodes(List<int> ids)
        {
            int size = ids.Count;
            List<FSTNode> outList = new List<FSTNode>(size);
            for (int i = 0; i < size; i++)
            {
                outList.Add(GetNode(ids[i]));
            }

            return outList;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref FSTNode GetNode(FSTNodeQR qr)
        {
            return ref NodeAllocator.GetStruct(qr.StructID);
        }

        public FSTNode[] GetNodes(FSTNodeQR[] qrs)
        {
            FSTNode[] nodes = new FSTNode[qrs.Length];
            for (int i = 0; i < qrs.Length; i++)
            {
                nodes[i] = GetNode(qrs[i]);
            }
            return nodes;
        }
        
        public List<FSTNode> GetNodes(List<FSTNodeQR> ids)
        {
            int size = ids.Count;
            List<FSTNode> outList = new List<FSTNode>(size);
            for (int i = 0; i < size; i++)
            {
                outList.Add(GetNode(ids[i]));
            }

            return outList;
        }

        private StructAllocator<FSTNode> NodeAllocator = new();
        
        #endregion

        #region Supporting Datatypes
        
        public struct FSTNode : IStructAllocatorObject
        {
            #region Leaf Properties

            public bool IsRootNode;
            public bool IsPrimaryLeaf;
            public bool IsLeaf;

            #endregion

            #region Structure Properties

            public int Left;
            public int Right;
            public int Parent;

            #endregion

            #region Physics Instance Properties
            
            public Vector3 NodeCenter { get; private set; }
            public Bounds NodeBounds { get; private set; }
            
            public HashSet<ZObject> AdoptedObjects;
            public HashSet<ZObject> AbsorbedObjects;
            public HashSet<SpatialHashGrid.SHGNodeQR> GridNodes;
            public ZObject HostObject;
            public int TicksSinceUpdate;
            
            #endregion

            #region Update Rules
            
            public const int UpdateThreshold = 4;
            public int updateTicker;


            public bool TickForwardRebound()
            {
                if (++updateTicker >= UpdateThreshold)
                {
                    updateTicker = 0;
                    return true;
                }

                return false;
            }

            #endregion
            
            public FSTNodeQR GetQR(int threshold, FSTLinkType linkType)
            {
                return new FSTNodeQR()
                {
                    StructID = StructID,
                    Threshold = threshold,
                    TicksSinceUpdate = 0,
                    CurrentLinkType = linkType
                };
            }

            public void AdoptObject(ZObject zObject, int threshold = 2, FSTLinkType linkType=FSTLinkType.Adopted)
            {
                if (zObject.ID == HostObject.ID)
                {
                    return;
                }
                
                if (AdoptedObjects.Count == 0)
                {
                    Vector3 size = Vector3.one * (IsPrimaryLeaf ? 2 * AdoptionDistance : 2 * AbsorptionDistance);
                    NodeBounds = new Bounds(NodeCenter, size);
                }

                AdoptedObjects.Add(zObject);
                NodeBounds.Encapsulate(zObject.ObjectBounds);
                zObject.AdoptingTreeNodes.Add(GetQR(threshold, linkType));
            }

            public void ReleaseAdoptedObject(ZObject zObject)
            {
                AdoptedObjects.Remove(zObject);
                AbsorbedObjects.Remove(zObject);
                var qr = GetQR(-1, FSTLinkType.None);
                zObject.AbsorbingTreeNodes.Remove(qr);
                zObject.AdoptingTreeNodes.Remove(qr);
            }
            
            /// <summary>
            /// Adopts the other node.
            /// </summary>
            /// <param name="otherNode"></param>
            public void AdoptNode(FSTNode otherNode)
            {
                // First, the host object needs to be absorbed.
                var otherHost = otherNode.HostObject;
                var otherHostLinDist = MathLDP.GetLinearDistance(otherHost.Position, NodeCenter);
                int threshold = GetThreshold(otherHostLinDist);
                AdoptObject(otherHost, threshold);
                
                // for simplicity, any "absorbed" ZObjects in otherNode will also
                // be absorbed by this node. 

                var zObjectsAbsorbedCpy = otherNode.AbsorbedObjects.ToArray();
                foreach (var absorbedZObject in zObjectsAbsorbedCpy)
                {
                    if (absorbedZObject.ID == HostObject.ID)
                    {
                        continue;
                    }
                    AdoptObject(absorbedZObject, 2);
                }
                var zObjectsInNodeCpy = otherNode.AdoptedObjects.ToArray();
                foreach (var zObject in zObjectsInNodeCpy)
                {
                    if (zObject.ID == HostObject.ID)
                    {
                        continue;
                    }
                    AdoptObject(zObject, 2);
                }
            }

            /// <summary>
            /// Absorbs an object, does not remove or clear an object's hosted node if it exists
            /// </summary>
            /// <param name="zObject"></param>
            /// <param name="threshold"></param>
            /// <returns></returns>
            public void AbsorbObject(ZObject zObject, int threshold = UpdateThreshHostAbsorptionDistance)
            {
                if (zObject.ID == HostObject.ID)
                {
                    return;
                }
                AbsorbedObjects.Add(zObject);
                NodeBounds.Encapsulate(zObject.ObjectBounds);
                var nodeQR = GetQR(threshold, FSTLinkType.Absorbed);
                zObject.AdoptingTreeNodes.Remove(nodeQR);
                zObject.AbsorbingTreeNodes.Add(nodeQR);
            }

            public void ReleaseAbsorbedObject(ZObject zObject)
            {
                AbsorbedObjects.Remove(zObject);
                var qr = GetQR(-1, FSTLinkType.None);
                zObject.AbsorbingTreeNodes.Remove(qr);
            }
            
            
            /// <summary>
            /// Absorbs another node
            /// For simplicity, all objects in the other node is absorbed even if they're outside
            /// the absorption or adoption range.
            /// Other node is not deallocated or cleared after this, nor is the node
            /// released from the host.
            /// 
            /// </summary>
            /// <param name="otherNode"></param>
            /// <returns></returns>
            public void AbsorbNode(ref FSTNode otherNode)
            {
                // First, the host object needs to be absorbed.
                var otherHost = otherNode.HostObject;
                var otherHostLinDist = MathLDP.GetLinearDistance(otherHost.Position, NodeCenter);
                int threshold = GetThreshold(otherHostLinDist);
                AbsorbObject(otherHost, threshold);
                
                // for simplicity, any "absorbed" ZObjects in otherNode will also
                // be absorbed by this node. 

                var zObjectsAbsorbedCpy = otherNode.AbsorbedObjects.ToArray();
                foreach (var absorbedZObject in zObjectsAbsorbedCpy)
                {
                    AbsorbObject(absorbedZObject, 2);
                }
                var zObjectsInNodeCpy = otherNode.AdoptedObjects.ToArray();
                foreach (var zObject in zObjectsInNodeCpy)
                {
                    AbsorbObject(zObject, 2);
                }
            }

            public void SetBounds(Bounds bounds)
            {
                NodeBounds = bounds;
            }
            
            public void UpdateBounds()
            {
                NodeCenter = HostObject.Position;
                Vector3 baseSize = Vector3.one;
                if (!IsPrimaryLeaf && AdoptedObjects.Count == 0 && AbsorbedObjects.Count == 0)
                {
                    baseSize *= (2 * AbsorptionDistance);
                }
                else
                {
                    baseSize *= (2 * AdoptionDistance);
                }
                
                Bounds bounds = new Bounds(NodeCenter, baseSize);

                foreach (var zObject in AdoptedObjects)
                {
                    bounds.Encapsulate(zObject.ObjectBounds);
                }

                // foreach (var zObject in AbsorbedObjects)
                // {
                //     bounds.Encapsulate(zObject.ObjectBounds);
                // }

                NodeBounds = bounds;
            }
            
            public void InitHostObject(ZObject zObject)
            {
                if (zObject.IsHostOfTreeNode)
                {
                    throw new Exception("Ya Fek'd");
                }
                Init();
                IsPrimaryLeaf = zObject.TreeObjectType != TreeObjectType.Type2;
                IsLeaf = true;
                
                HostObject = zObject;
                NodeCenter = zObject.Position;
                Vector3 size = Vector3.one * (IsPrimaryLeaf ? 2 * AdoptionDistance : 2 * AbsorptionDistance);
                NodeBounds = new Bounds(NodeCenter, size);
                zObject.HostingTreeNode = GetQR(-1, FSTLinkType.None);
            }

            public void ClearNode()
            {
                if (AbsorbedObjects != null)
                {
                    var zObjAbsorbedCpy = AbsorbedObjects.ToArray();
                    foreach (var absorbedZObject in zObjAbsorbedCpy)
                    {
                        ReleaseAdoptedObject(absorbedZObject);
                    }
                }

                if (AdoptedObjects != null)
                {
                    var zObjAdoptedCpy = AdoptedObjects.ToArray();
                    foreach (var adoptedZObject in zObjAdoptedCpy)
                    {
                        ReleaseAdoptedObject(adoptedZObject);
                    }
                }

                if (GridNodes != null)
                {
                    var nc = NodeController.Singleton;
                    foreach (var gridNodeQR in GridNodes)
                    {
                        ref var gridNode = ref nc.GetSHGNode(gridNodeQR.ElementID);
                        gridNode.FSTNodeIDs.Remove(StructID);
                    }
                }

                if (HostObject != null)
                {
                    if (HostObject.HostingTreeNode.StructID == StructID)
                    {
                        HostObject.HostingTreeNode = FSTNodeQR.NULL_VALUE;
                        HostObject = null;
                    }
                }
            }

            public bool TickForward()
            {
                if (++TicksSinceUpdate == LookForObjectsToAbsorbFrequency)
                {
                    TicksSinceUpdate = 0;
                    return true;
                }

                return false;
            }
            
            public void InitBlank()
            {
                HostObject = null;
                Left = -1;
                Right = -1;
                Parent = -1;
                IsRootNode = false;
                IsPrimaryLeaf = false;
                IsLeaf = false;
            }

            #region Interface Implementation

            public void Init()
            {
                InitBlank();
                TicksSinceUpdate = 0;
                AdoptedObjects = new();
                AbsorbedObjects = new HashSet<ZObject>();
                GridNodes = new(new SpatialHashGrid.SHGNodeQRLightCompare());
            }
            
            public void Destroy()
            {
                ClearNode();
                InitBlank();
            }

            public int StructID { get; set; }

            #endregion

            #region Debug Functions

            public void DrawBranchBoundaries(Color color)
            {
                DebugLDP.DrawBox(NodeBounds, color, Time.fixedDeltaTime);
            }

            private static Color OutsideBoundColor = Color.green;
            private static Color InsideBoundColor = Color.red;
            
            public void DebugDrawBoundaries(bool detailedBoundaries = true)
            {
                if (detailedBoundaries)
                {
                    if (!IsPrimaryLeaf && AdoptedObjects.Count == 0 && AbsorbedObjects.Count == 0)
                    {
                        DrawBoundariesHalf();
                    }
                    else
                    {
                        DrawBoundariesFull();
                    }
                    foreach (var zObject in AdoptedObjects)
                    {
                        var zObjPosition = zObject.Position;

                        Color color = AbsorbedObjects.Contains(zObject) ? InsideBoundColor : OutsideBoundColor;
                        Debug.DrawLine(zObjPosition, NodeCenter, color, Time.fixedDeltaTime);
                    }
                }
            }

            private void DrawBoundariesHalf()
            {
                Vector3 upprExtent = Vector3.one * AbsorptionReleaseDistance;
                Vector3 lowerExtent = Vector3.one * AbsorptionDistance;

                Bounds lowerBounds = new Bounds(NodeCenter, upprExtent * 2);
                Bounds upperBounds = new Bounds(NodeCenter, lowerExtent * 2);
                        
                DebugLDP.DrawBox(lowerBounds, OutsideBoundColor, Time.fixedDeltaTime);
                DebugLDP.DrawBox(upperBounds, OutsideBoundColor, Time.fixedDeltaTime);
            }

            private void DrawBoundariesFull()
            {
                Vector3 upprExtent1 = Vector3.one * AdoptionReleaseDistance;
                Vector3 lowerExtent1 = Vector3.one * AdoptionDistance;

                Bounds lowerBounds = new Bounds(NodeCenter, upprExtent1 * 2);
                Bounds upperBounds = new Bounds(NodeCenter, lowerExtent1 * 2);
                        
                DebugLDP.DrawBox(lowerBounds, OutsideBoundColor, Time.fixedDeltaTime);
                DebugLDP.DrawBox(upperBounds, OutsideBoundColor, Time.fixedDeltaTime);
                        
                Vector3 upprExtent2 = Vector3.one * AbsorptionReleaseDistance;
                Vector3 lowerExtent2 = Vector3.one * AbsorptionDistance;

                Bounds lowerBounds2 = new Bounds(NodeCenter, upprExtent2 * 2);
                Bounds upperBounds2 = new Bounds(NodeCenter, lowerExtent2 * 2);
                        
                DebugLDP.DrawBox(lowerBounds2, InsideBoundColor, Time.fixedDeltaTime);
                DebugLDP.DrawBox(upperBounds2, InsideBoundColor, Time.fixedDeltaTime);
            }

            #endregion
            
        }

        public struct FSTNodeQR
        {
            [FormerlySerializedAs("ElementID")] public int StructID;
            public int TicksSinceUpdate;
            public int Threshold;
            [FormerlySerializedAs("CurrentLinkStatus")] public FSTLinkType CurrentLinkType;
            public override int GetHashCode()
            {
                return StructID;
            }

            public static readonly FSTNodeQR NULL_VALUE = new FSTNodeQR()
            {
                StructID = -1,
                Threshold = -1,
                TicksSinceUpdate = -1
            };

            /// <summary>
            /// Ticks the threshold counters forward, returns true if threshold was reached
            /// </summary>
            /// <returns></returns>
            public bool TickForward()
            {
                if (++TicksSinceUpdate >= Threshold)
                {
                    TicksSinceUpdate = 0;
                    return true;
                }
                return false;
            }
        }
        
        public class FSTNodeQRLightCompare : IEqualityComparer<FSTNodeQR>
        {
            public bool Equals(FSTNodeQR x, FSTNodeQR y)
            {
                return x.StructID == y.StructID;
            }

            public int GetHashCode(FSTNodeQR obj)
            {
                return obj.GetHashCode();
            }
        }

        public enum FSTLinkType
        {
            None,
            Absorbed,
            Adopted
        }
        

        #region Asynchronous Operations

        public enum TreeObjectType
        {
            Type0,
            Type1,
            Type2
        }
        
        #endregion

        public struct LeafNodeCrawlJob : IJob
        {
            public void Execute()
            {
                throw new System.NotImplementedException();
            }
        }
        #endregion


        #region Debugging Methods
        
        public Color gradientColor1 = new Color(45/255, 255/255f, 45/255f);
        public Color gradientColor2 = new Color(250/255f, 25/255f, 25/255f);
        

        public void DebugDrawDepth(int minDepth, int width)
        {
            if (RootNodeID == -1)
            {
                return;
            }
            
            var colors = new GradientColorKey[2];
            colors[0] = new GradientColorKey(gradientColor1, -0.001f);
            colors[1] = new GradientColorKey(gradientColor2, 1.001f);
            
            var alphas = new GradientAlphaKey[2];
            alphas[0] = new GradientAlphaKey(1.0f, -1.0f);
            alphas[1] = new GradientAlphaKey(1.0f, 2.0f);

            var gradient = new Gradient();
            gradient.SetKeys(colors, alphas);;
        
            int currentDepth = 0;
            Queue<(int, int)> nodeIDQueue = new();
            int maxDepth = minDepth + width;
            nodeIDQueue.Enqueue((RootNodeID, 0));
            while (nodeIDQueue.Count != 0)
            {
                var (nodeID, depth) = nodeIDQueue.Dequeue();

                if (depth > maxDepth)
                {
                    continue;
                }
                var targetNode = GetNode(nodeID);
                if (depth >= minDepth)
                {
                    int k = depth - minDepth;
                    float z = (float)k / width;
                    var color = gradient.Evaluate(z);
                    targetNode.DrawBranchBoundaries(color);
                }
                if (targetNode.Left != -1) nodeIDQueue.Enqueue((targetNode.Left, depth + 1));
                if (targetNode.Right != -1) nodeIDQueue.Enqueue((targetNode.Right, depth + 1));
            }
        }

        #endregion
        
        
    }
}