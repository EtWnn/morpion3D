using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button StartButton { get; private set; }
    public Button OptionsButton { get; private set; }
    public Button QuitButton { get; private set; }

    private void Awake()
    {
        StartButton = transform.Find("StartButton").GetComponent<Button>();
        OptionsButton = transform.Find("OptionButton").GetComponent<Button>();
        QuitButton = transform.Find("QuitButton").GetComponent<Button>();
    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }

    public void OnMenuStateChange(object sender, EventArgs e)
    {
        var ui = sender as UIController;
        SetActive(ui && ui.State == UIController.EStateUI.InMainMenu);
    }
}
