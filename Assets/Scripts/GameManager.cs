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

        private GameState rootState;

        protected void Awake()
        {
            Instance = this;
            Data = new GameData();
            rootState = new RootState();
            if (Console != null)
            {
                Console.OnUserInputReceived += OnUserInputReceived;
            }
        }

        protected void OnDestroy()
        {
            if (Console != null)
            {
                Console.OnUserInputReceived -= OnUserInputReceived;
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
            Data.VisualConsoleHistory.AddText(Console.FormatUserInputString(input));
            rootState.ProcessUserIntent(InterfaceManager.ParseTextIntent(input));
        }
    }
}