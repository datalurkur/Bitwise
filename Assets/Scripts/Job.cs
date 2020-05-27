using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bitwise.Game
{
    [Serializable]
    public class ResourceUsageSpec
    {
        public int Index;

        public string Name;
        public float Cycles;
        public float MemoryRatio;
        public float DiskRatio;
        public int MemoryRequired;
        public int DiskRequired;

        public ResourceUsageSpec() {}
        public ResourceUsageSpec(ResourceUsageSpec other)
        {
            Index = other.Index;
            Cycles = other.Cycles;
            MemoryRatio = other.MemoryRatio;
            DiskRatio = other.DiskRatio;
            MemoryRequired = other.MemoryRequired;
            DiskRequired = other.DiskRequired;
        }

        public static ResourceUsageSpec DefaultProcessUsage()
        {
            return new ResourceUsageSpec()
            {
                Index = GameData.InvalidPropertyIndex,
                Cycles = 60f,
                MemoryRatio = 0.1f,
                DiskRatio = 0f,
                MemoryRequired = 1,
                DiskRequired = 0
            };
        }

        public static ResourceUsageSpec ScaledUsageSpec(ResourceUsageSpec specBase, int iteration, float exponent)
        {
            ResourceUsageSpec ret = new ResourceUsageSpec(specBase);
            ret.Cycles *= Mathf.Pow(iteration, exponent);
            return ret;
        }
    }

    [Serializable]
    public class Job : IComparable<Job>
    {
        public delegate void JobCompleted(GameData data);

        public int Index { get; }
        public string Name;
        public GameDataProperty<float> Progress { get; }
        public GameDataProperty<bool> Complete { get; }
        public GameDataProperty<bool> Unlocked { get; }

        public virtual ResourceUsageSpec ResourceUsage
        {
            get => resourceUsageBase;
            set => resourceUsageBase = value;
        }

        protected ResourceUsageSpec resourceUsageBase;

        private readonly JobCompleted onJobCompleted;
        private readonly List<Objective> requirements;

        public Job(int index, string name, ResourceUsageSpec resourceUsage, List<Objective> requiredObjectives, JobCompleted jobCompletedCallback)
        {
            Index = index;
            Name = name;
            Progress = new GameDataProperty<float>(Index, "Progress", 0f);
            Complete = new GameDataProperty<bool>(Index, "Complete", false);
            Unlocked = new GameDataProperty<bool>(Index, "Unlocked", false);
            resourceUsageBase = resourceUsage;
            onJobCompleted = jobCompletedCallback;
            requirements = requiredObjectives;

            if (requirements != null)
            {
                foreach (Objective objective in requirements)
                {
                    objective.Complete.Subscribe(OnObjectiveUpdated);
                }
            }
            Progress.Subscribe(OnProgressUpdated);

            OnObjectiveUpdated(null);
        }

        private void OnProgressUpdated(GameDataProperty prop)
        {
            Complete.Value = prop.GetValue<float>() >= 1f;
        }

        private void OnObjectiveUpdated(GameDataProperty prop)
        {
            Unlocked.Value = requirements?.All(r => r.Complete) ?? true;
        }

        public int CompareTo(Job other) { return Index.CompareTo(other.Index); }

        public virtual void Finish(GameData data)
        {
            onJobCompleted?.Invoke(data);
        }

    }

    [Serializable]
    public class RepeatableJob : Job
    {
        public GameDataProperty<int> Completions;

        public override ResourceUsageSpec ResourceUsage
        {
            get => scaledResourceUsage;
        }

        [SerializeField]
        private ResourceUsageSpec scaledResourceUsage;
        [SerializeField]
        private float exponent;

        public RepeatableJob(int index, string name, ResourceUsageSpec resourceUsage, List<Objective> requiredObjectives, JobCompleted jobCompletedCallback, float exponentScale) : base(index, name, resourceUsage, requiredObjectives, jobCompletedCallback)
        {
            Completions = new GameDataProperty<int>(index, "Completions", 0);
            exponent = exponentScale;
            Reset();
        }

        public override void Finish(GameData data)
        {
            base.Finish(data);
            Completions.Value += 1;
            Reset();
        }

        private void Reset()
        {
            Progress.Value = 0f;
            Complete.Value = false;
            scaledResourceUsage = ResourceUsageSpec.ScaledUsageSpec(resourceUsageBase, Completions, exponent);
        }
    }
}