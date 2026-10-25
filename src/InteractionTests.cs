// SPDX-License-Identifier: MIT
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
static class InteractionTests {
 internal static void Run() {
  var settings=new Settings();if(settings.WheelVolume || settings.Widget.HideFullscreen)throw new Exception("New options must be opt-in");
  settings.WheelVolume=true;settings.Widget.HideFullscreen=true;
  string file=AppPaths.Report("interaction-roundtrip.ini");settings.Save(file);var read=Settings.Read(file);File.Delete(file);
  if(!read.WheelVolume || !read.Widget.HideFullscreen)throw new Exception("Interaction persistence");
  read.Widget.ApplyPreset(true);if(!read.Widget.HideFullscreen)throw new Exception("Preset erased behavior");
  var monitor=new Rectangle(-1920,0,1920,1080);
  if(!FullscreenWindow.Covers(monitor,monitor) || FullscreenWindow.Covers(new Rectangle(-1920,0,1920,1040),monitor) || FullscreenWindow.Covers(new Rectangle(0,0,1920,1080),monitor))throw new Exception("Fullscreen monitor geometry");
  if(!FullscreenWindow.ShouldHide(false,true,false) || FullscreenWindow.ShouldHide(false,false,true) || !FullscreenWindow.ShouldHide(true,false,true))throw new Exception("Fullscreen manual visibility");
  if(Tray.WheelTarget(99,1)!=100 || Tray.WheelTarget(1,-1)!=0 || Tray.WheelTarget(50,3)!=56)throw new Exception("Wheel clamping");
  var language=L.Language;foreach(var code in L.Codes){L.Language=code;foreach(var key in new[]{"Bedienung","Weitere Aktionen","Widget bei Vollbild automatisch ausblenden","Lautstärke per Mausrad über dem Gerätesymbol"})if(code!="de" && L.T(key)==key)throw new Exception("Interaction translation "+code);}L.Language=language;
 }
}
partial class Tray {
 public void InteractionUiCheck() {
  settings.Widget.Mode=DisplayMode.Widget;Render();Application.DoEvents();
  fullscreenProbe=rect=>true;settings.Widget.HideFullscreen=true;UpdateFullscreen();
  if(widget.Visible || !icons.Any(x=>x.Visible))throw new Exception("Fullscreen hide lost tray access");
  fullscreenProbe=rect=>false;UpdateFullscreen();if(!widget.Visible)throw new Exception("Fullscreen restore");
  widgetHidden=true;fullscreenProbe=rect=>true;UpdateFullscreen();fullscreenProbe=rect=>false;UpdateFullscreen();if(widget.Visible)throw new Exception("Fullscreen reset manual hide");
  widgetHidden=false;Render();Application.DoEvents();
  // Exercise the real wheel routing/queue while replacing audio calls with a fake endpoint.
  dispatcher=new Control();var handle=dispatcher.Handle;
  var profile=settings.Profile(selected[0],0);profile.AudioId="fake-output";settings.Profiles[selected[0]]=profile;
  audioOutputs.Clear();audioOutputs.Add(new AudioOutput{Id="fake-output",Name="Test",Active=true});
  int volume=50,calls=0;getWheelVolume=id=>{if(id!="fake-output")throw new Exception("Wrong endpoint");return volume;};setWheelVolume=(id,value)=>{volume=value;calls++;};
  var point=widget.PointToScreen(new Point(settings.Widget.Padding+settings.Widget.IconSize/2,settings.Widget.Padding+settings.Widget.IconSize/2));
  windowAtPoint=pointOnScreen=>widget.Handle;
  settings.WheelVolume=false;if(HandleWheel(point,120) || calls!=0)throw new Exception("Disabled wheel changed audio");
  settings.WheelVolume=true;if(HandleWheel(new Point(-20000,-20000),120))throw new Exception("Unrelated wheel consumed");
  if(!HandleWheel(point,60))throw new Exception("High resolution wheel target");Application.DoEvents();if(calls!=0)throw new Exception("Partial notch changed volume");
  if(!HandleWheel(point,60))throw new Exception("Wheel routing");Application.DoEvents();if(volume!=52 || calls!=1)throw new Exception("Wheel volume delta");
  HandleWheel(point,-120);Application.DoEvents();if(volume!=50 || calls!=2)throw new Exception("Wheel down");
  profile.RememberVolume=true;HandleWheel(point,120);Application.DoEvents();if(profile.SavedVolume!=52)throw new Exception("Wheel volume memory");
  windowAtPoint=pointOnScreen=>IntPtr.Zero;if(HandleWheel(point,120))throw new Exception("Covered widget consumed input");windowAtPoint=pointOnScreen=>widget.Handle;
  profile.AudioId="";if(HandleWheel(point,120))throw new Exception("Unassigned device fallback");
  settings.WheelVolume=false;settings.Widget.HideFullscreen=false;settings.Widget.Mode=DisplayMode.Beides;Render();
  foreach(string language in L.Codes) {
   L.Language=language;Options();var tabs=settingsForm.Controls.OfType<TabControl>().Single();var behavior=tabs.TabPages.Cast<TabPage>().Single(x=>x.Text==L.T("Bedienung"));tabs.SelectedTab=behavior;
   var wheelBox=behavior.Controls.OfType<CheckBox>().Single(x=>x.AccessibleName=="WheelVolume");var fullBox=behavior.Controls.OfType<CheckBox>().Single(x=>x.AccessibleName=="HideFullscreen");wheelBox.Checked=true;fullBox.Checked=true;
   var advanced=settingsForm.Controls.OfType<CheckBox>().Single(x=>x.AccessibleName=="AdvancedSettings");advanced.Checked=true;advanced.Checked=false;
   if(!wheelBox.Checked || !fullBox.Checked || !tabs.TabPages.Contains(behavior))throw new Exception("Behavior draft lost");tabs.SelectedTab=behavior;
   using(var image=new Bitmap(settingsForm.Width,settingsForm.Height)){settingsForm.DrawToBitmap(image,new Rectangle(0,0,image.Width,image.Height));image.Save(AppPaths.Report("Controls-"+language+".png"));}
   using(var menu=Menu()){if(menu.Items.Count!=6 || !MenuLabels(menu.Items).Contains(L.T("Neustart")))throw new Exception("Compact menu");using(var image=new Bitmap(menu.PreferredSize.Width,menu.PreferredSize.Height)){menu.Size=menu.PreferredSize;menu.DrawToBitmap(image,new Rectangle(Point.Empty,image.Size));image.Save(AppPaths.Report("Menu-"+language+".png"));}}
   settingsForm.Close();
  }
  File.WriteAllText(AppPaths.Report("Tray-Wheel-Bounds.txt"),string.Join("\n",icons.Select(x=>DeviceWheel.IconBounds(x).ToString()).ToArray()));
  ExitThread();File.WriteAllText(AppPaths.Report("Interaction-Test.txt"),"OK: fullscreen hide/restore and manual-hide precedence, tray recovery, actual wheel routing and deferred fake volume calls, high-resolution wheel, unassigned/disabled/outside protection; menus and settings in five languages. Hardware and full-screen application testing remains necessary.");
 }
}
