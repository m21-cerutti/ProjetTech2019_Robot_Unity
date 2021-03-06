﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(InputRobot))]
[RequireComponent(typeof(Rigidbody))]
public class MotorRobot : MonoBehaviour
{
    [SerializeField]
    [Range(0,20)]
    private float m_speed_translation_multiplier = 1;

    [SerializeField]
    [Range(0, 20)]
    private float m_speed_rotation_multiplier = 1;

    private Rigidbody rb;
    private InputRobot input;
    private Vector3 local_rotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<InputRobot>();

        local_rotation = transform.localRotation.eulerAngles;
    }

    void FixedUpdate()
    {
        PerformMovement();
        PerformRotation();
    }

    private void PerformMovement()
    {
        Vector3 velocity = transform.InverseTransformDirection(rb.velocity);
		velocity.x = input.Aside * m_speed_translation_multiplier;
		velocity.z = input.Forward * m_speed_translation_multiplier;
        rb.velocity = transform.TransformDirection(velocity);
    }


    private void PerformRotation()
    {
        local_rotation = new Vector3(0f, m_speed_rotation_multiplier * input.Rotation, 0f);
        transform.Rotate(local_rotation);
    }

}







