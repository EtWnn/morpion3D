using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyClient.Models;

/// <summary>
/// Allow to manage and draw all UI elements on the same Canvas.
/// </summary>
public class UIController : MonoBehaviour
{
    // ---- Enums ----

    public enum EStateUI
    {
        Default,
        InMainMenu,
        InOptionsMenu,
        SearchingOpponents,
        InOpponentsMenu,
    }
    
    // ---- Events ----

    public event EventHandler StateChange;
    /// <summary>
    /// Notify that UI elements are ready to begin transitioning from menu to game.
    /// </summary>
    public event EventHandler ReadyToGame;
    /// <summary>
    /// Notify that UI elements are ready to begin transitioning from game to menu.
    /// </summary>
    public event EventHandler ReadyToMenu;

    // ---- Public fields / properties ----

    public OnlineStatusOverlay OnlineStatusOverlay { get; private set; }
    public MainMenu MainMenu { get; private set; }
    public OpponentsMenu OpponentsMenu { get; private set; }
    public OptionsMenu OptionsMenu { get; private set; }
    public PopupPanel PopupPanel { get; private set; }
    public TurnIndicator TurnIndicator { get; private set; }
    public MatchRequestHandler MatchRequestHandler { get; private set; }


    private EStateUI _state;
    public EStateUI State
    {
        get => _state;
        private set { _state = value; StateChange?.Invoke(this, EventArgs.Empty); }
    }

    // ---- Private fields / properties ----

    private Canvas canvas;
    private GameObject GameCommandsOverlay;

    // ---- Event wrappers ----

    public void RaiseReadyToGame() => ReadyToGame?.Invoke(this, EventArgs.Empty);

    // ---- Event handlers ----

    /// <summary>
    /// Handle <see cref="MainScript.State"/> changes.
    /// </summary>
    /// <param name="sender">Must be the <see cref="MainScript"/></param>
    /// <param name="args">Ignored.</param>
    public void OnStateChange(object sender, EventArgs e)
    {
        var ms = sender as MainScript;
        if (ms && ms.State == EState.InMainMenu)
            State = EStateUI.InMainMenu;
        else
            State = EStateUI.Default;

        GameCommandsOverlay.SetActive(ms && ms.State == EState.InGame);
        TurnIndicator.SetActive(ms && ms.State == EState.InGame);

    }

    /// <summary>
    /// Handle player <see cref="GridScript.PlayerTurnChanged"/> events .
    /// </summary>
    /// <param name="sender">Ignored.</param>
    /// <param name="args">Must contains the up-to-date player turn.</param>
    public void OnGameTurnChanged(object sender, TEventArgs<GridScript.EPlayerTurn> e)
    {
        TurnIndicator.SetTurn(e.Data);
    }

    /// <summary>
    /// Handles managed sub menu UI component exiting. Change <see cref="State"/> to <see cref="EStateUI.InMainMenu"/>. 
    /// </summary>
    /// <param name="sender">Ignored.</param>
    /// <param name="e">Ignored.</param>
    public void OnSubMenuExiting(object sender, EventArgs e) => State = EStateUI.InMainMenu;

    // ---- Private Methods ----

    void Awake()
    {
        // Get components 
        canvas = GetComponentInChildren<Canvas>();
        MainMenu = transform.Find("Canvas/MainMenuGO").GetComponent<MainMenu>();
        OptionsMenu = transform.Find("Canvas/OptionsMenuGO").GetComponent<OptionsMenu>();
        OnlineStatusOverlay = transform.Find("Canvas/OnlineStatusGO").GetComponent<OnlineStatusOverlay>();
        OpponentsMenu = transform.Find("Canvas/OpponentsMenuGO").GetComponent<OpponentsMenu>();
        TurnIndicator = transform.Find("Canvas/TurnIndicator").GetComponent<TurnIndicator>();
        GameCommandsOverlay = transform.Find("Canvas/GameCommandsOverlay").gameObject;
        var popupPanelGo = transform.Find("Canvas/PopupPanel");
        PopupPanel = popupPanelGo.GetComponent<PopupPanel>();
        MatchRequestHandler = popupPanelGo.GetComponent<MatchRequestHandler>();
    }

    private void Start()
    {
        // Set sub menu event handler for State changes.
        StateChange += MainMenu.OnMenuStateChange;
        StateChange += OptionsMenu.OnMenuStateChange;
        StateChange += OpponentsMenu.OnMenuStateChange;

        // Subscribe event handlers provoking State changes
        MainMenu.StartButton.onClick.AddListener(() => State = EStateUI.InOpponentsMenu);
        MainMenu.OptionsButton.onClick.AddListener(() => State = EStateUI.InOptionsMenu);
        MainMenu.QuitButton.onClick.AddListener(() => Application.Quit(0));
        OptionsMenu.Exiting += OnSubMenuExiting;
        OpponentsMenu.Exiting += OnSubMenuExiting;

        // Subscribe event handler to notify MainScript the app can begin transitioning from game to menu.
        TurnIndicator.Exiting += (sender, e) => ReadyToMenu?.Invoke(this, EventArgs.Empty);

        State = EStateUI.InMainMenu;
    }
}
