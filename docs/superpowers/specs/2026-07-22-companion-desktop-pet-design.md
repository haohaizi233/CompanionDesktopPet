# Companion Desktop Pet — Design Specification

**Date:** 2026-07-22  
**Status:** Approved
**Target:** Windows 10/11 x64, single-file executable

## 1. Goal

Build a lightweight Windows desktop pet using the user-provided character image. The pet must launch by double-clicking a single `.exe`, display the character as a transparent cutout, remain interactive without a console window, and provide simple local companionship features.

The application is deliberately offline. It does not call an API, collect telemetry, or require an account.

## 2. Chosen Scope

The chosen product tier is the companionship version:

- transparent real-person cutout based on the supplied image;
- frameless, transparent, always-on-top desktop window;
- idle breathing, gentle sway, and small vertical floating animations;
- drag-to-move interaction;
- click reaction with a short bounce and speech bubble;
- a 75,375-line embedded persona corpus focused on computer-science and programmer life;
- state-driven scenes, local-time context, festival modifiers, semantic cooldowns, and micro-stories;
- quiet automatic pacing with intentional action-only and silent moments;
- context menu for common controls;
- persisted position, size, animation, and always-on-top preferences;
- self-contained, single-file Windows executable.

Not included in this version:

- AI or network chat;
- microphone, camera, or audio recording;
- automatic startup with Windows;
- Live2D skeletal animation or facial reenactment;
- cloud synchronization or telemetry.

## 3. Technology Decision

Use **C# with WPF on .NET 9**.

Reasons:

- WPF supports alpha-transparent, frameless windows and smooth transforms natively.
- The target computer already has the .NET 9 SDK and Windows Desktop runtime.
- A self-contained `win-x64` publish produces a double-clickable executable with no Python or Node installation requirement.
- Compared with PySide6/PyInstaller and Electron, WPF gives faster startup and lower packaging/runtime overhead for this Windows-only application.

## 4. Window and Visual Design

The main pet window uses:

- `WindowStyle=None` and `AllowsTransparency=True`;
- transparent background;
- no taskbar button;
- topmost enabled by default;
- DPI-aware sizing in WPF device-independent pixels;
- a tight visual footprint around the character so invisible window area does not obstruct normal desktop clicks.

Initial behavior:

- target character height: approximately 360 DIP;
- initial location: lower-right of the primary work area with a 24 DIP margin;
- minimum/maximum size presets: 75%, 100%, and 125%;
- saved position takes precedence when it remains visible on a current monitor;
- an invalid or off-screen saved position is clamped back into the nearest work area.

The overall styling is intentionally cute without altering the person's appearance. The speech bubble appears above or beside the character depending on available screen space. It uses a plump rounded shape, a warm cream-to-blush palette, dark cocoa text, a soft pink border, subtle shadow, and a small heart-like pointer. The bubble automatically disappears after approximately five seconds.

## 5. Character Asset Processing

The supplied source image is converted into a transparent PNG asset:

- remove only the scene background;
- preserve the character's face, hairstyle, clothing, pose, and photographic appearance;
- keep edges around hair and clothing softly feathered;
- crop excessive transparent margins;
- do not sexualize, change age appearance, or materially redesign the person;
- derive a simple application icon from the processed character asset.

The processed PNG and icon are embedded into the executable as WPF resources so the delivered application does not depend on loose asset files.

## 6. Interaction Model

### Dragging

Pressing and dragging the visible character moves the window. A small movement threshold distinguishes a drag from a click. Releasing after a drag saves the new position.

### Clicking

A normal left click:

1. plays a short, soft squash-and-bounce response with a tiny cheerful tilt;
2. chooses a local phrase that avoids immediate repetition;
3. displays the phrase in the speech bubble.

### Right-click menu

The character context menu contains:

- Say something;
- Pause/Resume animation;
- Size: Small / Normal / Large;
- Always on top (checked toggle);
- Restore default position;
- Exit.

The menu uses native WPF commands and keyboard-accessible labels.

## 7. Animation

Idle animation combines three low-amplitude transforms:

- breathing scale: roughly 0.985–1.015 over four seconds;
- sway rotation: roughly -1.2° to +1.2° over six seconds;
- vertical float: roughly -3 to +3 DIP over five seconds.

Durations differ so the motion does not look mechanically repetitive. Transforms use eased, repeating WPF storyboards and remain on the UI thread without blocking work. Pausing freezes the idle animation cleanly; the click response remains available.

## 8. Persona, Scene Engine, and Local Life Rhythm

The embedded UTF-8 corpus contains 75,375 materialized, unique lines across 20 categories. At least 46,800 are programmer-focused. The persona is named 佳怡: concise, slightly sharp-tongued, inward, caring, and occasionally soft. Catchphrases are seasoning rather than a prefix on every line. Three hundred low-frequency easter eggs include the name 雷琳玥 and the affectionate nicknames 小玥 and 玥玥.

