using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bitwise.Interface
{
    public class VirtualConsoleComplexLineSegment : MonoBehaviour
    {
        public Image Background;
        public TMP_Text Text;

        public void SetBackgroundColor(Color color)
        {
            Background.enabled = true;
            Background.color = color;
        }

        public void ClearBackgroundColor()
        {
            Background.enabled = false;
        }

        public void SetText(string text)
        {
            Text.text = text;
        }
    }
}
