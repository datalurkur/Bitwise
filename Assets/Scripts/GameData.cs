using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bitwise.Game
{
    public class GameData
    {
        // Reserved
        public const int InvalidPropertyIndex = -1;
        private const int Reserved = 0;
        private const int Properties = 1000;
        private const int Jobs = 2000;

        // Base numeric properties
        private const int BaseNumerics  = Properties + 0;
        public const int CpuSpeed       = BaseNumerics + 0;
        public const int CpuCores       = BaseNumerics + 1;
        public const int Power          = BaseNumerics + 2;
        public const int DiskSpeed      = BaseNumerics + 3;
        public const int DiskCapacity   = BaseNumerics + 4;
        public const int MemorySpeed    = BaseNumerics + 5;
        public const int MemoryCapacity = BaseNumerics + 6;

        private readonly Dictionary<int, GameDataProperty> properties = new Dictionary<int, GameDataProperty>();

        // Jobs
        public const int RunDiagnostic         = Jobs + 0;
        public const int RegulateAmperage      = Jobs + 1;
        public const int ResetHardwareDefaults = Jobs + 2;
        public const int RepairBootSector      = Jobs + 3;
        public const int ExitLimpMode          = Jobs + 4;

        private readonly Dictionary<int, Job> jobs = new Dictionary<int, Job>();

        // Objectives
        private const int Objectives = 3000;
        public const int FullyBooted = Objectives + 0;

        private readonly Dictionary<int, Objective> objectives = new Dictionary<int, Objective>();

        public ConsoleHistory VisualConsoleHistory { get; private set; } = new ConsoleHistory();

        public GameData()
        {
            AddProperty(CpuSpeed, 1);
            AddProperty(CpuCores, 1);
            AddProperty(Power, 1);
            AddProperty(DiskSpeed, 1);
            AddProperty(DiskCapacity, 1);
            AddProperty(MemorySpeed, 1);
            AddProperty(MemoryCapacity, 1);

            AddJob(RunDiagnostic, ResourceUsageSpec.SlimProcessUsage());
            AddJob(RegulateAmperage, ResourceUsageSpec.SlimProcessUsage());
            AddJob(ResetHardwareDefaults, ResourceUsageSpec.SlimProcessUsage());
            AddJob(RepairBootSector, ResourceUsageSpec.SlimProcessUsage());
            AddJob(ExitLimpMode, ResourceUsageSpec.SlimProcessUsage());

            AddObjective(FullyBooted, new []
            {
                RunDiagnostic,
                RegulateAmperage,
                ResetHardwareDefaults,
                RepairBootSector,
                ExitLimpMode
            });
        }


        public T GetPropertyValue<T>(int propertyIndex) where T : IComparable<T>
        {
            return GetProperty<T>(propertyIndex).Value;
        }

        public void SetPropertyValue<T>(int propertyIndex, T newValue) where T : IComparable<T>
        {
            GetProperty<T>(propertyIndex).Value = newValue;
        }

        public void ListenForChanges(int propertyIndex, GameDataProperty.PropertyChanged callback)
        {
            properties[propertyIndex].OnPropertyChanged += callback;
        }

        public void StopListening(int propertyIndex, GameDataProperty.PropertyChanged callback)
        {
            properties[propertyIndex].OnPropertyChanged -= callback;
        }

        public Job GetJob(int jobIndex)
        {
            return jobs[jobIndex];
        }

        public Objective GetObjective(int objectiveIndex)
        {
            return objectives[objectiveIndex];
        }

        private void AddProperty<T>(int propertyIndex, T defaultValue) where T :IComparable<T>
        {
            if (properties.ContainsKey(propertyIndex))
            {
                throw new ArgumentException($"Property index {propertyIndex} being reused");
            }
            properties[propertyIndex] = new GameDataProperty<T>(propertyIndex, defaultValue);
        }

        private void AddJob(int jobIndex, ResourceUsageSpec resourceUsage)
        {
            if (jobs.ContainsKey(jobIndex))
            {
                throw new ArgumentException($"Job index {jobIndex} being reused");
            }
            jobs[jobIndex] = new Job(jobIndex, resourceUsage);
        }

        private void AddObjective(int objectiveIndex, int[] dependentJobIndices)
        {
            if (objectives.ContainsKey(objectiveIndex))
            {
                throw new ArgumentException($"Objective index {objectiveIndex} being reused");
            }
            objectives[objectiveIndex] = new Objective(objectiveIndex, dependentJobIndices.Select(index => jobs[index]).ToList());
        }

        private GameDataProperty<T> GetProperty<T>(int propertyIndex) where T : IComparable<T>
        {
            return (GameDataProperty<T>) properties[propertyIndex];
        }
    }
}