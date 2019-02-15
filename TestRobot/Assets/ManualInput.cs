using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputRobot))]
public class ManualInput : MonoBehaviour
{
    //Manual input
    public float Horizontal
    {
        get { return Input.GetAxis("Horizontal"); }
    }

    public float Vertical
    {
        get { return Input.GetAxis("Vertical"); }
    }

    public bool A
    {
        get { return Input.GetButtonDown("LeftA"); }
    }

}
