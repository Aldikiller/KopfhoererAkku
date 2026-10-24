# Publishing BleAirDock

The public repository stays at https://github.com/Aldikiller/KopfhoererAkku for compatibility with update checks in existing releases. The product is BleAirDock.

1. Update VERSION, assembly metadata, installer fallback version and all five language pages/readmes.
2. Run build.ps1 -Installer -Iscc <official Inno Setup compiler>. Review self-tests and UI checks.
3. Check the original logo in assets/logo.svg, derived icon assets and the About window.
4. Commit the reviewed source and push main. After validation, tag v1.7.0 and push the tag.
5. The tag workflow builds/tests on Windows, then publishes installer, portable ZIP, source ZIP and SHA256SUMS.txt with docs/RELEASE-NOTES.md.

Keep installer AppId, legacy settings directory, mutex/event names and startup shortcut filename stable. Do not upload work folders, private device settings, credentials or diagnostic logs. Releases remain unsigned. MIT permits commercial redistribution even though the project itself is freeware.
