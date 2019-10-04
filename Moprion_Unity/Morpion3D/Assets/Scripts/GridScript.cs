using System;
using UnityEngine;

public class GridScript : MonoBehaviour
{
    public enum EGridState
    {
        ToGame,
        ReadyGame,
        InGame,
        NotInGame,
    }

    public int TransitionNumRotation;

    public float TransitionTime;

    public float RotatationSpeed;

    public GameObject CubeletPrefab;

    public EGridState GridState { get; private set; }

    private GameObject[,,] CubeletGOs;
    private Action updateFunction;

    // Start is called before the first frame update
    void Start()
    {
        CreateGrid();
        TransitionTime = 1;
        TransitionNumRotation = 3;
        RotatationSpeed = 10f;
        updateFunction = NotInGameBehaviour;
    }

    // Update is called once per frame
    void Update()
    {
        updateFunction();
    }

    void UpdateGridState(EGridState gridState)
    {
        switch (gridState)
        {
            case EGridState.ToGame:
                if (GridState != EGridState.NotInGame)
                    throw new Exception("State change forbiden");
                updateFunction = ToGameBehaviour;
                break;
            case EGridState.ReadyGame:
                if (GridState != EGridState.ToGame)
                    throw new Exception("State change forbiden");
                updateFunction = NoOpBehaviour;
                break;
            case EGridState.InGame:
                if (GridState != EGridState.ReadyGame)
                    throw new Exception("State change forbiden");
                updateFunction = InGameBehaviour;
                break;
            case EGridState.NotInGame:
                updateFunction = NotInGameBehaviour;
                break;
            default:
                updateFunction = NoOpBehaviour;
                break;
        }
        GridState = gridState;
    }

    private void InGameBehaviour()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            SwitchHideShowExternCubelets();
    }

    private void ToGameBehaviour()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        UpdateGridState(EGridState.ReadyGame);
    }

    private void NotInGameBehaviour()
    {
        transform.Rotate(new Vector3(0, RotatationSpeed * Time.deltaTime, 0));
    }

    private void NoOpBehaviour() { }

    private void CreateGrid()
    {
        CubeletGOs = new GameObject[3, 3, 3];
        for (var x = 0; x < 3; x++)
            for (var y = 0; y < 3; y++)
                for (var z = 0; z < 3; z++)
                    CubeletGOs[x, y, z] = Instantiate(CubeletPrefab, new Vector3(x - 1, y - 1, z - 1), new Quaternion(0, 0, 0, 0), transform);
    }

    private void SwitchHideShowExternCubelets()
    {
        for (var x = 0; x < 3; x++)
            for (var y = 0; y < 3; y++)
                for (var z = 0; z < 3; z++)
                    if (x != 1 || y != 1 || z != 1)
                        CubeletGOs[x, y, z].SetActive(!CubeletGOs[x, y, z].activeSelf);
    }
}
