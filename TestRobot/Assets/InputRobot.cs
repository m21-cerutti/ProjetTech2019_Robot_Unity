using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MotorRobot))]
public class InputRobot : MonoBehaviour
{
    [SerializeField]
    float m_translation, m_rotation;

    public float Forward { get { return m_translation; } set { m_translation = value; } }

    public float Rotation{ get { return m_rotation; } set { m_rotation = value; } }

}
