// SPDX-License-Identifier: MIT
using System; using System.Drawing; using System.Collections.Generic; using System.IO;
static class WidgetPresetTests {
 internal static void Run() {
  var o=new WidgetOptions{Mode=DisplayMode.Beides,X=250,Y=90,Locked=true}; o.ApplyPreset(true); o.Normalize();
  if(!o.Tiny || !o.Horizontal || o.Width!=112 || o.Height!=56 || o.Mode!=DisplayMode.Beides || o.X!=250 || o.Y!=90 || !o.Locked) throw new Exception("Tiny preset");
  var items=new List<WidgetItem>{new WidgetItem{Name="Big",Status="Getrennt",Connected=false,Battery=90,Profile=new DeviceProfile("#13705B")},new WidgetItem{Name="Retro",Status="Verbunden",Connected=true,Battery=90,AudioActive=true,Profile=new DeviceProfile("#1C579F")}};
  var settings=new Settings{Widget=o}; string file=AppPaths.Report("widget-preset.ini"); settings.Save(file); var loaded=Settings.Read(file).Widget; File.Delete(file);
  if(!loaded.Tiny || loaded.Width!=112 || loaded.Height!=56) throw new Exception("Tiny persistence");
  foreach(bool horizontal in new[]{true,false}) { o.Horizontal=horizontal; if(!horizontal) { o.Width=56; o.Height=112; }
   for(int i=0;i<2;i++) {var cell=WidgetDrawing.Cell(o,2,i); if(WidgetDrawing.Hit(o,2,new Point((int)(cell.X+cell.Width/2),(int)(cell.Y+cell.Height/2)))!=i) throw new Exception("Tiny hit targets"); }
   using(var image=WidgetDrawing.Render(o,settings,items)) image.Save(AppPaths.Report(horizontal ? "Widget-Tiny-Horizontal.png" : "Widget-Tiny-Vertical.png"));
  }
  o.ApplyPreset(false); if(o.Tiny || o.Width!=300 || o.Height!=300 || !o.Names || !o.Status) throw new Exception("Standard preset");
  using(var image=WidgetDrawing.Render(o,settings,items)) image.Save(AppPaths.Report("Widget-Standard-1.6.3.png"));
 }
}
