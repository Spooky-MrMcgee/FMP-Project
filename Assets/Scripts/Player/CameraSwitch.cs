using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    [SerializeField] List<GameObject> camerasToSwitchTo = new List<GameObject>();
    Camera activeCam;
    [SerializeField] public bool isColliding;

    private void Start()
    {
        this.GetComponent<Camera>().aspect = ((float)Screen.width / Screen.height);
    }

    // CameraSwitch makes sure to always update to the current camera.
    void Update()
    {
        foreach (GameObject camera in camerasToSwitchTo)
        {
            if (camera.GetComponent<CameraSwitch>().isColliding)
            {
                activeCam = camera.GetComponent<Camera>();
            }
            if (camera.GetComponent<Camera>() == activeCam)
            {
                camera.GetComponent<Camera>().enabled = true;
            }
            else
            {
                camera.GetComponent<Camera>().enabled = false;
            }
        }
    }
}