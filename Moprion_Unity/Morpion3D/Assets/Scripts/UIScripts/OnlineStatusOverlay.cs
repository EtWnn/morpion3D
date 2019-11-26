using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//internal class TestClient
//{
//    public event EventHandler Connected;
//    public event EventHandler Disconnected;

//    public void Start()
//    {
//        Thread thread = new Thread(() =>
//        {
//            while(true)
//            {
//                Connected?.Invoke(this, EventArgs.Empty);
//                Thread.Sleep(500);
//                Disconnected?.Invoke(this, EventArgs.Empty);
//                Thread.Sleep(500);
//            }
//        });
//        thread.Start();
//    }
//}


/// <summary>
/// Handles the connection status indicator.
/// </summary>
public class OnlineStatusOverlay : MonoBehaviour
{
    // ---- Enums ----

    public enum EState
    {
        None,
        Offline,
        Online,
    }

    // ---- Public fields / properties ----

    public Color OfflineColor;
    public Color OnlineColor;

    public string OfflineText;
    public string OnlineText;

    // ---- Private fields / properties ----

    SharedUpdatable<EState> State;
    private Image image;
    private TextMeshProUGUI text;

    // ---- Event handlers ----

    public void OnConnected(object sender, EventArgs e)
    {
        State.Write(EState.Online);
    }

    public void OnDisconnected(object sender, EventArgs e)
    {
        Debug.Log("Overlay status OnDisconnected");
        State.Write(EState.Offline);
    }

    // ---- Private methods ----

    void Awake()
    {
        OfflineColor = Color.red;
        OnlineColor = Color.green;

        OfflineText = "Offline";
        OnlineText = "Online";

        image = transform.Find("Background").GetComponent<Image>();
        text = transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();

        image.color = OfflineColor;
        text.text = OfflineText;

        State = new SharedUpdatable<EState>(EState.Offline);
        State.UpdateAction = updateState;

        //TestClient client = new TestClient();
        //client.Connected += OnConnected;
        //client.Disconnected += OnDisconnected;
        //client.Start();
    }

    private void updateState(EState state)
    {
        switch (state)
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
        State.TryProcessIfNew();
    }

}
