using System;
using UnityEngine;
using UnityEngine.UI;


public class MainScript : MonoBehaviour
{
    public enum EState
    {
        ToMenu,
        InMainMenu,
        InOptionMenu,
        ToGame,
        InGame,
    }

    public EState State 
    { 
        get => State;
        private set { StateChange(this, EventArgs.Empty); State = value; }
    }

    public event EventHandler StateChange;
    
    private Action updateFunction;

    public GameObject CameraHandlerPrefab;
    private CameraScript cameraScript;

    public GameObject MainMenuPrefab;
    // private MainMenuScript mainMenuScript;

    public GameObject GridPrefab;
    private GridScript gridScript;


    // Start is called before the first frame update
    void Start()
    {
        CameraHandlerPrefab = Instantiate(CameraHandlerPrefab, transform);
        cameraScript = CameraHandlerPrefab.GetComponent<CameraScript>();
        
        MainMenuPrefab = Instantiate(MainMenuPrefab, transform);

        GridPrefab = Instantiate(GridPrefab, transform);
        gridScript = GridPrefab.GetComponent<GridScript>();

        State = EState.InMainMenu;
        updateFunction = NoOpBehaviour;
    }

    void Update()
    {
        updateFunction();
    }

    public void NoOpBehaviour() { }

    public void ToGameBehaviour()
    {
        if (IsReadyGame())
        {
            cameraScript.UpdateCameraState(CameraScript.ECameraState.InGame);
            gridScript.UpdateGridState(GridScript.EState.InGame);
            State = EState.InGame;
            updateFunction = NoOpBehaviour;
        }
    }

    public void BeginToGame()
    {
        Debug.Log("BeginToGame function");
        cameraScript.UpdateCameraState(CameraScript.ECameraState.ToGame);
        gridScript.UpdateGridState(GridScript.EState.ToGame);
        MainMenuPrefab.SetActive(false);
        State = EState.ToGame;
        updateFunction = ToGameBehaviour;
    }

    public bool IsReadyGame()
    {
        return 
            cameraScript.CamState == CameraScript.ECameraState.ReadyGame
            && gridScript.GridState == GridScript.EState.ReadyGame;
    }
}

