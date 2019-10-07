using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartButtonScript : MonoBehaviour
{
    public GameObject MainScriptGo;
    private MainScript mainScrpit;
    public Button button;

    // Start is called before the first frame update
    void Start()
    {
        MainScriptGo = GameObject.Find("MainScriptGO");
        mainScrpit = MainScriptGo.GetComponent<MainScript>();
        button = GetComponent<Button>();
        button.onClick.AddListener(mainScrpit.BeginToGame);
    }

    //void OnMouseOver()
    //{
    //    if(Input.GetMouseButtonDown(0))
    //    {
    //        Debug.Log("Click on Start button!");
    //        mainScrpit.BeginToGame();
    //    }
    //}
}
