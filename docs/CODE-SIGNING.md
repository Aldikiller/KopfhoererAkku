# Code signing policy

## Current status

We are preparing an application for free open-source code signing through SignPath Foundation. Approval has not been granted and no SignPath signing service is active for this project yet. All currently published 0.1.0-beta.1 binaries are unsigned.

If the project is accepted, code signing would be provided by SignPath.io with a certificate issued to SignPath Foundation. We will state this explicitly on the download page when signed releases are available.

## Project roles

- Author and maintainer: [Aldikiller](https://github.com/Aldikiller)
- Reviewer of external contributions: Aldikiller
- Intended release and signing approver: Aldikiller

Only artifacts built from this public repository through the reviewed Windows build workflow are intended for signing. Each signing request will require explicit maintainer approval. Project signing credentials will not be stored in the source tree. Multi-factor authentication for the source repository and signing account is required before signing is enabled.

## Privacy

The application does not send telemetry. The optional update check queries the public GitHub releases API and is disabled by default. This sends the network IP address and application version to GitHub, but no device names, identifiers or battery data. See [PRIVACY.md](../PRIVACY.md). Build and signing services process release artifacts separately from the installed application.

## Downloads

[Releases](https://github.com/Aldikiller/KopfhoererAkku/releases)

This policy describes the intended process. It is not a claim that the project has been accepted by SignPath or that the existing downloads are signed.
