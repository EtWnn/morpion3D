using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script which make it's attatched GameObject rotating while facing a given camera.
/// </summary>
public class FaceCamAndRotateScript : MonoBehaviour
{
    /// <summary>The Camera to face, if left null, <c>Camera.MainCamera</c> is used.</summary>
    public Camera MainCamera;

    /// <summary>The rotation speed in half-turn per second.</summary>
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
