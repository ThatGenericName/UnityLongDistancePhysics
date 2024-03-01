using LongDistancePhysics;
using UnityEngine;

namespace Cone
{
    public class ConeDrawer : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 center = transform.position;
            Vector3 end = target.transform.position;

            Conef cone = new Conef();
            cone.origin = center;
            cone.angle = angle;
            cone.end = end;
            cone.segments = segments;
            

            Vector3 bk;
            Vector3 v2;
            Color color;
            if (IntersectionLDP.ConeBoxIntersect(cone, boxIntersector.bounds, out bk, out v2))
            {
                color = Color.green;
            }
            else
            {
                color = Color.blue;
            }
            
            cone.DebugVisualize(color);
            DebugLDP.DrawTriangle(center, end, v2, Color.red);
            Debug.DrawLine(center, bk, Color.cyan, Time.fixedDeltaTime);
        }
    
        public GameObject target;
        public float angle = 5.0f;
        public int segments = 1;
        public BoxTest boxIntersector;
    }
}