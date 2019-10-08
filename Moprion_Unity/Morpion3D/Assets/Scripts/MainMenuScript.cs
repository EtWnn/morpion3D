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
        StartButton = transform.Find("Canvas/Panel/Start TMP Button").GetComponent<Button>();
        OptionsButton = transform.Find("Canvas/Panel/Options TMP Button").GetComponent<Button>();
        QuitButton = transform.Find("Canvas/Panel/Quit TMP Button").GetComponent<Button>();
    }

    public void OnStateChange(object sender, EventArgs args)
    {
        MainScript ms = sender as MainScript;
        switch (ms.State)
        {
            case EState.InMainMenu:
                gameObject.SetActive(true);
                break;
            default:
                gameObject.SetActive(false);
                break;
        }
    }
}
