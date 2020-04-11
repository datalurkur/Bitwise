using System;
using Bitwise.Interface;
using UnityEngine;

namespace Bitwise.Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public VirtualConsoleDisplay Console;

        public GameData Data { get; private set; }

        private InterfaceManager interfaceManager;
        private GameState rootState;

        protected void Awake()
        {
            Instance = this;
            Data = new GameData();
            interfaceManager = new InterfaceManager();
            rootState = new RootState();
            if (Console != null)
            {
                Console.OnUserInputUpdated += OnUserInputUpdated;
                Console.OnUserInputCommitted += OnUserInputReceived;
            }
        }

        protected void OnDestroy()
        {
            if (Console != null)
            {
                Console.OnUserInputUpdated -= OnUserInputUpdated;
                Console.OnUserInputCommitted -= OnUserInputReceived;
            }
        }

        protected void Update()
        {
            float deltaTime = Time.deltaTime;
            Data.VisualConsoleHistory.Update(deltaTime);
            rootState.Update(deltaTime);
        }

        private void OnUserInputReceived(string input)
        {
            Data.VisualConsoleHistory.AddLine(Console.FormatUserInputString(input));
            rootState.ProcessUserIntent(interfaceManager.ParseTextIntent(input));
        }

        private string OnUserInputUpdated(string input)
        {
            return interfaceManager.GetTextSuggestion(input);
        }
    }
}