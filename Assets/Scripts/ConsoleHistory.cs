using System;
using System.Collections.Generic;
using System.Linq;
using Bitwise.Interface;
using UnityEngine;

namespace Bitwise.Game
{
    public class TextBlock : IComparable<TextBlock>
    {
        public static TextBlock PlainText(string text) { return new TextBlock() { Content = text }; }
        public static TextBlock ColoredText(string text, Color? bgColor) { return new TextBlock()
        {
            Content = text,
            BackgroundColor = bgColor
        }; }

        public string Content;
        public Color? BackgroundColor;

        public int CompareTo(TextBlock other)
        {
            if (other == null) { return 1; }
            if (Content == null && other.Content == null) { return 0; }
            if (Content == null) { return -1; }
            if (other.Content == null) { return 1; }
            int strCmp = Content.CompareTo(other.Content);
            if (strCmp != 0) { return strCmp; }
            return (BackgroundColor == other.BackgroundColor) ? 0 : 1;
        }
    }

    public class ConsoleHistory
    {
        public delegate void EventComplete();
        public delegate void TextBlockCompleted(bool newline);

        public const float DefaultCharactersPerSecond = 100;
        public const float InstantaneousCharactersPerSecond = 1000000;
        public const int IndentationSize = 4;

        public class ConsoleEvent
        {
            private EventComplete callback;
            public TextBlock Text
            {
                get => new TextBlock()
                {
                    Content = textBuffer,
                    BackgroundColor = backgroundColor
                };
            }

            public bool NewlineWhenFinished { get; private set; }
            public float DelayWhenFinished { get; private set; }
            public float CharactersPerSecond { get; private set; }

            private float textDuration
            {
                get => (textContent?.Length ?? 0) / CharactersPerSecond;
            }

            private string textContent;
            private readonly string textColor;
            private readonly Color? backgroundColor;
            private float progress;
            private bool firstRender = true;
            private string textBuffer;

            public ConsoleEvent(string content, bool newlineWhenFinished = true, float delayWhenFinished = 0f, float? cps = DefaultCharactersPerSecond, string tColor = null, Color? bColor = null, EventComplete onEventComplete = null)
            {
                NewlineWhenFinished = newlineWhenFinished;
                DelayWhenFinished = delayWhenFinished;
                CharactersPerSecond = cps ?? DefaultCharactersPerSecond;

                textContent = content;
                textColor = tColor;
                backgroundColor = bColor;
                callback = onEventComplete;

                progress = 0f;
            }

            public void SetIndent(string indent)
            {
                if (textContent == null) { return; }
                textContent = indent + textContent;
            }

            public float Progress(float deltaTime)
            {
                int startIndex = Mathf.FloorToInt(progress * CharactersPerSecond);
                progress += deltaTime;

                if (firstRender && textColor != null)
                {
                    textBuffer += $"<color=#{textColor}>";
                }

                firstRender = false;
                int endIndex = 0;
                if (Text != null && startIndex < textContent.Length)
                {
                    endIndex = Math.Min(textContent.Length, Mathf.FloorToInt(progress * CharactersPerSecond));
                    textBuffer += textContent.Substring(startIndex, (endIndex - startIndex));
                }

                float leftoverProgress = progress - (textDuration + DelayWhenFinished);
                if (leftoverProgress >= 0f)
                {
                    if (textColor != null)
                    {
                        textBuffer += "</color>";
                    }
                    callback?.Invoke();
                }
                return leftoverProgress;
            }
        }

        public TextBlockCompleted OnTextBlockCompleted;
        public List<List<TextBlock>> CompletedLines { get; } = new List<List<TextBlock>>();
        public readonly GameDataProperty<TextBlock> ActiveTextBlock = new GameDataProperty<TextBlock>(new TextBlock());

        private readonly Queue<ConsoleEvent> textQueue = new Queue<ConsoleEvent>();

        private readonly HashSet<object> indentationContexts = new HashSet<object>();
        private string indentation = "";
        private bool wasLastEventNewline = true;

        #region Convenience Functions

        public void AddLine(string text)
        {
            QueueConsoleEvent(new ConsoleEvent(text));
        }

        public void AppendText(string text)
        {
            QueueConsoleEvent(new ConsoleEvent(text, false));
        }

        #endregion

        public void QueueConsoleEvents(IEnumerable<ConsoleEvent> events)
        {
            foreach (ConsoleEvent consoleEvent in events)
            {
                QueueConsoleEvent(consoleEvent);
            }
        }

        public void QueueConsoleEvent(ConsoleEvent evt)
        {
            if (wasLastEventNewline)
            {
                evt.SetIndent(indentation);
            }

            wasLastEventNewline = evt.NewlineWhenFinished;
            textQueue.Enqueue(evt);
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
            float delta = deltaTime;
            while (delta > 0f && textQueue.Count > 0)
            {
                ConsoleEvent evt = textQueue.Peek();
                delta = evt.Progress(delta);

                ActiveTextBlock.Value = evt.Text;
                if (delta > 0f)
                {
                    textQueue.Dequeue();
                    if (CompletedLines.Count == 0)
                    {
                        CompletedLines.Add(new List<TextBlock>());
                    }

                    CompletedLines.Last().Add(evt.Text);
                    OnTextBlockCompleted?.Invoke(evt.NewlineWhenFinished);

                    ActiveTextBlock.Value = null;
                    if (evt.NewlineWhenFinished)
                    {
                        CompletedLines.Add(new List<TextBlock>());
                    }
                }
            }
        }
    }
}
