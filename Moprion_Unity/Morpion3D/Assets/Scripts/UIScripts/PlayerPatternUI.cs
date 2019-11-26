using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for player pattern in UI (Options menu).
/// Make its attached object rotate, and implement material swaping when selected.
/// </summary>
public class PlayerPatternUI : MonoBehaviour
{
    // ---- Public fields / properties ----

    [Range(0.1f, 10f)]
    public float RotationSpeed=1;

    public Material DefaultMaterial;
    public Material SelectedMaterial;

    // ---- Private fields / properties ----

    private bool rotationRunning;
    private Coroutine rotationCoroutine;
    private MeshRenderer meshRenderer;

    // ---- Event handers ----

    /// <summary>
    /// Event handler for UnityEvent Toggled.
    /// </summary>
    /// <param name="value"></param>
    public void OnToggled(bool value)
    {
        SetRotationActive(value);
        meshRenderer.material = value ? SelectedMaterial : DefaultMaterial;
    }

    // ---- Private methods ----

    private void Awake() => meshRenderer = GetComponent<MeshRenderer>();

    private void OnEnable()
    {
        // We need to restart animation Coroutine on gameobject enabled if it were running before disabled.
        if (rotationRunning)
        {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = StartCoroutine(IERotation());
        }
    }

    private void SetRotationActive(bool value)
    {
        if (value && !rotationRunning)
        {
            rotationCoroutine = StartCoroutine(IERotation());
            rotationRunning = true;
        }
        else if(!value && rotationRunning)
        {
            StopCoroutine(rotationCoroutine);
            rotationRunning = false;
            transform.eulerAngles = Vector3.zero;
        }
    }

    private IEnumerator IERotation()
    {
        var cnt = 0;
        while (true)
        {
            if (cnt++ == 10)
            {
                //Debug.Log("PatternUI Rotating...");
                cnt = 0;
            }
            transform.Rotate(0, 180 * Time.deltaTime * RotationSpeed, 0);
            yield return null;
        }
    }
}
