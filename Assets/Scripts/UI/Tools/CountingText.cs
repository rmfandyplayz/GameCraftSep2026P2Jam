using TMPro;
using UnityEngine;

// AI GENERATED. author: claude (anthropic) opus 5.5
// Proxy that lets a UIAnimationPlayer Custom Property step count a TMP text between
// numbers chosen in code. The step tweens Progress 0 -> 1 (Absolute); code picks the numbers:
//
//     counter.CountTo(newValue);
//     player.Play("CountUp");
[RequireComponent(typeof(TMP_Text))]
public class CountingText : MonoBehaviour
{
    [Tooltip("string.Format pattern, {0} is the number. e.g. \"P: {0}\" or \"({0:+0;-0;0})\" for a signed value.")]
    public string Format = "{0}";

    [Tooltip("Numbers the tween runs between. Set by CountTo at runtime; edit here to test the Inspector preview.")]
    public int From, To;

    TMP_Text text;
    float progress = 1f;

    /// <summary>The number currently on screen (mid-tween values included).</summary>
    public int Displayed { get; private set; }

    /// <summary>Driven by the animation step, 0 -> 1.</summary>
    public float Progress
    {
        get => progress;
        set
        {
            progress = value;
            Show(Mathf.RoundToInt(Mathf.Lerp(From, To, value)));
        }
    }

    /// <summary>Counts from whatever is on screen now to target. Play the animation after calling this.</summary>
    public void CountTo(int target)
    {
        From = Displayed;
        To = target;
    }

    /// <summary>Jumps straight to value with no animation.</summary>
    public void SetImmediate(int value)
    {
        From = To = value;
        progress = 1f;
        Show(value);
    }

    void Show(int value)
    {
        Displayed = value;
        if (text == null) text = GetComponent<TMP_Text>();
        text.text = string.Format(Format, value);
    }
}
