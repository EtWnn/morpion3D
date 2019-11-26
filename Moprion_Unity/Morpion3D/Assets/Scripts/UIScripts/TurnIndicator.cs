using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TurnIndicator : MonoBehaviour
{
    // ---- Events ----

    /// <summary>
    /// Fired when a game is finished and the player wants to return the menu.
    /// </summary>
    public event EventHandler Exiting;

    // ---- Public fields and properties ----

    /// <summary>
    /// Button allowing the player to return on the menu when a game is finished.
    /// </summary>
    public Button BackButton { get; private set; }

    private TextMeshProUGUI _tmpText;
    /// <summary>
    /// Text to display on the right hand side of the "grid" game board.
    /// </summary>
    public string Text
    {
        get => _tmpText.text;
        set => _tmpText.text = value;
    }

    // ---- Private fields / properties ----

    private Image background;
    private SharedUpdatable<bool> opponentHasDisconectoned;

    private const string PlayerWonText = "<color=#66FFD9><size=110%>You</size=150%></color=#66FFD9> won !";
    private const string PlayerLoseText = "<color=#66FFD9><size=110%>You</size=150%></color=#66FFD9> lose !";
    private const string PlayerTurnText = "<color=#66FFD9><size=110%>Your</size=150%></color=#66FFD9> turn !";
    private const string OpponentTurnText = "<color=#66FFD9><size=110%>Opponent's</size=150%></color=#66FFD9> turn !";
    private const string OpponentDisconnectedText = "Your opponent is <color=#66FFD9><size=110%></size=150%>disconnected</color=#66FFD9> !";

    // ---- Event handlers

    /// <summary>
    /// Handles opponent disconnection. Allow the player to return to the menu.
    /// </summary>
    /// <param name="sender">Ignored</param>
    /// <param name="e">Ignored</param>
    public void OnOpponentDisconnected(object sender, EventArgs e) => opponentHasDisconectoned.Write(true);

    // ---- Public methods ----

    public void SetActive(bool value) => gameObject.SetActive(value);

    public void SetTurn(GridScript.EPlayerTurn playerEstate)
    {
        switch(playerEstate)
        {
            case (GridScript.EPlayerTurn.IsPlayerTurn):
                Text = PlayerTurnText;
                SetBackgroundActive(false);
                break;
            case (GridScript.EPlayerTurn.IsOpponentTurn):
                Text = OpponentTurnText;
                SetBackgroundActive(true);
                break;
            case (GridScript.EPlayerTurn.PlayerWon):
                Text = PlayerWonText;
                SetBackgroundActive(true);
                BackButton.gameObject.SetActive(true);
                break;
            case (GridScript.EPlayerTurn.OpponentWon):
                Text = PlayerLoseText;
                SetBackgroundActive(true);
                BackButton.gameObject.SetActive(true);
                break;
        }
    }

    private void Awake()
    {
        _tmpText = GetComponentInChildren<TextMeshProUGUI>();
        background = GetComponent<Image>();
        BackButton = GetComponentInChildren<Button>(true);
        Debug.Log("BackButton: " + BackButton);
        opponentHasDisconectoned = new SharedUpdatable<bool>(false);
        opponentHasDisconectoned.UpdateAction = ProcessOpponentDisconnected;
    }

    private void Start()
    {
        BackButton.onClick.AddListener(() =>
        {
            Exiting?.Invoke(this, EventArgs.Empty);
            BackButton.gameObject.SetActive(false);
        });
    }

    private void Update()
    {
        opponentHasDisconectoned.TryProcessIfNew();
    }

    private void ProcessOpponentDisconnected(bool hasDisconnected)
    {
        if (hasDisconnected)
            Text = OpponentDisconnectedText;
            SetBackgroundActive(true);
            BackButton.gameObject.SetActive(true);
    }

    private void SetBackgroundActive(bool value) => background.enabled = value;
}
