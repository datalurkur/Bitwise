using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bitwise.Game;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.Serialization;

namespace Bitwise.Interface
{
    public class VirtualConsoleDisplay : MonoBehaviour
    {
        private static ConsoleHistory History => GameManager.Instance.Data.VisualConsoleHistory;

        public delegate string UserInputUpdated(string input);
        public delegate void UserInputCommitted(string input);
        public delegate void PrintingFinished();

        public UserInputUpdated OnUserInputUpdated;
        public UserInputCommitted OnUserInputCommitted;
        public PrintingFinished OnPrintingFinished;

        public RectTransform VirtualConsoleComplexLinePrefab;
        public RectTransform LineLayoutGroup;
        public VirtualConsoleComplexLine UserInputField;

        public RectTransform TabsContainer;
        public VirtualConsoleTab TabPrefab;

        private VirtualConsoleComplexLine ActiveTextContainer => textContainers.Count > 0 ? textContainers[textContainers.Count - 1] : null;
        private List<TextBlock> activeTextBlocks = new List<TextBlock>() { null };

        private float previousLayoutGroupHeight = 0f;
        private readonly List<VirtualConsoleComplexLine> textContainers = new List<VirtualConsoleComplexLine>();

        private string promptText = ">";
        private string previousUserInputString = "";
        private string userInputString = "";
        private string completionString = "";

        public string FormatUserInputString(string userInput)
        {
            return $"{promptText} {userInput}";
        }

        public List<TextBlock> FormatUserInputStringWithCompletion(string userInput, string completion)
        {
            string cursorCharacter = completion.Length > 0 ? completion.Substring(0, 1) : " ";
            string completionTrail = completion.Length > 1 ? completion.Substring(1) : "";
            return new List<TextBlock>()
            {
                TextBlock.PlainText(FormatUserInputString(userInput)),
                TextBlock.ColoredText($"<color=#000000>{cursorCharacter}</color>", Color.white),
                TextBlock.PlainText(completionTrail)
            };
        }

        /*
        public float GetWidthOfString(string text)
        {
            return UserInputField.GetPreferredValues(text, Mathf.Infinity, Mathf.Infinity).x;
        }
        */

        /*
        public float GetMaxStringWidth()
        {
            return UserInputField.rectTransform.rect.width;
        }
        */

        protected void Start()
        {
            UserInputField.SetContent(FormatUserInputStringWithCompletion("", ""));
            History.OnTextBlockCompleted += BeginNewTextBlock;
            History.ActiveTextBlock.Subscribe(OnActiveLineChanged);
        }

        protected void OnDestroy()
        {
            History.OnTextBlockCompleted -= BeginNewTextBlock;
            History.ActiveTextBlock.Unsubscribe(OnActiveLineChanged);
        }

        protected void Update()
        {
            if (!Mathf.Approximately(LineLayoutGroup.rect.height, previousLayoutGroupHeight))
            {
                RepopulateLayoutGroup();
                previousLayoutGroupHeight = LineLayoutGroup.rect.height;
            }

            HandleInput();
        }

        private void HandleInput()
        {
            string newText = Input.inputString;
            for (int i = 0; i < newText.Length; ++i)
            {
                switch (newText[i])
                {
                    case '\b':
                        if (userInputString.Length > 0)
                        {
                            userInputString = userInputString.Remove(userInputString.Length - 1);
                        }
                        break;
                    case '\n':
                    case '\r':
                        OnUserInputCommitted?.Invoke(userInputString + completionString);
                        userInputString = "";
                        break;
                    default:
                        userInputString += newText[i];
                        break;
                }
            }
            if (!string.Equals(userInputString, previousUserInputString))
            {
                string suggestedUserInput = OnUserInputUpdated?.Invoke(userInputString);
                if (!string.IsNullOrEmpty(suggestedUserInput))
                {
                    completionString = suggestedUserInput.Substring(userInputString.Length);
                }
                else
                {
                    completionString = "";
                }

                UserInputField.SetContent(FormatUserInputStringWithCompletion(userInputString, completionString));
                previousUserInputString = userInputString;
            }
        }

        private void BeginNewTextBlock(bool newline)
        {
            if (newline)
            {
                VirtualConsoleComplexLine line = textContainers[0];
                textContainers.RemoveAt(0);
                textContainers.Add(line);
                line.SetContent(null);
                line.UITransform.SetAsLastSibling();
                activeTextBlocks.Clear();
            }
            activeTextBlocks.Add(null);
        }

        private void RepopulateLayoutGroup()
        {
            int prevNumLines = textContainers.Count;
            int numLines = Mathf.Max(0, Mathf.FloorToInt(LineLayoutGroup.rect.height / VirtualConsoleComplexLinePrefab.rect.height));
            int deltaLines = numLines - prevNumLines;
            if (deltaLines < 0)
            {
                for (int i = 0; i < -deltaLines; ++i)
                {
                    Destroy(textContainers[i].gameObject);
                }
                textContainers.RemoveRange(0, -deltaLines);
            }
            else if (deltaLines > 0)
            {
                for (int i = 0; i < deltaLines; ++i)
                {
                    RectTransform newLine = Instantiate(VirtualConsoleComplexLinePrefab, LineLayoutGroup);
                    VirtualConsoleComplexLine tContainer = newLine.GetComponent<VirtualConsoleComplexLine>();

                    int historyLineIndex = History.CompletedLines.Count - numLines + i;
                    if (historyLineIndex >= 0 && historyLineIndex < History.CompletedLines.Count)
                    {
                        tContainer.SetContent(History.CompletedLines[historyLineIndex]);
                    }
                    else
                    {
                        tContainer.SetContent(new List<TextBlock>());
                    }
                    newLine.SetAsFirstSibling();
                    textContainers.Insert(0, tContainer);
                }
            }
        }

        private void OnActiveLineChanged(GameDataProperty property)
        {
            activeTextBlocks[activeTextBlocks.Count - 1] = property.GetValue<TextBlock>();
            if (ActiveTextContainer == null) { return; }
            ActiveTextContainer.SetContent(activeTextBlocks);
        }
    }
}