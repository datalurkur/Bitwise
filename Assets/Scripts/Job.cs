namespace Bitwise.Game
{
    public class ResourceUsageSpec
    {
        public float CPURatio { get; private set; }
        public float MemoryRatio { get; private set; }
        public float DiskRatio { get; private set; }
        public int MemoryRequired { get; private set; }
        public int DiskRequired { get; private set; }

        public static ResourceUsageSpec SlimProcessUsage()
        {
            return new ResourceUsageSpec()
            {
                CPURatio = 0.9f,
                MemoryRatio = 0.1f,
                DiskRatio = 0f,
                MemoryRequired = 1,
                DiskRequired = 0
            };
        }
    }

    public class Job
    {
        public int Id { get; private set; }
        public GameDataProperty<float> Progress { get; private set; }
        public GameDataProperty<bool> Complete { get; private set; }

        public ResourceUsageSpec ResourceUsage { get; private set; }

        public Job(int id, ResourceUsageSpec resourceUsage)
        {
            Id = id;
            Progress = new GameDataProperty<float>(Id, 0f);
            Complete = new GameDataProperty<bool>(Id, false);
            ResourceUsage = resourceUsage;

            Progress.OnPropertyChanged += OnProgressUpdated;
        }

        private void OnProgressUpdated(GameDataProperty prop)
        {
            Complete.Value = prop.GetValue<float>() >= 1f;
        }
    }
}