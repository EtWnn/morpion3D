using System;
using System.Collections;
using UnityEngine;
using MyClient;
using MyClient.Models;
using MyClient.ModelGame;


public class GridScript : MonoBehaviour
{
    ////// Events //////

    public event EventHandler ReadyGame;
    public event EventHandler<TEventArgs<bool>> PlayerTurn;
    public event EventHandler<TEventArgs<System.Numerics.Vector3>> PositionPlayed;

    ////// Prefabs //////

    public GameObject CubeletPrefab;
    public GameObject CrossPrefab;
    public GameObject CrossWonPrefab;
    public GameObject TorePrefab;
    public GameObject ToreWonPrefab;

    ////// Public fields/properties //////

    public float RotatationSpeed;
    public GameObject PlayerFillingObject { get; private set; }
    public GameObject OpponentFillingObject { get; private set; }
    public GameObject PlayerWonFillingObject { get; private set; }
    public GameObject OpponentWonFillingObject { get; private set; }
    ////// Private fields/properties //////

    private SharedUpdatable<Game> gameState;
    private CubeletScript[,,] cubelets;
    private Action updateFunction;
    
    private GameObject player1FillingObject;
    private GameObject player1WonFillingObject;
    private GameObject player2FillingObject;
    private GameObject player2WonFillingObject;

    private bool firstUpdate = true;
    private bool isPlayer1;

    private MainScript mainScript;

    ////// Public methods //////

    public void SetActive(bool value) => gameObject.SetActive(value);

    ////// Events Handlers //////


    public void OnStateChange(object sender, EventArgs e)
    {
        MainScript ms = sender as MainScript;
        switch (ms.State)
        {
            case EState.ToGame:
                updateFunction = ToGameBehaviour;
                break;
            case EState.InGame:
                updateFunction = InGameBehaviour;
                break;
            case EState.InMainMenu:
                updateFunction = NotInGameBehaviour;
                break;
            case EState.ToMenu:
                updateFunction = NotInGameBehaviour;
                break;
            default:
                updateFunction = NoOpBehaviour;
                break;
        }

        if (ms.State != EState.InGame)
        {
            SetExternCubeletsActive(true);
        }
    }

    public void OnGameUpdated(object sender, EventArgs e)
    {
        var client = sender as Client;
        gameState.Write(client.GameClient);
    }

    public void OnPlayerPatternChanged(object sender, TEventArgs<EPlayerPatterns> e)
    {
        switch (e.Data)
        {
            case EPlayerPatterns.Cross:
                PlayerFillingObject = CrossPrefab;
                PlayerWonFillingObject = CrossWonPrefab;
                OpponentFillingObject = TorePrefab;
                OpponentWonFillingObject = ToreWonPrefab;
                break;
            case EPlayerPatterns.Tore:
                PlayerFillingObject = TorePrefab;
                PlayerWonFillingObject = ToreWonPrefab;
                OpponentFillingObject = CrossPrefab;
                OpponentWonFillingObject = CrossWonPrefab;
                break;
            default:
                break;
        }
    }

    public void OnCubeletClicked(object sender, EventArgs e)
    {
        var cubelet = sender as CubeletScript;
        PositionPlayed?.Invoke(this, new TEventArgs<System.Numerics.Vector3>(cubelet.Position));
    }

    ///// Private Methods /////

    private void Awake()
    {
        RotatationSpeed = 10f;
        updateFunction = NoOpBehaviour;
        gameState = new SharedUpdatable<Game>();
        gameState.UpdateAction = UpdateGameState;
        PlayerFillingObject = CrossPrefab;
        PlayerWonFillingObject = CrossWonPrefab;
        OpponentFillingObject = TorePrefab;
        OpponentWonFillingObject = ToreWonPrefab;
        mainScript = GetComponentInParent<MainScript>();
        CreateGrid();
    }

    void Update()
    {
        updateFunction();
    }

