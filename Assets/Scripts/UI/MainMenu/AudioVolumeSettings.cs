using rmf_claude.DOTweenUI;
using UnityEngine;
using UnityEngine.Audio;

// AI GENERATED. author: claude (anthropic) opus 5.5
// Saved Master/Music/SFX volumes, applied to MainMixer. Values are 0-1 slider positions, stored in
// PlayerPrefs and converted to dB on a perceptual (squared) curve for the mixer. Applied automatically when the game starts,
// so no scene needs a bootstrap object. Put a VolumeSlider on each slider to drive it from UI.
//
// Mixer contract (Assets/Audio/Mixers/Resources/MainMixer.mixer): groups Master > Music, SFX, with
// their Volume exposed as MasterVolume / MusicVolume / SFXVolume. Those exposed volumes are owned by
// this script at runtime - balance the mix with AudioSource volumes, not the group faders.
public enum VolumeChannel { Master, Music, SFX }

public static class AudioVolumeSettings
{
    const string MixerResource = "MainMixer";
    const float DefaultMaster = 0.8f; // ~ -3.9 dB on the squared curve
    const float MinDecibels = -80f;

    static AudioMixer mixer;

    public static AudioMixer Mixer => mixer != null ? mixer : (mixer = Resources.Load<AudioMixer>(MixerResource));

    public static float Get(VolumeChannel channel) =>
        PlayerPrefs.GetFloat(Key(channel), channel == VolumeChannel.Master ? DefaultMaster : 1f);

    /// Sets and applies a linear 0-1 volume. Call Save() once the change is final (not every drag frame).
    public static void Set(VolumeChannel channel, float linear)
    {
        linear = Mathf.Clamp01(linear);
        PlayerPrefs.SetFloat(Key(channel), linear);
        Apply(channel, linear);
    }

    // Unity only writes PlayerPrefs on a clean quit; WebGL tab closes and crashes skip that.
    public static void Save() => PlayerPrefs.Save();

    public static void ApplyAll()
    {
        foreach (VolumeChannel channel in System.Enum.GetValues(typeof(VolumeChannel)))
            Apply(channel, Get(channel));
    }

    static void Apply(VolumeChannel channel, float linear)
    {
        if (Mixer == null)
        {
            Debug.LogWarning($"AudioVolumeSettings: no AudioMixer named '{MixerResource}' in a Resources folder.");
            return;
        }
        // Squared curve (gain = v^2, so 40*log10 rather than 20*log10) to roughly follow perceived
        // loudness: 50% -> -12 dB, near "half as loud". Plain 20*log10 feels loud across most of the range.
        float db = linear <= 0.0001f ? MinDecibels : Mathf.Max(MinDecibels, Mathf.Log10(linear) * 40f);
        Mixer.SetFloat(Param(channel), db);
    }

    static string Param(VolumeChannel channel) => channel + "Volume";
    static string Key(VolumeChannel channel) => "Volume." + channel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static async void Init()
    {
        // UI Animation Player's shared click/hover source -> SFX, so the SFX slider covers UI sounds.
        var sfx = Mixer != null ? System.Array.Find(Mixer.FindMatchingGroups("SFX"), g => g.name == "SFX") : null;
        if (sfx != null) UIAnimationAudio.Shared.outputAudioMixerGroup = sfx;

        ApplyAll();
        // SetFloat during the startup frame can be overwritten by the mixer's snapshot initialising;
        // applying again a frame later makes it stick.
        await Awaitable.NextFrameAsync();
        ApplyAll();
    }
}
