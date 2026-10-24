// SPDX-License-Identifier: MIT
using System;
using System.Drawing;
using System.Linq;
using System.IO;
partial class Tray {
 readonly System.Windows.Forms.Timer pulseTimer=new System.Windows.Forms.Timer();
 readonly System.Diagnostics.Stopwatch pulseClock=System.Diagnostics.Stopwatch.StartNew();
 bool pulseWasActive;
 Func<string> pulseTestColor; Func<Settings> pulseTestMotion; DateTime pulseTestUntil;
 void StopPulseTest() { pulseTestColor=null; pulseTestMotion=null; pulseWasActive=false; if(!exiting) Render(); }
 float PulseAmount(Settings config=null) { var look=config ?? settings; return PulseLevel(look,pulseClock.Elapsed.TotalSeconds); }
 internal static float PulseLevel(Settings config,double seconds) { float maximum=config.PulseStrength/100f; return config.PulseSteady ? maximum : (float)(maximum*(0.2+0.8*(1-Math.Cos(seconds*2*Math.PI/(config.PulsePeriod/10.0)))/2)); }
 internal static bool ShouldPulse(Settings config,int? battery,bool? connected) {
  return config.PulseLow && connected==true && battery.HasValue && battery.Value>=0 && battery.Value<=config.PulseThreshold;
 }
 public void CheckPulseButton() {
  Options();
  var tabs=settingsForm.Controls.OfType<System.Windows.Forms.TabControl>().Single();
  var tab=tabs.TabPages.Cast<System.Windows.Forms.TabPage>().Single(x=>x.Text==L.T("Leuchtwarnung")); tabs.SelectedTab=tab;
  var button=tab.Controls.OfType<System.Windows.Forms.Button>().Single(x=>x.Text==L.T("Im Infobereich testen (30 s)"));
  button.PerformClick();
  var until=DateTime.UtcNow.AddMilliseconds(400); while(DateTime.UtcNow<until) { System.Windows.Forms.Application.DoEvents(); System.Threading.Thread.Sleep(10); }
  if(pulseTestColor==null || !icons[0].Text.Contains("TEST")) throw new Exception("Tray pulse test not active");
  using(var image=icons[0].Icon.ToBitmap()) image.Save(AppPaths.Report("Pulse-Test-Tray.png"));
  pulseTestColor=()=>"#0077FF";
  until=DateTime.UtcNow.AddMilliseconds(200); while(DateTime.UtcNow<until) { System.Windows.Forms.Application.DoEvents(); System.Threading.Thread.Sleep(10); }
  using(var image=icons[0].Icon.ToBitmap()) image.Save(AppPaths.Report("Pulse-Test-Blue.png"));
  pulseTestUntil=DateTime.UtcNow.AddSeconds(-1);
  until=DateTime.UtcNow.AddMilliseconds(200); while(DateTime.UtcNow<until) { System.Windows.Forms.Application.DoEvents(); System.Threading.Thread.Sleep(10); }
  if(pulseTestColor!=null || icons[0].Text.Contains("TEST")) throw new Exception("Tray test did not restore normal state");
  using(var image=new Bitmap(settingsForm.Width,settingsForm.Height)) { settingsForm.DrawToBitmap(image,new Rectangle(0,0,image.Width,image.Height)); image.Save(AppPaths.Report("Pulse-Settings.png")); }
  settingsForm.Close(); ExitThread();
  File.WriteAllText(AppPaths.Report("Pulse-UI-Test.txt"),"OK: real tray test button, color changes, timeout restores icons; settings screenshot captured.");
 }
 void StartPulse() {
  pulseTimer.Interval=150;
  pulseTimer.Tick+=(s,e)=> {
   if(pulseTestColor!=null && DateTime.UtcNow>=pulseTestUntil) StopPulseTest();
   bool active=false;
   for(int i=0;i<icons.Count;i++) {
    string id=i<selected.Count ? selected[i] : null; var d=devices.FirstOrDefault(x=>x.Id==id);
    bool testing=i==0 && pulseTestColor!=null;
    var warningLook=WarningSettings(id);
    bool low=testing || (warningsPausedUntil<=DateTime.UtcNow && d!=null && error.Length==0 && ShouldPulse(warningLook,d.Battery,EffectiveConnection(d)));
    if(low) active=true;
    if(testing) icons[i].Visible=true;
    if(!icons[i].Visible || (!low && !pulseWasActive)) continue;
    var profile=settings.Profile(id,i); var look=warningLook;
    if(testing) { look=new Settings{PulseColor=pulseTestColor(),BatteryRadius=settings.BatteryRadius,BatteryStroke=settings.BatteryStroke,BatteryWidth=settings.BatteryWidth,BatteryHeight=settings.BatteryHeight,FontName=settings.FontName,TextSize=settings.TextSize,TextColor=settings.TextColor}; if(pulseTestMotion!=null) { var motion=pulseTestMotion(); look.PulsePeriod=motion.PulsePeriod; look.PulseStrength=motion.PulseStrength; look.PulseSteady=motion.PulseSteady; } icons[i].Text="BleAirDock · "+L.T("Leuchtwarnung")+" · TEST"; }
    var old=icons[i].Icon;
    icons[i].Icon=BatteryIcon(testing ? 15 : error.Length==0 && d!=null ? d.Battery : null,i,testing ? true : d!=null ? EffectiveConnection(d) : null,profile.Color,settings.NumbersOnly,look,profile.Symbol,profile.RenderMarker,id!=null && IsAudioActive(id,i),low ? PulseAmount(look) : 0,System.Windows.Forms.SystemInformation.SmallIconSize.Width,true);
    if(old!=null) old.Dispose();
   }
   pulseWasActive=active;
  };
  pulseTimer.Start();
 }
}
static class PulseTests {
 internal static void Run() {
  foreach(bool? connected in new bool?[]{false,null}) using(var a=Tray.BatteryIcon(90,0,connected)) using(var b=Tray.BatteryIcon(10,0,connected)) using(var aa=a.ToBitmap()) using(var bb=b.ToBitmap()) {
   for(int y=0;y<32;y++) for(int x=0;x<32;x++) if(aa.GetPixel(x,y)!=bb.GetPixel(x,y)) throw new Exception("Disconnected or unknown battery still displayed");
  }
  var s=new Settings{PulseLow=true,PulseThreshold=25};
  if(!Tray.ShouldPulse(s,25,true) || !Tray.ShouldPulse(s,0,true) || Tray.ShouldPulse(s,26,true) || Tray.ShouldPulse(s,null,true) || Tray.ShouldPulse(s,10,false) || Tray.ShouldPulse(s,10,null)) throw new Exception("Pulse eligibility");
  s.PulseLow=false; if(Tray.ShouldPulse(s,10,true)) throw new Exception("Pulse disabled");
  string file=AppPaths.Report("pulse-test.ini"); s.PulseThreshold=37; s.PulseColor="#2288DD"; s.Save(file); var loaded=Settings.Read(file);
  if(loaded.PulseLow || loaded.PulseThreshold!=37 || loaded.PulseColor!="#2288DD") throw new Exception("Pulse persistence");
  File.WriteAllText(file,"PulseLow=true\nPulseThreshold=999"); loaded=Settings.Read(file); if(!loaded.PulseLow || loaded.PulseThreshold!=90) throw new Exception("Pulse limits"); File.WriteAllText(file,"PulseColor=invalid"); if(Settings.Read(file).PulseColor!="#FF9119") throw new Exception("Pulse color fallback"); File.Delete(file);
  using(var a=Tray.BatteryIcon(15,0,true,null,false,null,0,"A",false,0)) using(var b=Tray.BatteryIcon(15,0,true,null,false,null,0,"A",false,0.75f)) using(var aa=a.ToBitmap()) using(var bb=b.ToBitmap()) {
   int changed=0; for(int y=0;y<32;y++) for(int x=0;x<32;x++) if(aa.GetPixel(x,y)!=bb.GetPixel(x,y)) changed++;
   if(changed<20) throw new Exception("Pulse frames do not change");
  }
  using(var a=Tray.BatteryIcon(15,0,true,null,false,new Settings{PulseColor="#00FF00"},0,"A",false,0.75f)) using(var b=Tray.BatteryIcon(15,0,true,null,false,new Settings{PulseColor="#0000FF"},0,"A",false,0.75f)) using(var aa=a.ToBitmap()) using(var bb=b.ToBitmap()) {
   int changed=0; for(int y=0;y<32;y++) for(int x=0;x<32;x++) if(aa.GetPixel(x,y)!=bb.GetPixel(x,y)) changed++;
   if(changed<20) throw new Exception("Custom pulse color rendering");
  }

 }
}
