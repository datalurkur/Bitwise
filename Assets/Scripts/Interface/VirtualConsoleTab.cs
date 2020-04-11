using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VirtualConsoleTab : MonoBehaviour
{
    public TMP_Text TabLabel;

    private bool _active = false;

    public bool Active
    {
        get => _active;
        set
        {
            _active = value;
            SetTabActive(_active);
        }
    }

    private void SetTabActive(bool active)
    {
        TabLabel.fontStyle = active ? FontStyles.Underline : FontStyles.Normal;
    }
}
