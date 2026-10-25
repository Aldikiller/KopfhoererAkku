# 1.7.1 validation

Passed: self-tests including mixed classic/AEP Bluetooth connection reports; --maintenance-check (menu entries, About/Updates in Settings in five languages, connected/disconnected/unknown tray snapshots); --language-check; --pulse-check; --release165-check; same-executable restart parent/child handoff via --restart-probe.

No real headset connection or system audio output was changed by the tests. Reproduce the reported connected-headset/tray-dash case on hardware before considering it confirmed resolved. Both Windows queries happen within the regular scan; no new Bluetooth polling timer was added.
