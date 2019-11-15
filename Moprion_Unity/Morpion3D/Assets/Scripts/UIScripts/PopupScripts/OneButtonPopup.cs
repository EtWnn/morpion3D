using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OneButtonPopup : PopupBase
{
    public Button Button { get; private set; }

    private TextMeshProUGUI _buttonTextEl;
    public string ButtonText
    {
        get => _buttonTextEl.text;
        set => _buttonTextEl.text = value;
    }

    private TextMeshProUGUI _textEl;
    public string Text
    {
        get { return _textEl.text; }
        set { _textEl.text = value; }
    }
    void Awake()
    {
        _textEl = transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        Button = GetComponentInChildren<Button>();
        _buttonTextEl = Button.GetComponent<TextMeshProUGUI>();
    }
}
