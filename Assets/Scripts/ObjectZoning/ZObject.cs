using System;
using System.Collections.Generic;
using System.Linq;
using LongDistancePhysics;
using LongDistancePhysics.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Object = System.Object;

namespace ObjectZoning
{
    public class ZObject : MonoBehaviour
    {
        
        public float width = 1.0f;

        public Vector3 Position
        {
            get;
            private set;
        }

        public void UpdatePosition()
        {
            Position = transform.position;
            var itemsArray = new FloatingSpatialTree.FSTNodeQR[AdoptingTreeNodes.Count];

            int i = 0;
            foreach (var nodeQR in AdoptingTreeNodes)
            {
                itemsArray[i] = nodeQR;
                itemsArray[i].TicksSinceUpdate++;
                i++;
            }
            AdoptingTreeNodes.Clear();
            AdoptingTreeNodes.UnionWith(itemsArray);

        }
        
        public Bounds ObjectBounds;
        public UpdatingHashSet<SpatialHashGrid.SHGNodeQR> GridNodes = new();

        #region FST Properties

        public bool IsHostOfTreeNode => HostingTreeNode.StructID != FloatingSpatialTree.FSTNodeQR.NULL_VALUE.StructID;
        public bool Absorbed => AbsorbingTreeNodes.Count != 0;
        /// <summary>
        /// Whether this ZObject is currently in any leaf nodes
        /// </summary>
        public bool IsFree => AbsorbingTreeNodes.Count == 0 
                              && AdoptingTreeNodes.Count == 0 
                              && HostingTreeNode.StructID == FloatingSpatialTree.FSTNodeQR.NULL_VALUE.StructID;

        public FloatingSpatialTree.FSTNodeQR HostingTreeNode = FloatingSpatialTree.FSTNodeQR.NULL_VALUE;
        public UpdatingHashSet<FloatingSpatialTree.FSTNodeQR> AdoptingTreeNodes = new(new FloatingSpatialTree.FSTNodeQRLightCompare());
        public UpdatingHashSet<FloatingSpatialTree.FSTNodeQR> AbsorbingTreeNodes = new(new FloatingSpatialTree.FSTNodeQRLightCompare());
        public FloatingSpatialTree.TreeObjectType TreeObjectType;

        public int UpdateCounter = 0;

        public bool FSTUpdateTickForward()
        {
            if (++UpdateCounter >= FloatingSpatialTree.LookForObjectsToAbsorbFrequency)
            {
                UpdateCounter = 0;
                return true;
            }

            return false;
        }
        
        
        #endregion
        
        public int ID;
        
        private void Awake()
        {
            ObjectBounds.center = transform.position;
            Vector3 size = Vector3.one * width;
            transform.localScale = size;
            ObjectBounds.size = size;
        }

#if UNITY_EDITOR    
        private void OnDrawGizmosSelected()
        {

            var nc = NodeController.Singleton;
            foreach (var tnID in AdoptingTreeNodes)
            {
                var node = nc.GetFSTNode(tnID);
                Vector3 nodePos = node.HostObject.Position;
                
                Debug.DrawLine(nodePos, Position, Color.cyan);
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(nodePos, 1);
            }
            if (DrawDebugLines)
            {
                DebugDrawLine();
            }
            
        }
#endif
        
        

#if UNITY_EDITOR
        public bool DrawLineToFST = true;
        public bool DrawDebugLines = true;  
#endif
        private void Update()
        {
#if UNITY_EDITOR

#endif
        }

        
        public static Color LineColor = Color.green;
        private void DebugDrawLine()
        {
            var nodes = NodeController.Singleton.GetSHGNodesFromQR(GridNodes.ToArray());
            foreach (var node in nodes)
            {
                Vector3 nodeCenter = node.NodeBounds.center;
                Debug.DrawLine(transform.position, nodeCenter, LineColor, Time.fixedDeltaTime);
            }
        }
    }
}