using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    public Button StartButton { get; private set; }
    public Button OptionsButton { get; private set; }
    public Button QuitButton { get; private set; }

    private void Awake()
    {
        StartButton = transform.Find("Start TMP Button").GetComponent<Button>();
        OptionsButton = transform.Find("Options TMP Button").GetComponent<Button>();
        QuitButton = transform.Find("Quit TMP Button").GetComponent<Button>();
    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }

    public void OnMenuStateChange(object sender, EventArgs e)
    {
        var ui = sender as UIControllerScript;
        SetActive(ui && ui.State == UIControllerScript.EStateUI.InMainMenu);
    }
}
