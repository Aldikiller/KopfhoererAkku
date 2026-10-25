// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

static partial class Program {
 internal static ProcessStartInfo RestartInfo(int parentId,string extra="") {
  return new ProcessStartInfo(Application.ExecutablePath,"--restart-after="+parentId+(extra.Length>0 ? " "+extra : "")){UseShellExecute=false,WorkingDirectory=AppPaths.Base};
 }
 static bool WaitForRestartParent(string[] args) {
  var option=args.FirstOrDefault(x=>x.StartsWith("--restart-after=",StringComparison.Ordinal));
  if(option==null)return true;
  int id;if(!int.TryParse(option.Substring(16),out id) || id<=0 || id==Process.GetCurrentProcess().Id){Environment.ExitCode=2;return false;}
  try {
   using(var parent=Process.GetProcessById(id)) {
    if(!parent.HasExited && !string.Equals(parent.MainModule.FileName,Application.ExecutablePath,StringComparison.OrdinalIgnoreCase)){Environment.ExitCode=2;return false;}
    if(!parent.WaitForExit(15000)){Environment.ExitCode=2;return false;}
   }
  } catch(ArgumentException) { } // Parent exited before the child could open it.
  catch(InvalidOperationException) { } // Process exited while its identity was being read.
  catch(System.ComponentModel.Win32Exception) {
   try { using(var remaining=Process.GetProcessById(id)) { if(!remaining.HasExited){Environment.ExitCode=2;return false;} } }
   catch(ArgumentException) { }
  }
  return true;
 }
}
partial class Tray {
 void Restart() {
  if(exiting)return;
  try {
   using(var child=Process.Start(Program.RestartInfo(Process.GetCurrentProcess().Id))) {
    if(child==null)throw new InvalidOperationException("Process did not start.");
   }
  } catch(Exception ex) {MessageBox.Show(L.T("Neustart fehlgeschlagen: ")+ex.Message,"BleAirDock",MessageBoxButtons.OK,MessageBoxIcon.Error);return;}
  // Release tray icons, callbacks and the single-instance lock before child startup.
  ExitThread();
 }
 public void MaintenanceUiCheck() {
  foreach(string language in L.Codes) {
   L.Language=language;
   using(var menu=Menu()) {
    var labels=menu.Items.Cast<ToolStripItem>().Select(x=>x.Text).ToArray();
    if(labels.Count(x=>x==L.T("Neustart"))!=1 || labels.Contains(L.T("Nach Updates suchen …")) || labels.Contains(L.T("Über BleAirDock …")))throw new Exception("Tray maintenance menu: "+language);
   }
   Options();
   var about=settingsForm.Controls.OfType<Button>().Single(x=>x.AccessibleName=="AboutBleAirDock");
   if(about.Text!=L.T("Über BleAirDock …") || !settingsForm.ClientRectangle.Contains(about.Bounds))throw new Exception("About settings layout");
   var tabs=settingsForm.Controls.OfType<TabControl>().Single();
   if(!tabs.TabPages.Cast<TabPage>().Any(x=>x.Text==L.T("Updates")))throw new Exception("Updates missing from simple settings");
   var advanced=settingsForm.Controls.OfType<CheckBox>().Single(x=>x.AccessibleName=="AdvancedSettings");
   advanced.Checked=true;
   var updates=tabs.TabPages.Cast<TabPage>().Single(x=>x.Text==L.T("Updates"));
   if(!updates.Controls.OfType<Button>().Any())throw new Exception("Update action removed from settings");
   using(var image=new Bitmap(settingsForm.Width,settingsForm.Height)){settingsForm.DrawToBitmap(image,new Rectangle(0,0,image.Width,image.Height));image.Save(AppPaths.Report("Maintenance-"+language+".png"));}
   settingsForm.Close();
  }
  L.Language="de";settings.Widget.Mode=DisplayMode.Beides;Render();
  if(widget.ContextMenuStrip.Items.Cast<ToolStripItem>().Count(x=>x.Text==L.T("Neustart"))!=1)throw new Exception("Widget restart menu");
  // Exercise connected, disconnected and invalidated readings on both surfaces.
  foreach(bool? connected in new bool?[]{true,false,null}) {
   devices[0].Connected=connected;devices[0].Battery=90;readings.Observe(devices,DateTime.UtcNow);Render();
   var snapshot=WidgetItems()[0];
   if(snapshot.Connected!=connected)throw new Exception("Display snapshot changed connection");
   var profile=settings.Profile(selected[0],0);
   using(var expected=BatteryIcon(snapshot.Battery,0,snapshot.Connected,profile.Color,settings.NumbersOnly,settings,profile.Symbol,profile.RenderMarker,IsAudioActive(selected[0],0),0,SystemInformation.SmallIconSize.Width,true))
   using(var actual=icons[0].Icon.ToBitmap())using(var reference=expected.ToBitmap()) {
    for(int y=0;y<actual.Height;y++)for(int x=0;x<actual.Width;x++)if(actual.GetPixel(x,y)!=reference.GetPixel(x,y))throw new Exception("Tray/widget snapshot mismatch");
   }
  }
  readings.Invalidate(selected);Render();if(WidgetItems().Any(x=>x.Connected!=null))throw new Exception("Invalidated reading reported as connected");
  ExitThread();File.WriteAllText(AppPaths.Report("Maintenance-Test.txt"),"OK: restart/menu placement and About/Updates in five languages; shared tray/widget display states.");
 }
}
static class MaintenanceTests {
 internal static void Run() {
  var state=new Dictionary<ulong,bool>{{1,false},{2,true},{3,false}};
  Native.MergeConnections(state,new Dictionary<ulong,bool>{{1,true},{2,false},{4,false}});
  if(!state[1] || !state[2] || state[3] || state[4] || state.ContainsKey(5))throw new Exception("Bluetooth source reconciliation");
  Native.MergeConnections(state,new Dictionary<ulong,bool>());if(!state[1])throw new Exception("Empty source erased connected evidence");
  var languages=L.Language;
  foreach(var language in L.Codes){L.Language=language;if(language!="de" && L.T("Neustart")=="Neustart")throw new Exception("Restart translation missing");}
  L.Language=languages;
 }
}
