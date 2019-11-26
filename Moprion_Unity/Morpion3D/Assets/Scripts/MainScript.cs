/// Global comments and insights about Unity 
/// --------------------------------------------------------------------------
/// - Every script attached to a GameObject must derive from UnityEngine.MonoBehaviour
/// - MonoBehaviour base class let us overide a few special method from which
///     - private void Awake():
///         Called once in a script lifetime, after object are initialized (so you can use Find() or GetComponent())
///     - private void Start():
///         Called before the first frame the script is enabled (note: script are enabled / disabled independantly from 


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyClient;
using CoroutineExtension;

/// <summary>
/// Application main states enumeration.
/// </summary>
public enum EState
{
    Default,
    InMainMenu,
    ToMenu,
    InGame,
    ToGame,
}

/// <summary>
/// Main "Unity" script.
/// <param>Attached to the only existing GameObject when executing the program.</param>
/// <param>It's responsible for instanciating other GameObject, a Client and linking event and event handler</param>
/// <param>It also define a application main state.</param>
/// </summary>
public class MainScript : MonoBehaviour
{
    /// <summary>
    /// Event fired when <see cref="State"/> changes.
    /// </summary>
    public event EventHandler StateChanged;
    private EState _state;
    public EState State 
    { 
        get => _state;
        private set
        { 
            Debug.Log($"State change: {_state} to {value}");
            _state = value;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Prefab GameObject initialized through UnityEditor.
    /// </summary>
    public GameObject CameraHandlerPrefab;
    private CameraScript cameraScript;

    /// <summary>
    /// Prefab GameObject initialized through UnityEditor.
    /// </summary>
    public GameObject GridPrefab;
    private GridScript gridScript;

    /// <summary>
    /// Prefab GameObject initialized through UnityEditor.
    /// </summary>
    public GameObject UIControllerPrefab;
    private UIController uiControllerScript;

    /// <summary>
    /// Used to keep track of the components which fired an event handled by
    /// <see cref="OnGameReadyEvents"/>.
    /// </summary>
    private Dictionary<object, bool> gameReadyEventsState;

    /// <summary>
    /// TCP Client.
    /// </summary>
    public Client Client { get; private set; }

    /// <summary>
    /// Coroutine which periodically ask the client to connect too the server until success.
    /// </summary>
    private CoroutineController TryConnectCO { get; set; }

    private void Awake()
    {
        State = EState.Default;

        // Instanciating base GameObjects
        CameraHandlerPrefab = Instantiate(CameraHandlerPrefab, transform);
        GridPrefab = Instantiate(GridPrefab, transform);
        UIControllerPrefab = Instantiate(UIControllerPrefab, transform);

        // Initializing client
        Client.InnitMethods();
        Client = new Client();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get scripts components
        cameraScript = CameraHandlerPrefab.GetComponent<CameraScript>();
        gridScript = GridPrefab.GetComponent<GridScript>();
        uiControllerScript = UIControllerPrefab.GetComponent<UIController>();

        gameReadyEventsState = new Dictionary<object, bool>();
        gameReadyEventsState.Add(cameraScript, false);
        gameReadyEventsState.Add(gridScript, false);

        // Subscribe event handlers to StateChanged
        StateChanged += cameraScript.OnStateChange;
        StateChanged += gridScript.OnStateChange;
        StateChanged += uiControllerScript.OnStateChange;

        // Set the components which event are needed to trigger GameReady state change
        gameReadyEventsState = new Dictionary<object, bool>();
        gameReadyEventsState.Add(gridScript, false);
        gameReadyEventsState.Add(cameraScript, false);

        // Subscribe event handlers responsible for main state changes
        uiControllerScript.ReadyToGame += (sender, e) => State = EState.ToGame;
        uiControllerScript.ReadyToMenu += (sender, e) => State = EState.ToMenu;
        cameraScript.ReadyMenu += (sender, e) => State = EState.InMainMenu;
        cameraScript.ReadyGame += OnGameReadyEvents;
        gridScript.ReadyGame += OnGameReadyEvents;

        // Subscribe event handlers relative to optionsmenu events
        uiControllerScript.OptionsMenu.PlayerPatternChanged += gridScript.OnPlayerPatternChanged;
        uiControllerScript.OptionsMenu.ServerInfoEntered += Client.OnServerInfoUpdated;
        uiControllerScript.OptionsMenu.UsernameEntered += Client.OnUsernameUpdate;

        // Subscribe event handlers relative to connection / disconnecction
        Client.Connected += uiControllerScript.OnlineStatusOverlay.OnConnected;
        Client.Connected += (sender, e) => TryConnectCO.Pause();
        Client.Disconnected += uiControllerScript.OnlineStatusOverlay.OnDisconnected;
        Client.Disconnected += (sender, e) => TryConnectCO.Resume();

        // Subscribe event handlers relative to updating opponent list
        uiControllerScript.OpponentsMenu.UpdatingOpponentList += Client.OnMatchUpdatingOpponentList;
        Client.OpponentListUpdated += uiControllerScript.OpponentsMenu.OnOpponentListUpdated;
        uiControllerScript.OpponentsMenu.UpdatingOpponentList += Client.OnMatchUpdatingOpponentList;

        // Subscribe event handlers relative to handling match requests
        Client.MatchRequestUpdated += uiControllerScript.MatchRequestHandler.OnMatchRequestUpdated;
        uiControllerScript.MatchRequestHandler.MatchRequestUpdated += Client.OnMatchRequestUpdated;

        // Subscribe event handlers relative to a game progression
        Client.GameUpdated += gridScript.OnGameUpdated;
        gridScript.PositionPlayed += Client.OnPositionPlayed;
        gridScript.PlayerTurnChanged += uiControllerScript.OnGameTurnChanged;
        Client.OpponentDisconnected += uiControllerScript.TurnIndicator.OnOpponentDisconnected;

        State = EState.InMainMenu;

        // Default IP:port of the server
        Client.port = 13000;
        Client.localAddr = System.Net.IPAddress.Parse("127.0.0.1");
        TryConnectCO = this.StartCoroutineEx(IERepeatTryConnect(1f));
    }

    /// <summary>
    /// Register object firing the event in <see cref="gameReadyEventsState"/>. When all expected sender has been registered,
    /// switch <see cref="State"/> to InGame and reset <see cref="gameReadyEventsState"/>;
    /// </summary>
    /// <param name="sender">Used, the sender.</param>
    /// <param name="args">Ignored</param>
    void OnGameReadyEvents(object sender, EventArgs args)
    {
        gameReadyEventsState[sender] = true;

        if (!gameReadyEventsState.ContainsValue(false))
        {
            State = EState.InGame;
            foreach (var key in gameReadyEventsState.Keys)
                gameReadyEventsState[key] = false;
        }
    }

    IEnumerator IERepeatTryConnect(float period)
    {
        yield return null; // Needed in case client connect on first try to prevent Error when trying to pause coroutine afterward
        while (true)
        {
            Debug.Log("Client: " + Client);
            Client.tryConnect();
            yield return new WaitForSeconds(period);
            Debug.Log("Client: " + Client);
        }
    }

    private void OnApplicationQuit()
    {
        
    }
}

