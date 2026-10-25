# BleAirDock 1.8.0 validation

Passed: build self-tests, settings serialization for optional wheel/full-screen flags, legacy settings defaults, preset behavior preservation, multi-monitor full-screen rectangle geometry, 0–100% volume bounds and translations.

`--interaction-check` tests the normal widget wheel target routing and UI queue with injected audio functions (no actual system volume changes): disabled option, outside icons, two half-notches, down/up, wrong or missing assignment. It tests full-screen hide/restore, manual-hide precedence and visible tray recovery with an injected foreground probe. It captures compact menus and settings in all five languages and verifies drafts survive simple/advanced switching.

`--maintenance-check` and `--language-check` passed on the working build; final build is retested after layout adjustments. Documentation and media remain included.

Not yet verified on real hardware: Shell tray icon bounds and mouse-hook input on the user's desktop, actual audio volume changes on both headphones, full-screen games/videos and multi-monitor behavior. In this test desktop Shell_NotifyIconGetRect returned no icon rectangles; the widget routing passed. No claim of an end-to-end hardware pass.

No added Bluetooth polling. Foreground full-screen detection uses the existing one-second UI timer. Both options are off by default. Turning off wheel control removes its local hook.

## Maintainer confirmation

On 25 October 2026, the maintainer reported that all requested local tests passed and authorized publication. This is a user report, separate from the automated checks described above.
