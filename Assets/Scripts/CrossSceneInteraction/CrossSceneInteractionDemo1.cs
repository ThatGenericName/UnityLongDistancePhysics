using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace CrossSceneInteraction
{
    public class CrossSceneInteractionDemo1 : MonoBehaviour
    {
        public GameObject WallSection;
        
        public int depth = 0;

        public bool CreateDemoScene = false;

        public bool CopyAcrossAllScenes = false;

        public int SegmentsPerScene = 1;

        public void FixedUpdate()
        {
            if (CreateDemoScene)
            {
                CreateDemoScene = false;
                CreateDemoSceneFunc();
            }
        }

        private void CreateDemoSceneFunc()
        {
            var csi_controller = CrossSceneInteractionController.Singleton;

            Vector3 left = new Vector3(4, 0, 0);
            Vector3 right = new Vector3(-4, 0, 0);
            Vector3 forward = new Vector3(0, 0, 1);
            float offset = 5.0f;
            
            for (int i = 0; i < depth; i++)
            {
                csi_controller.CreatePhysicsScene();
            }

            if (CopyAcrossAllScenes)
            {
                for (int i = 0; i < depth; i++)
                {
                    Vector3 forwardOffset = offset * i * forward;
                    Vector3 leftPos = forwardOffset + left;
                    Vector3 rightPos = forwardOffset + right;
                
                    csi_controller.InstantiatePrefab(WallSection, leftPos, Quaternion.identity);
                    csi_controller.InstantiatePrefab(WallSection, rightPos, Quaternion.identity);
                }
            }
            else
            {
                int counter = 0;
                for (int i = 0; i < depth; i++)
                {
                    for (int j = 0; j < SegmentsPerScene; j++)
                    {
                        Vector3 forwardOffset = offset * counter++ * forward;
                        Vector3 leftPos = forwardOffset + left;
                        Vector3 rightPos = forwardOffset + right;
                        csi_controller.InstantiatePrefabSingleScene(WallSection, leftPos, Quaternion.identity, i);
                        csi_controller.InstantiatePrefabSingleScene(WallSection, rightPos, Quaternion.identity, i);
                    }
                }
            }
            
            csi_controller.CreateDemoPrefabs();
        }
    }
}