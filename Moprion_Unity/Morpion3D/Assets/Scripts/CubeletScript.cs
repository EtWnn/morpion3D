using System;
using UnityEngine;

/// <summary>
/// Script attached to the cubelet GameObject.
/// Handles the click and provide methods to fill the cubelet with a GameObject.
/// Implement a color swap when hovering over the cubelet in game.
/// </summary>
public class CubeletScript : MonoBehaviour
{
    // ---- Events ----

    /// <summary>Fired when the cubelet GameObject is clicked.</summary>
    public event EventHandler Clicked;
    
    // ---- Public fields / properties ----
    
    /// <summary>Field set through UnityEditor.</summary>
    public Color BaseColor;
    /// <summary>Field set through UnityEditor.</summary>
    public Color HoverColor;

    /// <summary>True if the cubelet is filled.</summary>
    public bool Filled { get; private set; }
    /// <summary>Reference to the current filling GameObject, null if the cubelet is not filled.</summary>
    public GameObject CurrentFillingObject { get; private set; }
    /// <summary>Cubelet associated position in the grid.</summary>
    public System.Numerics.Vector3 Position { get; set; }

    // ---- Private fields / properties ----

    private bool inGame;
    private MeshRenderer meshRenderer;

    // ---- Public methods ----

    /// <summary>Activate / Desactivate the attached GameObject.</summary>
    public void SetActive(bool value) => gameObject.SetActive(value);

    /// <summary>
    /// Handle <see cref="MainScript.State"/> changes.
    /// </summary>
    /// <param name="sender">Must be the <see cref="MainScript"/></param>
    /// <param name="args">Ignored.</param>
    public void OnStateChange(object sender, EventArgs args)
    {
        MainScript ms = sender as MainScript;
        switch (ms.State)
        {
            case EState.InGame:
                inGame = true;
                break;
            default:
                inGame = false;
                break;
        }
    }

    /// <summary>
    /// Clear cubelet filling object and reset its material color.
    /// </summary>
    public void ResetCubelet()
    {
        if (CurrentFillingObject != null)
            Destroy(CurrentFillingObject);
        Filled = false;
        meshRenderer.material.color = BaseColor;
    }

    /// <summary>
    /// Fill the cubelet with a clone of <paramref name="prefab"/>, if already filled, destory the previous filling GameObject.
    /// </summary>
    public void FillWith(GameObject prefab)
    {
        if (Filled)
            Destroy(CurrentFillingObject);
        CurrentFillingObject = Instantiate(prefab, transform);
        Filled = true;
    }

    // ---- Private methods ----

    private void Awake()
    {
        Filled = false;
        CurrentFillingObject = null;

        meshRenderer = GetComponent<MeshRenderer>();

        BaseColor = meshRenderer.material.color;
        // Create hover color from base color
        Color hoverColor = BaseColor;
        hoverColor.a = 0.4f;
        HoverColor = hoverColor;
    }

    private void OnMouseOver()
    {
        if (inGame)
        {
            meshRenderer.material.color = HoverColor;
            if (Input.GetMouseButtonDown(0))
                Clicked?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnMouseExit()
    {
        if (inGame)
            meshRenderer.material.color = BaseColor;
    }
}
