using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the main menu and its button.
/// </summary>
public class MainMenu : MonoBehaviour
{
    // ---- Public fields / properties ----

    public Button StartButton { get; private set; }
    public Button OptionsButton { get; private set; }
    public Button QuitButton { get; private set; }

    // ---- Public methods ----

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }

    /// <summary>
    /// Handles <see cref="UIController.State"/> changes
    /// </summary>
    /// <param name="sender">Must be the <see cref="UIController"/> instance</param>
    /// <param name="e">Ignored</param>
    public void OnMenuStateChange(object sender, EventArgs e)
    {
        var ui = sender as UIController;
        SetActive(ui && ui.State == UIController.EStateUI.InMainMenu);
    }

    // ---- Private methods ----

    private void Awake()
    {
        StartButton = transform.Find("StartButton").GetComponent<Button>();
        OptionsButton = transform.Find("OptionButton").GetComponent<Button>();
        QuitButton = transform.Find("QuitButton").GetComponent<Button>();
    }
}
