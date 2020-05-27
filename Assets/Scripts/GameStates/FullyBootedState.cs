using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bitwise.Game
{
    public class FullyBootedState : GameState
    {
        protected override void OnPush()
        {
            base.OnPush();
            gameData.SetPropertyValue(GameData.TabsVisible, true);
        }

        protected override Type OnPop()
        {
            base.OnPop();
            gameData.SetPropertyValue(GameData.TabsVisible, false);
            return null;
        }
    }
}
