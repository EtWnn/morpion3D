using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ServerInfoEventArgs : EventArgs
{
    public string IP { get; set; }
    public string Port { get; set; }
}

public class UsernameEventArgs : EventArgs
{
    public string Username;
}

public enum EPlayerPatterns
{
    None,
    Cross,
    Tore,
}

public class OptionsMenu : MonoBehaviour
{
    public event EventHandler Exiting;
    public event EventHandler<ServerInfoEventArgs> ServerInfoEntered;
    public event EventHandler<UsernameEventArgs> UsernameEntered;
    public event EventHandler<TEventArgs<EPlayerPatterns>> PlayerPatternChanged;

    public Button BackButton { get; private set; }
    public TMP_InputField UsernameField { get; private set; }
    public TMP_InputField ServerIPField { get; private set; }
    public TMP_InputField ServerPortField { get; private set; }

    private EPlayerPatterns _playerPattern;
    public EPlayerPatterns PlayerPattern
    {
        get => _playerPattern;
        private set { _playerPattern = value; PlayerPatternChanged?.Invoke(this, new TEventArgs<EPlayerPatterns>(value)); }
    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }

    public void OnMenuStateChange(object sender, EventArgs e)
    {
        UIController ui = sender as UIController;
        if (ui && ui.State == UIController.EStateUI.InOptionsMenu)
            SetActive(true);
        else
            SetActive(false);
    }

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
        UsernameField.onValidateInput += OnValidateUsernameInput;
        ServerIPField.onValidateInput += OnValidateServerIpInput;
        ServerPortField.onValidateInput += OnValidateServerPortInput;
        BackButton.onClick.AddListener(OnExiting);

        SetupSelectPatternTogglesOnStart();
    }

    char OnValidateUsernameInput(string text, int charIndex, char addedChar)
    {
        if (!Char.IsLetterOrDigit(addedChar) && !(addedChar == '_'))
            addedChar = '\0';
        return addedChar;
    }

    char OnValidateServerIpInput(string text, int charIndex, char addedChar)
    {
        if (!Char.IsDigit(addedChar) && addedChar != '.')
            addedChar = '\0';
        return addedChar;
    }

    char OnValidateServerPortInput(string text, int charIndex, char addedChar)
    {
        if (!Char.IsDigit(addedChar))
            addedChar = '\0';
        return addedChar;
    }

    protected virtual void OnExiting()
    {
        Exiting?.Invoke(this, EventArgs.Empty);
    }

    private void SetupSelectPatternTogglesOnStart()
    {
        // Get components and CrossToreUI scripts
        var toggleGroup = GetComponentInChildren<ToggleGroup>();
        var toggles = toggleGroup.gameObject.GetComponentsInChildren<Toggle>();
        
        // Define a table between toggles gameobject names and the enumeration of player patterns
        Dictionary<string, EPlayerPatterns> patternTable = new Dictionary<string, EPlayerPatterns>();
        patternTable["CrossSelector"] = EPlayerPatterns.Cross;
        patternTable["ToreSelector"] = EPlayerPatterns.Tore;
        
        // Setup
        foreach (Toggle toggle in toggles)
        {
            // Set group
            toggle.group = toggleGroup;
            // Add listener to update PlayerPattern property
            toggle.onValueChanged.AddListener(
                (bool value) => { if (value) {PlayerPattern = patternTable[toggle.gameObject.name]; Debug.Log(PlayerPattern); }});
            // Add listener defined in PlayerPatternUI script in toggle gameobject children
            toggle.onValueChanged.AddListener(toggle.GetComponentInChildren<PlayerPatternUI>().OnToggled);
        }

        // Select cross pattern by def
        toggles[0].isOn = true;
    }
}
