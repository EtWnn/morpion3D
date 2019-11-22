using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyClient.Models;
using TMPro;

internal class CientMatchReq
{
    public event EventHandler<MatchRequestEventArgs> MatchRequestUpdated;

    private Thread listeningThread;

    public void OnMatchRequestUpdated(object sender, MatchRequestEventArgs e)
    {  
        switch (e.Status)
        {
            case MatchRequestEventArgs.EStatus.New:
                // Simulate opponent respoonse
                var eCopy = e;
                listeningThread = new Thread(() =>
                {
                    try
                    {
                        Thread.Sleep(2000);
                        MatchRequestUpdated?.Invoke(this, new MatchRequestEventArgs(eCopy.User, MatchRequestEventArgs.EStatus.Accepted));
                    }
                    catch (ThreadInterruptedException)
                    {}
                });
                listeningThread.Start();
                break;
            case MatchRequestEventArgs.EStatus.Canceled:
                // Notifyy opponent
                listeningThread.Interrupt();
                break;
            case MatchRequestEventArgs.EStatus.Accepted:
                // Notify opponent
                break;
            case MatchRequestEventArgs.EStatus.Declined:
                // Notify opponent
                break;
            default:
                break;
        }
    }

    public void SimulateRequestFromOpponent(int msFromStart, bool cancelRequest = false)
    {
        Debug.Log("Incoming match request in " + msFromStart + " ms!");
        var th = new Thread(() =>
        {
            User opponent = new User(654321, "CeriseDeGroupama");
            Thread.Sleep(msFromStart);
            MatchRequestUpdated?.Invoke(this, new MatchRequestEventArgs(opponent, MatchRequestEventArgs.EStatus.New));
            if (cancelRequest)
            {
                Thread.Sleep(100);
                MatchRequestUpdated?.Invoke(this, new MatchRequestEventArgs(opponent, MatchRequestEventArgs.EStatus.Canceled));
            }
        });
        th.Start();
    }
}

/// <summary>
/// Event args derived class for communication between unity client and web client
/// </summary>
public class MatchRequestEventArgs : EventArgs
{
    public enum EStatus
    {
        New,
        Canceled,
        Accepted,
        Declined,
        CannotBeReached,
    }

    public User User { get; set; }
    public EStatus Status { get; set; }

    public MatchRequestEventArgs(User user, EStatus status)
    {
        User = user;
        Status = status;
    }
}

public class MatchRequestHandler : MonoBehaviour
{
    public event EventHandler<MatchRequestEventArgs> MatchRequestUpdated;

    public UIController UIController { get; private set; }
    public PopupPanel PopupPanel { get; private set; }

    private bool update;
    private List<Func<MatchRequestEventArgs, bool>> popupUpdaters;
    public MatchRequestEventArgs MatchRequestInfo { get; set; }

    // Test field: simulate client
    // TODO: Replace with real client
    //CientMatchReq client;

    private void Awake()
    {
        UIController = GetComponentInParent<UIController>();
        PopupPanel = GetComponent<PopupPanel>();

        //client = new CientMatchReq();
        popupUpdaters = new List<Func<MatchRequestEventArgs, bool>>();
    }

    private void Start()
    {
        UIController.OpponentsMenu.SendingMatchRequest += OnSendingMatchRequest;
        // TODO: replace with real client
        //MatchRequestUpdated += client.OnMatchRequestUpdated;
        //client.MatchRequestUpdated += (sender, e) => { MatchRequestInfo = e; update = true; };

        //client.SimulateRequestFromOpponent(10000);
    }

    private void Update()
    {
        if (update)
        {
            if (NewMatchRequestUpdater(MatchRequestInfo))
                update = false;
            else
                foreach (var updater in popupUpdaters)
                    if (updater(MatchRequestInfo))
                    {
                        update = false;
                        break;
                    }

            if (update)
                Debug.LogError("New MatchRequestInfo has not been handled! UserName : "
                    + MatchRequestInfo.User.UserName + " [" + MatchRequestInfo.Status + "]");
        }
    }

    ///// Event handlers /////

    private void OnSendingMatchRequest(object sender, MatchRequestEventArgs e)
    {
        // Set popup
        var popup = PopupPanel.InstanciateWaitingPopupCancelButton();
        popup.Text = "Waiting for \n<color=#66FFD9><b>" + e.User.UserName + "</b></color>";
        popup.CancelButton.onClick.AddListener(() =>
        {
            Destroy(popup.gameObject);
            MatchRequestUpdated?.Invoke(this, new MatchRequestEventArgs(e.User, MatchRequestEventArgs.EStatus.Canceled));
        });
        popup.StatorAnimation.StartRotate(1);
        
        // Set popup updater function
        var popupUpdater = MatchRequestResponseUpdaterGen(popup, e.User);
        popupUpdaters.Add(popupUpdater);
        popup.Disabled += (sender_, e_) => popupUpdaters.Remove(popupUpdater);
        
        // Notify web client
        MatchRequestUpdated?.Invoke(this, new MatchRequestEventArgs(e.User, MatchRequestEventArgs.EStatus.New));
    }

