using rmf_claude.DOTweenUI;
using UnityEngine;
using UnityEngine.Audio;

// AI GENERATED. author: claude (anthropic) opus 5.5
// Saved Master/Music/SFX volumes, applied to MainMixer. Values are linear 0-1 (what a slider shows),
// stored in PlayerPrefs and converted to dB for the mixer. Applied automatically when the game starts,
// so no scene needs a bootstrap object. Put a VolumeSlider on each slider to drive it from UI.
//
// Mixer contract (Assets/Audio/Mixers/Resources/MainMixer.mixer): groups Master > Music, SFX, with
// their Volume exposed as MasterVolume / MusicVolume / SFXVolume. Those exposed volumes are owned by
// this script at runtime - balance the mix with AudioSource volumes, not the group faders.
public enum VolumeChannel { Master, Music, SFX }

public static class AudioVolumeSettings
{
    const string MixerResource = "MainMixer";
    const float DefaultMaster = 0.6f;
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
        float db = linear <= 0.0001f ? MinDecibels : Mathf.Log10(linear) * 20f;
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
