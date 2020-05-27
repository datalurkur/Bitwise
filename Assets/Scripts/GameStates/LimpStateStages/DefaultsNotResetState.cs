using System;
using System.Collections.Generic;
using Bitwise.Interface;

namespace Bitwise.Game
{
    public class DefaultsNotResetState : GameState
    {
        protected override List<int> SubscribedJobs { get; } = new List<int>() { GameData.ResetHardwareDefaults };

        public DefaultsNotResetState() { }

        protected override void OnPush()
        {
            base.OnPush();

            if (gameData.JobComplete(GameData.ResetHardwareDefaults))
            {
                Done = true;
            }
            else
            {
                gameData.VisualConsoleHistory.QueueConsoleEvents(Utils.BuildStatusCheck("PSU Amperage", "", "UNSTABLE", "DDDD33", InterfaceManager.LongDelayTime));
                gameData.VisualConsoleHistory.QueueConsoleEvents(Utils.BuildStatusCheck("CPU Voltages", "", "UNSTABLE", "DDDD33", InterfaceManager.LongDelayTime));
                PushState(new ConfirmState("Reset hardware defaults? Note: this will discard any user settings. (yes/no)", ResetDefaults, () => Done = true));
            }
        }

        protected override Type OnPop()
        {
            base.OnPop();
            return typeof(BootSectorNotRepairedState);
        }

        protected override void OnJobChanged(Job job)
        {
            if (job.Complete.Value)
            {
                gameData.VisualConsoleHistory.AddLine("Hardware defaults restored.");
                Done = true;
            }
        }

        private void ResetDefaults()
        {
            processor.QueueJob(GameData.ResetHardwareDefaults);
        }
    }
}