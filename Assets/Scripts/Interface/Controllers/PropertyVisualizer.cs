using System;
using System.Collections;
using System.Collections.Generic;
using Bitwise.Game;
using UnityEngine;

public class PropertyVisualizer : MonoBehaviour
{
    public int PropertyIndexBinding = GameData.InvalidPropertyIndex;

    protected virtual void Start()
    {
        if (PropertyIndexBinding == GameData.InvalidPropertyIndex)
        {
            throw new ArgumentException($"Invalid property index given to {nameof(PropertyVisualizer)} {gameObject.name}");
        }
        GameManager.Instance.Data.ListenForChanges(PropertyIndexBinding, OnPropertyUpdated);
    }

    protected virtual void OnDestroy()
    {
        if (PropertyIndexBinding == GameData.InvalidPropertyIndex)
        {
            throw new ArgumentException($"Invalid property index given to {nameof(PropertyVisualizer)} {gameObject.name}");
        }
        GameManager.Instance.Data.StopListening(PropertyIndexBinding, OnPropertyUpdated);
    }

    protected virtual void OnPropertyUpdated(GameDataProperty prop) { }
}
