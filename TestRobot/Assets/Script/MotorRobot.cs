using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(InputRobot))]
[RequireComponent(typeof(Rigidbody))]
public class MotorRobot : MonoBehaviour
{
    [SerializeField]
    [Range(0,5)]
    private float m_speed_translation_multiplier = 1;

    [SerializeField]
    [Range(0, 5)]
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
        Vector3 velocity = new Vector3( 0f, 0f, input.Forward) * Time.deltaTime;
        rb.AddRelativeForce(velocity * m_speed_translation_multiplier, ForceMode.VelocityChange);
    }


    private void PerformRotation()
    {
        /* Physics system
        local_rotation = new Vector3(0f, m_speed_rotation_multiplier * input.Rotation, 0f);
        rb.AddRelativeTorque(local_rotation, ForceMode.VelocityChange);
        */

        local_rotation = new Vector3(0f, local_rotation.y + m_speed_rotation_multiplier * input.Rotation, 0f);
        transform.localRotation = Quaternion.Euler(local_rotation);
    }

}







