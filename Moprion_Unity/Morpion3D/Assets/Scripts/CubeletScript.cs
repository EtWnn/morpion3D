using System;
using UnityEngine;

public class CubeletScript : MonoBehaviour
{
    public event EventHandler Clicked;
    ///// Public fields / properties /////
    
    public Color BaseColor;
    public Color HoverColor;
    
    public bool Filled { get; private set; }
    public GameObject CurrentFillingObject { get; private set; }
    public System.Numerics.Vector3 Position { get; set; }

    ///// Private fields / properties /////

    private MeshRenderer MeshRenderer;
    private Action updateFunction;
    
    private Action onMouseOverFunction;
    private Action onMouseExitFunction;

    private GridScript gridScript;

    ///// Public methods /////

    public void SetActive(bool value) => gameObject.SetActive(value);

    public void OnStateChange(object sender, EventArgs args)
    {
        MainScript ms = sender as MainScript;
        switch (ms.State)
        {
            case EState.InGame:
                updateFunction = InGameUpdateBehaviouor;
                onMouseOverFunction = InGameOnMouseOverBehaviour;
                onMouseExitFunction = InGameOnMouseExitBehaviour;
                break;
            default:
                updateFunction = NoOpBehaviour;
                onMouseOverFunction = NoOpBehaviour;
                onMouseExitFunction = NoOpBehaviour;
                break;
        }
    }

    public void FillWith(GameObject prefab)
    {
        if (Filled)
            Destroy(CurrentFillingObject);
        CurrentFillingObject = Instantiate(prefab, transform);
        Filled = true;
    }

    ///// Private methods /////

    private void Awake()
    {
        Filled = false;
        CurrentFillingObject = null;

        MeshRenderer = GetComponent<MeshRenderer>();

        BaseColor = MeshRenderer.material.color;
        Color hoverColor = BaseColor;
        hoverColor.a = 0.4f;
        HoverColor = hoverColor;

        updateFunction = NoOpBehaviour;
        onMouseOverFunction = NoOpBehaviour;
        onMouseExitFunction = NoOpBehaviour;

        gridScript = GetComponentInParent<GridScript>();
    }

    // Update is called once per frame
    private void OnMouseOver()
    {
        onMouseOverFunction();
    }

    private void OnMouseExit()
    {
        onMouseExitFunction();
    }

    private void Update()
    {
        updateFunction();
    }

    private void NoOpBehaviour() { }

    private void InGameOnMouseOverBehaviour()
    {
        MeshRenderer.material.color = HoverColor;
        if (Input.GetMouseButtonDown(0))
            Clicked?.Invoke(this, EventArgs.Empty);
    }

    private void InGameOnMouseExitBehaviour()
    {
        MeshRenderer.material.color = BaseColor;
    }

    private void InGameUpdateBehaviouor()
    {
        if (Filled && Input.GetKeyDown(KeyCode.R))
        {
            Destroy(CurrentFillingObject);
            Filled = false;
        }
    }
}
