
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ObjectZoning1
{
    public class TrackedObject : MonoBehaviour
    {

        public HashSet<DynamicBoundingVolume.DBVNode> dynamicZones = new();
        public StaticBoundingVolume.SBVNode staticZone;

        public int ObjectZoningPriority = 1; 
        // 0 is will never be absorbed into other zones
        // 1 is will be absorbed if within baseZoneWidth/4, otherwise create it's own zone
        // 2 is will be absorbed if within baseZoneWidth.

        public MeshRenderer mr;

        public float objectSelfBuffer;

        private Vector3 _pos = Vector3.zero;

        public Vector3 position
        {
            get => _pos;
            set {
                recalculateXYZ();
                _pos = value;
            }
        }

        public void Awake()
        {
            mr = GetComponent<MeshRenderer>();
        }

        private (int, int, int) _xyz;
        public (int, int, int) XYZPos => _xyz;

        public (int, int, int) recalculateXYZ(out (int, int, int) last)
        {
            last = _xyz;
            
            var pos = position;
            int x = Mathf.FloorToInt(pos.x / TrackedObjectController.HalfZoneWidth);
            int y = Mathf.FloorToInt(pos.y / TrackedObjectController.HalfZoneWidth);
            int z = Mathf.FloorToInt(pos.z / TrackedObjectController.HalfZoneWidth);

            _xyz = (x, y, z);
            return _xyz;
        }
        
        public (int, int, int) recalculateXYZ()
        {
            
            var pos = position;
            int x = Mathf.FloorToInt(pos.x / TrackedObjectController.HalfZoneWidth);
            int y = Mathf.FloorToInt(pos.y / TrackedObjectController.HalfZoneWidth);
            int z = Mathf.FloorToInt(pos.z / TrackedObjectController.HalfZoneWidth);

            _xyz = (x, y, z);
            return _xyz;
        }

        public bool AddToDBV(DynamicBoundingVolume.DBVNode dbvNode)
        {
            if (dynamicZones.Count == 0)
            {
                dbvNode.TrackedObjects.Add(this);
                dynamicZones.Add(dbvNode);
                return true;
            }
            else
            {
                bool addZone = true;
                List<DynamicBoundingVolume.DBVNode> removalList = new();
                foreach (var zone in dynamicZones)
                {
                    Vector3 diff1 = zone.CenterObject.position - position;
                    Vector3 diff2 = dbvNode.CenterObject.position - position;

                    float ang = Vector3.Angle(diff1, diff2);
                    if (ang <= TrackedObjectController.MultiZoneAngThresh)
                    {
                        if (diff1.sqrMagnitude >= diff2.sqrMagnitude)
                        {
                            // new dbv node is closer, remove the original
                            removalList.Add(zone);
                            break;
                        }

                        addZone = false;
                        break;
                    }
                }

                if (addZone)
                {
                    foreach (var zoneToRemove in removalList)
                    {
                        dynamicZones.Remove(zoneToRemove);
                        zoneToRemove.TrackedObjects.Remove(this);
                    }

                    dynamicZones.Add(dbvNode);
                    dbvNode.TrackedObjects.Add(this);
                    return true;
                }

                return false;
            }
        }
    }
}