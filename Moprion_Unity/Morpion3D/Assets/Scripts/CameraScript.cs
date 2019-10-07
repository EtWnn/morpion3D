using System;
using System.Collections;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    const float LERP_END_DIST = 10e-2f;

    public enum ECameraState
    {
        ToMenu,
        ReadyMenu,
        InMenu,
        ToGame,
        ReadyGame,
        InGame,
    }

    static public Vector3 menuPosition = new Vector3(0, 0, -5f);
    static public Vector3 gamePosition = new Vector3(0, 0, 0);

    public float CameraTransitionTime;
    public float RotationSpeed;
    public ECameraState CamState { get; private set; }

    private Action UpdateFunction;

    void Start()
    {
        transform.position = menuPosition;
        Debug.Log(transform.position);
        RotationSpeed = 1;
        CameraTransitionTime = 5f;
        CamState = ECameraState.InMenu;
        UpdateFunction = NoOpBehaviour;
    }

    void Update()
    {
        UpdateFunction();
    }

    private void ToGameBehaviour()
    {
        var itPosition = LerpMove(transform.position, gamePosition, 8);
        if(itPosition.MoveNext())
            transform.position = (Vector3)itPosition.Current;
        else
        {
            Debug.Log("Camera finished ToGame!");
            UpdateCameraState(ECameraState.ReadyGame);
        }
    }

    private void ToMenuBehaviour()
    {
        transform.position = Vector3.Lerp(transform.position, menuPosition, CameraTransitionTime);
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
        switch (cameraState)
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

    IEnumerator LerpMove(Vector3 initPos, Vector3 targetPos, float duration, bool smoothed)
    {

        Func<float, float> F = (float t) => t;
        if (smoothed)
            F = (float t) => -  
        float t = 0;
        float dt = Time.timeScale / duration;
        while ((t += dt) < 1)
        {
            yield return Vector3.Lerp(initPos, targetPos, t);
        }
        yield break;
    }
}
