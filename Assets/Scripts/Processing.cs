using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

namespace Bitwise.Game
{
    public class Processing
    {
        private GameData data;
        private Dictionary<int, float> jobCpuConsumptionRatio;

        public Processing(GameData d)
        {
            data = d;
            jobCpuConsumptionRatio = new Dictionary<int, float>();
        }

        public void Update(float deltaTime)
        {
            bool jobFinished = false;
            for (int i = 0; i < data.RunningJobs.Count; ++i)
            {
                Job job = data.RunningJobs.GetElementAt(i);
                job.Progress.Value += jobCpuConsumptionRatio[job.Index] * deltaTime * data.GetPropertyValue<float>(GameData.CpuSpeed) / job.ResourceUsage.Cycles;
                if (job.Complete)
                {
                    jobFinished = true;
                    job.Finish(data);
                    data.RunningJobs.RemoveElementAt(i--);
                }
            }
            if (jobFinished) { UpdateRunningJobs(true); }
        }

        public void QueueJob(int jobId)
        {
            data.QueuedJobs.AddElement(data.GetJob(jobId));
            UpdateRunningJobs(false);
        }

        public void HaltJob(int jobId)
        {
            Job job = data.GetJob(jobId);
            if (data.RunningJobs.RemoveElement(job))
            {
                UpdateRunningJobs(true);
            }
            else if (data.QueuedJobs.RemoveElement(job))
            {
                UpdateRunningJobs(false);
            }
        }

        public bool JobRunning(int jobId) { return data.RunningJobs.Value.Contains(data.GetJob(jobId)); }
        public bool JobQueued(int jobId) { return data.QueuedJobs.Value.Contains(data.GetJob(jobId)); }

        private void UpdateRunningJobs(bool forceUpdateStats)
        {
            float availableMemory = data.GetPropertyValue<float>(GameData.MemoryCapacity) - data.GetPropertyValue<float>(GameData.MemoryUsage);
            float availableDisk = data.GetPropertyValue<float>(GameData.DiskCapacity) - data.GetPropertyValue<float>(GameData.DiskUsage);

            bool jobAdded = false;
            while (data.QueuedJobs.Count > 0)
            {
                Job topJob = data.QueuedJobs.GetElementAt(0);
                availableDisk -= topJob.ResourceUsage.DiskRequired;
                availableMemory -= topJob.ResourceUsage.MemoryRequired;
                if (availableMemory >= 0f && availableDisk >= 0f)
                {
                    data.RunningJobs.AddElement(topJob);
                    data.QueuedJobs.RemoveElementAt(0);
                    jobAdded = true;
                }
                else
                {
                    break;
                }
            }

            if (jobAdded || forceUpdateStats) { UpdateStatCache(); }
        }

        private void UpdateStatCache()
        {
            jobCpuConsumptionRatio.Clear();

            List<Job> jobsLeft = new List<Job>(data.RunningJobs.Value);
            float totalMemoryDemand = jobsLeft.Aggregate(0f, (f, job) => f + job.ResourceUsage.MemoryRatio);
            float totalDiskDemand = jobsLeft.Aggregate(0f, (f, job) => f + job.ResourceUsage.DiskRatio);
            int totalConsumers = jobsLeft.Count;

            float cpuSpeed = data.GetPropertyValue<float>(GameData.CpuSpeed) * data.GetPropertyValue<int>(GameData.CpuCores);
            float memorySpeed = data.GetPropertyValue<float>(GameData.MemorySpeed);
            float diskSpeed = data.GetPropertyValue<float>(GameData.DiskSpeed);

            bool memoryBottlenecked = false;
            bool diskBottlenecked = false;
            float adjustedCpuDemand = 0f;

            while (jobsLeft.Count > 0)
            {
                float cpuDemand = jobsLeft.Count;
                float memoryDemand = jobsLeft.Aggregate(0f, (f, job) => f + job.ResourceUsage.MemoryRatio);
                float diskDemand = jobsLeft.Aggregate(0f, (f, job) => f + job.ResourceUsage.DiskRatio);

                float cpuFulfillment = Mathf.Approximately(0f, cpuDemand) ? float.MaxValue : (cpuSpeed / cpuDemand);
                float memoryFulfillment = Mathf.Approximately(0f, memoryDemand) ? float.MaxValue : (memorySpeed / memoryDemand);
                float diskFulfillment = Mathf.Approximately(0f, diskDemand) ? float.MaxValue : (diskSpeed / diskDemand);

                if (memoryFulfillment < cpuFulfillment && memoryFulfillment < diskFulfillment)
                {
                    for (int i = 0; i < jobsLeft.Count; ++i)
                    {
                        if (!Mathf.Approximately(0f, jobsLeft[i].ResourceUsage.MemoryRatio))
                        {
                            float jobCpuDemand = 1f / jobsLeft[i].ResourceUsage.MemoryRatio;
                            jobCpuConsumptionRatio[jobsLeft[i].Index] = jobCpuDemand / totalConsumers;
                            adjustedCpuDemand += jobCpuDemand;
                            jobsLeft.RemoveAt(i--);
                        }
                    }
                }
                else if (diskFulfillment < cpuFulfillment)
                {
                    for (int i = 0; i < jobsLeft.Count; ++i)
                    {
                        if (!Mathf.Approximately(0f, jobsLeft[i].ResourceUsage.DiskRatio))
                        {
                            float jobCpuDemand = 1f / jobsLeft[i].ResourceUsage.DiskRatio;
                            jobCpuConsumptionRatio[jobsLeft[i].Index] = jobCpuDemand / totalConsumers;
                            adjustedCpuDemand += jobCpuDemand;
                            jobsLeft.RemoveAt(i--);
                        }
                    }
                }
                else
                {
                    foreach (Job job in jobsLeft)
                    {
                        jobCpuConsumptionRatio[job.Index] = 1f / totalConsumers;
                        adjustedCpuDemand += 1f;
                    }

                    jobsLeft.Clear();
                }
            }

            float cpuUsage = (totalConsumers == 0) ? 0f : (adjustedCpuDemand / totalConsumers);
            float memoryBusUsage = 1f;
            float diskBusUsage = 1f;

            if (!memoryBottlenecked)
            {
                memoryBusUsage = cpuSpeed * totalMemoryDemand / memorySpeed * adjustedCpuDemand;
            }

            if (!diskBottlenecked)
            {
                diskBusUsage = cpuSpeed * totalDiskDemand / diskSpeed * adjustedCpuDemand;
            }


            float memoryUsage = data.RunningJobs.Value.Aggregate(0f, (f, job) => f + job.ResourceUsage.MemoryRequired);
            float diskUsage = data.RunningJobs.Value.Aggregate(0f, (f, job) => f + job.ResourceUsage.DiskRequired);

            data.SetPropertyValue(GameData.CpuUsage, cpuUsage);
            data.SetPropertyValue(GameData.MemoryUsage, memoryUsage);
            data.SetPropertyValue(GameData.MemoryBusUsage, memoryBusUsage);
            data.SetPropertyValue(GameData.DiskUsage, diskUsage);
            data.SetPropertyValue(GameData.DiskBusUsage, diskBusUsage);
        }
    }
}
