using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MotorRobot))]
public class InputRobot : MonoBehaviour
{
    [SerializeField]
    float m_translation_x, m_translation_y, m_rotation;

    [SerializeField]
    float m_max_spped = 0.3f;

    public float Forward { get { return Mathf.Clamp(m_translation_x, -m_max_spped, m_max_spped); } set { m_translation_x = value; } }

	public float Aside { get { return Mathf.Clamp(m_translation_y, -m_max_spped, m_max_spped); } set { m_translation_y = value; } }

	public float Rotation{ get { return Mathf.Clamp(m_rotation, -m_max_spped, m_max_spped); } set { m_rotation = value; } }

}
