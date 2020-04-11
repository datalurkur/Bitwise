using System;
using System.Collections.Generic;
using System.Linq;
using Bitwise.Interface;
using UnityEditor;
using UnityEngine;

namespace Bitwise.Game
{
    public abstract class GameState
    {
        private class DelayedEvent
        {
            public Action Event { get; private set; }
            public float Delay { get; private set; }

            public DelayedEvent(Action evt, float delay)
            {
                Event = evt;
                Delay = delay;
            }

            public bool Update(float deltaTime)
            {
                Delay -= deltaTime;
                if (Delay > 0f) return false;
                Event();
                return true;
            }
        }

        private GameState subState = null;
        private GameState parent = null;

        private readonly List<DelayedEvent> delayedEvents = new List<DelayedEvent>();

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
            delayedEvents.IterateAndRemove((delayedEvent) => delayedEvent.Update(deltaTime));
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

        protected void QueueDelayedEvent(Action evt, float delay)
        {
            delayedEvents.Add(new DelayedEvent(evt, delay));
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
            gameData.VisualConsoleHistory.AddLine(promptContent);
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
                    gameData.VisualConsoleHistory.AddLine("Unable to parse response");
                    gameData.VisualConsoleHistory.AddLine(promptContent);
                    break;
            }

            return true;
        }
    }
}