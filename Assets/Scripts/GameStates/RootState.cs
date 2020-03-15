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
                    gameData.VisualConsoleHistory.AddText("TODO: Insert command list here");
                    return true;
                }
                break;
            case UserIntent.IntentType.DebugRandom:
                for (int i = 0; i < 100; ++i)
                {
                    gameData.VisualConsoleHistory.AddText(Utils.GenerateRandomAlphaNumericString(100), true, 0f, 1000f);
                }
                return true;
            case UserIntent.IntentType.Unknown:
                gameData.VisualConsoleHistory.AddText($"Unknown command");
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
            gameData.VisualConsoleHistory.AddText("Powering down...", true, 0.5f, null, DoQuit);
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