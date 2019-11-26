using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// EventArgs derivated class containing IP and Port string properties.
/// </summary>
public class ServerInfoEventArgs : EventArgs
{
    public string IP { get; set; }
    public string Port { get; set; }

    public ServerInfoEventArgs(string ip, string port) { IP = ip; Port = port; }
}

/// <summary>
/// EventArgs derivated class containing Username string property.
/// </summary>
public class UsernameEventArgs : EventArgs
{
    public string Username;

    public UsernameEventArgs(string username) { Username = username; }
}

/// <summary>
/// Enum oof the 2 playable patterns.
/// </summary>
public enum EPlayerPatterns
{
    None,
    Cross,
    Tore,
}

/// <summary>
/// Handles the Options menu and its components
/// </summary>
public class OptionsMenu : MonoBehaviour
{
    // ---- Events ----

    /// <summary>
    /// Triggered when exiting the Option menu.
    /// </summary>
    public event EventHandler Exiting;
    /// <summary>
    /// Triggered when new IP or port value are entered and the validate button is pressed.
    /// </summary>
    public event EventHandler<ServerInfoEventArgs> ServerInfoEntered;
    /// <summary>
    /// Triggered when a username is entered, the validate button is pressed and the client is currently connected to rhe server.
    /// </summary>
    public event EventHandler<UsernameEventArgs> UsernameEntered;
    /// <summary>
    /// Triggered when the selected player pattern changes
    /// </summary>
    public event EventHandler<TEventArgs<EPlayerPatterns>> PlayerPatternChanged;

    // ---- Public fields / properties ----

    public Button BackButton { get; private set; }
    public Button ValidateButton { get; private set; }

    public TMP_InputField UsernameField { get; private set; }
    public TMP_InputField ServerIPField { get; private set; }
    public TMP_InputField ServerPortField { get; private set; }

    private EPlayerPatterns _playerPattern;
    public EPlayerPatterns PlayerPattern
    {
        get => _playerPattern;
        private set { _playerPattern = value; PlayerPatternChanged?.Invoke(this, new TEventArgs<EPlayerPatterns>(value)); }
    }

    // ---- Private fields / properties ----

    private SharedUpdatable<bool> isClientConnected;

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
        UIController ui = sender as UIController;
        if (ui && ui.State == UIController.EStateUI.InOptionsMenu)
            SetActive(true);
        else
            SetActive(false);
    }

    public void OnConnected(object sender, EventArgs e) => isClientConnected?.Write(true);
    public void OnDisconnected(object sender, EventArgs e) => isClientConnected?.Write(false);

    // ---- Private methods / properties ----

    private void Awake()
    {
        BackButton = transform.Find("Back Button").GetComponent<Button>();
        ValidateButton = transform.Find("Validate Button").GetComponent<Button>();

        UsernameField = transform.Find("Username/InputField (TMP)").GetComponent<TMP_InputField>();
        ServerIPField = transform.Find("ServerIP/InputField (TMP)").GetComponent<TMP_InputField>();
        ServerPortField = transform.Find("ServerPort/InputField (TMP)").GetComponent<TMP_InputField>();

        Debug.Log($"(Awake)UsernameField: {UsernameField}");

        UsernameField.interactable = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        var mainScript = GetComponentInParent<MainScript>();

        ServerIPField.text = mainScript.Client.Ip.ToString();
        ServerPortField.text = mainScript.Client.Port.ToString();

        UsernameField.onValidateInput += OnValidateUsernameInput;
        ServerIPField.onValidateInput += OnValidateServerIpInput;
        ServerPortField.onValidateInput += OnValidateServerPortInput;

        BackButton.onClick.AddListener(() => Exiting?.Invoke(this, EventArgs.Empty));
        ValidateButton.onClick.AddListener(() => 
        {
            Debug.Log(mainScript.Client.Ip.ToString() + " " + mainScript.Client.Port.ToString());
            if(mainScript.Client.Ip.ToString() != ServerIPField.text
            || mainScript.Client.Port.ToString() != ServerPortField.text)
                ServerInfoEntered?.Invoke(this, new ServerInfoEventArgs(ServerIPField.text, ServerPortField.text));
            
            if(mainScript.Client.is_connected && UsernameField.text != "")
                UsernameEntered?.Invoke(this, new UsernameEventArgs(UsernameField.text));
        });
        SetupSelectPatternTogglesOnStart();

        isClientConnected = new SharedUpdatable<bool>();
        isClientConnected.UpdateAction = (bool value) => UsernameField.interactable = value;
        isClientConnected.UpdateAction(mainScript.Client.is_connected);
    }

    private void Update()
    {
        isClientConnected.TryProcessIfNew();
    }
    
    /// <summary>
    /// Validate each new char added.
    /// </summary>
    char OnValidateUsernameInput(string text, int charIndex, char addedChar)
    {
        if (!Char.IsLetterOrDigit(addedChar) && !(addedChar == '_'))
            addedChar = '\0';
        return addedChar;
    }

    /// <summary>
    /// Validate each new char added.
    /// </summary>
    char OnValidateServerIpInput(string text, int charIndex, char addedChar)
    {
        if (!Char.IsDigit(addedChar) && addedChar != '.')
            addedChar = '\0';
        return addedChar;
    }

    /// <summary>
    /// Validate each new char added.
    /// </summary>
    char OnValidateServerPortInput(string text, int charIndex, char addedChar)
    {
        if (!Char.IsDigit(addedChar))
            addedChar = '\0';
        return addedChar;
    }

    /// <summary>
    /// Setup code for the playable patterns UI elements
    /// </summary>
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
