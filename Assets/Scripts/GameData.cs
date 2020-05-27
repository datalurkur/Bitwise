using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Bitwise.Game
{
    [Serializable]
    public class GameData
    {
        #region Factory
        public static void Save(GameData gameData, string path)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                formatter.Serialize(stream, gameData);
                stream.Close();
            }
        }

        public static GameData Load(string path)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                GameData gameData = formatter.Deserialize(stream) as GameData;
                stream.Close();
                return gameData;
            }
        }

        public static List<string> GetAllGameDataPaths()
        {
            string path = GetGameDataPath();
            return new List<string>(Directory.GetFiles(path)).Where(t => !t.Contains(".meta")).ToList();
        }

        public static string GetGameDataPath()
        {
            string path = Path.Combine(Application.dataPath, "DataBlobs");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
        #endregion

        public IEnumerable<int> PropertyKeys => properties.Keys;
        public IEnumerable<int> ResourceUsageKeys => resourceUsageTemplates.Keys;
        public IEnumerable<int> ResourceKeys => resources.Keys;
        public IEnumerable<int> JobKeys => jobs.Keys;
        public IEnumerable<int> ObjectiveKeys => objectives.Keys;

        public const int InvalidPropertyIndex = -1;

        public string Name;

        [SerializeField]
        private Dictionary<int, GameDataProperty> properties;
        [SerializeField]
        private Dictionary<int, ResourceUsageSpec> resourceUsageTemplates;
        [SerializeField]
        private Dictionary<int, Resource> resources;
        [SerializeField]
        private Dictionary<int, Job> jobs;
        [SerializeField]
        private Dictionary<int, Objective> objectives;

        [SerializeField]
        private int globalIndexCount;

        public readonly GameDataListProperty<Job> QueuedJobs;
        public readonly GameDataListProperty<Job> RunningJobs;

        #region Built-In Data
        private const int Properties = 1000;
        private const int ResourceUsageTemplates = 2000;
        private const int Resources = 3000;
        private const int Jobs = 4000;
        private const int Objectives = 5000;
        private const int Runtime = 6000;
        private const int UserDefinedProperties = 7000;

        // Base numeric properties
        private const int BaseNumerics  = Properties + 0;
        public const int CpuSpeed       = BaseNumerics + 0;
        public const int CpuCores       = BaseNumerics + 1;
        public const int Power          = BaseNumerics + 2;
        public const int DiskSpeed      = BaseNumerics + 3;
        public const int DiskCapacity   = BaseNumerics + 4;
        public const int MemorySpeed    = BaseNumerics + 5;
        public const int MemoryCapacity = BaseNumerics + 6;

        // Resource usage templates
        public const int DefaultResourceUsageSpec = ResourceUsageTemplates + 0;

        // Resources

        // Jobs
        // Phase 0 jobs
        public const int RunDiagnostic         = Jobs + 0;
        public const int ResetHardwareDefaults = Jobs + 1;
        public const int RepairBootSector      = Jobs + 2;

        // Objectives
        public const int FullyBooted = Objectives + 0;

        // Runtime properties
        public const int CpuUsage       = Runtime + 0;
        public const int MemoryUsage    = Runtime + 1;
        public const int MemoryBusUsage = Runtime + 2;
        public const int DiskUsage      = Runtime + 3;
        public const int DiskBusUsage   = Runtime + 4;
        public const int TabsVisible    = Runtime + 7;
        #endregion

        public readonly ConsoleHistory VisualConsoleHistory;

        public GameData()
        {
            properties = new Dictionary<int, GameDataProperty>();
            resourceUsageTemplates = new Dictionary<int, ResourceUsageSpec>();
            resources = new Dictionary<int, Resource>();
            jobs = new Dictionary<int, Job>();
            objectives = new Dictionary<int, Objective>();
            VisualConsoleHistory = new ConsoleHistory();

            globalIndexCount = UserDefinedProperties;

            QueuedJobs = new GameDataListProperty<Job>();
            RunningJobs = new GameDataListProperty<Job>();
        }

        public T GetPropertyValue<T>(int propertyIndex) where T : IComparable<T>
        {
            return GetProperty<T>(propertyIndex).Value;
        }

        public void SetPropertyValue<T>(int propertyIndex, T newValue) where T : IComparable<T>
        {
            GetProperty<T>(propertyIndex).Value = newValue;
        }

        public GameDataListProperty<T> GetListProperty<T>(int propertyIndex) where T : IComparable<T>
        {
            return (GameDataListProperty<T>) properties[propertyIndex];
        }

        public void ListenForChanges(int propertyIndex, GameDataProperty.PropertyChanged callback) => properties[propertyIndex].Subscribe(callback);
        public void StopListening(int propertyIndex, GameDataProperty.PropertyChanged callback) => properties[propertyIndex].Unsubscribe(callback);

        public GameDataProperty GetProperty(int propertyIndex) => properties[propertyIndex];
        public ResourceUsageSpec GetResourceUsageTemplate(int resourceUsageIndex) => resourceUsageTemplates[resourceUsageIndex];
        public Resource GetResource(int resourceIndex) => resources[resourceIndex];
        public Job GetJob(int jobIndex) => jobs[jobIndex];
        public Objective GetObjective(int objectiveIndex) => objectives[objectiveIndex];

        public bool JobComplete(int jobIndex) { return jobs[jobIndex].Complete; }
        public bool ObjectiveComplete(int objectiveIndex) { return objectives[objectiveIndex].Complete; }

        private GameDataProperty<T> GetProperty<T>(int propertyIndex) where T : IComparable<T>
        {
            return (GameDataProperty<T>) properties[propertyIndex];
        }

#if UNITY_EDITOR
        public void DefaultInit()
        {
            AddProperty(CpuSpeed, "CPU Speed", 8f);
            AddProperty(CpuCores, "CPU Cores", 1);
            AddProperty(Power, "Power", 50f);
            AddProperty(DiskSpeed, "Disk Speed", 0.1f);
            AddProperty(DiskCapacity, "Disk Capacity", 32f);
            AddProperty(MemorySpeed, "Memory Speed", 2f);
            AddProperty(MemoryCapacity, "Memory Capacity", 32f);

            AddProperty(CpuUsage, "CPU Usage", 0f);
            AddProperty(MemoryUsage, "Memory Usage", 0f);
            AddProperty(MemoryBusUsage, "Memory Bus Usage", 0f);
            AddProperty(DiskUsage, "Disk Usage", 0f);
            AddProperty(DiskBusUsage, "Disk Bus Usage", 0f);

            AddJob(RunDiagnostic, "Run Diagnostic", ResourceUsageSpec.DefaultProcessUsage());
            AddJob(ResetHardwareDefaults, "Reset Hardware Defaults", ResourceUsageSpec.DefaultProcessUsage());
            AddJob(RepairBootSector, "Repair Boot Sector", ResourceUsageSpec.DefaultProcessUsage());

            AddObjective(FullyBooted, "Fully Booted", new []
            {
                RunDiagnostic,
                ResetHardwareDefaults,
                RepairBootSector
            });

            AddProperty(TabsVisible, "Tabs Visible", false);
        }

        #region Property Creation and Destruction

        public void AddProperty<T>(string name, T defaultValue) where T : IComparable<T> => AddProperty(globalIndexCount++, name, defaultValue);
        public void AddListProperty<T>(string name) where T : IComparable<T> => AddListProperty<T>(globalIndexCount++, name);
        public void AddResourceUsageTemplate(ResourceUsageSpec spec) => AddResourceUsageTemplate(globalIndexCount++, spec);
        public void AddResource(int storagePropertyIndex, string name, float defaultValue) => AddResource(globalIndexCount++, storagePropertyIndex, name, defaultValue);
        public void AddJob(string name, ResourceUsageSpec resourceUsage, List<int> requiredObjectives = null, Job.JobCompleted jobCompletedCallback = null) =>
            AddJob(globalIndexCount++, name, resourceUsage, requiredObjectives, jobCompletedCallback);
        public void AddRepeatableJob(string name, ResourceUsageSpec resourceUsage, float exponent, List<int> requiredObjectives = null, Job.JobCompleted jobCompletedCallback = null) =>
            AddRepeatableJob(globalIndexCount++, name, resourceUsage, exponent, requiredObjectives, jobCompletedCallback);
        public void AddObjective(string name, int[] dependentJobIndices) => AddObjective(globalIndexCount++, name, dependentJobIndices);

        public void DeleteProperty(int propertyIndex) => properties.Remove(propertyIndex);
        public void DeleteResourceUsageTemplate(int rutIndex) => resourceUsageTemplates.Remove(rutIndex);
        public void DeleteResource(int resourceIndex) => resources.Remove(resourceIndex);
        public void DeleteJob(int jobIndex) => jobs.Remove(jobIndex);
        public void DeleteObjective(int objectiveIndex) => objectives.Remove(objectiveIndex);

        private void AddProperty<T>(int propertyIndex, string name, T defaultValue) where T : IComparable<T>
        {
            if (properties.ContainsKey(propertyIndex))
            {
                throw new ArgumentException($"Property index {propertyIndex} being reused");
            }
            properties[propertyIndex] = new GameDataProperty<T>(propertyIndex, name, defaultValue);
        }

        private void AddListProperty<T>(int propertyIndex, string name) where T : IComparable<T>
        {
            if (properties.ContainsKey(propertyIndex))
            {
                throw new ArgumentException($"Property index {propertyIndex} being reused");
            }
            properties[propertyIndex] = new GameDataListProperty<T>(propertyIndex, name);
        }

        private void AddResourceUsageTemplate(int resourceUsageIndex, ResourceUsageSpec spec)
        {
            if (resourceUsageTemplates.ContainsKey(resourceUsageIndex))
            {
                throw new ArgumentException($"Resource usage template index {resourceUsageIndex} being reused");
            }

            spec.Index = resourceUsageIndex;
            resourceUsageTemplates[resourceUsageIndex] = spec;
        }

        private void AddResource(int resourceIndex, int storagePropertyIndex, string name, float defaultValue)
        {
            if (resources.ContainsKey(resourceIndex))
            {
                throw new ArgumentException($"Resource index {resourceIndex} being reused");
            }
            resources[resourceIndex] = new Resource(resourceIndex, storagePropertyIndex, name, defaultValue);
        }

        private void AddJob(int jobIndex, string name, ResourceUsageSpec resourceUsage, List<int> requiredObjectives = null, Job.JobCompleted jobCompletedCallback = null)
        {
            if (jobs.ContainsKey(jobIndex))
            {
                throw new ArgumentException($"Job index {jobIndex} being reused");
            }
            jobs[jobIndex] = new Job(jobIndex, name, resourceUsage, requiredObjectives?.Select(GetObjective).ToList(), jobCompletedCallback);
        }

        private void AddRepeatableJob(int jobIndex, string name, ResourceUsageSpec resourceUsage, float exponent, List<int> requiredObjectives = null, Job.JobCompleted jobCompletedCallback = null)
        {
            if (jobs.ContainsKey(jobIndex))
            {
                throw new ArgumentException($"Job index {jobIndex} being reused");
            }
            jobs[jobIndex] = new RepeatableJob(jobIndex, name, resourceUsage, requiredObjectives?.Select(GetObjective).ToList(), jobCompletedCallback, exponent);
        }

        private void AddObjective(int objectiveIndex, string name, int[] dependentJobIndices)
        {
            if (objectives.ContainsKey(objectiveIndex))
            {
                throw new ArgumentException($"Objective index {objectiveIndex} being reused");
            }
            objectives[objectiveIndex] = new Objective(objectiveIndex, name, dependentJobIndices.Select(index => jobs[index]).ToList());
        }

        #endregion
#endif
    }
}