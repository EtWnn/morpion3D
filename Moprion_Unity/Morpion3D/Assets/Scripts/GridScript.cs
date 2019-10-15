using System;
using System.Collections;
using UnityEngine;

public class GridScript : MonoBehaviour
{
    public int TransitionNumRotation;

    public float TransitionTime;

    public float RotatationSpeed;

    public GameObject CubeletPrefab;

    private GameObject[,,] CubeletGOs;
    
    private Action updateFunction;

    public event EventHandler ReadyGame;


    private void Awake()
    {
        CreateGrid();
        TransitionTime = 1;
        TransitionNumRotation = 3;
        RotatationSpeed = 10f;
        updateFunction = NoOpBehaviour;
    }


    void Update()
    {
        updateFunction();
    }

    public void OnStateChange(object sender, EventArgs args)
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
            case EState.InOptionsMenu:
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

    private void InGameBehaviour()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            SetExternCubeletsActive(!CubeletGOs[0,0,0].activeInHierarchy);
    }

    private void ToGameBehaviour()
    {
        StartCoroutine("ToGameCoroutine");
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
        OnReadyGame();
        yield break;
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
                {
                    var cubelet = Instantiate(CubeletPrefab, new Vector3(x - 1, y - 1, z - 1), new Quaternion(0, 0, 0, 0), transform);
                    var cubeletScript = cubelet.GetComponent<CubeletScript>();
                    GetComponentInParent<MainScript>().StateChange += cubeletScript.OnStateChange;
                    CubeletGOs[x, y, z] = cubelet;
                }
    }

    private void SetExternCubeletsActive(bool active)
    {
        for (var x = 0; x < 3; x++)
            for (var y = 0; y < 3; y++)
                for (var z = 0; z < 3; z++)
                    if (x != 1 || y != 1 || z != 1)
                        CubeletGOs[x, y, z].SetActive(active);
    }

    private void OnReadyGame()
    {
        if (ReadyGame != null)
            ReadyGame(this, EventArgs.Empty);
    }
}
