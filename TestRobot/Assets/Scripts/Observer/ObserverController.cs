using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ObserverController
{

    public static bool Click_Down
    {
        get { return Input.GetMouseButtonDown(0); }
    }

    public static bool XTouch
    {
        get { return Input.GetKeyDown(KeyCode.X); }
    }

    public static bool Cancel
    {
        get { return Input.GetButtonDown("Cancel"); }
    }

    public static float MouseX
    {
        get { return Input.GetAxis("Mouse X"); }
    }

    public static float MouseY
    {
        get { return Input.GetAxis("Mouse Y"); }
    }

    public static float Forward
    {
        get { return Input.GetAxis("Forward"); }
    }

    public static float Aside
    {
        get { return Input.GetAxis("Aside"); }
    }

    public static float Height
    {
        get { return Input.GetAxis("Height"); }
    }
}
