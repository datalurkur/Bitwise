using System;
using System.Collections.Generic;
using System.Linq;
using Bitwise.Interface;
using UnityEditor;
using UnityEngine;

namespace Bitwise.Game
{
    public class RootState : GameState
    {
        public override List<string> SupportedCommands { get; } = new List<string>()
        {
            "help",
            "quit",
            "reboot"
        };

        public RootState() { QueueDelayedEvent(Reboot, 1f); }

        public override bool ProcessUserIntent(UserIntent intent)
        {
            if (base.ProcessUserIntent(intent)) { return true; }

            switch (intent.Intent)
            {
            case UserIntent.IntentType.Quit:
                PushState(new ConfirmState( "Are you sure you want to quit? (yes/no)", OnQuitConfirmed, null));
                return true;
            case UserIntent.IntentType.Query:
                if (intent.Target == IntentTarget.Commands)
                {
                    gameData.VisualConsoleHistory.AddLine("Known Commands:");
                    object idContext = gameData.VisualConsoleHistory.Indent();
                    ActiveState.SupportedCommands.ForEach(command => gameData.VisualConsoleHistory.AddLine(command));
                    gameData.VisualConsoleHistory.Unindent(idContext);
                    return true;
                }
                break;
            case UserIntent.IntentType.Reboot:
                PushState(new ConfirmState( "System will restart, confirm (yes/no)", Reboot, null));
                return true;
            case UserIntent.IntentType.Unknown:
                gameData.VisualConsoleHistory.AddLine($"Unknown command");
                return true;
            }

            return false;
        }

        private void Reboot()
        {
            PushState(typeof(LimpState));
        }

        private void OnQuitConfirmed()
        {
            gameData.VisualConsoleHistory.QueueConsoleEvent(new ConsoleHistory.ConsoleEvent("Powering down...", true, 0.5f, null, null, null, DoQuit));
        }

        protected void DoQuit()
        {
            if (Application.isEditor)
            {
                EditorApplication.isPlaying = false;
            }
            else
            {
                Application.Quit();
            }
        }
    }
}