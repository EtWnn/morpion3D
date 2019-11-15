using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BasicPopup : PopupBase
{
    private TextMeshProUGUI TMPtextEl;

    public string Text
    {
        get { return TMPtextEl.text; }
        set { TMPtextEl.text = value; }
    }
    
    void Awake() => TMPtextEl = GetComponentInChildren<TextMeshProUGUI>();
}
