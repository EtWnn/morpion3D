using System;
using UnityEngine;
using UnityEngine.UI;
using MyClient.Models;
using TMPro;


public class OpponentSlot : MonoBehaviour
{
    public User User { get; private set; }
    public Toggle Toggle { get; private set; }
    private Color deselectedColor;
    private Color selectedColor;
    private Image image;

    public event EventHandler OnToggled;

    private void Awake()
    {
        Toggle = GetComponent<Toggle>();
        image = GetComponent<Image>();

        deselectedColor = Toggle.colors.normalColor;
        selectedColor = Toggle.colors.pressedColor;
        Toggle.onValueChanged.AddListener(OnValueChange);
    }

    public void SetUser(User user)
    {
        User = user;
        GetComponentInChildren<TextMeshProUGUI>().text = User.UserName;
    }

    public void SetToggleGroup(ToggleGroup toggleGroup)
    {
        Toggle.group = toggleGroup;
    }

    private void OnValueChange(bool value)
    {
        if (value)
        {
            image.color = selectedColor;
            OnToggled(this, EventArgs.Empty);
        }
        else
            image.color = deselectedColor;
    }
}

