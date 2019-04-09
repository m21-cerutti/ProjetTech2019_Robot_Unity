using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class StereoCamera : MonoBehaviour
{
   
    static readonly object _cameras = new object();
    private byte[] cameraLeft;
    private byte[] cameraRight;

    public byte[][] getCamerasImages()
    {
        lock (_cameras)
        {
            byte[][] cameras = new byte[2][];
            byte[] left = new byte[cameraLeft.Length];
            cameraLeft.CopyTo(left ,0);
            byte[] right = new byte[cameraRight.Length];
            cameraRight.CopyTo(right, 0);
            cameras[0] = left;
            cameras[1] = right;
            return cameras;
        }
    }

    private bool compute = false;
    private IEnumerator computeCamerasImages()
    {
        yield return null;
        compute = true;
        lock (_cameras)
        {
            cameraLeft = GetCameraImage(left_cam);
            cameraRight = GetCameraImage(right_cam);
        }
        compute = false;
    }

    private byte[] GetCameraImage(Camera cam)
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;

        cam.Render();

        Texture2D Image = new Texture2D(cam.activeTexture.width, cam.activeTexture.height);
        Image.ReadPixels(new Rect(0, 0, cam.activeTexture.width, cam.activeTexture.height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;

        byte[] bytes = Image.EncodeToPNG();
        Destroy(Image);

        return bytes;
    }

    [SerializeField]
    private Camera left_cam;
    [SerializeField]
    private Camera right_cam;
    [SerializeField]
    private float distance_cam = 0.5f;
    [SerializeField]
    private float height_cam = 0.5f;

    public float refreshTime = 30f;

	float getRefreshTimeMs()
	{
		return refreshTime / 1000f;
	}

    void Start()
    {
        SetCamerasPositions();
        timer = getRefreshTimeMs();
        computeCamerasImages();
    }

    private float timer;
    void LateUpdate()
	{
        if (timer < 0 && !compute)
        {
            StartCoroutine(computeCamerasImages());
            timer = getRefreshTimeMs();
        }
        else
        {
			timer -= Time.deltaTime;
        }
	}

	public void SetCamerasPositions()
    {
        //Distance beetween eyes
        right_cam.transform.localPosition = new Vector3(distance_cam, height_cam);
        left_cam.transform.localPosition = new Vector3(-distance_cam, height_cam);
    }
}

