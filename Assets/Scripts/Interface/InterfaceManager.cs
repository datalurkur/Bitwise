using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bitwise.Interface
{
    public class InterfaceManager
    {
        private readonly Dictionary<string, TextIntent> textIntentCache = new Dictionary<string, TextIntent>();
        private readonly TextIntent unknownTextIntent = new TextIntent(UserIntent.IntentType.Unknown);

        public InterfaceManager()
        {
            textIntentCache["help"] = new TextIntent(UserIntent.IntentType.Query, IntentTarget.Commands);
            textIntentCache["quit"] = new TextIntent(UserIntent.IntentType.Quit);
            textIntentCache["yes"] = new TextIntent(UserIntent.IntentType.Confirm);
            textIntentCache["no"] = new TextIntent(UserIntent.IntentType.Cancel);
            textIntentCache["debug"] = new TextIntent(UserIntent.IntentType.Debug);
        }

        public string GetTextSuggestion(string text)
        {
            if (string.IsNullOrEmpty(text)) { return null; }
            string userInput = text.Trim().ToLower();
            int firstWhitespace = userInput.IndexOf(' ');
            string command = (firstWhitespace == -1) ? userInput : userInput.Substring(0, firstWhitespace);
            return textIntentCache.Keys.FirstOrDefault(key => key.StartsWith(command));
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
    }
}