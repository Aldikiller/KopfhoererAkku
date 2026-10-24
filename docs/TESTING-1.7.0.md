# BleAirDock 1.7.0 validation

Passed locally: complete self-tests; all five language changes, settings persistence and restart; 1.6.5 feature UI checks; advanced settings UI; About window with supplied logo and clickable GitHub/website links. Original SVG copied byte-for-byte. Installer AppId, settings directory, single-instance mutex/event and startup shortcut filename intentionally retained. Portable users keep the Einstellungen folder.

The language UI test now expects the three existing simple tabs (General, warnings, updates), rather than the outdated expectation of two. No headphone/audio feature behavior changed for branding.

The previous real-device tests do not constitute a new two-PC test of this rebranding release. Check an upgrade on a second PC before treating that scenario as hardware-verified.
