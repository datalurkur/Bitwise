using System.Collections.Generic;
using Bitwise.Interface;

namespace Bitwise.Game
{
    public class BootSectorNotRepairedState : GameState
    {
        protected override List<int> SubscribedJobs { get; } = new List<int>() { GameData.RepairBootSector };

        public BootSectorNotRepairedState() { }

        protected override void OnPush()
        {
            base.OnPush();

            if (gameData.JobComplete(GameData.RepairBootSector))
            {
                Done = true;
            }
            else
            {
                gameData.VisualConsoleHistory.AppendText($"Partition table{Utils.NonBreakingSpace}");
                gameData.VisualConsoleHistory.QueueConsoleEvents(Utils.BuildStatusBlock("CORRUPTED", ThemeManager.ErrorColor));
                gameData.VisualConsoleHistory.AddLine($", boot sector not found.");
                gameData.VisualConsoleHistory.AppendText($"Rebuild partition table?{Utils.NonBreakingSpace}");
                gameData.VisualConsoleHistory.QueueConsoleEvents(Utils.BuildStatusBlock("WARNING", ThemeManager.WarningColor));
                gameData.VisualConsoleHistory.AddLine($"{Utils.NonBreakingSpace}ALL USER DATA WILL BE LOST.");
                PushState(new ConfirmState( "Continue? (yes/no)", RepairBootSector, () => Done = true));
            }
        }

        protected override void OnJobChanged(Job job)
        {
            if (job.Complete.Value)
            {
                gameData.VisualConsoleHistory.AddLine("Partition table rebuilt and boot sector repaired.");
                Done = true;
            }
        }

        private void RepairBootSector()
        {
            gameData.VisualConsoleHistory.QueueConsoleEvent(new ConsoleHistory.ConsoleEvent("Rebuilding partition table", true, InterfaceManager.LongDelayTime));
            processor.QueueJob(GameData.RepairBootSector);
        }
    }
}