using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(StereoCamera))]
public class LevelScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        StereoCamera myTarget = (StereoCamera)target;

        if (GUILayout.Button("Calculate position cameras"))
        {
            myTarget.SetCamerasPositions();
        }

        if (GUILayout.Button("Get cameras"))
        {
            myTarget.getCamerasImages();
        }
    }
}
#endif
