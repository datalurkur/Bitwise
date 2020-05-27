using System;
using Bitwise.Game;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JobListItemController : MonoBehaviour
{
    public TMP_Text JobNameText;
    public RectTransform ProgressBar;
    public TMP_Text ProgressPercentageText;
    public Button CancelButton;
    public Vector3 MinimumScale = new Vector3(0f, 1f, 1f);
    public Vector3 MaximumScale = new Vector3(1f, 1f, 1f);

    public Job WatchedJob;

    protected void Start()
    {
        CancelButton.onClick.AddListener(OnCancelButtonClick);
        WatchedJob.Progress.Subscribe(OnJobProgressUpdated);
        JobNameText.text = WatchedJob.Name;
    }

    protected void OnDestroy()
    {
        CancelButton.onClick.RemoveListener(OnCancelButtonClick);
        WatchedJob.Progress.Unsubscribe(OnJobProgressUpdated);
    }

    private void OnJobProgressUpdated(GameDataProperty prop)
    {
        float percentage = Mathf.Clamp01(prop.GetValue<float>());
        ProgressBar.localScale = Vector3.Lerp(MinimumScale, MaximumScale, percentage);
        int intPercentage = (int) (percentage * 100);
        ProgressPercentageText.text = $"{intPercentage}%";
    }

    private void OnCancelButtonClick()
    {
        GameManager.Instance.Processor.HaltJob(WatchedJob.Index);
    }
}
