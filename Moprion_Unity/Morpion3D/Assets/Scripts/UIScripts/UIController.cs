using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyClient.Models;

public class UIController : MonoBehaviour
{
    public enum EStateUI
    {
        Default,
        InMainMenu,
        InOptionsMenu,
        SearchingOpponents,
        InOpponentsMenu,
    }

    public OnlineStatusOverlay OnlineStatusOverlay { get; private set; }
    public MainMenu MainMenu { get; private set; }
    public OpponentsMenu OpponentsMenu { get; private set; }
    public OptionsMenu OptionsMenu { get; private set; }
    public PopupPanel PopupPanel { get; private set; }
    public TurnIndicator TurnIndicator { get; private set; }
    public MatchRequestHandler MatchRequestHandler { get; private set; }

    public event EventHandler ReadyToGame;
    public event EventHandler StateChange;

    private EStateUI _state;
    public EStateUI State
    {
        get => _state;
        private set { _state = value; StateChange?.Invoke(this, EventArgs.Empty); }
    }

    private Canvas canvas;
    private GameObject GameCommandsOverlay;
    // Start is called before the first frame update
    void Awake()
    {
        canvas = GetComponentInChildren<Canvas>();
        var MainMenuGO = transform.Find("Canvas/MainMenuGO");
        MainMenu = MainMenuGO.GetComponent<MainMenu>();

        OptionsMenu = transform.Find("Canvas/OptionsMenuGO").GetComponent<OptionsMenu>();
        OnlineStatusOverlay = transform.Find("Canvas/OnlineStatusGO").GetComponent<OnlineStatusOverlay>();
        OpponentsMenu = transform.Find("Canvas/OpponentsMenuGO").GetComponent<OpponentsMenu>();
        var popupPanelGo = transform.Find("Canvas/PopupPanel");
        PopupPanel = popupPanelGo.GetComponent<PopupPanel>();
        MatchRequestHandler = popupPanelGo.GetComponent<MatchRequestHandler>();
        TurnIndicator = transform.Find("Canvas/TurnIndicator").GetComponent<TurnIndicator>();
        GameCommandsOverlay = transform.Find("Canvas/GameCommandsOverlay").gameObject;
    }

    private void Start()
    {
        StateChange += MainMenu.OnMenuStateChange;
        StateChange += OptionsMenu.OnMenuStateChange;
        StateChange += OpponentsMenu.OnMenuStateChange;

        MainMenu.StartButton.onClick.AddListener(() => State = EStateUI.InOpponentsMenu);
        MainMenu.OptionsButton.onClick.AddListener(() => State = EStateUI.InOptionsMenu);
        MainMenu.QuitButton.onClick.AddListener(() => Application.Quit(0));

        OptionsMenu.Exiting += OnSubMenuExiting;
        OpponentsMenu.Exiting += OnSubMenuExiting;

        State = EStateUI.InMainMenu;
    }

    ///// Event handlers /////

    public void OnMainStateChange(object sender, EventArgs e)
    {
        var ms = sender as MainScript;
        if (ms && ms.State == EState.InMainMenu)
            State = EStateUI.InMainMenu;
        else
            State = EStateUI.Default;

        GameCommandsOverlay.SetActive(ms && ms.State == EState.InGame);
        TurnIndicator.SetActive(ms && ms.State == EState.InGame);

    }

    private void OnSubMenuExiting(object sender, EventArgs e) => State = EStateUI.InMainMenu;

    public void OnGameTurnChanged(object sender, TEventArgs<bool> e)
    {
        TurnIndicator.SetTurn(e.Data);
    }

    ///// Event wrappers /////

    public void RaiseReadyToGame() => ReadyToGame?.Invoke(this, EventArgs.Empty);
}
