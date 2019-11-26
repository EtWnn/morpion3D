using System;
using System.Collections;
using UnityEngine;
using MyClient;
using MyClient.Models;
using MyClient.ModelGame;

/// <summary>
/// Handles the "grid" gameboard and is responsible for processing new game execution updates sent by the <see cref="Client"/>
/// as well as notifying who's turn is it and played positions.
/// </summary>
public class GridScript : MonoBehaviour
{
    // ---- Enums ---

    public enum EPlayerTurn
    {
        Default,
        IsPlayerTurn,
        IsOpponentTurn,
        PlayerWon,
        OpponentWon,
    }

    // ---- Events ---

    /// <summary>Fired when the grid is ready for begining a game.</summary>
    public event EventHandler ReadyGame;
    /// <summary>Fired when the player turn has changed.</summary>
    public event EventHandler<TEventArgs<EPlayerTurn>> PlayerTurnChanged;
    /// <summary>Fired when the player has played.</summary>
    public event EventHandler<TEventArgs<System.Numerics.Vector3>> PositionPlayed;

    // ---- Prefabs ----

    /// <summary>Prefab object set through UnityEditor</summary>
    public GameObject CubeletPrefab;
    /// <summary>Prefab object set through UnityEditor</summary>
    public GameObject CrossPrefab;
    /// <summary>Prefab object set through UnityEditor</summary>
    public GameObject CrossWonPrefab;
    /// <summary>Prefab object set through UnityEditor</summary>
    public GameObject TorePrefab;
    /// <summary>Prefab object set through UnityEditor</summary>
    public GameObject ToreWonPrefab;

    // ---- Public fields/properties ----

    /// <summary>Rotation speed in degrees per second (default: 10)</summary>
    public float RotatationSpeed;
    /// <summary>Player filling pattern GameObject according to its setting</summary>
    public GameObject PlayerFillingObject { get; private set; }
    /// <summary>Player winning filling pattern GameObject according to its setting</summary>
    public GameObject OpponentFillingObject { get; private set; }
    /// <summary>Opponent filling pattern GameObject</summary>
    public GameObject PlayerWonFillingObject { get; private set; }
    /// <summary>Opponent winning filling pattern GameObject</summary>
    public GameObject OpponentWonFillingObject { get; private set; }

    // ---- Private fields/properties ----

    private SharedUpdatable<Game> gameState;
    private CubeletScript[,,] cubelets;

    /// <summary>Action called each frame by <see cref="Update"/></summary>
    private Action updateFunction;
    
    private GameObject player1FillingObject;
    private GameObject player1WonFillingObject;
    private GameObject player2FillingObject;
    private GameObject player2WonFillingObject;

    private bool firstUpdate = true;
    private bool isPlayer1;

    private MainScript mainScript;

    // ---- Public methods ----

    /// <summary>Activate / Desactivate the attached GameObject.</summary>
    public void SetActive(bool value) => gameObject.SetActive(value);

    // ---- Events Handlers ----

    /// <summary>
    /// Process <see cref="MainScript.State"/> changes;
    /// </summary>
    /// <param name="sender">Must be the <see cref="MainScript"/>.</param>
    /// <param name="e">Ignored</param>
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
                // Completely reset the grid before going to menu.
                ResetGrid();
                firstUpdate = true;
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

    /// <summary>
    /// Store the new <see cref="gameState"/>, which will be processed in <see cref="Upate"/> by <see cref="InGameBehaviour"/> in the next frame.
    /// </summary>
    /// <param name="sender">Must be the <see cref="Client"/></param>
    /// <param name="e">Ignored.</param>
    public void OnGameUpdated(object sender, EventArgs e)
    {
        var client = sender as Client;
        gameState.Write(client.GameClient);
    }

    /// <summary>
    /// Process player plattern setting changes in the <see cref="OptionsMenu"/>
    /// </summary>
    /// <param name="sender">Ignored</param>
    /// <param name="e">The new player pattern.</param>
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

    /// <summary>
    /// Process a <see cref="CubeletScript.Clicked"/> event, attach the cubelet position and fire <see cref="PositionPlayed"/> event.
    /// </summary>
    /// <param name="sender">A <see cref="CubeletScript"/></param>
    /// <param name="e">Ignored</param>
    public void OnCubeletClicked(object sender, EventArgs e)
    {
        var cubelet = sender as CubeletScript;
        PositionPlayed?.Invoke(this, new TEventArgs<System.Numerics.Vector3>(cubelet.Position));
    }

    // ---- Private Methods ----

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
                    GetComponentInParent<MainScript>().StateChanged += cubeletScript.OnStateChange;
                    cubelets[x, y, z] = cubeletScript;
                }
    }

    private void ResetGrid()
    {
        for (var x = 0; x < 3; x++)
            for (var y = 0; y < 3; y++)
                for (var z = 0; z < 3; z++)
                    cubelets[x, y, z].ResetCubelet();
    }

    private void SetExternCubeletsActive(bool active)
    {
        for (var x = 0; x < 3; x++)
            for (var y = 0; y < 3; y++)
                for (var z = 0; z < 3; z++)
                    if (x != 1 || y != 1 || z != 1)
                        cubelets[x, y, z].SetActive(active);
    }

    /// <summary>
    /// Process new <see cref="gameState"/>.
    /// </summary>
    private void UpdateGameState(Game gameState)
    {
        // If it's the first gameState set who is player1 and associate patterns to player1 / player2
        if(firstUpdate)
        {
            isPlayer1 = gameState.IdPlayer1 != mainScript.Client.Opponent.Id;
            player1FillingObject = isPlayer1 ? PlayerFillingObject : OpponentFillingObject;
            player2FillingObject = !isPlayer1 ? PlayerFillingObject : OpponentFillingObject;
            player1WonFillingObject = isPlayer1 ? PlayerWonFillingObject : OpponentWonFillingObject;
            player2WonFillingObject = !isPlayer1 ? PlayerWonFillingObject : OpponentWonFillingObject;

            firstUpdate = false;
        }

        // Updating whole grid crosses and tores
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


        // Fires playerTurnChanged event
        TEventArgs<EPlayerTurn> turn = new TEventArgs<EPlayerTurn>(default);
        switch (gameState.Mode)
        {
            case GameMode.Player1:
                turn.Data = isPlayer1 ? EPlayerTurn.IsPlayerTurn : EPlayerTurn.IsOpponentTurn;
                break;
            case GameMode.Player2:
                turn.Data = !isPlayer1 ? EPlayerTurn.IsPlayerTurn : EPlayerTurn.IsOpponentTurn;
                break;
            case GameMode.Player1Won:
                turn.Data = isPlayer1 ? EPlayerTurn.PlayerWon : EPlayerTurn.OpponentWon;
                break;
            case GameMode.Player2Won:
                turn.Data = !isPlayer1 ? EPlayerTurn.PlayerWon : EPlayerTurn.OpponentWon;
                break;
            case GameMode.NoneWon:
                break;
        }
        PlayerTurnChanged?.Invoke(this, turn);

    }
}
