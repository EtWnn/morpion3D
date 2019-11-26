using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handle popup with a timeout <see cref="StatorAnimation"/> and Accept / decline popup.
/// </summary>
public class TimeoutPopup : PopupBase
{
    public Button DeclineButton { get; private set; }
    public Button AcceptButton { get; private set; }

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
        DeclineButton = transform.Find("Decline Button").GetComponent<Button>();
        AcceptButton = transform.Find("Accept Button").GetComponent<Button>();
    }
}
