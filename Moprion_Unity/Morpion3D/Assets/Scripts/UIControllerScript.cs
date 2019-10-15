using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIControllerScript : MonoBehaviour
{
    public enum EStateUI
    {
        NoMenu,
        InMainMenu,
        InOptionsMenu,
        SearchingOpponents,
    }

    private Canvas canvas;

    public GameObject MainMenuGO;
    public MainMenuScript MainMenuGOScript { get; private set; }

    public GameObject OptionsMenuGO;
    public OptionsMenuScript OptionsMenuGOScript { get; private set; }

    public GameObject SearchOpponentGO;
    public SearchOpponentPopupScript SearchOpponentGOScript { get; private set; }

    public GameObject OnlineStatusGO;
    public OnlineStatusScript OnlineStatusGOScript { get; private set; }

    public event EventHandler StateChange;

    private EStateUI _state;
    public EStateUI State
    {
        get => _state;
        private set { _state = value; OnMenuStateChange(); }
    }

    // Start is called before the first frame update
    void Awake()
    {
        canvas = GetComponentInChildren<Canvas>();
        
        MainMenuGO = Instantiate(MainMenuGO, canvas.transform);
        OptionsMenuGO = Instantiate(OptionsMenuGO, canvas.transform);
        SearchOpponentGO = Instantiate(SearchOpponentGO, canvas.transform);
        OnlineStatusGO = Instantiate(OnlineStatusGO, canvas.transform);

        MainMenuGOScript = MainMenuGO.GetComponent<MainMenuScript>();
        OptionsMenuGOScript = OptionsMenuGO.GetComponent<OptionsMenuScript>();
        SearchOpponentGOScript = SearchOpponentGO.GetComponent<SearchOpponentPopupScript>();
        OnlineStatusGOScript = OnlineStatusGO.GetComponent<OnlineStatusScript>();

        StateChange += MainMenuGOScript.OnMenuStateChange;
        StateChange += OptionsMenuGOScript.OnMenuStateChange;
        StateChange += SearchOpponentGOScript.OnMenuStateChange;

        MainMenuGOScript.StartButton.onClick.AddListener(() => State = EStateUI.SearchingOpponents);
        MainMenuGOScript.OptionsButton.onClick.AddListener(() => State = EStateUI.InOptionsMenu);
        MainMenuGOScript.QuitButton.onClick.AddListener(() => Application.Quit(0));
        OptionsMenuGOScript.BackButton.onClick.AddListener(() => State = EStateUI.InMainMenu);

        State = EStateUI.InMainMenu;
    }

    private void OnMenuStateChange()
    {
        StateChange?.Invoke(this, EventArgs.Empty);
    }

    void OnStateChange(object sender, EventArgs e)
    {
        var ms = sender as MainScript;
        if(ms && ms.State == EState.InMainMenu)
        {
            State = EStateUI.InMainMenu;
        }
        else
        {
            State = EStateUI.NoMenu;
        }
    }
}
