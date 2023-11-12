using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LongDistancePhysics;
using UnityEngine.Serialization;

namespace ObjectZoning1
{
    [RequireComponent(typeof(StaticBoundingVolume))]
    [RequireComponent(typeof(TrackedObjectController))]
    public class DynamicBoundingVolume : MonoBehaviour
    {
        /*
         * General rules for a DBV:
         * Leaf nodes can overlap in coverage with other nodes.
         * Branch Nodes cannot overlap with other branch nodes.
         * 
         */
        
        
        private DBVNode _root;

        public DBVNode GetRoot()
        {
            return _root;
        }
        
        public const int MaxElementsPerNode = 2;

        public StaticBoundingVolume staticBoundingVolume;
        public TrackedObjectController trackedObjectController;

        private void Awake()
        {
            staticBoundingVolume = GetComponent<StaticBoundingVolume>();
            trackedObjectController = GetComponent<TrackedObjectController>();
        }

        /// <summary>
        /// DFS Insertion into the tree.
        ///
        /// Not intended to be used to Initialize the tree.
        /// </summary>
        /// <param name="trackedObject"></param>
        public void InsertTrackedObject(TrackedObject trackedObject)
        {

            if (_root == null)
            {
                _root = DBVNode.InitializeNode(this);
                _root.InsertObjectIntoEmptyNode(trackedObject);
                return;
            }
            
            switch (trackedObject.ObjectZoningPriority)
            {
                case 0:
                    InsertPriority0(trackedObject);
                    break;
                case 1:
                    InsertPriority1(trackedObject);
                    break;
                case 2:
                    InsertPriority2(trackedObject);
                    break;
                default:
                    InsertPriority2(trackedObject);
                    break;
            }
        }
        
        private void InsertPriority0(TrackedObject priorityTwoObject)
        {
            
        }
        private void InsertPriority1(TrackedObject priorityTwoObject)
        {
            
        }
        private void InsertPriority2(TrackedObject priorityTwoObject)
        {
            
        }

        public void Reset()
        {
            if (_root != null)
            {
                _root.Clear();
                _root = null;
            }
        }
        


        public void SetupDynamicBoundingVolume(List<TrackedObject>[] objectsListArray)
        {
            List<TrackedObject> priorityZeroList = objectsListArray[0];
            List<TrackedObject> priorityOneList = objectsListArray[1];
            List<TrackedObject> priorityTwoList = objectsListArray[2];

            HashSet<DBVNode> newLeaves = SetupLeafNodesFromList(objectsListArray);
            
            SetupTreeFromLeaves(newLeaves);
            InsertRemainingPriority2IntoTree(priorityTwoList);
        }

        private HashSet<DBVNode> SetupLeafNodesFromList(List<TrackedObject>[] objectsListArray)
        {
            List<TrackedObject> priorityZeroList = objectsListArray[0];
            List<TrackedObject> priorityOneList = objectsListArray[1];
            List<TrackedObject> priorityTwoList = objectsListArray[2];

            HashSet<DBVNode> newLeaves = new();

            HashSet<TrackedObject> absorbedPri1Objs = new HashSet<TrackedObject>();

            foreach (var priorityZeroObj in priorityZeroList)
            {
                // each priority 0 object requires it's own leaf node.

                var xyz = priorityZeroObj.XYZPos;
                var proximityList = staticBoundingVolume.GetObjectsInProximity(xyz, 2);
                var objPos = priorityZeroObj.position;
                
                DBVNode newLeaf = DBVNode.InitializeNode(this);
                newLeaf.InsertObjectIntoEmptyNode(priorityZeroObj);

                staticBoundingVolume.InsertDBVNode(newLeaf);
                
                foreach (var proxObj in proximityList)
                {
                    if (proxObj.ObjectZoningPriority == 0)
                    {
                        continue;
                    }
                    else if (proxObj.ObjectZoningPriority == 1)
                    {
                        if (MathLDP.WithinLinearDistance(
                                proxObj.position, 
                                objPos, 
                                TrackedObjectController.QuarterZoneWidth)
                            )
                        {
                            if (proxObj.AddToDBV(newLeaf))
                            {
                                absorbedPri1Objs.Add(proxObj);
                            }
                        }
                    }
                    else if (proxObj.ObjectZoningPriority == 2)
                    {
                        if (MathLDP.WithinLinearDistance(
                                proxObj.position, 
                                objPos, 
                                TrackedObjectController.BaseZoneWidth)
                           )
                        {
                            proxObj.AddToDBV(newLeaf);
                        }
                    }
                }
                newLeaves.Add(newLeaf);
            }

            foreach (var priorityOneObj in priorityOneList)
            {
                // priority 2 objects gets it's own leaf unless it is already
                // been absorbed by another zone.
                if (absorbedPri1Objs.Contains(priorityOneObj))
                {
                    continue;
                }
                
                var xyz = priorityOneObj.XYZPos;
                var proximityList = staticBoundingVolume.GetObjectsInProximity(xyz, 2);
                var objPos = priorityOneObj.position;
                
                DBVNode newLeaf = DBVNode.InitializeNode(this);
                newLeaf.InsertObjectIntoEmptyNode(priorityOneObj);

                staticBoundingVolume.InsertDBVNode(newLeaf);
                
                foreach (var proxObj in proximityList)
                {
                    if (proxObj.ObjectZoningPriority == 0)
                    { // ignore priority 1 objects
                        continue;
                    }
                    else if (proxObj.ObjectZoningPriority == 1)
                    {
                        if (MathLDP.WithinLinearDistance(
                                proxObj.position, 
                                objPos, 
                                TrackedObjectController.QuarterZoneWidth)
                           )
                        {
                            proxObj.AddToDBV(newLeaf);
                            absorbedPri1Objs.Add(proxObj);
                        }
                    }
                    else if (proxObj.ObjectZoningPriority == 2)
                    {
                        if (MathLDP.WithinLinearDistance(
                                proxObj.position, 
                                objPos, 
                                TrackedObjectController.BaseZoneWidth)
                           )
                        {
                            proxObj.AddToDBV(newLeaf);
                        }
                    }
                }
                newLeaves.Add(newLeaf);
            }

            return newLeaves;
        }


