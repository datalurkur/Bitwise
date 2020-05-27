using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bitwise.Game;
using UnityEngine;

public class JobsListController : MonoBehaviour
{
    public RectTransform JobsContainer;
    public RectTransform JobListItemPrefab;

    private readonly Dictionary<int, JobListItemController> jobViews = new Dictionary<int, JobListItemController>();

    protected void Start()
    {
        GameManager.Instance.Data.RunningJobs.Subscribe(OnPropertyUpdated);
    }

    protected void OnDestroy()
    {
        GameManager.Instance.Data.RunningJobs.Unsubscribe(OnPropertyUpdated);
    }

    protected void OnPropertyUpdated(GameDataProperty prop)
    {
        List<int> unusedKeys = jobViews.Keys.ToList();
        List<Job> jobs = prop.GetListValue<Job>();
        foreach (Job job in jobs)
        {
            if (!unusedKeys.Contains(job.Index))
            {
                RectTransform newJobView = Instantiate(JobListItemPrefab, JobsContainer);
                JobListItemController newJobController = newJobView.GetComponent<JobListItemController>();
                newJobController.WatchedJob = job;
                jobViews[job.Index] = newJobController;
            }
        }

        foreach (int key in unusedKeys)
        {
            Destroy(jobViews[key].gameObject);
            jobViews.Remove(key);
        }
    }
}
