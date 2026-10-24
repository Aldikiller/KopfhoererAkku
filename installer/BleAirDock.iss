; SPDX-License-Identifier: MIT
#ifndef AppVersion
  #define AppVersion "1.7.0"
#endif
#define ProjectRoot ".."

[Setup]
AppId={{F8C1DD68-4695-44CD-BBD3-10C24A653937}
AppName=BleAirDock
AppVersion={#AppVersion}
AppVerName=BleAirDock {#AppVersion}
AppPublisher=Aldikiller
VersionInfoVersion={#AppVersion}.0
VersionInfoCopyright=Copyright (c) 2026 Aldikiller
DefaultDirName={localappdata}\Programs\KopfhoererAkku
DefaultGroupName=BleAirDock
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.19041
OutputDir={#ProjectRoot}\dist
OutputBaseFilename=BleAirDock-{#AppVersion}-Setup
SetupIconFile={#ProjectRoot}\assets\app.ico
UninstallDisplayIcon={app}\BleAirDock.exe
LicenseFile={#ProjectRoot}\LICENSE
InfoBeforeFile={#ProjectRoot}\DISCLAIMER.de.md
WizardStyle=modern
WizardImageFile={#ProjectRoot}\assets\setup-banner.bmp
WizardSmallImageFile={#ProjectRoot}\assets\setup-small.bmp
Compression=lzma2
SolidCompression=yes
CloseApplications=yes
CloseApplicationsFilter=BleAirDock.exe,KopfhoererAkku.exe
RestartApplications=no
AppMutex=Local\KopfhoererAkku.Release.Tray
SetupMutex=Local\KopfhoererAkku.Release.Setup
UninstallDisplayName=BleAirDock
DisableProgramGroupPage=yes
UsePreviousTasks=yes
#ifdef SmokeTest
CreateUninstallRegKey=no
#endif

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "ukrainian"; MessagesFile: "compiler:Languages\Ukrainian.isl"

[CustomMessages]
german.Startup=BleAirDock bei der Windows-Anmeldung starten
english.Startup=Start BleAirDock when signing in to Windows
german.Desktop=Desktop-Verknüpfung erstellen
english.Desktop=Create desktop shortcut
german.Launch=BleAirDock starten
english.Launch=Start BleAirDock

german.NeedFramework=Bitte zuerst Microsoft .NET Framework 4.8 installieren. Das Setup lädt keine Komponenten automatisch herunter.
english.NeedFramework=Please install Microsoft .NET Framework 4.8 first. Setup does not download components automatically.
german.StopFailed=Die laufende Anwendung konnte nicht beendet werden. Bitte über das Symbol beenden.
english.StopFailed=Could not close the running app. Please exit it using its tray icon.
german.StillRunning=BleAirDock läuft noch. Bitte über das Symbol beenden und die Installation wiederholen.
english.StillRunning=BleAirDock is still running. Please exit it using its tray icon and retry setup.

spanish.Startup=Iniciar BleAirDock al entrar en Windows
spanish.Desktop=Crear acceso directo en el escritorio
spanish.Launch=Iniciar BleAirDock
spanish.NeedFramework=Instala primero Microsoft .NET Framework 4.8. No se descargan componentes automáticamente.
spanish.StopFailed=No se pudo cerrar la aplicación. Ciérrala desde su icono.
spanish.StillRunning=La aplicación sigue en ejecución. Ciérrala desde su icono y repite la instalación.

french.Startup=Démarrer BleAirDock à l’ouverture de session Windows
french.Desktop=Créer un raccourci sur le bureau
french.Launch=Démarrer BleAirDock
french.NeedFramework=Installez d’abord Microsoft .NET Framework 4.8. Aucun composant n’est téléchargé automatiquement.
french.StopFailed=Impossible de fermer l’application. Quittez-la depuis son icône.
french.StillRunning=L’application est encore ouverte. Quittez-la depuis son icône et recommencez.

ukrainian.Startup=Запускати BleAirDock під час входу у Windows
ukrainian.Desktop=Створити ярлик на робочому столі
ukrainian.Launch=Запустити BleAirDock
ukrainian.NeedFramework=Спочатку встановіть Microsoft .NET Framework 4.8. Компоненти автоматично не завантажуються.
ukrainian.StopFailed=Не вдалося закрити програму. Закрийте її через значок.
ukrainian.StillRunning=Програма ще працює. Закрийте її через значок і повторіть встановлення.

[Tasks]
#ifndef SmokeTest
Name: "autostart"; Description: "{cm:Startup}"; Flags: checkedonce
Name: "desktopicon"; Description: "{cm:Desktop}"; Flags: unchecked
#endif

[Files]
Source: "{#ProjectRoot}\dist\BleAirDock-{#AppVersion}-portable\BleAirDock.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\DISCLAIMER.de.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\PRIVACY.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\CHANGELOG.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\CONTRIBUTING.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\SECURITY.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\THIRD-PARTY.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#ProjectRoot}\docs\*"; DestDir: "{app}\docs"; Flags: ignoreversion recursesubdirs createallsubdirs

Source: "{#ProjectRoot}\assets\*"; DestDir: "{app}\assets"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
#ifndef SmokeTest
Name: "{group}\BleAirDock"; Filename: "{app}\BleAirDock.exe"; WorkingDir: "{app}"
Name: "{group}\BleAirDock deinstallieren"; Filename: "{uninstallexe}"
Name: "{autodesktop}\BleAirDock"; Filename: "{app}\BleAirDock.exe"; Tasks: desktopicon
Name: "{userstartup}\KopfhoererAkku"; Filename: "{app}\BleAirDock.exe"; WorkingDir: "{app}"; Tasks: autostart
#endif

[Run]
#ifndef SmokeTest
Filename: "{app}\BleAirDock.exe"; Description: "{cm:Launch}"; Flags: nowait postinstall skipifsilent
#endif

[UninstallRun]
Filename: "{app}\BleAirDock.exe"; Parameters: "--shutdown"; Flags: runhidden waituntilterminated; RunOnceId: "StopApp"

[InstallDelete]
Type: files; Name: "{app}\KopfhoererAkku.exe"
Type: files; Name: "{autodesktop}\Kopfhörer-Akku.lnk"
Type: files; Name: "{userprograms}\Kopfhörer-Akku\Kopfhörer-Akku.lnk"
Type: files; Name: "{userprograms}\Kopfhörer-Akku\Kopfhörer-Akku deinstallieren.lnk"
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
    MsgBox(CustomMessage('NeedFramework'), mbError, MB_OK);
#endif
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var Code: Integer; Exe: String;
begin
  Result := '';
  Exe := ExpandConstant('{app}\BleAirDock.exe');
  if not FileExists(Exe) then Exe := ExpandConstant('{app}\KopfhoererAkku.exe');
  if FileExists(Exe) then begin
    if not Exec(Exe, '--shutdown', '', SW_HIDE, ewWaitUntilTerminated, Code) then
      Result := CustomMessage('StopFailed')
    else if Code <> 0 then
      Result := CustomMessage('StillRunning');
  end;
end;



