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
        public LimpState(GameState parent) : base(parent) { }

        protected override bool ProcessUserIntent_Internal(UserIntent intent)
        {
            switch (intent.Intent)
            {
                case UserIntent.IntentType.Debug:
                    return true;
            }

            return false;
        }

        protected override void OnPush()
        {
            gameData.GetObjective(GameData.FullyBooted).Complete.OnPropertyChanged += PropertyChanged;

            QueueDelayedEvent(() =>
            {
                gameData.VisualConsoleHistory.AddLine("SepiaSoft OS v1.5.1s");
                int cpuCores = gameData.GetPropertyValue<int>(GameData.CpuCores);
                string cpuStatus = $"{cpuCores} core{(cpuCores > 1 ? "s" : "")} at {gameData.GetPropertyValue<int>(GameData.CpuSpeed)} MHz ";
                string ramStatus = $"{gameData.GetPropertyValue<int>(GameData.MemoryCapacity)} MB at {gameData.GetPropertyValue<int>(GameData.MemorySpeed)} MHz ";
                string psuStatus = $"{gameData.GetPropertyValue<int>(GameData.Power)} W ";
                string hddStatus = $"{gameData.GetPropertyValue<int>(GameData.DiskCapacity)} GB at {gameData.GetPropertyValue<int>(GameData.DiskSpeed)} MHz ";
                float basicCheckSpeed = 0f;
                float secondaryCheckSpeed = 0f;
                /*
                PrintStatusCheck("CPU Check", cpuStatus, "OK", "00FF00", basicCheckSpeed);
                PrintStatusCheck("RAM Check", ramStatus, "OK", "00FF00", basicCheckSpeed);
                PrintStatusCheck("PSU Check", psuStatus, "UNSTABLE", "DDDD33", basicCheckSpeed);
                PrintStatusCheck("HDD Check", hddStatus, "OK", "00FF00", basicCheckSpeed);
                gameData.VisualConsoleHistory.AddLine("Checking disk partitions");
                */
                object indentContext = gameData.VisualConsoleHistory.Indent();
                /*
                PrintStatusCheck("Partition Map", "", "OK", "00FF00", secondaryCheckSpeed);
                PrintStatusCheck("File Index", "", "OK", "00FF00", secondaryCheckSpeed);
                PrintStatusCheck("Command Database", "", "CORRUPT", "FF0000", secondaryCheckSpeed);
                PrintStatusCheck("User Credentials", "", "CORRUPT", "FF0000", secondaryCheckSpeed);
                PrintStatusCheck("User Data", "", "CORRUPT", "FF0000", secondaryCheckSpeed);
                PrintStatusCheck("Recovery Partition", "", "CORRUPT", "FF0000", secondaryCheckSpeed);
                */
                gameData.VisualConsoleHistory.Unindent(indentContext);
                gameData.VisualConsoleHistory.QueueConsoleEvents(BuildStatusBlock("ERROR", "FF0000"));
                gameData.VisualConsoleHistory.AddLine(" Irrecoverable data loss, booting in limp mode");
            }, 1f);
        }

        private List<ConsoleHistory.ConsoleEvent> BuildStatusBlock(string status, string color)
        {
            return new List<ConsoleHistory.ConsoleEvent>()
            {
                new ConsoleHistory.ConsoleEvent("[", false),
                new ConsoleHistory.ConsoleEvent(status, false, 0f, null, color),
                new ConsoleHistory.ConsoleEvent("]", false)
            };
        }

        protected override void OnPop()
        {
            gameData.GetObjective(GameData.FullyBooted).Complete.OnPropertyChanged -= PropertyChanged;
        }

        private void PropertyChanged(GameDataProperty property)
        {
            if (property.GetValue<bool>())
            {
                Finish();
            }
        }
    }
}

