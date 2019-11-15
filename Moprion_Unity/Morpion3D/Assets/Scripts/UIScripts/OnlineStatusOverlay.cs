using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnlineStatusOverlay : MonoBehaviour
{
    public enum EState
    {
        None,
        Offline,
        Online,
    }

    private bool update;
    private EState _state;
    public EState State
    { 
        get => _state;
        set
        {
            if (value != State)
            {
                _state = value;
                update = true;
            }
        }
    }

    public Color OfflineColor;
    public Color OnlineColor;

    public string OfflineText;
    public string OnlineText;

    private Image image;
    private TextMeshProUGUI text;
    
    public void OnConnected(object sender, EventArgs e)
    {
        State = EState.Online;
    }

    public void OnDisconnected(object sender, EventArgs e)
    {
        State = EState.Offline;
    }

    // Start is called before the first frame update
    void Start()
    {
        OfflineColor = Color.red;
        OnlineColor = Color.green;

        OfflineText = "Offline";
        OnlineText = "Online";

        image = transform.Find("Background").GetComponent<Image>();
        text = transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();

        image.color = OfflineColor;
        text.text = OfflineText;
    }

    private void updateState()
    {
        switch (State)
        {
            case EState.Offline:
                image.color = OfflineColor;
                text.text = OfflineText;
                break;
            case EState.Online:
                image.color = OnlineColor;
                text.text = OnlineText;
                break;
            default:
                image.color = OfflineColor;
                text.text = OfflineText;
                break;
        }
    }

    private void Update()
    {
        if (update)
            updateState();
    }

}
