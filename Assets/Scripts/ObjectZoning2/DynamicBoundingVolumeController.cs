using System;
using System.Collections.Generic;
using System.Linq;
using LongDistancePhysics;
using UnityEngine;

namespace ObjectZoning2
{
    [RequireComponent(typeof(StaticBoundingVolumeController))]
    public class DynamicBoundingVolumeController : MonoBehaviour
    {
        private StaticBoundingVolumeController _sbv;
        public HashSet<DynamicBoundingVolumeNode> Nodes = new HashSet<DynamicBoundingVolumeNode>();
        
        public GameObject boundaryMarkerPrefab;

        private void Awake()
        {
            _sbv = GetComponent<StaticBoundingVolumeController>();
        }

        public int zoningMode = 0; // 0 simple.

        private void FixedUpdate()
        {
            foreach (var dbvNode in Nodes)
            {
                dbvNode.UpdateBounds();
            }
        }

        public void SetupBoundingVolumes(List<TrackedObject> trackedObjectsList)
        {
            switch (zoningMode)
            {
                case 1:
                    InitLeafNodesMode1(trackedObjectsList);
                    break;
            }
        }


        public void InitLeafNodesMode1(List<TrackedObject> trackedObjectsList)
        {
            /*
             * For mode 1, we simply create all the necessary nodes, that is priority 0 and priority 1,
             * along with absorbing certain priority 1 objects WITHOUT populating these nodes.
             *
             * Next, we take all the priority 2 objects and attempt to insert them into
             * existing nodes.
             *
             * If an object can be inserted into multiple nodes, if the nodes are considered
             * "inline" with each other (in this case angular separation < 60 degrees), then
             * an object will be inserted ONLY into the closer node.
             */
            
            List<TrackedObject> priorityZero = new List<TrackedObject>();
            List<TrackedObject> priorityOne = new List<TrackedObject>();
            List<TrackedObject> priorityTwo = new List<TrackedObject>();
            
            foreach (var trackedObject in trackedObjectsList)
            {
                switch (trackedObject.ObjectZoningPriority)
                {
                    case 0:
                        priorityZero.Add(trackedObject);
                        break;
                    case 1:
                        priorityOne.Add(trackedObject);
                        break;
                    case 2:
                        priorityTwo.Add(trackedObject);
                        break;
                }
            }
            
            foreach (var priZeroObj in priorityZero)
            {
                var newNode = DynamicBoundingVolumeNode.InitializeNode(this, priZeroObj);
                Nodes.Add(newNode);
                _sbv.AddDBVNode(newNode);
            }
            
            foreach (var priOneObj in priorityOne)
            {
                // check if any collision with existing nodes the object belongs in
                // for priority one objects, a collision is within a linear distance of QuarterBaseWidth
                // if a collision occurs, a new node is not created and instead the object is attempted
                // to be inserted according to mode 1 insertion rules.
                
                var sbvNode = priOneObj.staticZone;
                bool zoneInsertion = false;
                foreach (var checkDynamicNode in sbvNode.dbvNodes)
                {
                    if (checkDynamicNode.CheckCollision(priOneObj))
                    {
                        DynamicBoundingVolumeNode possibleReplacementNode;
                        if (Mode1AngularOcclusionCheck(checkDynamicNode, priOneObj, out possibleReplacementNode))
                        {
                            possibleReplacementNode.RemoveObject(priOneObj);
                            zoneInsertion = true;
                        }
                    }
                }

                if (!zoneInsertion)
                {
                    var newNode = DynamicBoundingVolumeNode.InitializeNode(this, priOneObj);
                    Nodes.Add(newNode);
                    _sbv.AddDBVNode(newNode);
                }
            }


            foreach (var priTwoObj in priorityTwo)
            {
                // check if any collision with existing nodes the object belongs in
                // for priority one objects, a collision is within a linear distance of QuarterBaseWidth
                // if a collision occurs, a new node is not created and instead the object is attempted
                // to be inserted according to mode 1 insertion rules.
                
                var sbvNode = priTwoObj.staticZone;
                bool zoneInsertion = false;
                foreach (var checkDynamicNode in sbvNode.dbvNodes)
                {
                    if (checkDynamicNode.CheckCollision(priTwoObj))
                    {
                        if (Mode1AngularOcclusionCheck(checkDynamicNode, priTwoObj, out var possibleReplacementNode))
                        {
                            if (possibleReplacementNode != null)
                            {
                                possibleReplacementNode.RemoveObject(priTwoObj);
                                checkDynamicNode.AddObject(priTwoObj);
                                zoneInsertion = true;
                            }
                        }
                        else
                        {
                            checkDynamicNode.AddObject(priTwoObj);
                            zoneInsertion = true;
                        }
                    }
                }

                if (!zoneInsertion)
                {
                    var newNode = DynamicBoundingVolumeNode.InitializeNode(this, priTwoObj);
                    Nodes.Add(newNode);
                    _sbv.AddDBVNode(newNode);
                }
            }
        }

        public void UpdateLeavesMode1()
        {
            foreach (var leaf in Nodes)
            {
                foreach (var trackedObject in leaf.TrackedObjects)
                {
                    if (trackedObject == leaf.CenterObject)
                    {
                        // ignore center object since leaf travels with the center object
                        continue;
                    }

                    if (!leaf.CheckCollision(trackedObject))
                    {
                        
                    }
                }
            }
        }


        /// <summary>
        /// Returns true if there was a collision.
        ///
        /// if there is a collision and no insertion is
        /// valid, then collisionNode will be null.
        /// Otherwise, collision node will contain the node
        /// to be replaced.
        /// </summary>
        /// <param name="checkNode"></param>
        /// <param name="trackedObject"></param>
        /// <param name="collisionNode"></param>
        /// <returns></returns>
        private bool Mode1AngularOcclusionCheck(
            DynamicBoundingVolumeNode checkNode, 
            TrackedObject trackedObject, 
            out DynamicBoundingVolumeNode collisionNode)
        {
            if (trackedObject.dynamicZones.Count == 0)
            {
                collisionNode = null;
                return false;
            }
            if (trackedObject.dynamicZones.Contains(checkNode))
            {
                collisionNode = null;
                return true;
            }
            var diff1 = checkNode.Mid - trackedObject.position;
            DynamicBoundingVolumeNode smallest = null;
            float smallestAngularSep = float.PositiveInfinity;
            foreach (var otherNode in trackedObject.dynamicZones)
            {
                var diff2 = otherNode.Mid - trackedObject.position;
                var angularSep = Vector3.Angle(diff1, diff2);
                if (angularSep < TrackedObjectController.MultiZoneAngThresh)
                {
                    if (diff1.sqrMagnitude > diff2.sqrMagnitude &&
                        angularSep < TrackedObjectController.MultZoneAngThreshLwr)
                    {
                        collisionNode = null;
                        return true;
                    }
                    if (diff1.sqrMagnitude < diff2.sqrMagnitude)
                    {
                        if (angularSep < smallestAngularSep)
                        {
                            smallestAngularSep = angularSep;
                            smallest = otherNode;
                        }
                    }
                    
                    break; 
                }
            }
            
            collisionNode = smallest;
            return collisionNode == null;
        }
    }
}