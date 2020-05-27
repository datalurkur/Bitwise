using System;
using System.Collections.Generic;

namespace Bitwise.Game
{
    public class RunDiagnosticState : GameState
    {
        protected override List<int> SubscribedJobs { get; } = new List<int>() { GameData.RunDiagnostic };

        public RunDiagnosticState() { }

        protected override void OnPush()
        {
            base.OnPush();

            if (gameData.JobComplete(GameData.RunDiagnostic))
            {
                Done = true;
            }
            else
            {
                gameData.VisualConsoleHistory.AddLine("Running diagnostic");
                processor.QueueJob(GameData.RunDiagnostic);
            }
        }

        protected override Type OnPop()
        {
            base.OnPop();

            if (gameData.ObjectiveComplete(GameData.FullyBooted))
            {
                gameData.VisualConsoleHistory.AddLine("System is stable. Please restart to resume normal operation.");
                return null;
            }
            else
            {
                gameData.VisualConsoleHistory.AddLine("System is unstable.");
                return typeof(DefaultsNotResetState);
            }
        }

        protected override void OnJobChanged(Job job)
        {
            if (job.Complete)
            {
                Done = true;
            }
        }
    }
}

