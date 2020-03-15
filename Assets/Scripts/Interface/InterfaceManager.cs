using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bitwise.Interface
{
    public static class InterfaceManager
    {
        public static TextIntent ParseTextIntent(string text)
        {
            string userInput = text.Trim().ToLower();
            int firstWhitespace = userInput.IndexOf(' ');
            string command = (firstWhitespace == -1) ? userInput : userInput.Substring(0, firstWhitespace);
            switch (command)
            {
                case "help":
                    return new TextIntent(UserIntent.IntentType.Query, IntentTarget.Commands);
                case "quit":
                    return new TextIntent(UserIntent.IntentType.Quit);
                case "y":
                case"yes":
                    return new TextIntent(UserIntent.IntentType.Confirm);
                case "n":
                case "no":
                    return new TextIntent(UserIntent.IntentType.Cancel);
                case "debugrandom":
                    return new TextIntent(UserIntent.IntentType.DebugRandom);
                default:
                    return new TextIntent(UserIntent.IntentType.Unknown);
            }
        }
    }
}