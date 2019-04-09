using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MotorRobot))]
public class InputRobot : MonoBehaviour
{
    [SerializeField]
    float m_translation, m_rotation;

    [SerializeField]
    float m_max_spped = 0.3f;

    public float Forward { get { return Mathf.Clamp(m_translation, -m_max_spped, m_max_spped); } set { m_translation = value; } }

    public float Rotation{ get { return Mathf.Clamp(m_rotation, -m_max_spped, m_max_spped); } set { m_rotation = value; } }

}
