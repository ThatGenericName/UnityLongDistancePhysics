using System;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Serialization;

namespace ObjectZoning
{
    public class ZObjectNearbyGrabber : MonoBehaviour
    {
        public Color debugLineColor = Color.red;
        public float threshold = 15f;

        public bool LookForNearby = false;
        private ZObject zobj;
        
        private void Awake()
        {
            zobj = GetComponent<ZObject>();
            LookForNearby = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (LookForNearby)
            {
                Gizmos.color = debugLineColor;
                Gizmos.DrawWireSphere(transform.position, threshold);
            }
        }

        public void DrawLinesToNearby()
        {
            var nc = NodeController.Singleton;

            var nearby = nc.GetNearbyObjects(transform.position, threshold);
            Vector3 here = transform.position;
            for (int i = 0; i < nearby.Length; i++)
            {
                if (zobj != nearby[i])
                {
                    Vector3 there = nearby[i].transform.position;
                    Debug.DrawLine(here, there, debugLineColor, 2 * Time.fixedDeltaTime, false);
                }
            }
        }

        public void DrawToNearby(int[] nearby)
        {
            Vector3 here = transform.position;
            var nc = NodeController.Singleton;
            for (int i = 0; i < nearby.Length; i++)
            {
                var tzobj = nc.GetZObject(nearby[i]);
                if (zobj != tzobj)
                {
                    Vector3 there = tzobj.transform.position;
                    Debug.DrawLine(here, there, debugLineColor, Time.fixedDeltaTime, false);
                }
            }
        }
    }
}