using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// AI GENERATED. author: claude (anthropic) opus 5.5
// Drop on any Slider and pick a channel. It shows the saved volume whenever it is enabled and
// writes changes back through AudioVolumeSettings. Works with any slider range (uses normalizedValue).
[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour, IPointerUpHandler
{
    [Header("Volume")]
    [SerializeField] private VolumeChannel channel;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        // Refresh on every enable so a menu reopened in another scene shows the current value.
        slider.SetValueWithoutNotify(Mathf.Lerp(slider.minValue, slider.maxValue, AudioVolumeSettings.Get(channel)));
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(OnValueChanged);
        AudioVolumeSettings.Save();
    }

    private void OnValueChanged(float _)
    {
        AudioVolumeSettings.Set(channel, slider.normalizedValue);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        AudioVolumeSettings.Save();
    }
}
