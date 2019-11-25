using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyClient;

public enum EState
{
    Default,
    InMainMenu,
    ToMenu,
    InGame,
    InGameMenu,
    ToGame,
    SearchingOpponent,
}

public class MainScript : MonoBehaviour
{
    private EState _state;
    public EState State 
    { 
        get => _state;
        private set
        { 
            Debug.Log($"State change: {_state} to {value}");
            _state = value;
            OnStateChange();
        }
    }

    public event EventHandler StateChange;
    
    public GameObject CameraHandlerPrefab;
    private CameraScript cameraScript;

    public GameObject GridPrefab;
    private GridScript gridScript;

    public GameObject UIControllerPrefab;
    private UIController uiControllerScript;

    private Dictionary<object, bool> gameReadyEventsState;

    public Client Client { get; private set; }

    private void Awake()
    {
        State = EState.Default;
        CameraHandlerPrefab = Instantiate(CameraHandlerPrefab, transform);
        GridPrefab = Instantiate(GridPrefab, transform);
        UIControllerPrefab = Instantiate(UIControllerPrefab, transform);

        Client.InnitMethods();
        Client = new Client();
        
    }

    // Start is called before the first frame update
    void Start()
    {
        cameraScript = CameraHandlerPrefab.GetComponent<CameraScript>();
        uiControllerScript = UIControllerPrefab.GetComponent<UIController>();
        gridScript = GridPrefab.GetComponent<GridScript>();

        StateChange += cameraScript.OnStateChange;
        StateChange += gridScript.OnStateChange;
        StateChange += uiControllerScript.OnMainStateChange;

        uiControllerScript.ReadyToGame += (sender, e) => State = EState.ToGame;
        uiControllerScript.OptionsMenu.PlayerPatternChanged += gridScript.OnPlayerPatternChanged;

        gridScript.PlayerTurn += uiControllerScript.OnGameTurnChanged;

        cameraScript.ReadyGame += OnGameReadyEvents;
        gridScript.ReadyGame += OnGameReadyEvents;

        gameReadyEventsState = new Dictionary<object, bool>();
        gameReadyEventsState.Add(gridScript, false);
        gameReadyEventsState.Add(cameraScript, false);

        Client.Connected += uiControllerScript.OnlineStatusOverlay.OnConnected;
        Client.Disconnected += uiControllerScript.OnlineStatusOverlay.OnDisconnected;

        uiControllerScript.OpponentsMenu.UpdatingOpponentList += Client.OnMatchUpdatingOpponentList;
        Client.OpponentListUpdated += uiControllerScript.OpponentsMenu.OnOpponentListUpdated;

        Client.MatchRequestUpdated += uiControllerScript.MatchRequestHandler.OnMatchRequestUpdated;
        uiControllerScript.MatchRequestHandler.MatchRequestUpdated += Client.OnMatchRequestUpdated;

        Client.GameUpdated += gridScript.OnGameUpdated;
        gridScript.PositionPlayed += Client.OnPositionPlayed;

        State = EState.InMainMenu;

        Client.port = 13000;
        Client.localAddr = System.Net.IPAddress.Parse("127.0.0.1");

        Client.tryConnect();
        //StartCoroutine(IERepeatTryConnect(100));
    }

    void StartGame()
    {
        State = EState.ToGame;
    }

    void OnStateChange()
    {
        if (StateChange != null)
            StateChange(this, EventArgs.Empty);
    }

    void OnGameReadyEvents(object sender, EventArgs args)
    {
        gameReadyEventsState[sender] = true;

        if (!gameReadyEventsState.ContainsValue(false))
            State = EState.InGame;
    }

    //void OnConnected(object sender, EventArgs e)
    //{
    //    StopCoroutine()
    //}

    //IEnumerator IERepeatTryConnect(float period)
    //{
    //    while(!client.is_connected)
    //    {
    //        client.tryConnect();
    //        yield return new WaitForSeconds(period);
    //    }
    //    yield break;
    //}
}