        private void SetupTreeFromLeaves(HashSet<DBVNode> leafNodes)
        {
            List<DBVNode> leafNodeList = leafNodes.ToList();
            Stack<(DBVNode, List<DBVNode>)> nodeQueue = new();
            
            _root = DBVNode.InitializeNode(this);
            nodeQueue.Push((_root, leafNodeList));

            while (nodeQueue.Count != 0)
            {
                (DBVNode targetNode, List<DBVNode> leaves) = nodeQueue.Pop();

                int axis = GetMidpointAndAxis(leaves, out var mid, out var min, out var max);

                List<DBVNode> left = new List<DBVNode>();
                List<DBVNode> right = new List<DBVNode>();
                
                foreach (var leaf in leaves)
                {
                    if (leaf.Mid[axis] <= mid[axis])
                    {
                        left.Add(leaf);
                    }
                    else
                    {
                        right.Add(leaf);
                    }
                }

                if (left.Count > 1)
                {
                    DBVNode leftNode = DBVNode.InitializeNode(this, targetNode);
                    nodeQueue.Push((leftNode, left));
                    targetNode.Left = leftNode;
                }
                else if (left.Count == 1)
                {
                    targetNode.Left = left[0];
                }

                if (right.Count > 1)
                {
                    DBVNode rightNode = DBVNode.InitializeNode(this, targetNode);
                    nodeQueue.Push((rightNode, right));
                    targetNode.Right = rightNode;
                }
                else if (right.Count == 1)
                {
                    targetNode.Right = right[0];
                }

                targetNode.Min = min;
                targetNode.Max = max;
            }
        }

        private void InsertRemainingPriority2IntoTree(List<TrackedObject> priorityTwoList)
        {
            foreach (var trackedObject in priorityTwoList)
            {
                if (trackedObject.dynamicZones.Count == 0)
                {
                    InsertPriority2(trackedObject);
                }
            }
        }

        private static int GetMidpointAndAxis(
            List<DBVNode> nodes, 
            out Vector3 midpoint, 
            out Vector3 min, 
            out Vector3 max
            )
        {
            midpoint = Vector3.zero;
            min = Vector3.positiveInfinity;
            max = Vector3.negativeInfinity;
            
            foreach (var dbvNode in nodes)
            {
                midpoint += dbvNode.Mid;
                min = Vector3.Min(dbvNode.Min, min);
                max = Vector3.Max(dbvNode.Max, max);
            }

            midpoint /= nodes.Count;
            Vector3 diff = max - min;
            float maxV = Math.Max(Math.Max(diff.x, diff.y), diff.z);
            if (maxV == diff.x)
            {
                return 0;
            }
            if (maxV == diff.y)
            {
                return 1;
            }
            return 2;
        }
        
        public GameObject boundaryMarkerPrefab;
        public GameObject centerMarkerPrefab;
        
        
        public class DBVNode
        {
            public DBVNode Parent;

            public DBVNode Left;
            public DBVNode Right;
            
            public Vector3 Min = Vector3.positiveInfinity;
            public Vector3 Max = Vector3.negativeInfinity;
            public Vector3 Mid => CenterObject == null ? (Min + Max) / 2 : CenterObject.position;
            
            public readonly HashSet<TrackedObject> TrackedObjects = new ();
            public TrackedObject CenterObject;

            public DynamicBoundingVolume DBVManager;
            
            public LineRenderer BoundaryMarker1;
            public LineRenderer BoundaryMarker2;
            public LineRenderer CenterMarker;
            
            private static readonly Queue<DBVNode> inactiveNodes = new();

