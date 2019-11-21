using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPatternUI : MonoBehaviour
{
    public Camera Camera;

    [Range(0.1f, 10f)]
    public float RotationSpeed=1;

    public Material DefaultMaterial;
    public Material SelectedMaterial;

    private bool rotationRunning;
    private Coroutine rotationCoroutine;
    private MeshRenderer meshRenderer;

    public void OnToggled(bool value)
    {
        SetRotationActive(value);
        meshRenderer.material = value ? SelectedMaterial : DefaultMaterial;
    }

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
