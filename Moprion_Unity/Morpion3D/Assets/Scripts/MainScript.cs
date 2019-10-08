﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EState
{
    Default,
    InMainMenu,
    InOptionMenu,
    ToMenu,
    InGame,
    InGameMenu,
    ToGame,
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

    public GameObject MainMenuPrefab;
    private MainMenuScript mainMenuScript;

    public GameObject GridPrefab;
    private GridScript gridScript;

    public GameObject OnlineStatusPrefab;
    private OnlineStatusScript onlineStatusScript;

    private Dictionary<object, bool> gameReadyEventsState;

    private void Awake()
    {
        State = EState.Default;
        CameraHandlerPrefab = Instantiate(CameraHandlerPrefab, transform);
        GridPrefab = Instantiate(GridPrefab, transform);
        MainMenuPrefab = Instantiate(MainMenuPrefab, transform);
        OnlineStatusPrefab = Instantiate(OnlineStatusPrefab, transform);
    }

    // Start is called before the first frame update
    void Start()
    {
        cameraScript = CameraHandlerPrefab.GetComponent<CameraScript>();
        StateChange += cameraScript.OnStateChange;
        cameraScript.ReadyGame += OnGameReadyEvents;

        gridScript = GridPrefab.GetComponent<GridScript>();
        StateChange += gridScript.OnStateChange;
        gridScript.ReadyGame += OnGameReadyEvents;

        mainMenuScript = MainMenuPrefab.GetComponent<MainMenuScript>();
        StateChange += mainMenuScript.OnStateChange;

        onlineStatusScript = OnlineStatusPrefab.GetComponent<OnlineStatusScript>();

        mainMenuScript.StartButton.onClick.AddListener(StartGame);
        mainMenuScript.QuitButton.onClick.AddListener(() => Application.Quit(0));

        gameReadyEventsState = new Dictionary<object, bool>();
        gameReadyEventsState.Add(gridScript, false);
        gameReadyEventsState.Add(cameraScript, false);

        State = EState.InMainMenu;
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
}

