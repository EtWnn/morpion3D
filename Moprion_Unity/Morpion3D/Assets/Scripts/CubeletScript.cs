using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeletScript : MonoBehaviour
{
    public Color BaseColor;
    public Color HoverColor;
    public GameObject FillingItemLeftClick;
    public GameObject FillingItemRightClick;
    public MeshRenderer MeshRenderer { get; private set; }
    public GameObject CurrentFillingObject { get; private set; }
    public bool Filled { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        Filled = false;
        CurrentFillingObject = null;
        MeshRenderer = GetComponent<MeshRenderer>();
        BaseColor = MeshRenderer.material.color;
        Color hoverColor = BaseColor;
        hoverColor.a = 0.4f;
        HoverColor = hoverColor;
        Debug.Log("BaseColor: " + BaseColor);
    }

    // Update is called once per frame
    void OnMouseOver()
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

    private void OnMouseExit()
    {
        MeshRenderer.material.color = BaseColor;
    }

    private void Update()
    {
        if (Filled && Input.GetKeyDown(KeyCode.R))
        {
            Destroy(CurrentFillingObject);
            Filled = false;
        }
    }
}
