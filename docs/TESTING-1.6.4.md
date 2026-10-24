# Local validation: 1.6.4

Build and self-tests passed. Covered settings persistence and validation for per-device warning overrides, pulse period/intensity/steady glow, direct shortcut settings, Tiny tooltip stale-value handling, screen-edge snapping (including negative monitor coordinates), and correcting off-screen positions. Five-language warning and hotkey dialogs were rendered. General settings and pulse controls were visually inspected.

Global Windows hotkey registration cannot be verified in the restricted execution environment. Please test switching, each direct shortcut, conflicts with another application, drag/click behavior, monitor disconnect/reconnect, notification pause/resume, and real low-battery behavior on the normal Windows desktop before publication.

Existing settings are preserved. Export/import is intentionally not included. No extra Bluetooth polling is introduced by animations.
