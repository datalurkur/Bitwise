using System;
using System.Collections;
using System.Collections.Generic;
using Bitwise.Game;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

namespace Bitwise.Interface
{
    public class VirtualConsoleDisplay : MonoBehaviour
    {
        public delegate void UserInputReceived(string input);
        public delegate void PrintingFinished();

        public UserInputReceived OnUserInputReceived;
        public PrintingFinished OnPrintingFinished;

        public RectTransform VirtualConsoleLinePrefab;
        public RectTransform LineLayoutGroup;
        public TMP_Text UserInputField;

        private TMP_Text ActiveTextContainer => textContainers[textContainers.Count - 1];
        private ConsoleHistory History => GameManager.Instance.Data.VisualConsoleHistory;

        private float previousLayoutGroupHeight = 0f;
        private List<TMP_Text> textContainers = new List<TMP_Text>();

        private string promptText = ">";
        private string previousUserInputString = "";
        private string userInputString = "";

        public string FormatUserInputString(string str)
        {
            return $"{promptText} {str}";
        }

        protected void Start()
        {
            UserInputField.text = FormatUserInputString("");
            History.OnLineCompleted += BeginNewLine;
            History.ActiveLine.OnPropertyChanged += OnActiveLineChanged;
        }

        protected void OnDestroy()
        {
            History.OnLineCompleted -= BeginNewLine;
            History.ActiveLine.OnPropertyChanged -= OnActiveLineChanged;
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
                        OnUserInputReceived?.Invoke(userInputString);
                        userInputString = "";
                        break;
                    default:
                        userInputString += newText[i];
                        break;
                }
            }
            if (!string.Equals(userInputString, previousUserInputString))
            {
                UserInputField.text = FormatUserInputString(userInputString);
                previousUserInputString = userInputString;
            }
        }

        private void BeginNewLine()
        {
            TMP_Text line = textContainers[0];
            textContainers.RemoveAt(0);
            textContainers.Add(line);
            line.rectTransform.SetAsLastSibling();
        }

        private void RepopulateLayoutGroup()
        {
            int prevNumLines = textContainers.Count;
            int numLines = Mathf.FloorToInt(LineLayoutGroup.rect.height / VirtualConsoleLinePrefab.rect.height);
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
                    RectTransform newLine = Instantiate(VirtualConsoleLinePrefab, LineLayoutGroup);
                    TMP_Text tContainer = newLine.GetComponent<TMP_Text>();

                    int historyLineIndex = History.CompletedLines.Count - numLines + i;
                    if (historyLineIndex >= 0 && historyLineIndex < History.CompletedLines.Count)
                    {
                        tContainer.text = History.CompletedLines[historyLineIndex];
                    }
                    else
                    {
                        tContainer.text = "";
                    }
                    newLine.SetAsFirstSibling();
                    textContainers.Insert(0, tContainer);
                }
            }
        }

        private void OnActiveLineChanged(GameDataProperty property)
        {
            ActiveTextContainer.text = property.GetValue<string>();
        }
    }
}