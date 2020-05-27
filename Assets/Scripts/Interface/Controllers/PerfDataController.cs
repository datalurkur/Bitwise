using System;
using System.Collections;
using System.Collections.Generic;
using Bitwise.Game;
using TMPro;
using UnityEngine;

public class PerfDataController : PropertyVisualizer
{
    public RectTransform PercentageBar;
    public TMP_Text PercentageText;
    public TMP_Text UsageText;
    public TMP_Text TotalText;
    public TMP_Text MultiplierText;

    public Vector3 MinimumScale = Vector3.zero;
    public Vector3 MaximumScale = Vector3.one;

    public int TotalCapacityBinding = GameData.InvalidPropertyIndex;
    public int MultiplierBinding = GameData.InvalidPropertyIndex;

    private float lastCapacity;
    private int lastMultiplier = 1;
    private float lastPercentage;

    protected override void Start()
    {
        base.Start();
        if (TotalCapacityBinding == GameData.InvalidPropertyIndex)
        {
            throw new ArgumentException($"Invalid property index given to {nameof(PropertyVisualizer)} {gameObject.name}");
        }
        if (MultiplierBinding != GameData.InvalidPropertyIndex)
        {
            GameManager.Instance.Data.ListenForChanges(MultiplierBinding, OnMultiplierChanged);
        }
        GameManager.Instance.Data.ListenForChanges(TotalCapacityBinding, OnCapacityChanged);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (TotalCapacityBinding == GameData.InvalidPropertyIndex)
        {
            throw new ArgumentException($"Invalid property index given to {nameof(PropertyVisualizer)} {gameObject.name}");
        }
        if (MultiplierBinding != GameData.InvalidPropertyIndex)
        {
            GameManager.Instance.Data.StopListening(MultiplierBinding, OnMultiplierChanged);
        }
        GameManager.Instance.Data.StopListening(TotalCapacityBinding, OnCapacityChanged);
    }

    protected override void OnPropertyUpdated(GameDataProperty prop)
    {
        lastPercentage = Mathf.Clamp01(prop.GetValue<float>());
        PercentageBar.localScale = Vector3.Lerp(MinimumScale, MaximumScale, lastPercentage);
        int intPercentage = (int) (lastPercentage * 100);
        PercentageText.text = $"{intPercentage}%";
        UpdateUsageTexts();
    }

    private void OnCapacityChanged(GameDataProperty prop)
    {
        lastCapacity = prop.GetValue<float>();
        UpdateUsageTexts();
    }

    private void OnMultiplierChanged(GameDataProperty prop)
    {
        lastMultiplier = prop.GetValue<int>();
        UpdateUsageTexts();
    }

    private void UpdateUsageTexts()
    {
        TotalText.text = lastCapacity.ToString("0.00");
        UsageText.text = (lastCapacity * lastPercentage).ToString("0.00");
        if (MultiplierBinding == GameData.InvalidPropertyIndex)
        {
            MultiplierText.text = "";
        }
        else
        {
            MultiplierText.text = lastMultiplier.ToString();
        }
    }
}
