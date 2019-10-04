using System;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    const float LERP_END_DIST = 10e-3f;

    public enum ECameraState
    {
        ToMenu,
        ReadyMenu,
        InMenu,
        ToGame,
        ReadyGame,
        InGame,
    }

    public Vector3 menuPosition = new Vector3(0, 0, -5f);
    public Vector3 gamePosition = new Vector3(0, 0, 0);

    public float CameraLerpSpeed;
    public float RotationSpeed;
    public ECameraState CamState { get; private set; }

    private Action UpdateFunction;

    void Start()
    {
        transform.position = menuPosition;
        RotationSpeed = 1;
        CameraLerpSpeed = 100;
        CamState = ECameraState.InMenu;
        UpdateFunction = NoOpBehaviour;
    }

    void Update()
    {
        UpdateFunction();
    }

    private void ToGameBehaviour()
    {
        transform.position = Vector3.Lerp(transform.position, gamePosition, CameraLerpSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, gamePosition) < LERP_END_DIST)
        {
            transform.position = gamePosition;
            UpdateCameraState(ECameraState.ReadyGame);
        }
    }

    private void ToMenuBehaviour()
    {
        transform.position = Vector3.Lerp(transform.position, menuPosition, CameraLerpSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, menuPosition) < LERP_END_DIST)
        {
            transform.position = menuPosition;
            UpdateCameraState(ECameraState.ReadyMenu);
        }
    }

    private void InGameCameraBehaviour()
    {
        if (Input.GetMouseButton(0))
        {
            var strenght = 360 * Time.deltaTime * RotationSpeed;
            var eulerDelta = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * strenght;
            var currentEuler = transform.rotation.eulerAngles;
            var newEuler = currentEuler + eulerDelta;
            newEuler.z = 0;
            Debug.Log(newEuler);
            transform.rotation = Quaternion.Euler(newEuler);
        }
    }

    private void NoOpBehaviour()
    {
    }

    public void UpdateCameraState(ECameraState cameraState)
    {
        switch (CamState)
        {
            case ECameraState.ToMenu:
                break;
            case ECameraState.ToGame:
                UpdateFunction = ToGameBehaviour;
                break;
            case ECameraState.InGame:
                if (CamState != ECameraState.ReadyGame)
                    throw new Exception("Trying to set InGame mode while camera not in ReadyGame mode");
                UpdateFunction = InGameCameraBehaviour;
                break;
            default:
                UpdateFunction = NoOpBehaviour;
                break;
        }
        CamState = cameraState;
    }
}
