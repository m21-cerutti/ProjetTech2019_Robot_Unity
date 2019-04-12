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
		if (ObserverController.XTouch)
		{
			viewCam.SetActive(!viewCam.activeSelf);
			if (viewCam.activeSelf)
				Cursor.lockState = CursorLockMode.None;
			else
				Cursor.lockState = CursorLockMode.Locked;
		}

		if (!viewCam.activeSelf)
		{
			PerformMovement();
			PerformRotation();

			if (ObserverController.Click_Down)
			{

				///Click spawn
				Ray ray = new Ray(transform.position, transform.TransformDirection(Vector3.forward));
				RaycastHit hit = new RaycastHit();
				if (Physics.Raycast(ray, out hit, Mathf.Infinity))
				{
					if (hit.collider.gameObject.tag == "Terrain")
					{
						Debug.DrawRay(hit.point, hit.normal * 10, Color.green);
						GameObject.FindGameObjectWithTag("Character").GetComponent<PathAI>().setTarget(hit.point);
					}
				}
			}
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
        rotationY += ObserverController.MouseY * speed_angle_up;
        rotationY = Mathf.Clamp(rotationY, -cameraRotationLimit, cameraRotationLimit);
        transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
    }
}