using System;
using UnityEngine;
using UnityEngine.UI;
using MyClient.Models;
using TMPro;

/// <summary>
/// Handles an opponent slot, appearing in the <see cref="OpponentsMenu"/>.
/// </summary>
public class OpponentSlot : MonoBehaviour
{
    // ---- Events ----

    public event EventHandler OnToggled;
    
    // ---- Public fields / properties ----
    
    public User User { get; private set; }
    public Toggle Toggle { get; private set; }

    // ---- Private fields / properties ----

    private Color deselectedColor;
    private Color selectedColor;
    private Image image;

    // ---- Public methods ----

    public void SetUser(User user)
    {
        User = user;
        GetComponentInChildren<TextMeshProUGUI>().text = User.UserName;
    }

    public void SetToggleGroup(ToggleGroup toggleGroup)
    {
        Toggle.group = toggleGroup;
    }

    // ---- Private methods ----

    private void Awake()
    {
        Toggle = GetComponent<Toggle>();
        image = GetComponent<Image>();

        deselectedColor = Toggle.colors.normalColor;
        selectedColor = Toggle.colors.pressedColor;
        Toggle.onValueChanged.AddListener(OnValueChange);
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

