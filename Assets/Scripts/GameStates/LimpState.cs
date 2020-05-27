using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bitwise.Interface;
using UnityEngine;

namespace Bitwise.Game
{
    public class LimpState : GameState
    {
        public override List<string> SupportedCommands { get; } = new List<string>()
        {
            "diag",
            "reboot"
        };

        public LimpState() { }

        public override bool ProcessUserIntent(UserIntent intent)
        {
            if (base.ProcessUserIntent(intent)) { return true; }

            switch (intent.Intent)
            {
                case UserIntent.IntentType.Diag:
                    PushState(typeof(RunDiagnosticState));
                    return true;
                case UserIntent.IntentType.Reboot:
                    if (!gameData.ObjectiveComplete(GameData.FullyBooted))
                    {
                        gameData.VisualConsoleHistory.AddLine("System unstable, reboot will likely corrupt essential processes.");
                        return true;
                    }
                    break;
            }

            return false;
        }

        protected override void OnPush()
        {
            base.OnPush();

            if (gameData.ObjectiveComplete(GameData.FullyBooted))
            {
                Done = true;
                return;
            }

            gameData.VisualConsoleHistory.AddLine("SepiaSoft OS v1.5.1s");
            int cpuCores = gameData.GetPropertyValue<int>(GameData.CpuCores);
            string cpuStatus = $"{cpuCores} core{(cpuCores > 1 ? "s" : "")} at {gameData.GetPropertyValue<float>(GameData.CpuSpeed)} MHz{Utils.NonBreakingSpace}";
            string ramStatus = $"{gameData.GetPropertyValue<float>(GameData.MemoryCapacity)} MB at {gameData.GetPropertyValue<float>(GameData.MemorySpeed)} MHz{Utils.NonBreakingSpace}";
            string psuStatus = $"{gameData.GetPropertyValue<float>(GameData.Power)} W{Utils.NonBreakingSpace}";
            string hddStatus = $"{gameData.GetPropertyValue<float>(GameData.DiskCapacity)} GB at {gameData.GetPropertyValue<float>(GameData.DiskSpeed)} MHz{Utils.NonBreakingSpace}";
            gameData.VisualConsoleHistory.QueueConsoleEvents(Utils.BuildStatusCheck("CPU Check", cpuStatus, "OK", "00FF00", InterfaceManager.NormalDelayTime));
            gameData.VisualConsoleHistory.QueueConsoleEvents(Utils.BuildStatusCheck("RAM Check", ramStatus, "OK", "00FF00", InterfaceManager.NormalDelayTime));
            gameData.VisualConsoleHistory.QueueConsoleEvents(Utils.BuildStatusCheck("PSU Check", psuStatus, "UNSTABLE", "DDDD33", InterfaceManager.NormalDelayTime));
            gameData.VisualConsoleHistory.QueueConsoleEvents(Utils.BuildStatusCheck("HDD Check", hddStatus, "OK", "00FF00", InterfaceManager.NormalDelayTime));
            gameData.VisualConsoleHistory.AddLine("Checking disk partitions");
            object indentContext = gameData.VisualConsoleHistory.Indent();
            gameData.VisualConsoleHistory.QueueConsoleEvents(Utils.BuildStatusCheck("Partition Map", "", "OK", "00FF00", InterfaceManager.ShortDelayTime));
            gameData.VisualConsoleHistory.QueueConsoleEvents(Utils.BuildStatusCheck("File Index", "", "OK", "00FF00", InterfaceManager.ShortDelayTime));
            gameData.VisualConsoleHistory.QueueConsoleEvents(Utils.BuildStatusCheck("Command Database", "", "CORRUPT", "FF0000", InterfaceManager.ShortDelayTime));
            gameData.VisualConsoleHistory.QueueConsoleEvents(Utils.BuildStatusCheck("User Credentials", "", "CORRUPT", "FF0000", InterfaceManager.ShortDelayTime));
            gameData.VisualConsoleHistory.QueueConsoleEvents(Utils.BuildStatusCheck("User Data", "", "CORRUPT", "FF0000", InterfaceManager.ShortDelayTime));
            gameData.VisualConsoleHistory.QueueConsoleEvents(Utils.BuildStatusCheck("Recovery Partition", "", "CORRUPT", "FF0000", InterfaceManager.ShortDelayTime));
            gameData.VisualConsoleHistory.Unindent(indentContext);
            gameData.VisualConsoleHistory.QueueConsoleEvents(Utils.BuildStatusBlock("ERROR", "FF0000"));
            gameData.VisualConsoleHistory.AddLine(" Irrecoverable data loss, booting in limp mode");
        }

        protected override Type OnPop()
        {
            base.OnPop();
            return gameData.ObjectiveComplete(GameData.FullyBooted) ? typeof(FullyBootedState) : null;
        }
    }
}

