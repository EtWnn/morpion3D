using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SearchOpponentPopupScript : MonoBehaviour
{
    private Image Rotator;
    private Image Stator;
    private TMP_Text TextElement;
    private Action updateFunction;
    public float rotateSpeed = 200f;

    private void Awake()
    {
        var rectComponent = GetComponent<RectTransform>();
        Stator = transform.Find("Stator").GetComponent<Image>();
        TextElement = Stator.GetComponentInChildren<TMP_Text>();
        Rotator = Stator.transform.Find("Rotator").GetComponent<Image>();

        var width = rectComponent.rect.width - rectComponent.rect.height;

        TextElement.rectTransform.sizeDelta = new Vector2(width, 0);

        updateFunction = NoOpBehaviour;
    }

    // Update is called once per frame
    void Update()
    {
        updateFunction();
    }

    void NoOpBehaviour()
    {
    }

    void SearchingBehaviour()
    {
        Rotator.rectTransform.Rotate(0f, 0f, -(rotateSpeed * Time.deltaTime));
    }

    public void OnOpponentFound()
    {
        updateFunction = NoOpBehaviour;
        Rotator.gameObject.SetActive(false);
        Stator.color = Color.green;
        TextElement.text = "Opponent found !";
    }
    public void OnOpponentFound(string opponentName)
    {
        updateFunction = NoOpBehaviour;
        Rotator.gameObject.SetActive(false);
        Stator.color = Color.green;
        TextElement.text = "Opponent found !\n" + opponentName;
    }

    public void OnOpponentNotFound()
    {
        updateFunction = NoOpBehaviour;
        Rotator.gameObject.SetActive(false);
        Stator.color = Color.red;
        TextElement.text = "No opponent found !";
    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
        if (value)
            updateFunction = SearchingBehaviour;
        else
            updateFunction = NoOpBehaviour;
    }

    public void OnMenuStateChange(object sender, EventArgs e)
    {
        UIControllerScript ui = sender as UIControllerScript;
        if (ui && ui.State == UIControllerScript.EStateUI.SearchingOpponents)
            SetActive(true);
        else
            SetActive(false);
    }
}