The corpus forest is an indexing layer, not the global decision maker. Runtime output follows:

`local event → character state update → scene candidates → score/cooldown/budget → performance → memory/follow-up`

Character state tracks energy, sociability, boredom, mood, current activity, and days together. The scene catalog contains at least 250 scenes split among direct speech, self-talk, ambient observations, and action-only performances. Ten micro-story arcs each contain establishment, progress, and payoff nodes scheduled across hours or days.

Candidate scoring rewards event fit, current activity, local-time fit, story callbacks, novelty, and related-topic continuity. It penalizes semantic repetition, interruption cost, and direct-to-user speech. A semantic group cools down as a unit even when its wording differs.

The application reads only local date/time and its own interaction/history state. It does not read typed content, clipboard contents, filenames, or window-title text. Festivals include fixed Gregorian holidays, six Chinese lunar festivals with leap-month handling, the 256th-day programmer holiday, and 1024. A festival modifies the day's activity and scene candidates instead of acting only as a greeting label.

Automatic pacing is 20–50 minutes in normal periods, 45–90 minutes at dawn/late night, and reserves a 90–150 minute full-screen cadence for future passive-state integration. Two recent active scenes force a quiet period. Only one bubble is visible at a time, and it hides after approximately five seconds.

## 9. State and Reliability

Settings are stored at:

`%LOCALAPPDATA%\CompanionDesktopPet\settings.json`

Saved fields:

- window position;
- scale preset;
- animation paused state;
- always-on-top state.

Continuous role memory is stored separately at:

`%LOCALAPPDATA%\CompanionDesktopPet\agent-memory.json`

It records character state, recent scenes and semantic groups, current story nodes, next due times, recent lines, and days together. Both settings and memory use temporary-file replacement and malformed files fall back safely.

Writes use a temporary file followed by replacement so an interrupted write does not leave half-written JSON. Missing, malformed, or incompatible settings fall back to defaults without preventing startup.

A process-wide named mutex prevents accidental duplicate instances. Closing through the context menu stops timers and animations, saves settings, releases resources, and terminates the process.

## 10. Cute Motion Pack

The still PNG gains a small set of readable, low-amplitude actions without requiring a layered Live2D model:

- click affection: the existing squash response is accompanied by three rising, fading hearts;
- drag lean: horizontal window velocity tilts the character up to eight degrees while it is being moved;
- landing recovery: releasing a drag plays a short spring back to the neutral rotation and vertical position.

Idle breathing, one-shot reactions, and drag/landing motion use separate transforms so they can compose safely. One-shot actions replace only their own previous animation. Hearts are rendered inside the transparent stage and are removed or reset after each effect. Blink and greeting animations are intentionally excluded.

## 11. Packaging

Publish with a Release, self-contained, single-file `win-x64` configuration:

- no console subsystem/window;
- application icon embedded;
- WPF assemblies and character resources bundled;
- ReadyToRun disabled unless measurement proves it improves the final experience;
- single-file compression enabled to reduce delivery size.

Primary delivery artifact:

`outputs/CompanionDesktopPet/佳怡桌宠.exe`

A short `使用说明.txt` may accompany the executable for controls and troubleshooting, but the executable itself must not require adjacent assets or configuration files.

## 12. Error Handling

- Asset loading failure: show a concise native error dialog and exit instead of displaying an invisible process.
- Settings failure: continue with defaults and overwrite settings only after a successful interaction or clean exit.
- Monitor layout change: clamp the window into a visible work area.
- Unexpected UI exception: show a short error dialog, attempt a settings save, then exit cleanly.

## 13. Verification and Acceptance

Completion requires evidence for every item below:

1. `dotnet test` passes corpus, scene scoring, semantic cooldown, memory round-trip, temporal/festival, 30-day simulation, settings, placement, and animation tests.
2. Release publish completes and emits the requested single `.exe` artifact.
3. The executable launches by double-click without a console window.
4. The rendered window has a transparent background and shows the processed character correctly.
5. Idle animation visibly runs and can be paused/resumed.
6. Dragging moves the pet with lean/recovery; a click triggers hearts and a context-aware scene.
7. Every context-menu command performs its documented action.
8. Position, preferences, role state, micro-story progress, and recent semantic memory survive a clean restart.
9. A deliberately off-screen saved position is recovered to a visible monitor.
10. Exit removes the process completely, and a second launch cannot create a duplicate pet.
11. Click hearts, drag lean, and landing recovery are visibly distinguishable and never leave the character in a transformed state; no blink or greeting animation is present.
12. The embedded corpus contains exactly the materialized resource rows and the delivered directory requires no adjacent DLL.
13. A deterministic 30-day compressed simulation stays within the interruption budget and completes at least one micro-story.

Verification will include automated tests, build/publish output inspection, process/window checks, and a real GUI smoke test with screenshots.
