using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamAndRotateScript : MonoBehaviour
{
    // Start is called before the first frame update
    public Camera MainCamera;
    public float RotationSpeed;
    // Update is called once per frame

    private void Start()
    {
        if (MainCamera == null)
            MainCamera = Camera.main;
        RotationSpeed = 1;
    }

    void Update()
    {
        transform.rotation = MainCamera.transform.rotation;
        transform.Rotate(0, 180 * Time.time * RotationSpeed, 0);
    }
}
