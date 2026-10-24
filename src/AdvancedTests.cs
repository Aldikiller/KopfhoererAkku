// SPDX-License-Identifier: MIT
using System; using System.Drawing; using System.IO;
static class AdvancedTests {
 internal static void Run() {
  var options=new Settings{PulsePeriod=30,PulseStrength=40,PulseSteady=true,HotkeyOne=true,HotkeyOneKey=80,HotkeyTwo=true,HotkeyTwoKey=81};
  options.Profiles["test"]=new DeviceProfile("#123456"){WarningOverride=true,DeviceWarnings=false,DevicePulse=true,WarningThreshold=35,PulseThreshold=15,PulseColor="#2244CC"};
  string file=AppPaths.Report("advanced-test.ini");options.Save(file); var loaded=Settings.Read(file);File.Delete(file);
  var p=loaded.Profiles["test"];
  if(!p.WarningOverride || p.DeviceWarnings || !p.DevicePulse || p.WarningThreshold!=35 || p.PulseThreshold!=15 || p.PulseColor!="#2244CC")throw new Exception("Per-device warnings persistence");
  if(!loaded.HotkeyOne || loaded.HotkeyOneKey!=80 || !loaded.HotkeyTwo || loaded.HotkeyTwoKey!=81 || loaded.PulsePeriod!=30 || loaded.PulseStrength!=40 || !loaded.PulseSteady)throw new Exception("Advanced settings persistence");
  if(Math.Abs(Tray.PulseLevel(options,0)-0.4f)>0.001 || Math.Abs(Tray.PulseLevel(options,2)-0.4f)>0.001)throw new Exception("Steady pulse level");
  options.PulseSteady=false;
  if(Math.Abs(Tray.PulseLevel(options,0)-Tray.PulseLevel(options,3))>0.001 || Tray.PulseLevel(options,1.5)<=Tray.PulseLevel(options,0))throw new Exception("Pulse period and intensity");
  var area=new Rectangle(-1920,0,1920,1080); var size=new Size(112,56);
  if(BatteryWidget.FitPosition(new Point(-1915,4),size,area,12)!=new Point(-1920,0))throw new Exception("Snap negative screen");
  if(BatteryWidget.FitPosition(new Point(8000,8000),size,area,0)!=new Point(-112,1024))throw new Exception("Keep widget visible");
  var tip=BatteryWidget.Tooltip(new WidgetItem{Name="Test",Connected=false,Battery=90,Status="Disconnected"}); if(tip.Contains("90"))throw new Exception("Tooltip stale value");
  if(!BatteryWidget.Tooltip(new WidgetItem{Name="Test",Connected=true,Battery=90}).Contains("90"))throw new Exception("Tooltip known value");
 }
}
