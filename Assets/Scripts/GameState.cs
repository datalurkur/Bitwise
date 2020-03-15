using System;
using System.Linq;
using Bitwise.Interface;
using UnityEditor;
using UnityEngine;

namespace Bitwise.Game
{
    public abstract class GameState
    {
        private GameState subState = null;
        private GameState parent = null;

        public GameState ActiveState
        {
            get => subState?.ActiveState ?? this;
        }

        protected GameData gameData => GameManager.Instance.Data;

        public GameState(GameState parentState = null)
        {
            parent = parentState;
        }

        public virtual void Update(float deltaTime)
        {
            subState?.Update(deltaTime);
        }

        public void ProcessUserIntent(UserIntent intent)
        {
            if (subState?.ProcessUserIntent_Internal(intent) ?? false) { return; }
            if (ProcessUserIntent_Internal(intent)) { return; }
            if (parent == null)
            {
                throw new NotImplementedException();
            }
        }

        protected void PushState(Type stateType)
        {
            if (subState != null) { throw new Exception("Substate exists"); }
            PushState(Activator.CreateInstance(stateType, this) as GameState);
        }

        protected void PushState(GameState state)
        {
            subState = state;
            subState.OnPush();
        }

        protected void PopState()
        {
            if (subState == null) { throw new Exception("Substate doesn't exist"); }
            subState.OnPop();
            subState = null;
        }

        protected void Finish()
        {
            if (parent == null) { throw new Exception("Root nodes cannot finish"); }
            parent.OnChildFinished();
        }

        protected virtual bool ProcessUserIntent_Internal(UserIntent intent) { return false; }

        protected virtual void OnPush() {}

        protected virtual void OnPop() {}

        protected virtual void OnChildFinished()
        {
            PopState();
        }

        private void SetParent(GameState state)
        {
            parent = state;
        }
    }

    public class ConfirmState : GameState
    {
        public delegate void PromptConfirmed();

        public delegate void PromptCanceled();

        private string promptContent;
        private PromptConfirmed onPromptConfirmed;
        private PromptCanceled onPromptCanceled;

        public ConfirmState(GameState parent, string prompt, PromptConfirmed confirmedCallback, PromptCanceled canceledCallback) : base(parent)
        {
            promptContent = prompt;
            onPromptConfirmed = confirmedCallback;
            onPromptCanceled = canceledCallback;
        }

        protected override void OnPush()
        {
            gameData.VisualConsoleHistory.AddText(promptContent);
        }

        protected override bool ProcessUserIntent_Internal(UserIntent intent)
        {
            switch (intent.Intent)
            {
                case UserIntent.IntentType.Confirm:
                    onPromptConfirmed?.Invoke();
                    Finish();
                    break;
                case UserIntent.IntentType.Cancel:
                    onPromptCanceled?.Invoke();
                    Finish();
                    break;
                default:
                    gameData.VisualConsoleHistory.AddText("Unable to parse response");
                    gameData.VisualConsoleHistory.AddText(promptContent);
                    break;
            }

            return true;
        }
    }
}