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

Only `Assets/Scenes/GamePatched.unity` (title menu + game, Jackson's) is in Build Settings; `Game.unity` is the older copy. Quotas per 4-day cycle come from `GameManager.quotaPerCycle`, set on that scene's instance.

## Architecture

- **`GameManager`** (`Assets/Scripts/GameManager.cs`) — per-scene singleton (`GameManager.Instance`, not `DontDestroyOnLoad`); returning to the menu or failing the quota reloads scene 0, which resets everything. Owns money/quota/days/`nightTime` and exposes `Action` fields for phase events (`OnDayBegin`, `OnNightBegin`, `OnNightEnd`, `OnPlayerDie`, …). Gameplay objects subscribe in `OnEnable`/`OnDisable`. `nightTime` is what switches the Interact input between planting (`Player.InteractPressed`, consumed by `Soil`) and attacking (`Player.TryAttack`).
- **UI boundary** — gameplay code talks to UI only through **`UI_API`** (`Assets/Scripts/UI/UI_API.cs`): public methods (`AdvanceTime`, `SetTempPoints`, `CombinePoints`) call down into controllers under `UI/Game/`; static events (`RequestStart/Pause/Resume/ReturnToMenu`) flow the other way for gameplay to handle. Keep new UI hooks behind this API rather than having gameplay reference UI controllers directly.
- **DOTween UI Animation Player** — git UPM package `com.rmfandyplayz.dotween-ui-animation` (source in `Library/PackageCache/`, namespace `rmf_claude.DOTweenUI`); inspector-authored named animations played via `UIAnimationPlayer.Play("Name")`. Its own `README.md` is the reference. DOTween's generated modules must exist (Tools → Demigiant → DOTween Utility Panel → Setup) or it won't compile.
- **Input** — `Assets/InputSystem_Actions.inputactions` (with generated `InputSystem_Actions.cs`); scripts take `InputActionReference` fields assigned in the Inspector rather than instantiating the generated class.
- `Assets/TutorialInfo/` and `Readme.asset` are URP-template leftovers.

## Conventions

- Always commit `.meta` files alongside their assets; never hand-edit GUIDs.
- Inspector-facing fields use `[SerializeField]` + `[Header(...)]` grouping.
- Andy's files carry an `// author:` header; AI-authored files carry an "AI-GENERATED" header — keep that when editing them.

## Living notes — keep this section current

Update this section whenever you learn something non-obvious while working here: an approach that worked, one that failed and why, an Editor/package gotcha, or a gap that was fixed (delete stale entries rather than letting them accumulate). Keep entries to one or two lines.

### Known gaps (as of 2026-10-03)
- `seed.OnEnable` dereferences `GameManager.Instance` without a null check (fails if the seed enables before `GameManager.Awake`). `PumpkinEnemy` does check.

### What worked / what didn't
- Scene/prefab edits while the Editor is open: drive it live with `unity command eval_file --file x.cs` (Pipeline package is installed) instead of hand-editing YAML. `eval` rejects `--caller/--skill` flags. `eval_file` wraps the file in a method body — no `using` lines, fully qualify types.
- `UI.unity`'s `UI (Canvas)` instance overrides layout values (e.g. `QuotaController` HLG spacing/padding), so prefab edits to those can look like they did nothing — check the instance's overrides.
- Fit-to-text UI needs no script: layout group with Control Child Size (no force expand) + TMP margins; for a background behind a group, Content Size Fitter on the group (point anchors, not stretch) + stretched `Image` with Ignore Layout (see `PointsController`).
- 2D lights need the 2D renderer: only the PC quality level's `PC_RPAsset` uses `Renderer2D` (`Mobile_RPAsset` uses the 3D Universal renderer), so every platform must default to PC — WebGL defaulting to Mobile is why lighting broke on web.
- Audio: route new AudioSources to `MainMixer` (`Assets/Audio/Mixers/Resources/`) Music/SFX groups. `AudioVolumeSettings` owns the exposed `*Volume` params at runtime (PlayerPrefs, squared/perceptual curve, Master default 0.8) — balance the mix via AudioSource volume, not group faders. Settings sliders just need a `VolumeSlider` component.
- Don't letterbox with `Camera.rect`: URP (2D renderer, post-processing on) ignores the viewport offset, so the image slides down one bar height. `AspectRatioLock` uses a full-screen off-center ortho projection instead.
- Play-mode testing via eval: the game HUD (`PointsController` etc.) is inactive until the title's `PlayButton.onClick` runs its intro (~10s until the quota line shows), so press that rather than calling `UI_API.StartGame` directly. Quota collection runs `GameManager.quotaEnforceDelay` (1.5s) after the new day so the profit-combine animation finishes first.
- UIAnimationPlayer steps can't take runtime endpoints. For code-determined values, tween a 0→1 `Progress` property on a proxy component whose From/To are set by code before `Play` (see `CountingText`).
