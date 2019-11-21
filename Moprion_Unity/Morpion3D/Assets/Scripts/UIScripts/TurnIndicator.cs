using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TurnIndicator : MonoBehaviour
{
    private const string PlayerTurnText = "<color=#66FFD9><size=110%>Your</size=150%></color=#66FFD9> turn !";
    private const string BaseOpponentTurnText = "<color=#66FFD9><size=110%>Opponent's</size=150%></color=#66FFD9> turn ";
    
    private TextMeshProUGUI _tmpText;
    public string Text
    {
        get => _tmpText.text;
        set => _tmpText.text = value;
    }

    public float TextCoroutinePerid = 1f;

    private Image background;
    private bool coroutineRunning;
    private Coroutine coroutine;

    public void SetActive(bool value) => gameObject.SetActive(value);

    public void SetTurn(bool isPlayerTurn)
    {
        if (isPlayerTurn)
        {
            SetSuspentionPointsAnimActive(false);
            Text = PlayerTurnText;
            SetBackgroundActive(false);
        }
        else
        {

            SetSuspentionPointsAnimActive(true);
            SetBackgroundActive(true);
        }
    }

    private void SetBackgroundActive(bool value) => background.enabled = value;

    private void SetSuspentionPointsAnimActive(bool value)
    {
        if (value && !coroutineRunning)
        {
            coroutine = StartCoroutine(IESuspensionPointsAnim());
            coroutineRunning = true;
        }
        else if (!value && coroutineRunning)
        {
            StopCoroutine(coroutine);
            coroutineRunning = false;
        }
    }

    private void Awake()
    {
        _tmpText = GetComponentInChildren<TextMeshProUGUI>();
        background = GetComponent<Image>();
    }

    private IEnumerator IESuspensionPointsAnim()
    {
        var cnt = 0;
        while(true)
        {
            Text = BaseOpponentTurnText + new String('.', ++cnt);
            if (cnt == 3)
                cnt = 0;
        }
    }
}
