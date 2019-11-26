using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Handles the popup instanciation and the background.
/// </summary>
public class PopupPanel : MonoBehaviour
{
    // ---- Prefabs ----
    // Set through Unity editor

    public GameObject BasicPopupGO;
    public GameObject TwoButtonPopupGO;
    public GameObject WaitingPopupGO;
    public GameObject WaitingPopupCancelButtonGO;
    public GameObject TimeoutPopupGO;

    // ---- Private fields / properties ----

    private Image background;
    private int activePopupCount = 0;

    // ---- Public methods ----

    public void SetBackgroundActive(bool value) { background.enabled = value; }

    public BasicPopup InstanciateBasicPopup()
    {
        var popup = Instantiate(BasicPopupGO, transform).GetComponent<BasicPopup>();
        popup.Enabled += OnPopupEnabled;
        popup.Disabled += OnPopupDisabled;
        // Call OnPopupEnabled because popup was enabled before the event listener was listening
        OnPopupEnabled(null, EventArgs.Empty);
        return popup;
    }

    public WaitingPopupCancelButton InstanciateWaitingPopupCancelButton()
    {
        var popup = Instantiate(WaitingPopupCancelButtonGO, transform).GetComponent<WaitingPopupCancelButton>();
        popup.Enabled += OnPopupEnabled;
        popup.Disabled += OnPopupDisabled;
        // Call OnPopupEnabled because popup was enabled before the event listener was listening
        OnPopupEnabled(null, EventArgs.Empty);
        return popup;
    }

    public TimeoutPopup InstanciateTimeoutPopup()
    {
        var popup = Instantiate(TimeoutPopupGO, transform).GetComponent<TimeoutPopup>();
        popup.Enabled += OnPopupEnabled;
        popup.Disabled += OnPopupDisabled;
        // Call OnPopupEnabled because popup was enabled before the event listener was listening
        OnPopupEnabled(null, EventArgs.Empty);
        return popup;
    }

    public void DestroyAllPopup()
    {
        foreach (GameObject child in transform)
            Destroy(child);
    }

    public void DesactivateAllPopup()
    {
        foreach (GameObject child in transform)
            if (child.activeSelf)
                child.SetActive(false);
    }

    public void SetActive(bool value) => gameObject.SetActive(value);

    // ---- Private methods ----

    void Awake() { background = GetComponent<Image>(); }

    private void OnPopupEnabled(object sender, EventArgs e)
    {
        activePopupCount++;
        if (!background.enabled)
            SetBackgroundActive(true);
    }
    private void OnPopupDisabled(object sender, EventArgs e)
    {
        activePopupCount--;
        if (activePopupCount == 0 && background.enabled)
            SetBackgroundActive(false);
    }
}
