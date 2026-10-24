param([switch]$Installer, [string]$Iscc, [switch]$SkipTests)
$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$version = (Get-Content -LiteralPath (Join-Path $projectRoot 'VERSION') -Raw).Trim()
$dist = Join-Path $projectRoot 'dist'
$portable = Join-Path $dist "BleAirDock-$version-portable"
$framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$csc = Join-Path $framework 'csc.exe'
if (!(Test-Path -LiteralPath $csc)) { throw 'Der .NET-Framework-C#-Compiler fehlt. Windows x64 mit .NET Framework 4.8 wird benötigt.' }
New-Item -ItemType Directory -Force -Path $dist, $portable | Out-Null
$exe = Join-Path $portable 'BleAirDock.exe'
$compilerArgs = @('/nologo','/target:winexe','/platform:x64','/optimize+',('/out:'+$exe),
  ('/resource:'+(Join-Path $projectRoot 'docs\logo.png')+',KopfhoererAkku.Logo.png'),
  '/reference:System.Web.Extensions.dll','/reference:System.Windows.Forms.dll','/reference:System.Drawing.dll',
  ('/reference:'+(Join-Path $framework 'System.Runtime.dll')),
  ('/reference:'+(Join-Path $framework 'System.Runtime.InteropServices.WindowsRuntime.dll')),
  ('/reference:'+(Join-Path $env:WINDIR 'System32\WinMetadata\Windows.Devices.winmd')),
  ('/reference:'+(Join-Path $env:WINDIR 'System32\WinMetadata\Windows.Foundation.winmd')),
  ('/win32manifest:'+(Join-Path $projectRoot 'src\app.manifest')))
$icon = Join-Path $projectRoot 'assets\app.ico'
if (Test-Path -LiteralPath $icon) { $compilerArgs += '/win32icon:'+$icon }
$sources = @(Get-ChildItem -LiteralPath (Join-Path $projectRoot 'src') -Filter '*.cs' | ForEach-Object { $_.FullName })
& $csc @compilerArgs @sources
if ($LASTEXITCODE -ne 0) { throw 'Kompilierung fehlgeschlagen.' }
Set-Content -LiteralPath (Join-Path $portable 'portable.flag') -Value 'Portable mode: settings stay next to the application.' -Encoding ASCII
foreach ($name in @('LICENSE','README.md','DISCLAIMER.de.md','PRIVACY.md','CHANGELOG.md','CONTRIBUTING.md','SECURITY.md','THIRD-PARTY.md')) {
  Copy-Item -LiteralPath (Join-Path $projectRoot $name) -Destination $portable
}
Copy-Item -LiteralPath (Join-Path $projectRoot 'docs') -Destination $portable -Recurse -Force
Copy-Item -LiteralPath (Join-Path $projectRoot 'assets') -Destination $portable -Recurse -Force
if (!$SkipTests) {
  $testDir = Join-Path $projectRoot 'work\tests'
  New-Item -ItemType Directory -Force -Path $testDir | Out-Null
  $process = Start-Process -FilePath $exe -ArgumentList @('--selftest', ('"--output-dir='+$testDir+'"')) -Wait -PassThru
  if ($process.ExitCode -ne 0) { throw "Selbsttest fehlgeschlagen: $testDir" }
  $report = Get-Content -LiteralPath (Join-Path $testDir 'Testbericht.txt') -Raw -Encoding UTF8
  if (!$report.StartsWith('OK:')) { throw $report }
  Write-Host $report.Trim()
}
$zip = Join-Path $dist "BleAirDock-$version-portable.zip"
Compress-Archive -Path $portable -DestinationPath $zip -Force
if ($Installer) {
  if (!$Iscc) {
    $found = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($found) { $Iscc = $found.Source }
    foreach ($candidate in @("${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe", "$env:ProgramFiles\Inno Setup 7\ISCC.exe")) {
      if (!$Iscc -and (Test-Path -LiteralPath $candidate)) { $Iscc = $candidate }
    }
  }
  if (!$Iscc -or !(Test-Path -LiteralPath $Iscc)) { throw 'Inno Setup fehlt. build.ps1 -Installer -Iscc <Pfad zu ISCC.exe> verwenden.' }
  & $Iscc ('/DAppVersion='+$version) (Join-Path $projectRoot 'installer\BleAirDock.iss')
  if ($LASTEXITCODE -ne 0) { throw 'Installer konnte nicht gebaut werden.' }
}
$sourceZip = Join-Path $dist "BleAirDock-$version-source.zip"
$sourceFiles = @(Get-ChildItem -LiteralPath $projectRoot -File | Where-Object { $_.Name -ne '.git' } | ForEach-Object { $_.FullName })
$sourceFiles += @('src','assets','docs','installer','.github') | ForEach-Object { Join-Path $projectRoot $_ }
Compress-Archive -LiteralPath $sourceFiles -DestinationPath $sourceZip -Force
$hashFiles = @(Get-Item -LiteralPath $zip), @(Get-Item -LiteralPath $sourceZip)
$setup = Join-Path $dist "BleAirDock-$version-Setup.exe"
if (Test-Path -LiteralPath $setup) { $hashFiles += Get-Item -LiteralPath $setup }
$hashes = foreach ($file in $hashFiles) { (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()+'  '+$file.Name }
[IO.File]::WriteAllLines((Join-Path $dist 'SHA256SUMS.txt'), $hashes, [Text.Encoding]::ASCII)
Write-Host "Fertig: $dist"
