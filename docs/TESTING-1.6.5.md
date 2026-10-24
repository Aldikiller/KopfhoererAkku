# 1.6.5 validation

Passed: complete self-tests, separate connection/battery/error statuses, image/volume setting persistence, transparent demo PNG generation, image rendering, temporary widget hide/restore and tray accessibility, all five localized volume dialogs, appearance layout.

Manual checks still required on real headphones: opt-in volume memory when switching away and back, same-output activation, disconnected outputs, global shortcuts and audio transitions. Native volume control uses the Windows IAudioEndpointVolume scalar API. No automatic mute/unmute or microphone change.
