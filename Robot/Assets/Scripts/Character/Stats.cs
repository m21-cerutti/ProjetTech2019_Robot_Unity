using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Stats : MonoBehaviour
{
    [SerializeField] private float m_speed;
    public float speed { get { return m_speed; } private set { m_speed = value; } }
}
