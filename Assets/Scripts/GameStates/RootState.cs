using System;
using System.Linq;
using Bitwise.Interface;
using UnityEditor;
using UnityEngine;

namespace Bitwise.Game
{
    public class RootState : GameState
    {
        public RootState()
        {
            UpdateActiveSubState();
        }

        protected override bool ProcessUserIntent_Internal(UserIntent intent)
        {
            switch (intent.Intent)
            {
            case UserIntent.IntentType.Quit:
                PushState(new ConfirmState(this, "Are you sure you want to quit? (Y/N)", OnQuitConfirmed, null));
                return true;
            case UserIntent.IntentType.Query:
                if (intent.Target == IntentTarget.Commands)
                {
                    gameData.VisualConsoleHistory.AddLine("TODO: Insert command list here");
                    return true;
                }
                break;
            case UserIntent.IntentType.Unknown:
                gameData.VisualConsoleHistory.AddLine($"Unknown command");
                return true;
            }

            return false;
        }

        protected override void OnChildFinished()
        {
            base.OnChildFinished();
            UpdateActiveSubState();
        }

        private void UpdateActiveSubState()
        {
            if (gameData.GetObjective(GameData.FullyBooted).Complete.Value)
            {
                throw new NotImplementedException();
            }
            else
            {
                PushState(typeof(LimpState));
            }
        }

        private void OnQuitConfirmed()
        {
            gameData.VisualConsoleHistory.QueueConsoleEvent(new ConsoleHistory.ConsoleEvent("Powering down...", true, 0.5f, null, null, null, DoQuit));
        }

        private void DoQuit()
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