    public void OnMatchRequestUpdated(object sender, MatchRequestEventArgs e) { MatchRequestInfo = e; update = true; }

///// Popup updater functions and generators /////

private bool NewMatchRequestUpdater(MatchRequestEventArgs matchRequest)
    {
        if (matchRequest.Status == MatchRequestEventArgs.EStatus.New)
        {
            // Set popup
            var popup = PopupPanel.InstanciateTimeoutPopup();
            popup.Text = "New match request from \n<color=#66FFD9><b>" + matchRequest.User.UserName + "</b></color>";
            
            popup.AcceptButton.onClick.AddListener(() =>
            {
                popup.StatorAnimation.Interrupt();
                popup.AcceptButton.interactable = false;
                popup.DeclineButton.interactable = false;
                popup.StatorAnimation.StartPulse(Color.green, 0.5f, 4);
                popup.StatorAnimation.Finished += (sender, e) =>
                {
                    Destroy(popup.gameObject);
                    UIController.RaiseReadyToGame();
                };
                MatchRequestUpdated?.Invoke(this, new MatchRequestEventArgs(matchRequest.User, MatchRequestEventArgs.EStatus.Accepted));
            });

            popup.DeclineButton.onClick.AddListener(() =>
            {
                Destroy(popup.gameObject);
                MatchRequestUpdated?.Invoke(this, new MatchRequestEventArgs(matchRequest.User, MatchRequestEventArgs.EStatus.Declined));
            });

            popup.StatorAnimation.StartTimeout(5);
            popup.StatorAnimation.Finished += (sender, e) =>
            {
                Destroy(popup.gameObject);
                MatchRequestUpdated?.Invoke(this, new MatchRequestEventArgs(matchRequest.User, MatchRequestEventArgs.EStatus.Declined));
            };

            // Set popup updater function
            var popupUpdater = NewMatchRequestCanceledUpdaterGen(popup, matchRequest.User);
            popupUpdaters.Add(popupUpdater);
            popup.Disabled += (sender, e) => popupUpdaters.Remove(popupUpdater);

            return true;
        }
        return false;
    }

    private Func<MatchRequestEventArgs, bool> NewMatchRequestCanceledUpdaterGen(TimeoutPopup popup, User targetUser)
    {
        return (matchRequest) =>
        {
            if (matchRequest.Status == MatchRequestEventArgs.EStatus.Canceled && matchRequest.User.Id == targetUser.Id)
            {
                popup.StatorAnimation.Interrupt();
                popup.Text = "<color=#66FFD9><b>" + matchRequest.User.UserName + "</b></color> has canceled the request!";

                popup.AcceptButton.GetComponentInChildren<TextMeshProUGUI>().text = "Back";
                popup.AcceptButton.onClick.RemoveAllListeners();
                popup.AcceptButton.onClick.AddListener(() => Destroy(popup.gameObject));
                popup.DeclineButton.gameObject.SetActive(false);

                popup.StatorAnimation.StartPulse(Color.red, 1f);
                return true;
            }
            return false;
        };
    }

    private Func<MatchRequestEventArgs, bool> MatchRequestResponseUpdaterGen(WaitingPopupCancelButton popup, User targetUser)
    {
        return (matchRequest) =>
        {
            var user = matchRequest.User;
            if (user.Id == targetUser.Id)
            {
                switch (matchRequest.Status)
                {
                    case MatchRequestEventArgs.EStatus.Accepted:
                        popup.StatorAnimation.Interrupt();
                        popup.Text = "<color=#66FFD9><b>" + user.UserName + "</b></color> has accepted the request!";
                        popup.CancelButton.interactable = false;
                        popup.StatorAnimation.StartPulse(Color.green, 1f, 3);
                        popup.StatorAnimation.Finished += (object sender_, TEventArgs<StatorAnimation.EAnimation> e_) => {
                            Destroy(popup.gameObject);
                            UIController.RaiseReadyToGame();
                        };
                        return true;

                    case MatchRequestEventArgs.EStatus.Declined:
                        popup.StatorAnimation.Interrupt();
                        popup.Text = "<color=#66FFD9><b>" + user.UserName + "</b></color> has declined the request!";
                        popup.StatorAnimation.StartPulse(Color.red, 1f);
                        return true;

                    case MatchRequestEventArgs.EStatus.CannotBeReached:
                        popup.StatorAnimation.Interrupt();
                        popup.Text = "<color=#66FFD9><b>" + user.UserName + "</b></color> cannot be reached!";
                        popup.StatorAnimation.StartPulse(Color.red, 1f);
                        return true;

                    case MatchRequestEventArgs.EStatus.Canceled:
                        popup.StatorAnimation.Interrupt();
                        popup.Text = "<color=#66FFD9><b>" + user.UserName + "</b></color> cannot be reached!";
                        popup.StatorAnimation.StartPulse(Color.red, 1f);
                        return true;
                }
            }
            return false;
        };
    }
}
