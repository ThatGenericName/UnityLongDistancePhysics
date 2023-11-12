
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ObjectZoning2
{
    public class TrackedObject : MonoBehaviour
    {
        
        public StaticBoundingVolumeNode staticZone;
        public HashSet<DynamicBoundingVolumeNode> dynamicZones = new();

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

        private void Update()
        {
            foreach (var dbvNode in dynamicZones)
            {
                Debug.DrawLine(position, dbvNode.Mid, Color.blue);
            }

            if (staticZone != null)
            {
                Vector3 mid = staticZone.Max + staticZone.Min;
                mid = mid / 2;
                Debug.DrawLine(position, mid, Color.white);
            }

        }

        private (int, int, int) _xyz;
        public (int, int, int) XYZPos => _xyz;

        public (int, int, int) recalculateXYZ(out (int, int, int) last)
        {
            last = _xyz;
            
            var pos = position;
            int x = Mathf.FloorToInt(pos.x / TrackedObjectController.HalfZoneOffset);
            int y = Mathf.FloorToInt(pos.y / TrackedObjectController.HalfZoneOffset);
            int z = Mathf.FloorToInt(pos.z / TrackedObjectController.HalfZoneOffset);

            _xyz = (x, y, z);
            return _xyz;
        }
        
        public (int, int, int) recalculateXYZ()
        {
            
            var pos = position;
            int x = Mathf.FloorToInt(pos.x / TrackedObjectController.HalfZoneOffset);
            int y = Mathf.FloorToInt(pos.y / TrackedObjectController.HalfZoneOffset);
            int z = Mathf.FloorToInt(pos.z / TrackedObjectController.HalfZoneOffset);

            _xyz = (x, y, z);
            return _xyz;
        }
    }
}