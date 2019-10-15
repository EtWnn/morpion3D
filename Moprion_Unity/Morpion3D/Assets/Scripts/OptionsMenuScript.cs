using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ServerInfoEventArgs : EventArgs
{
    public string ServerIP { get; set; }
    public string ServerPort { get; set; }
}

public class UsernameEventArgs : EventArgs
{
    public string Username;
}

public class OptionsMenuScript : MonoBehaviour
{
    public event EventHandler OptionMenuExited;
    public event EventHandler<ServerInfoEventArgs> ServerInfoEntered;
    public event EventHandler<UsernameEventArgs> UsernameEntered;

    public Button BackButton { get; private set; }
    public TMP_InputField UsernameField { get; private set; }
    public TMP_InputField ServerIPField { get; private set; }
    public TMP_InputField ServerPortField { get; private set; }

    private void Awake()
    {
        BackButton = transform.Find("Back Button").GetComponent<Button>();
        UsernameField = transform.Find("Username/InputField (TMP)").GetComponent<TMP_InputField>();
        ServerIPField = transform.Find("ServerIP/InputField (TMP)").GetComponent<TMP_InputField>();
        ServerPortField = transform.Find("ServerPort/InputField (TMP)").GetComponent<TMP_InputField>();
    }

    // Start is called before the first frame update
    void Start()
    {
        UsernameField.onValidateInput += delegate (string text, int charIndex, char addedChar) { return OnUsernameFieldUpdate(addedChar); };
        ServerIPField.onValidateInput += delegate (string text, int charIndex, char addedChar) { return OnServerIPFieldUpdate(addedChar); };
        ServerPortField.onValidateInput += delegate (string text, int charIndex, char addedChar) { return OnServerPortFieldUpdate(addedChar); };
        BackButton.onClick.AddListener(() => SetActive(false));
    }

    char OnUsernameFieldUpdate(char c)
    {
        if (!Char.IsLetterOrDigit(c) && !(c == '_'))
            c = '\0';
        return c;
    }

    char OnServerIPFieldUpdate(char c)
    {
        if (!Char.IsDigit(c) && c != '.')
            c = '\0';
        return c;
    }

    char OnServerPortFieldUpdate(char c)
    {
        if (!Char.IsDigit(c))
            c = '\0';
        return c;
    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }

    public void OnMenuStateChange(object sender, EventArgs e)
    {
        UIControllerScript ui = sender as UIControllerScript;
        if (ui && ui.State == UIControllerScript.EStateUI.InOptionsMenu)
            SetActive(true);
        else
            SetActive(false);
    }
}
