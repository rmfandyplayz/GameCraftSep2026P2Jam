# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Game jam entry (GameCraft, Sep 2026). Unity **6000.3.25f1**, 2D URP, new Input System, uGUI + TextMeshPro, DOTween Pro (`Assets/Plugins/Demigiant`). Multiple contributors each own an area: Andy (rmfandyplayz) — UI; others work in their own playground scenes (`JacksonPlayground.unity`, `TylerPlayground.unity`). Edit another person's scene or prefab only when asked — Unity YAML merges badly.

Core loop: during the **day** the player plants seeds in `Soil`; at **night** each `seed` turns into an enemy (`PumpkinEnemy`) and the player fights with the same Interact button. Money must meet a quota every N days or the player dies (`GameManager.EndNight`).

## Commands

There is no CLI build/test pipeline; work happens in the Unity Editor. No test assemblies exist yet. If headless compilation/tests are needed, the Editor must not have the project open:

```bash
"<UnityEditorPath>/Unity.exe" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults results.xml
```

Only `Assets/Scenes/SampleScene.unity` is in Build Settings.

## Architecture

- **`GameManager`** (`Assets/Scripts/GameManager.cs`) — `DontDestroyOnLoad` singleton (`GameManager.Instance`). Owns money/quota/days/`nightTime` and exposes `Action` fields for phase events (`OnDayBegin`, `OnNightBegin`, `OnNightEnd`, `OnPlayerDie`, …). Gameplay objects subscribe in `OnEnable`/`OnDisable`. `nightTime` is what switches the Interact input between planting (`Player.InteractPressed`, consumed by `Soil`) and attacking (`Player.TryAttack`).
- **UI boundary** — gameplay code talks to UI only through **`UI_API`** (`Assets/Scripts/UI/UI_API.cs`): public methods (`AdvanceTime`, `SetTempPoints`, `CombinePoints`) call down into controllers under `UI/Game/`; static events (`RequestStart/Pause/Resume`) flow the other way for gameplay to handle. Keep new UI hooks behind this API rather than having gameplay reference UI controllers directly.
- **DOTween UI Animation Player** (`Assets/Scripts/UI/Tools/DOTweenAnimationPlayer/`, namespace `rmf_claude.DOTweenUI`) — self-contained, inspector-authored named animations played via `UIAnimationPlayer.Play("Name")`. Its own `README.md` is the reference. Two hard rules from it: don't add an `.asmdef` to that folder (DOTween UI modules live in `Assembly-CSharp-firstpass`), and DOTween's generated modules must exist (Tools → Demigiant → DOTween Utility Panel → Setup) or it won't compile.
- **Input** — `Assets/InputSystem_Actions.inputactions` (with generated `InputSystem_Actions.cs`); scripts take `InputActionReference` fields assigned in the Inspector rather than instantiating the generated class.
- `Assets/TutorialInfo/` and `Readme.asset` are URP-template leftovers.

## Conventions

- Always commit `.meta` files alongside their assets; never hand-edit GUIDs.
- Inspector-facing fields use `[SerializeField]` + `[Header(...)]` grouping.
- Andy's files carry an `// author:` header; AI-authored files carry an "AI-GENERATED" header — keep that when editing them.

## Living notes — keep this section current

Update this section whenever you learn something non-obvious while working here: an approach that worked, one that failed and why, an Editor/package gotcha, or a gap that was fixed (delete stale entries rather than letting them accumulate). Keep entries to one or two lines.

### Known gaps (as of 2026-09-26)
- Nothing invokes `OnGameStart/OnDayBegin/OnDayEnd/OnNightBegin/OnNightEnd` yet; day/night transitions aren't driven by anything.
- `GameManager.OnEnable` subscribes `BeginNight` but `OnDisable` unsubscribes `EndNight`; `EndNight` is never subscribed to `OnNightEnd`.
- `seed.OnEnable` dereferences `GameManager.Instance` without a null check (fails if the seed enables before `GameManager.Awake`). `PumpkinEnemy` does check.
- `UI_API.StartGame/PauseGame/ResumeGame` call `.Invoke()` without `?.` — throws with no subscribers.
- `PointsController.SetTempPoints` shows `(+n)` for decreases too; `DayLightCycleController` plays placeholder animation name `"TODO: CHANGE LATER"`.
- `Player` loads scene `deathScene` on death, which doesn't exist / isn't in Build Settings.
- Nothing calls `UI_API.SetQuota(quota, daysLeft)` yet; the quota line shows `Req: ??? in ? days` until it does.

### What worked / what didn't
- Scene/prefab edits while the Editor is open: drive it live with `unity command eval_file --file x.cs` (Pipeline package is installed) instead of hand-editing YAML. `eval` rejects `--caller/--skill` flags. `eval_file` wraps the file in a method body — no `using` lines, fully qualify types.
- `UI.unity`'s `UI (Canvas)` instance overrides layout values (e.g. `QuotaController` HLG spacing/padding), so prefab edits to those can look like they did nothing — check the instance's overrides.
- Fit-to-text UI needs no script: layout group with Control Child Size (no force expand) + TMP margins; for a background behind a group, Content Size Fitter on the group (point anchors, not stretch) + stretched `Image` with Ignore Layout (see `PointsController`).
- Audio: route new AudioSources to `MainMixer` (`Assets/Audio/Mixers/Resources/`) Music/SFX groups. `AudioVolumeSettings` owns the exposed `*Volume` params at runtime (PlayerPrefs, squared/perceptual curve, Master default 0.8) — balance the mix via AudioSource volume, not group faders. Settings sliders just need a `VolumeSlider` component.
- UIAnimationPlayer steps can't take runtime endpoints. For code-determined values, tween a 0→1 `Progress` property on a proxy component whose From/To are set by code before `Play` (see `CountingText`).
