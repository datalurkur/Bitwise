using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bitwise.Game;
using UnityEngine;

namespace Bitwise.Interface
{
    public class VirtualConsoleComplexLine : MonoBehaviour
    {
        public RectTransform LineSegmentPrefab;

        public RectTransform UITransform;

        private readonly List<VirtualConsoleComplexLineSegment> segments = new List<VirtualConsoleComplexLineSegment>();

        public void SetContent(List<TextBlock> texts)
        {
            for (var i = 0; i < texts.Count; ++i)
            {
                if (texts[i] == null)
                {
                    continue;
                }
                if (i >= segments.Count)
                {
                    RectTransform newSegmentRect = Instantiate(LineSegmentPrefab, this.transform);
                    newSegmentRect.SetAsLastSibling();
                    segments.Add(newSegmentRect.GetComponent<VirtualConsoleComplexLineSegment>());
                }

                VirtualConsoleComplexLineSegment segment = segments[i];
                segment.SetText(texts[i].Content);
                if (texts[i].BackgroundColor.HasValue)
                {
                    segment.SetBackgroundColor(texts[i].BackgroundColor.Value);
                }
                else
                {
                    segment.ClearBackgroundColor();
                }
            }

            for (var i = segments.Count - 1; i >= texts.Count; --i)
            {
                Destroy(segments[i].gameObject);
                segments.RemoveAt(i);
            }
        }
    }
}
