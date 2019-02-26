using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StereoCamera))]
[RequireComponent(typeof(InputRobot))]
public class IARobot : MonoBehaviour
{

    private InputRobot input;
    private StereoCamera cams;

    void Start()
    {
        input = GetComponent<InputRobot>();
        cams = GetComponent<StereoCamera>();
    }

    void Update()
    {
        
    }
}
