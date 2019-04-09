using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObserverMotor : MonoBehaviour
{
    [SerializeField] private float speed_angle_up = 10f;
    [SerializeField] private float speed_angle_turn = 5f;
    [SerializeField] private float speed = 45f;
    [SerializeField] private float cameraRotationLimit = 90f;
    [SerializeField] private float speed_Up_Down = 1f;

    [SerializeField] private GameObject viewCam;

    private void Update()
    {
        PerformMovement();
        PerformRotation();

        if(ObserverController.Click_Down)
        {
            viewCam.SetActive(!viewCam.activeSelf);
        }

        if (ObserverController.Cancel)
        {
            Application.Quit(0);
        }
    }

    Vector3 velocity;
    private void PerformMovement()
    {
        velocity = transform.TransformDirection(new Vector3(speed * ObserverController.Aside, 0f, speed * ObserverController.Forward));
        velocity.y = speed_Up_Down * ObserverController.Height;
        transform.Translate(velocity, Space.World);
    }

    float rotationY = 0F;
    private void PerformRotation()
    {
        float rotationX = transform.localEulerAngles.y + speed_angle_turn * ObserverController.MouseX;
        rotationY += ObserverController.MouseY * speed_angle_turn;
        rotationY = Mathf.Clamp(rotationY, -cameraRotationLimit, cameraRotationLimit);
        transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
    }
}