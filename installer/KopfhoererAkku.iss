; SPDX-License-Identifier: MIT
#ifndef AppVersion
  #define AppVersion "1.1.0"
#endif
#define ProjectRoot ".."

[Setup]
AppId={{F8C1DD68-4695-44CD-BBD3-10C24A653937}
AppName=Kopfhörer-Akku
AppVersion={#AppVersion}
AppVerName=Kopfhörer-Akku {#AppVersion}
AppPublisher=Aldikiller
VersionInfoVersion=1.1.0.0
VersionInfoCopyright=Copyright (c) 2026 Aldikiller
DefaultDirName={localappdata}\Programs\KopfhoererAkku
DefaultGroupName=Kopfhörer-Akku
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.19041
OutputDir={#ProjectRoot}\dist
OutputBaseFilename=KopfhoererAkku-{#AppVersion}-Setup
SetupIconFile={#ProjectRoot}\assets\app.ico
UninstallDisplayIcon={app}\KopfhoererAkku.exe
LicenseFile={#ProjectRoot}\LICENSE
InfoBeforeFile={#ProjectRoot}\DISCLAIMER.de.md
WizardStyle=modern
Compression=lzma2
SolidCompression=yes
CloseApplications=yes
CloseApplicationsFilter=KopfhoererAkku.exe
RestartApplications=no
AppMutex=Local\KopfhoererAkku.Release.Tray
SetupMutex=Local\KopfhoererAkku.Release.Setup
UninstallDisplayName=Kopfhörer-Akku
DisableProgramGroupPage=yes
UsePreviousTasks=yes
#ifdef SmokeTest
CreateUninstallRegKey=no
#endif

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
#ifndef SmokeTest
Name: "autostart"; Description: "Kopfhörer-Akku bei der Windows-Anmeldung starten"; Flags: checkedonce
Name: "desktopicon"; Description: "Desktop-Verknüpfung erstellen"; Flags: unchecked
#endif

[Files]
Source: "{#ProjectRoot}\dist\KopfhoererAkku-{#AppVersion}-portable\KopfhoererAkku.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\DISCLAIMER.de.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\PRIVACY.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\CHANGELOG.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\CONTRIBUTING.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\SECURITY.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\THIRD-PARTY.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\docs\*"; DestDir: "{app}\docs"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
#ifndef SmokeTest
Name: "{group}\Kopfhörer-Akku"; Filename: "{app}\KopfhoererAkku.exe"; WorkingDir: "{app}"
Name: "{group}\Kopfhörer-Akku deinstallieren"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Kopfhörer-Akku"; Filename: "{app}\KopfhoererAkku.exe"; Tasks: desktopicon
Name: "{userstartup}\KopfhoererAkku"; Filename: "{app}\KopfhoererAkku.exe"; WorkingDir: "{app}"; Tasks: autostart
#endif

[Run]
#ifndef SmokeTest
Filename: "{app}\KopfhoererAkku.exe"; Description: "Kopfhörer-Akku starten"; Flags: nowait postinstall skipifsilent
#endif

[UninstallRun]
Filename: "{app}\KopfhoererAkku.exe"; Parameters: "--shutdown"; Flags: runhidden waituntilterminated; RunOnceId: "StopApp"

[InstallDelete]
#ifndef SmokeTest
Type: files; Name: "{userstartup}\KopfhoererAkku.lnk"; Tasks: not autostart
#endif

[UninstallDelete]
#ifndef SmokeTest
Type: files; Name: "{userstartup}\KopfhoererAkku.lnk"
#endif

[Code]
function IsDotNetAvailable(): Boolean;
var Release: Cardinal;
begin
  Result := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release) and (Release >= 528040);
end;

function InitializeSetup(): Boolean;
begin
#ifdef SmokeTest
  Result := True;
#else
  Result := IsDotNetAvailable();
  if not Result then
    MsgBox('Bitte zuerst Microsoft .NET Framework 4.8 installieren. Das Setup lädt keine Komponenten automatisch herunter.', mbError, MB_OK);
#endif
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var Code: Integer; Exe: String;
begin
  Result := '';
  Exe := ExpandConstant('{app}\KopfhoererAkku.exe');
  if FileExists(Exe) then begin
    if not Exec(Exe, '--shutdown', '', SW_HIDE, ewWaitUntilTerminated, Code) then
      Result := 'Die laufende Anwendung konnte nicht beendet werden. Bitte über das Symbol beenden.'
    else if Code <> 0 then
      Result := 'Kopfhörer-Akku läuft noch. Bitte über das Symbol beenden und die Installation wiederholen.';
  end;
end;
