using System;
using UnityEngine;


public class MainScript : MonoBehaviour
{
    public enum EApplicationState
    {
        ToMenu,
        MainMenu,
        OptionMenu,
        ToGame,
        InGame,
    }

    public GameObject CameraHandlerPrefab;
    private CameraScript cameraScript;

    public GameObject MainMenuPrefab;

    public GameObject GridPrefab;
    private GridScript gridScript;

    public EApplicationState AppState { get; private set; }
    private Action updateFunction;

    // Start is called before the first frame update
    void Start()
    {
        CameraHandlerPrefab = Instantiate(CameraHandlerPrefab);
        cameraScript = CameraHandlerPrefab.GetComponent<CameraScript>();
        
        MainMenuPrefab = Instantiate(MainMenuPrefab);
        
        GridPrefab = Instantiate(GridPrefab);
        gridScript = GridPrefab.GetComponent<GridScript>();

        AppState = EApplicationState.MainMenu;
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
            AppState = EApplicationState.InGame;
        }
    }

    public void BeginToGame()
    {
        cameraScript.UpdateCameraState(CameraScript.ECameraState.ToGame);
        MainMenuPrefab.SetActive(false);
        AppState = EApplicationState.ToGame;
    }

    public bool IsReadyGame()
    {
        return 
            cameraScript.CamState == CameraScript.ECameraState.ReadyGame
            && gridScript.GridState == GridScript.EGridState.ReadyGame;
    }
}

