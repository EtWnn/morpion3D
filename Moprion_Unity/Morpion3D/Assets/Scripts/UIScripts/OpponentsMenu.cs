using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyClient.Models;
using System.Threading;

internal class OpponentListUpdater
{
    public event EventHandler<TEventArgs<List<User>>> OpponentListUpdated;

    public void OnUpdatingOpponentList(object sender, EventArgs e)
    {
        Thread thread = new Thread(() =>
        {
            List<User> opponents = new List<User>();
            System.Random rd = new System.Random();
            int num = rd.Next(3, 15);
            for (int i = 0; i < num; i++)
                opponents.Add(new User(i, "Opponent " + i));
            RaiseOpponentListUpdated(opponents);
        });
        thread.Start();
    }

    protected virtual void RaiseOpponentListUpdated(List<User> opponents)
    {
        OpponentListUpdated?.Invoke(this, new TEventArgs<List<User>>(opponents));
    }
}

public class OpponentsMenu : MonoBehaviour
{
    ////// Prefabs /////

    /// <summary>
    /// OpponentSlot prefab public slot, this field should be set through Unity editor and not changed via scripting.
    /// </summary>
    public GameObject OpponentSlot;

    ///// Events /////

    public event EventHandler Exiting;
    public event EventHandler UpdatingOpponentList;
    public event EventHandler<MatchRequestEventArgs> SendingMatchRequest;

    ////// Properties /////

    public Button RefreshButton { get; private set; }
    public Button BackButton { get; private set; }
    public Button SendRequestButton { get; private set; }
    /// <summary>
    /// Either the selected opponent or null if no opponent is selected
    /// </summary>
    public User SelectedUser { get; private set; }
    
    ////// Private fields //////

    private GameObject ViewportContent;
    private ToggleGroup ToggleGroup;
    private SharedUpdatable<List<User>> Opponents;
    private SharedUpdatable<bool> isClientConnected;

    ///// Public method /////

    public void OnMenuStateChange(object sender, EventArgs e)
    {
        UIController ui = sender as UIController;
        if (ui && ui.State == UIController.EStateUI.InOpponentsMenu)
        {
            SetActive(true);
            Reinit();
        }
        else
            SetActive(false);
    }

    /// <summary>
    /// Event handler for when the opponents list is updated. 
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Must contains the updated opponent list in its Users field.</param>
    public void OnOpponentListUpdated(object sender, TEventArgs<List<User>> e) => Opponents.Write(e.Data);

    public void OnConnected(object sender, EventArgs e) => isClientConnected?.Write(true);
    public void OnDisconnected(object sender, EventArgs e) => isClientConnected?.Write(false);

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }

    ///// Private methods /////

    private void Awake()
    {
        RefreshButton = transform.Find("Refresh Button").GetComponent<Button>();
        BackButton = transform.Find("Back Button").GetComponent<Button>();
        SendRequestButton = transform.Find("Send Request Button").GetComponent<Button>();
        ViewportContent = transform.Find("Scroll View/Viewport/Content").gameObject;
        ToggleGroup = ViewportContent.GetComponent<ToggleGroup>();

        Opponents = new SharedUpdatable<List<User>>();

        isClientConnected = new SharedUpdatable<bool>();
        isClientConnected.UpdateAction = (bool value) => RefreshButton.interactable = value;
        isClientConnected.UpdateAction(GetComponentInParent<MainScript>().Client.is_connected);


        SelectedUser = null;

        BackButton.onClick.AddListener(RaiseExiting);
        RefreshButton.onClick.AddListener(RaiseUpdatingOpponentList);
        SendRequestButton.onClick.AddListener(RaiseSendingMatchRequest);

        SendRequestButton.interactable = false;

        /// Client simulation for testing

        //client = new OpponentListUpdater();

        //client.OpponentListUpdated += OnOpponentListUpdated;
        //UpdatingOpponentList += client.OnUpdatingOpponentList;
    }

    // Update is called once per frame
    private void Update()
    {
        Opponents.TryProcessIfNew();
    }

    private void UpdateViewport(List<User> opponents)
    {
        var viewportContentTransform = ViewportContent.transform;

        /// Reinitialisating all objects
        Reinit();

        /// Re-populate viewport
        foreach (var opp in opponents)
        {
            var slot = Instantiate(OpponentSlot, viewportContentTransform);
            var script = slot.GetComponent<OpponentSlot>();
            script.SetUser(opp);
            script.SetToggleGroup(ToggleGroup);
            script.OnToggled += OnOpponentSlotToggled;
        }
    }

    private void Reinit()
    {
        foreach (Transform child in ViewportContent.transform)
            Destroy(child.gameObject);
        SelectedUser = null;
        SendRequestButton.interactable = false;
    }

    ///// Event handlers /////

    private void OnOpponentSlotToggled(object sender, EventArgs args)
    {
        var slotScript = sender as OpponentSlot;
        if (slotScript != null)
        {
            SelectedUser = slotScript.User;
            SendRequestButton.interactable = true;
        }
    }

    ///// Event wrappers /////

    protected virtual void RaiseExiting() => Exiting?.Invoke(this, EventArgs.Empty);

    protected virtual void RaiseUpdatingOpponentList() => UpdatingOpponentList?.Invoke(this, EventArgs.Empty);

    protected virtual void RaiseSendingMatchRequest()
    {
        if (SelectedUser != null)
            SendingMatchRequest?.Invoke(this, new MatchRequestEventArgs(SelectedUser, MatchRequestEventArgs.EStatus.New));
    }
}
