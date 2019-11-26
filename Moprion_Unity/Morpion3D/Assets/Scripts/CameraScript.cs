using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Script attached to the main camera, implement "zoom" effect when transitioning from menu to game
/// and handle the camera rotation around the fixed "grid" game board.
/// </summary>
public class CameraScript : MonoBehaviour
{
    // ---- Static fields ----

    static public Vector3 menuPosition = new Vector3(0, 0, -5f);
    static public Vector3 gamePosition = new Vector3(0, 0, -0f);

    // ---- Events ----

    public event EventHandler ReadyGame;
    public event EventHandler ReadyMenu;

    // ---- Public fields / properties ----

    public float CameraTransitionTime;
    public float RotationSpeed;

    // ---- Private fields ----

    bool inGame;

    // ---- Event Handlers ----

    /// <summary>
    /// Handle <see cref="MainScript.State"/> changes.
    /// </summary>
    /// <param name="sender">Must be the <see cref="MainScript"/></param>
    /// <param name="args">Ignored.</param>
    public void OnStateChange(object sender, EventArgs args)
    {
        MainScript ms = sender as MainScript;
        inGame = false;
        switch (ms.State)
        {
            case EState.ToMenu:
                StartCoroutine(ToMenuCoroutine());
                break;
            case EState.ToGame:
                StartCoroutine(ToGameCoroutine());
                break;
            case EState.InGame:
                inGame = true;
                break;
        }
    }

    // ---- Private methods ----

    private void Awake()
    {
        RotationSpeed = 1;
        CameraTransitionTime = 2f;
        transform.position = menuPosition;
        inGame = false;
    }

    void Update()
    {
        if (inGame)
        {
            if (Input.GetMouseButton(0))
            {
                var strenght = 360 * Time.deltaTime * RotationSpeed;
                var eulerDelta = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * strenght;
                var currentEuler = transform.rotation.eulerAngles;
                var newEuler = currentEuler + eulerDelta;
                newEuler.z = 0;
                transform.rotation = Quaternion.Euler(newEuler);
            }
        }
    }

    IEnumerator ToGameCoroutine()
    {
        var itPosition = Utils.LerpMove(transform.position, gamePosition, CameraTransitionTime, true);
        
        while (itPosition.MoveNext())
        {
            transform.position = (Vector3)itPosition.Current;
            yield return null;
        }
        ReadyGame?.Invoke(this, EventArgs.Empty);
        yield break;
    }

    IEnumerator ToMenuCoroutine()
    {
        var itPosition = Utils.LerpMoveAndRotate(
            transform.position, transform.rotation,
            menuPosition, Quaternion.identity,
            CameraTransitionTime, true);

        while (itPosition.MoveNext())
        {
            var pair = (Tuple<Vector3, Quaternion>)itPosition.Current;
            transform.position = pair.Item1;
            transform.rotation = pair.Item2;
            yield return null;
        }
        ReadyMenu?.Invoke(this, EventArgs.Empty);
        yield break;
    }
}
