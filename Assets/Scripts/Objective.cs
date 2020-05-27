using System;
using System.Collections.Generic;
using System.Linq;

namespace Bitwise.Game
{
    [Serializable]
    public class Objective
    {
        public int Id { get; }

        public string Name { get; }

        public GameDataProperty<bool> Complete { get; }

        private readonly List<Job> dependentJobs;

        public Objective(int id, string name, List<Job> dJobs)
        {
            Id = id;
            Complete = new GameDataProperty<bool>(Id, "Complete", false);
            dependentJobs = dJobs;
            foreach (Job job in dJobs)
            {
                job.Complete.Subscribe(OnJobProgressUpdated);
            }
        }

        private void OnJobProgressUpdated(GameDataProperty prop)
        {
            Complete.Value = dependentJobs.All(job => job.Complete);
        }
    }
}