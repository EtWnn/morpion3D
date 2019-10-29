using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpponentsMenuScript : MonoBehaviour
{
    bool update;

    List<GameObject> Opponents;
    private GameObject ViewportContent;
    public GameObject OpponentSlot;

    public Button RefreshButton { get; private set; }
    public Button BackButton { get; private set; }
    public Button SendRequestButton { get; private set; }

    private void Awake()
    {
        RefreshButton = transform.Find("Refresh Button").GetComponent<Button>();
        BackButton = transform.Find("Back Button").GetComponent<Button>();
        SendRequestButton = transform.Find("Send Request Button").GetComponent<Button>();
        ViewportContent = transform.Find("Scroll View/Viewport/Content").gameObject;
        update = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (update)
        {

            update = false;
        }
    }

    private void UpdateViewport()
    {
        var viewportContentTransform = ViewportContent.transform;
        /// Destroy current opponent slots
        foreach (Transform child in viewportContentTransform)
            Destroy(child);

        /// Re-populate viewport
        foreach (var opponent in Opponents)
        {
            opponent.transform.parent = viewportContentTransform;
            opponent.SetActive(true);
        }
    }

    public void OnUpdateOpponentList(object sender, EventArgs e)
    {

    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }

    public void OnMenuStateChange(object sender, EventArgs e)
    {
        UIControllerScript ui = sender as UIControllerScript;
        if (ui && ui.State == UIControllerScript.EStateUI.InOpponentsMenu)
            SetActive(true);
        else
            SetActive(false);
    }
}
