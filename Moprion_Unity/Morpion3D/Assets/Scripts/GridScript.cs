using System;
using UnityEngine;

public class GridScript : MonoBehaviour
{
    public enum EState
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

    public EState GridState { get; private set; }

    private GameObject[,,] CubeletGOs;
    private Action updateFunction;

    // Start is called before the first frame update
    void Start()
    {
        CreateGrid();
        TransitionTime = 1;
        TransitionNumRotation = 3;
        RotatationSpeed = 10f;
        GridState = EState.NotInGame;
        updateFunction = NotInGameBehaviour;
    }

    // Update is called once per frame
    void Update()
    {
        updateFunction();
    }

    public void UpdateGridState(EState gridState)
    {
        switch (gridState)
        {
            case EState.ToGame:
                if (GridState != EState.NotInGame)
                    throw new Exception("State change forbiden: from " + GridState + " to ToGame");
                updateFunction = ToGameBehaviour;
                break;
            case EState.ReadyGame:
                if (GridState != EState.ToGame)
                    throw new Exception("State change forbiden: from " + GridState + " to ReadyGame");
                updateFunction = NoOpBehaviour;
                break;
            case EState.InGame:
                if (GridState != EState.ReadyGame)
                    throw new Exception("State change forbiden: from " + GridState + " to InGame");
                updateFunction = InGameBehaviour;
                break;
            case EState.NotInGame:
                updateFunction = NotInGameBehaviour;
                break;
            default:
                updateFunction = NoOpBehaviour;
                break;
        }

        if (gridState != EState.InGame)
        {
            SetExternCubeletsActive(true);
        }

        GridState = gridState;
    }

    private void InGameBehaviour()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            SetExternCubeletsActive(!CubeletGOs[0,0,0].activeInHierarchy);
    }

    private void ToGameBehaviour()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        UpdateGridState(EState.ReadyGame);
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

    private void SetExternCubeletsActive(bool active)
    {
        for (var x = 0; x < 3; x++)
            for (var y = 0; y < 3; y++)
                for (var z = 0; z < 3; z++)
                    if (x != 1 || y != 1 || z != 1)
                        CubeletGOs[x, y, z].SetActive(active);
    }

    private void UpdateCubeletsState(CubeletScript.ECubeletState cubeletState)
    {
        for (var x = 0; x < 3; x++)
            for (var y = 0; y < 3; y++)
                for (var z = 0; z < 3; z++)
                        CubeletGOs[x, y, z].GetComponent<CubeletScript>().UpdateCubeletState(cubeletState);
    }
}
