using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bitwise.Game;
using UnityEngine;

namespace Bitwise.Interface
{
    public class InterfaceManager
    {
        public const float TimeDilator = 1f;

        public const float ShortDelayTime = 0.25f;
        public const float NormalDelayTime = 0.5f;
        public const float LongDelayTime = 1f;

        private readonly Dictionary<string, TextIntent> textIntentCache = new Dictionary<string, TextIntent>();
        private readonly TextIntent unknownTextIntent = new TextIntent(UserIntent.IntentType.Unknown);

        public InterfaceManager()
        {
            AddIntentCommand("help", UserIntent.IntentType.Query, IntentTarget.Commands);
            AddIntentCommand("quit", UserIntent.IntentType.Quit);
            AddIntentCommand("yes", UserIntent.IntentType.Confirm);
            AddIntentCommand("no", UserIntent.IntentType.Cancel);
            AddIntentCommand("debug", UserIntent.IntentType.Debug);
            AddIntentCommand("diag", UserIntent.IntentType.Diag);
            AddIntentCommand("reboot", UserIntent.IntentType.Reboot);
        }

        public string GetTextSuggestion(string text, GameState state)
        {
            if (string.IsNullOrEmpty(text)) { return null; }
            string userInput = text.Trim().ToLower();
            int firstWhitespace = userInput.IndexOf(' ');
            string command = (firstWhitespace == -1) ? userInput : userInput.Substring(0, firstWhitespace);

            List<string> supportedCommands = new List<string>();
            state.GetSupportedCommands(ref supportedCommands);
            return textIntentCache.FirstOrDefault(pair => pair.Key.StartsWith(command) && (supportedCommands?.Contains(pair.Key) ?? true)).Key;
        }

        public TextIntent ParseTextIntent(string text)
        {
            string userInput = text.Trim().ToLower();
            int firstWhitespace = userInput.IndexOf(' ');
            string command = (firstWhitespace == -1) ? userInput : userInput.Substring(0, firstWhitespace);
            if (textIntentCache.TryGetValue(command, out TextIntent intent))
            {
                return intent;
            }
            return unknownTextIntent;
        }

        private void AddIntentCommand(string command, UserIntent.IntentType intent, IntentTarget target = IntentTarget.None)
        {
            TextIntent textIntent = new TextIntent(intent, target);
            textIntentCache[command] = textIntent;
        }
    }
}