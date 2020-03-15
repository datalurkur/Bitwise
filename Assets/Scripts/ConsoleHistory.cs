using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bitwise.Game
{
    public class ConsoleHistory
    {
        public delegate void EventComplete();

        public const float CharactersPerSecond = 10;
        public const int IndentationSize = 4;

        private class ConsoleEvent
        {
            private EventComplete callback;

            public string Text { get; private set; }
            public bool NewlineWhenFinished { get; private set; }
            public float DelayWhenFinished { get; private set; }
            public float CharactersPerSecond { get; private set; }

            private float textDuration;
            private float progress;

            public ConsoleEvent(string text, bool newlineWhenFinished, float delayWhenFinished, float cps, EventComplete onComplete = null)
            {
                Text = text;
                NewlineWhenFinished = newlineWhenFinished;
                DelayWhenFinished = delayWhenFinished;
                CharactersPerSecond = cps;
                callback = onComplete;

                textDuration = (Text?.Length ?? 0) / CharactersPerSecond;
                progress = 0f;
            }

            public float Progress(float deltaTime, out string newText)
            {
                int startIndex = Mathf.FloorToInt(progress * CharactersPerSecond);
                progress += deltaTime;

                if (Text == null || startIndex >= Text.Length)
                {
                    newText = "";
                }
                else
                {
                    int endIndex = Math.Min(Text.Length, Mathf.FloorToInt(progress * CharactersPerSecond));
                    newText = Text.Substring(startIndex, (endIndex - startIndex));
                }

                float leftoverProgress = progress - (textDuration + DelayWhenFinished);
                if (leftoverProgress >= 0f)
                {
                    callback?.Invoke();
                }
                return leftoverProgress;
            }
        }

        public delegate void LineCompleted();

        public LineCompleted OnLineCompleted;
        public List<string> CompletedLines { get; private set; } = new List<string>();
        public GameDataProperty<string> ActiveLine { get; private set; } = new GameDataProperty<string>("");

        private readonly Queue<ConsoleEvent> textQueue = new Queue<ConsoleEvent>();

        private readonly HashSet<object> indentationContexts = new HashSet<object>();
        private string indentation = "";
        private bool lastTextWasNewline = true;

        public void AddText(string text, bool newline = true, float delay = 0f, float? speed = null, EventComplete callback = null)
        {
            string fullText = (lastTextWasNewline ? (indentation + text) : text);
            textQueue.Enqueue(new ConsoleEvent(fullText, newline, delay, speed ?? CharactersPerSecond, callback));
            lastTextWasNewline = newline;
        }

        public void AddTextWithDotDelay(string preText, string postText, int dots, float delayPerDot)
        {
            AddText(preText, false, delayPerDot);
            dots.Times(() =>
            {
                AddText(".", false, delayPerDot);
            });
            AddText(postText);
        }

        public object Indent()
        {
            object context = new object();
            indentationContexts.Add(context);
            indentation += new String(' ', IndentationSize);
            return context;
        }

        public void Unindent(object context)
        {
            if (indentationContexts.Remove(context))
            {
                indentation = indentation.Substring(0, indentation.Length - IndentationSize);
            }
        }

        public void Update(float deltaTime)
        {
            if (textQueue.Count > 0)
            {
                ConsoleEvent evt = textQueue.Peek();
                float delta = evt.Progress(deltaTime, out string newText);
                ActiveLine.Value += newText;
                if (delta > 0f)
                {
                    textQueue.Dequeue();
                    if (evt.NewlineWhenFinished)
                    {
                        CompletedLines.Add(ActiveLine.Value);
                        OnLineCompleted?.Invoke();
                        ActiveLine.Value = "";
                    }
                    if (textQueue.Count > 0)
                    {
                        textQueue.Peek().Progress(delta, out string trailingText);
                        ActiveLine.Value += trailingText;
                    }
                }
            }
        }
    }
}
