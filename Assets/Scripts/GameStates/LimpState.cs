using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bitwise.Game
{
    public class LimpState : GameState
    {
        public LimpState(GameState parent) : base(parent) { }

        protected override void OnPush()
        {
            int cpuCores = gameData.GetPropertyValue<int>(GameData.CpuCores);
            gameData.GetObjective(GameData.FullyBooted).Complete.OnPropertyChanged += PropertyChanged;
            gameData.VisualConsoleHistory.AddText("SepiaSoft OS v1.5.1s");
            gameData.VisualConsoleHistory.AddTextWithDotDelay("CPU Check", $"{cpuCores} core{(cpuCores > 1 ? "s" : "")} at {gameData.GetPropertyValue<int>(GameData.CpuSpeed)} MHz [<color=#00FF00>OK</color>]", 5, 0.2f);
            gameData.VisualConsoleHistory.AddTextWithDotDelay("RAM Check", $"{gameData.GetPropertyValue<int>(GameData.MemoryCapacity)} MB at {gameData.GetPropertyValue<int>(GameData.MemorySpeed)} MHz [<color=#00FF00>OK</color>]", 5, 0.2f);
        }

        protected override void OnPop()
        {
            gameData.GetObjective(GameData.FullyBooted).Complete.OnPropertyChanged -= PropertyChanged;
        }

        private void PropertyChanged(GameDataProperty property)
        {
            if (property.GetValue<bool>())
            {
                Finish();
            }
        }
    }
}

