using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TurnIndicator : MonoBehaviour
{
    public event EventHandler Exiting;

    private const string PlayerWonText = "<color=#66FFD9><size=110%>You</size=150%></color=#66FFD9> won !";
    private const string PlayerLoseText = "<color=#66FFD9><size=110%>You</size=150%></color=#66FFD9> lose !";
    private const string PlayerTurnText = "<color=#66FFD9><size=110%>Your</size=150%></color=#66FFD9> turn !";
    private const string OpponentTurnText = "<color=#66FFD9><size=110%>Opponent's</size=150%></color=#66FFD9> turn !";
    private const string OpponentDisconnectedText = "Your opponent is <color=#66FFD9><size=110%></size=150%>disconnected</color=#66FFD9> !";

    private TextMeshProUGUI _tmpText;
    public string Text
    {
        get => _tmpText.text;
        set => _tmpText.text = value;
    }

    public float TextCoroutinePerid = 1f;
    public Button BackButton { get; private set; }

    private Image background;
    private SharedUpdatable<bool> opponentHasDisconectoned;

    public void SetActive(bool value) => gameObject.SetActive(value);

    public void OnOpponentDisconnected(object sender, EventArgs e)
    {
        opponentHasDisconectoned.Write(true);
    }

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
