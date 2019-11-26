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
    private const string OpponentDisconnectedText = "<color=#66FFD9><size=110%>Your opponent is</size=150%></color=#66FFD9> disconnected !";

    private TextMeshProUGUI _tmpText;
    public string Text
    {
        get => _tmpText.text;
        set => _tmpText.text = value;
    }

    public float TextCoroutinePerid = 1f;
    public Button BackButton { get; private set; }

    private Image background;

    public void SetActive(bool value) => gameObject.SetActive(value);

    public void SetTurn(GridScript.PlayerEstate playerEstate)
    {
        switch(playerEstate)
        {
            case (GridScript.PlayerEstate.IsTurn):
                Text = PlayerTurnText;
                SetBackgroundActive(false);
                break;
            case (GridScript.PlayerEstate.NotIsTurn):
                Text = OpponentTurnText;
                SetBackgroundActive(true);
                break;
            case (GridScript.PlayerEstate.Won):
                Text = PlayerWonText;
                SetBackgroundActive(true);
                BackButton.gameObject.SetActive(true);
                break;
            case (GridScript.PlayerEstate.Lose):
                Text = PlayerLoseText;
                SetBackgroundActive(true);
                BackButton.gameObject.SetActive(true);
                break;
            case (GridScript.PlayerEstate.Alone):
                Text = OpponentDisconnectedText;
                SetBackgroundActive(true);
                // BackButton.gameObject.SetActive(true);
                break;
        }
    }

    private void SetBackgroundActive(bool value) => background.enabled = value;

    private void Awake()
    {
        _tmpText = GetComponentInChildren<TextMeshProUGUI>();
        background = GetComponent<Image>();
        BackButton = GetComponentInChildren<Button>(true);
        Debug.Log("BackButton: " + BackButton);
    }

    private void Start()
    {
        BackButton.onClick.AddListener(() =>
        {
            Exiting?.Invoke(this, EventArgs.Empty);
            BackButton.gameObject.SetActive(false);
        });
    }
}
