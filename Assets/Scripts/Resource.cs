using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bitwise.Game
{
    [Serializable]
    public class Resource : GameDataProperty<float>
    {
        public int StoragePropertyIndex;

        public Resource(int index, int storagePropertyIndex, string name, float defaultValue) : base(index, name, defaultValue)
        {
            StoragePropertyIndex = storagePropertyIndex;
        }
    }
}