    private void InGameBehaviour()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            SetExternCubeletsActive(!cubelets[0,0,0].gameObject.activeInHierarchy);
        gameState.TryProcessIfNew();
    }

    private void ToGameBehaviour()
    {
        StartCoroutine(ToGameCoroutine());
        updateFunction = NoOpBehaviour;
    }

    private IEnumerator ToGameCoroutine()
    {
        var itRotation = Utils.LerpRotate(transform.rotation, Quaternion.identity, 1f);
        while (itRotation.MoveNext())
        {
            transform.rotation = (Quaternion)itRotation.Current;
            yield return null;
        }
        ReadyGame?.Invoke(this, EventArgs.Empty);
        yield break;
    }

    private void NotInGameBehaviour()
    {
        transform.Rotate(new Vector3(0, RotatationSpeed * Time.deltaTime, 0));
    }

    private void NoOpBehaviour() { }

    private void CreateGrid()
    {
        cubelets = new CubeletScript[3, 3, 3];
        for (var x = 0; x < 3; x++)
            for (var y = 0; y < 3; y++)
                for (var z = 0; z < 3; z++)
                {
                    var cubelet = Instantiate(CubeletPrefab, new Vector3(x - 1, y - 1, z - 1), new Quaternion(0, 0, 0, 0), transform);
                    var cubeletScript = cubelet.GetComponent<CubeletScript>();
                    cubeletScript.Position = new System.Numerics.Vector3(x, y, z);
                    cubeletScript.Clicked += OnCubeletClicked;
                    GetComponentInParent<MainScript>().StateChange += cubeletScript.OnStateChange;
                    cubelets[x, y, z] = cubeletScript;
                }
    }

    private void SetExternCubeletsActive(bool active)
    {
        for (var x = 0; x < 3; x++)
            for (var y = 0; y < 3; y++)
                for (var z = 0; z < 3; z++)
                    if (x != 1 || y != 1 || z != 1)
                        cubelets[x, y, z].SetActive(active);
    }

    private void UpdateGameState(Game gameState)
    {
        Debug.Log("In: UpdateGameState()");
        if(firstUpdate)
        {
            isPlayer1 = gameState.IdPlayer1 != mainScript.Client.Opponent.Id;
            player1FillingObject = isPlayer1 ? PlayerFillingObject : OpponentFillingObject;
            player2FillingObject = !isPlayer1 ? PlayerFillingObject : OpponentFillingObject;
            player1WonFillingObject = isPlayer1 ? PlayerWonFillingObject : OpponentWonFillingObject;
            player2WonFillingObject = !isPlayer1 ? PlayerWonFillingObject : OpponentWonFillingObject;

            Debug.Log("FirstUpdateGameState()");
            firstUpdate = false;
        }

        /// Updating whole grid crosses and tores
        for (var x = 0; x < 3; x++)
            for (var y = 0; y < 3; y++)
                for (var z = 0; z < 3; z++)
                    switch (gameState.GameBoardMatrix[x,y,z])
                    {
                        case (int)Cell.Player1Pattern:
                            cubelets[x, y, z].FillWith(player1FillingObject);
                            break;
                        case (int)Cell.Player2Pattern:
                            cubelets[x, y, z].FillWith(player2FillingObject);
                            break;
                        case (int)Cell.HighlightPlayer1:
                            cubelets[x, y, z].FillWith(player1WonFillingObject);
                            break;
                        case (int)Cell.HighlightPlayer2:
                            cubelets[x, y, z].FillWith(player2WonFillingObject);
                            break;
                        default:
                            break;
                    }

        Debug.Log("Game grid updated!");


        switch (gameState.Mode)
        {
            case GameMode.Player1:
                SetPlayerTurn(isPlayer1);
                break;
            case GameMode.Player2:
                SetPlayerTurn(!isPlayer1);
                break;
            case GameMode.Player1Won:
                break;
            case GameMode.Player2Won:
                break;
            case GameMode.NoneWon:
                break;
            default:
                break;
        }

        Debug.Log("Game mode changed!");

    }

    private void SetPlayerTurn(bool isPlayerTurn)
    {
        PlayerTurn?.Invoke(this, new TEventArgs<bool>(isPlayerTurn));
    }
}
