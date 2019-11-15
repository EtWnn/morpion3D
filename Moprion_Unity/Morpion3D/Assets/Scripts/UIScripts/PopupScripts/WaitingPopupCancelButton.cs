using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaitingPopupCancelButton : PopupBase
{
    public Button CancelButton { get; private set; }

    private TextMeshProUGUI _textEl;
    public string Text
    {
        get { return _textEl.text; }
        set { _textEl.text = value; }
    }

    public StatorAnimation StatorAnimation { get; private set; }

    private void Awake()
    {
        _textEl = transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        StatorAnimation = GetComponentInChildren<StatorAnimation>();
        CancelButton = GetComponentInChildren<Button>();
    }
}