            public bool IsEmpty => Left == null && Right == null && TrackedObjects.Count == 0;
            public bool IsLeaf => Left == null && Right == null;


            private DBVNode()
            {
                
            }
            
            public static void AllocateNodes(DynamicBoundingVolume manager, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    var newNode = DBVNode.InitializeNewNode(manager);
                    newNode.DBVManager = null;
                
                    inactiveNodes.Enqueue(newNode);
                }
            }
            
            public void Clear()
            {
                foreach (var trackedObject in TrackedObjects)
                {
                    trackedObject.dynamicZones.Remove(this);
                }
                
                if (Left != null)
                {
                    Left.Clear();
                }

                if (Right != null)
                {
                    Right.Clear();
                }

                CenterObject = null;
                TrackedObjects.Clear();

                Min = Vector3.positiveInfinity;
                Max = Vector3.negativeInfinity;
                
                HideBoundaryMarkers();
                
                inactiveNodes.Enqueue(this);
            }
            
            public void ShowBoundaryMarkers()
            {
                var corners = GetCornerSets();
                
                BoundaryMarker1.startColor = Color.green;
                BoundaryMarker1.endColor = Color.green;
                BoundaryMarker1.positionCount = corners.Item1.Length;
                BoundaryMarker1.SetPositions(corners.Item1);
                
                BoundaryMarker2.startColor = Color.green;
                BoundaryMarker2.endColor = Color.green;
                BoundaryMarker2.positionCount = corners.Item2.Length;
                BoundaryMarker2.SetPositions(corners.Item2);
            }
            
            public void HideBoundaryMarkers()
            {
                BoundaryMarker1.positionCount = 0;
                
                BoundaryMarker2.positionCount = 0;
            }

            public static DBVNode InitializeNewNode(DynamicBoundingVolume manager,
                DBVNode parent = null)
            {
                DBVNode returnNode = new()
                {
                    Parent = parent,
                    BoundaryMarker1 = Instantiate<GameObject>(manager.boundaryMarkerPrefab).GetComponent<LineRenderer>(),
                    BoundaryMarker2 = Instantiate<GameObject>(manager.boundaryMarkerPrefab).GetComponent<LineRenderer>(),
                    CenterMarker = Instantiate<GameObject>(manager.centerMarkerPrefab).GetComponent<LineRenderer>(),
                    DBVManager = manager
                };

                return returnNode;
            }
            
            public static DBVNode InitializeNode(
                DynamicBoundingVolume manager,
                DBVNode parent = null)
            {
                if (inactiveNodes.Count == 0)
                {
                    DBVNode returnNode = new()
                    {
                        Parent = parent,
                        BoundaryMarker1 = Instantiate<GameObject>(manager.boundaryMarkerPrefab).GetComponent<LineRenderer>(),
                        BoundaryMarker2 = Instantiate<GameObject>(manager.boundaryMarkerPrefab).GetComponent<LineRenderer>(),
                        CenterMarker = Instantiate<GameObject>(manager.centerMarkerPrefab).GetComponent<LineRenderer>(),
                        DBVManager = manager
                    };

                    return returnNode;
                }
                else
                {
                    DBVNode returnNode = inactiveNodes.Dequeue();
                    returnNode.Parent = parent;
                    returnNode.DBVManager = manager;
                    return returnNode;
                }
            }


            
            public int CenterObjectPriority => CenterObject == null ? 2 : CenterObject.ObjectZoningPriority;
            
            
            /// <summary>
            /// Inserts an object into an empty nodes
            ///
            /// Assumes that the node is empty
            /// </summary>
            /// <param name="trackedObject"></param>
            internal void InsertObjectIntoEmptyNode(TrackedObject trackedObject)
            {
                CenterObject = trackedObject;
                Vector3 offset = Vector3.one * TrackedObjectController.BaseZoneWidth;
                Vector3 position = CenterObject.position;
                Max = position + offset;
                Min = position - offset;
            }
            
            public Vector3[] Corners => new Vector3[]
            {
                Min,                        // 1
                new(Min.x, Min.y, Max.z),   // 2
                new(Max.x, Min.y, Max.z),   // 3
                new(Max.x, Min.y, Min.z),   // 4
                new(Min.x, Max.y, Min.z),   // 5
                new(Min.x, Max.y, Max.z),   // 6
                Max,                        // 7
                new(Max.x, Max.y, Min.z)    // 8
            };

            public (Vector3[], Vector3[]) GetCornerSets()
            {
                var corners = Corners;
                Vector3[] set1 = new[]
                {
                    corners[5],
                    corners[4],
                    corners[0],
                    corners[1],
                    corners[5],
                    corners[6],
                    corners[2],
                    corners[1]
                };
                Vector3[] set2 = new[]
                {
                    corners[7],
                    corners[3],
                    corners[2],
                    corners[6],
                    corners[7],
                    corners[4],
                    corners[0],
                    corners[3]
                };
                return (set1, set2);
            }
        }
    }
}