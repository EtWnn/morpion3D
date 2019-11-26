using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Base class for all popups. Define Enable / Disbale events which are triggered on GameObject enabled / disabled 
/// </summary>
public class PopupBase : MonoBehaviour
{
    public event EventHandler Enabled;
    public event EventHandler Disabled;

    protected virtual void OnDisable() => Disabled?.Invoke(this, EventArgs.Empty);

    protected virtual void OnEnable() => Enabled?.Invoke(this, EventArgs.Empty);

    public void SetActive(bool value) => gameObject.SetActive(value);
}
