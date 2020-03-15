using System.Collections.Generic;
using System.Linq;

namespace Bitwise.Game
{
    public class Objective
    {
        public int Id { get; private set; }

        public GameDataProperty<bool> Complete { get; private set; }

        private readonly List<Job> dependentJobs;

        public Objective(int id, List<Job> dJobs)
        {
            Id = id;
            Complete = new GameDataProperty<bool>(Id, false);
            dependentJobs = dJobs;
            foreach (Job job in dJobs)
            {
                job.Complete.OnPropertyChanged += OnJobProgressUpdated;
            }
        }

        private void OnJobProgressUpdated(GameDataProperty prop)
        {
            Complete.Value = dependentJobs.All(job => job.Complete.Value);
        }
    }
}