using System;
using System.Collections.Generic;
using System.Linq;
using Bitwise.Interface;
using JetBrains.Annotations;
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

        public GameState ActiveState
        {
            get => SubState?.ActiveState ?? this;
        }

        private GameState SubState
        {
            get => subStateStack.Count > 0 ? subStateStack.Last() : null;
        }

        public virtual List<string> SupportedCommands { get; } = new List<string>();

        protected GameData gameData => GameManager.Instance.Data;
        protected Processing processor => GameManager.Instance.Processor;

        protected virtual List<int> SubscribedProperties { get; } = null;
        protected virtual List<int> SubscribedJobs { get; } = null;
        protected virtual List<int> SubscribedObjectives { get; } = null;

        protected bool Done { get; set; } = false;

        private readonly List<DelayedEvent> delayedEvents = new List<DelayedEvent>();

        private List<GameState> subStateStack = new List<GameState>();

        public GameState() { }

        public virtual void Update(float deltaTime)
        {
            if (SubState?.Done ?? false) { PopState(); }
            SubState?.Update(deltaTime);
            delayedEvents.IterateAndRemove((delayedEvent) => delayedEvent.Update(deltaTime));
        }

        public virtual bool ProcessUserIntent(UserIntent intent)
        {
            if (SubState?.ProcessUserIntent(intent) ?? false) { return true; }
            return false;
        }

        public virtual bool GetSupportedCommands(ref List<string> supportedCommands)
        {
            if (SubState?.GetSupportedCommands(ref supportedCommands) ?? false) { return true; }
            supportedCommands.AddRange(SupportedCommands);
            return false;
        }

        protected void QueueDelayedEvent(Action evt, float delay)
        {
            delayedEvents.Add(new DelayedEvent(evt, delay));
        }

        protected void PushState(Type stateType, params object[] additionalArgs)
        {
            GameState nextState = Activator.CreateInstance(stateType, additionalArgs) as GameState;
            PushState(nextState);
        }

        protected void PushState(GameState state)
        {
            subStateStack.Add(state);
            state.OnPush();
        }

        protected void PopState()
        {
            if (subStateStack.Count == 0) { return; }
            Type transitionTo = subStateStack.Last().OnPop();
            subStateStack.RemoveAt(subStateStack.Count - 1);
            if (transitionTo != null)
            {
                PushState(transitionTo);
            }
        }

        protected virtual void OnPush()
        {
            SubscribedProperties?.ForEach(property => gameData.ListenForChanges(property, OnPropertyChanged));
            SubscribedJobs?.ForEach(job => gameData.GetJob(job).Complete.Subscribe(OnJobPropertyChanged));
            SubscribedObjectives?.ForEach(objective => gameData.GetObjective(objective).Complete.Subscribe(OnObjectivePropertyChanged));
        }

        protected virtual Type OnPop()
        {
            SubscribedProperties?.ForEach(property => gameData.StopListening(property, OnPropertyChanged));
            SubscribedJobs?.ForEach(job => gameData.GetJob(job).Complete.Unsubscribe(OnJobPropertyChanged));
            SubscribedObjectives?.ForEach(objective => gameData.GetObjective(objective).Complete.Unsubscribe(OnObjectivePropertyChanged));
            return null;
        }

        #region Subscription Callbacks
        protected virtual void OnPropertyChanged(GameDataProperty property) { }
        protected virtual void OnJobChanged(Job job) { }
        protected virtual void OnObjectiveChanged(Objective objective) { }

        private void OnJobPropertyChanged(GameDataProperty property)
        {
            OnJobChanged(gameData.GetJob(property.Index));
        }

        private void OnObjectivePropertyChanged(GameDataProperty property)
        {
            OnObjectiveChanged(gameData.GetObjective(property.Index));
        }
        #endregion
    }

    public class ConfirmState : GameState
    {
        public delegate void PromptResult();

        public override List<string> SupportedCommands { get; } = new List<string>()
        {
            "yes", "no"
        };

        private string promptContent;
        private PromptResult onPromptConfirmed;
        private PromptResult onPromptCanceled;

        public ConfirmState(string prompt, PromptResult confirmedCallback, PromptResult canceledCallback)
        {
            promptContent = prompt;
            onPromptConfirmed = confirmedCallback;
            onPromptCanceled = canceledCallback;
        }

        protected override void OnPush()
        {
            gameData.VisualConsoleHistory.AddLine(promptContent);
        }

        public override bool ProcessUserIntent(UserIntent intent)
        {
            switch (intent.Intent)
            {
                case UserIntent.IntentType.Confirm:
                    onPromptConfirmed?.Invoke();
                    Done = true;
                    break;
                case UserIntent.IntentType.Cancel:
                    onPromptCanceled?.Invoke();
                    Done = true;
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