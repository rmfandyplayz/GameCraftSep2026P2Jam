using TMPro;
using UnityEngine;

/// <summary>
/// Animates a world-space canvas popup that contains a TMP text child.
/// </summary>
public class EnemyValuePopup : MonoBehaviour
{
    TMP_Text popupText;
    CanvasGroup canvasGroup;
    Vector3 startingPosition;
    Vector3 finalScale;
    float elapsedTime;
    float riseAndGrowDuration;
    float stationaryDuration;
    float fadeOutDuration;
    float riseDistance;
    float startingScaleMultiplier;

    public void Play(
        int value,
        Color textColor,
        float riseDuration,
        float holdDuration,
        float fadeDuration,
        float popupRiseDistance,
        float popupStartingScale)
    {
        popupText = GetComponentInChildren<TMP_Text>(true);
        if (popupText == null)
        {
            Debug.LogWarning("Enemy value popup needs a TMP text child.");
            Destroy(gameObject);
            return;
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        popupText.text = $"+{value}";
        popupText.color = textColor;
        startingPosition = transform.position;
        finalScale = transform.localScale;
        riseAndGrowDuration = Mathf.Max(0.01f, riseDuration);
        stationaryDuration = Mathf.Max(0f, holdDuration);
        fadeOutDuration = Mathf.Max(0.01f, fadeDuration);
        riseDistance = Mathf.Max(0f, popupRiseDistance);
        startingScaleMultiplier = Mathf.Max(0.01f, popupStartingScale);
        transform.localScale = finalScale * startingScaleMultiplier;
        canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        if (popupText == null)
            return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime < riseAndGrowDuration)
        {
            float progress = elapsedTime / riseAndGrowDuration;
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

            transform.position = startingPosition + Vector3.up * (riseDistance * easedProgress);
            transform.localScale = Vector3.Lerp(finalScale * startingScaleMultiplier, finalScale, easedProgress);
            canvasGroup.alpha = progress;
            return;
        }

        if (elapsedTime < riseAndGrowDuration + stationaryDuration)
        {
            transform.position = startingPosition + Vector3.up * riseDistance;
            transform.localScale = finalScale;
            canvasGroup.alpha = 1f;
            return;
        }

        float fadeProgress = (elapsedTime - riseAndGrowDuration - stationaryDuration) / fadeOutDuration;
        canvasGroup.alpha = 1f - Mathf.Clamp01(fadeProgress);

        if (fadeProgress >= 1f)
            Destroy(gameObject);
    }
}
