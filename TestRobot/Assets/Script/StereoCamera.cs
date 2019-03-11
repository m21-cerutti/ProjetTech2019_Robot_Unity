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
            cameras[0] = cameraLeft;
            cameras[1] = cameraRight;
            return cameras;
        }
    }

    private void computeCameraLeft()
    {
        cameraLeft = GetCameraImage(left_cam);
    }

    private void computeCameraRight()
    {
        cameraRight = GetCameraImage(right_cam);
    }

    private void computeCamerasImages()
    {
        lock (_cameras)
        {
            computeCameraLeft();
            computeCameraRight();
        }
    }

    [SerializeField]
    private Camera left_cam;
    [SerializeField]
    private Camera right_cam;
    [SerializeField]
    private float distance_cam = 0.5f;

    public float refreshTime = 30f;
	float getRefreshTimeMs()
	{
		return refreshTime / 1000f;
	}

	private float timer;


    void Start()
    {
        SetCamerasDistance();
        timer = getRefreshTimeMs();
        computeCamerasImages();
    }

	void LateUpdate()
	{
        if (timer < 0)
        {
            computeCamerasImages();
            timer = getRefreshTimeMs();
        }
        else
        {
			timer -= Time.deltaTime;
        }
	}

	public void SetCamerasDistance()
    {
        //Distance beetween eyes
        right_cam.transform.localPosition = new Vector3(distance_cam, 0);
        left_cam.transform.localPosition = new Vector3(-distance_cam, 0);
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
}

/*
[CustomEditor(typeof(StereoCamera))]
public class LevelScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        StereoCamera myTarget = (StereoCamera)target;

        if (GUILayout.Button("Calculate distance cameras"))
        {
            myTarget.SetCamerasDistance();
        }

        if (GUILayout.Button("GetCameraLeft"))
        {
            myTarget.GetCameraLeft();
        }

        if (GUILayout.Button("GetCameraRight"))
        {
            myTarget.GetCameraRight();
        }
    }
}
*/
