using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// AI GENERATED. author: claude (anthropic) opus 5.5
// Layout groups can read child scale (Use Child Scale Width/Height), but changing a scale never
// marks the layout dirty, so a scale tween leaves the layout - and anything sized from it, like a
// Content Size Fitter background - stale. Put this on the layout group's GameObject; it queues a
// rebuild on any frame a direct child's scale changed.
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class RebuildLayoutOnChildScale : MonoBehaviour
{
    RectTransform rect;
    readonly List<Vector3> lastScales = new List<Vector3>();

    private void OnEnable()
    {
        rect = (RectTransform)transform;
        lastScales.Clear();
    }

    // DOTween updates in Update, so by LateUpdate this frame's scales are final, and the rebuild
    // still lands before the canvas renders.
    private void LateUpdate()
    {
        bool changed = lastScales.Count != rect.childCount;
        if (changed) lastScales.Clear();

        for (int i = 0; i < rect.childCount; i++)
        {
            Vector3 scale = rect.GetChild(i).localScale;
            if (i >= lastScales.Count) lastScales.Add(scale);
            else if (scale != lastScales[i])
            {
                lastScales[i] = scale;
                changed = true;
            }
        }

        if (changed) LayoutRebuilder.MarkLayoutForRebuild(rect);
    }
}
