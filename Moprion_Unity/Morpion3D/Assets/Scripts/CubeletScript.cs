using System;
using UnityEngine;

public class CubeletScript : MonoBehaviour
{
    public Color BaseColor;
    public Color HoverColor;
    public GameObject FillingItemLeftClick;
    public GameObject FillingItemRightClick;
    private MeshRenderer MeshRenderer;
    public GameObject CurrentFillingObject { get; private set; }
    public bool Filled { get; private set; }

    private Action updateFunction;
    private Action onMouseOverFunction;
    private Action onMouseExitFunction;

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
    }

    // Update is called once per frame
    void OnMouseOver()
    {
        onMouseOverFunction();
    }

    void OnMouseExit()
    {
        onMouseExitFunction();
    }

    void Update()
    {
        updateFunction();
    }

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

    void NoOpBehaviour() { }

    void InGameOnMouseOverBehaviour()
    {
        MeshRenderer.material.color = HoverColor;
        if (!Filled)
        {
            if (Input.GetMouseButtonDown(0))
            {
                CurrentFillingObject = Instantiate(FillingItemLeftClick, transform);
                Filled = true;
            }
            else if (Input.GetMouseButtonDown(1))
            {
                CurrentFillingObject = Instantiate(FillingItemRightClick, transform);
                Filled = true;
            }
        }
    }

    void InGameOnMouseExitBehaviour()
    {
        MeshRenderer.material.color = BaseColor;
    }

    void InGameUpdateBehaviouor()
    {
        if (Filled && Input.GetKeyDown(KeyCode.R))
        {
            Destroy(CurrentFillingObject);
            Filled = false;
        }
    }
}
