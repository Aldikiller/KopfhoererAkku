// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using Windows.Devices.Enumeration;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

partial class Tray {
 readonly ReadingBook readings=new ReadingBook();
 Control dispatcher;DeviceWatcher watcher;volatile bool watcherReady;bool pendingRefresh;
 readonly ConcurrentDictionary<string,string> watcherAddresses=new ConcurrentDictionary<string,string>();
 
 Form overview;Action updateOverview;System.Windows.Forms.Timer clickDelay;DateTime ignoreClickUntil;
 void StartObservers() {
  dispatcher=new Control();var handle=dispatcher.Handle;StartAudioEvents();
  SystemEvents.PowerModeChanged+=PowerChanged;SystemEvents.SessionSwitch+=SessionChanged;
  try {
   watcher=DeviceInformation.CreateWatcher("System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\" OR System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\"",new[]{"System.Devices.Aep.IsConnected","System.Devices.Aep.DeviceAddress"},DeviceInformationKind.AssociationEndpoint);
   watcher.Added+=WatcherAdded;watcher.Updated+=WatcherUpdated;watcher.Removed+=WatcherRemoved;watcher.EnumerationCompleted+=WatcherEnumerated;watcher.Start();
  } catch {StopWatcher();} // Regular polling remains available on systems without a watcher.
 }
 void PowerChanged(object sender,PowerModeChangedEventArgs args) {if(args.Mode==PowerModes.Resume) QueueRecovery();}
 void SessionChanged(object sender,SessionSwitchEventArgs args) {if(args.Reason==SessionSwitchReason.SessionUnlock || args.Reason==SessionSwitchReason.RemoteConnect || args.Reason==SessionSwitchReason.ConsoleConnect)QueueRecovery();}
 void WatcherEnumerated(DeviceWatcher sender,object args) {watcherReady=true;}
 bool IsSelectedEndpoint(string id){string address;if(!watcherAddresses.TryGetValue(id,out address) || address.Length!=12)return false;return selected.Any(deviceId=>Regex.Matches(deviceId,@"(?i)(?<![0-9a-f])[0-9a-f]{12}(?![0-9a-f])").Cast<Match>().Any(match=>string.Equals(match.Value,address,StringComparison.OrdinalIgnoreCase)));}
 void WatcherAdded(DeviceWatcher sender,DeviceInformation args) {object address;if(args.Properties.TryGetValue("System.Devices.Aep.DeviceAddress",out address))watcherAddresses[args.Id]=Convert.ToString(address).Replace(":","").Replace("-","");if(watcherReady && IsSelectedEndpoint(args.Id))QueueRecovery();}
 void WatcherUpdated(DeviceWatcher sender,DeviceInformationUpdate args) {if(watcherReady && args.Properties.ContainsKey("System.Devices.Aep.IsConnected") && IsSelectedEndpoint(args.Id))QueueRecovery();}
 void WatcherRemoved(DeviceWatcher sender,DeviceInformationUpdate args) {if(watcherReady && IsSelectedEndpoint(args.Id))QueueRecovery();string removed;watcherAddresses.TryRemove(args.Id,out removed);}
 void QueueRecovery() {
  var target=dispatcher;if(target==null || target.IsDisposed)return;
  try {target.BeginInvoke((Action)(()=>{if(exiting)return;readings.Invalidate(selected);Render();if(updateOverview!=null)updateOverview();}));}catch(InvalidOperationException){}
 }
 void StopWatcher() {
  watcherReady=false;watcherAddresses.Clear();if(watcher==null)return;
  try {watcher.Added-=WatcherAdded;watcher.Updated-=WatcherUpdated;watcher.Removed-=WatcherRemoved;watcher.EnumerationCompleted-=WatcherEnumerated;if(watcher.Status==DeviceWatcherStatus.Started || watcher.Status==DeviceWatcherStatus.EnumerationCompleted)watcher.Stop();}catch{}watcher=null;
 }
 void StopObservers() {
  SystemEvents.PowerModeChanged-=PowerChanged;SystemEvents.SessionSwitch-=SessionChanged;StopWatcher();
  if(audioEvents!=null){audioEvents.Dispose();audioEvents=null;}
  if(dispatcher!=null){dispatcher.Dispose();dispatcher=null;}if(clickDelay!=null)clickDelay.Dispose();if(overview!=null)overview.Close();
 }
 bool? EffectiveConnection(Device device) {return error.Length>0 || readings.Get(device.Id).NeedsCheck ? null : device.Connected;}
 public void CheckObservers() {
  StartObservers();Application.DoEvents();
  if(watcher==null)throw new Exception("Bluetooth watcher could not start");
  QueueRecovery();Application.DoEvents();
  if(selected.Any(id=>!readings.Get(id).NeedsCheck))throw new Exception("Recovery did not invalidate readings");
  StopObservers();
  if(watcher!=null || dispatcher!=null)throw new Exception("Observer cleanup failed");
  File.WriteAllText(AppPaths.Report("Recovery-Test.txt"),"OK: Bluetooth watcher, recovery dispatch, invalidation and cleanup.");
 }
 string QueryAge(string id) {var entry=readings.Get(id);return L.T("Geprüft vor: ")+ReadingBook.Age(entry.CheckedUtc,DateTime.UtcNow);}
 string HoverText(int slot) {
  var device=devices.FirstOrDefault(d=>selected.Count>slot && d.Id==selected[slot]);if(device==null)return L.T("Kopfhörer-Akku · Doppelklick: Einstellungen");
  string name=(IsAudioActive(device.Id,slot) ? "✓ " : "")+settings.Name(device);if(name.Length>24)name=name.Substring(0,23)+"…";
  string value=EffectiveConnection(device)==true && device.Battery.HasValue ? device.Battery+"%" : "–";
  string text=name+" · "+value+" · "+(error.Length>0 ? L.T("Abfrage fehlgeschlagen") : QueryAge(device.Id));return text.Length>63 ? text.Substring(0,63) : text;
 }
 void AddOverviewClicks(NotifyIcon icon) {
  if(clickDelay==null){clickDelay=new System.Windows.Forms.Timer{Interval=SystemInformation.DoubleClickTime};clickDelay.Tick+=(s,e)=>{clickDelay.Stop();if(settings.QuickAudio)SwitchAudio(clickSlot);else ShowOverview();};}
  icon.MouseClick+=(s,e)=>{if(e.Button!=MouseButtons.Left || DateTime.UtcNow<ignoreClickUntil)return;clickSlot=icons.IndexOf(icon);clickDelay.Stop();clickDelay.Start();};
  icon.DoubleClick+=(s,e)=>{clickDelay.Stop();ignoreClickUntil=DateTime.UtcNow.AddMilliseconds(SystemInformation.DoubleClickTime);Options();};
  icon.MouseMove+=(s,e)=>{int slot=icons.IndexOf(icon);if(slot>=0)icon.Text=HoverText(slot);};
 }
 public void ShowOverview(bool preview=false) {
  if(overview!=null){overview.Activate();return;}
  var form=new Form{Text=L.T("Geräteübersicht"),Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath),ClientSize=new Size(570,420),FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false,ShowInTaskbar=false,StartPosition=FormStartPosition.Manual,AutoScaleMode=AutoScaleMode.Dpi,BackColor=Color.FromArgb(245,245,245)};overview=form;
  var cards=new List<Label[]>();var names=selected.ToArray();
  for(int i=0;i<names.Length;i++) {
   var panel=new System.Windows.Forms.Panel{Left=16,Top=16+i*132,Width=538,Height=122,BackColor=Color.White,BorderStyle=BorderStyle.FixedSingle};
   var name=new Label{Left=14,Top=10,Width=395,Height=25,Font=new Font("Segoe UI",11,FontStyle.Bold),AutoEllipsis=true};
   var level=new Label{Left=416,Top=8,Width=105,Height=32,Font=new Font("Segoe UI",19,FontStyle.Bold),TextAlign=ContentAlignment.TopRight};
   var status=new Label{Left=14,Top=43,Width=505,Height=23};var age=new Label{Left=14,Top=78,Width=505,Height=32,ForeColor=Color.FromArgb(90,95,100)};
   int slot=i;var play=new Button{Left=320,Top=48,Width=200,Height=25,Text=L.T("Ton hier wiedergeben")};play.Click+=(s,e)=>SwitchAudio(slot);panel.Controls.Add(play);status.Width=300;
   panel.Controls.AddRange(new Control[]{name,level,status,age});form.Controls.Add(panel);cards.Add(new[]{name,level,status,age});
  }
  if(names.Length==0)form.Controls.Add(new Label{Left=20,Top=25,Width=525,Height=60,Text=L.T("Keine Kopfhörer ausgewählt")});
  var note=new Label{Left=18,Top=286,Width=535,Height=60,Text=L.T("Windows-Werte können zwischengespeichert sein. Die Zeit zeigt die letzte Abfrage, keine neue Messung am Kopfhörer."),ForeColor=Color.FromArgb(90,95,100)};
  var refresh=new Button{Left=16,Top=366,Width=160,Height=34,Text=L.T("Jetzt aktualisieren")};refresh.Click+=(s,e)=>Refresh();
  var options=new Button{Left=192,Top=366,Width=174,Height=34,Text=L.T("Einstellungen …")};options.Click+=(s,e)=>{form.Close();Options();};
  var close=new Button{Left=382,Top=366,Width=172,Height=34,Text=L.T("Schließen"),DialogResult=DialogResult.Cancel};form.CancelButton=close;
  close.Click+=(s,e)=>form.Close();
  form.Controls.AddRange(new Control[]{note,refresh,options,close});
  updateOverview=()=>{
   if(form.IsDisposed)return;refresh.Enabled=!busy;
   for(int i=0;i<names.Length;i++) {
    var device=devices.FirstOrDefault(d=>d.Id==names[i]);var entry=readings.Get(names[i]);var labels=cards[i];
    labels[0].Text=device==null ? settings.Profile(names[i],i).Name : settings.Name(device);if(IsAudioActive(names[i],i))labels[0].Text="✓ "+labels[0].Text;
    bool current=device!=null && EffectiveConnection(device)==true && device.Battery.HasValue;
    labels[1].Text=current ? device.Battery+" %" : "–";labels[1].ForeColor=current && device.Battery<=20 ? Color.Firebrick : Color.FromArgb(41,45,50);
    labels[2].Text=error.Length>0 ? L.T("Abfrage fehlgeschlagen") : entry.NeedsCheck ? L.T("Verbindung unbekannt") : device==null ? L.T("Status unbekannt") : device.Status;
    labels[3].Text=QueryAge(names[i])+"\n"+(entry.LastValue.HasValue ? L.T("Letzter Windows-Wert: ")+entry.LastValue+" % · "+L.T("Gelesen vor: ")+ReadingBook.Age(entry.ValueReadUtc,DateTime.UtcNow)+(current ? "" : " · "+L.T("nicht aktuell")) : L.T("kein Akkustand"));
   }
  };updateOverview();
  form.FormClosed+=(s,e)=>{updateOverview=null;overview=null;foreach(var card in cards){card[0].Font.Dispose();card[1].Font.Dispose();}form.Dispose();};
  var area=Screen.FromPoint(Cursor.Position).WorkingArea;form.Location=new Point(Math.Max(area.Left,Math.Min(Cursor.Position.X-form.Width,area.Right-form.Width)),Math.Max(area.Top,area.Bottom-form.Height-8));form.Show();
  if(preview){Application.DoEvents();using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(0,0,form.Width,form.Height));bitmap.Save(AppPaths.Report("Geraeteuebersicht.png"));}form.Close();}
 }
 public void QuietOverviewCheck(){
  readings.Invalidate(selected);ShowOverview();Application.DoEvents();
  if(overview.Controls.Cast<Control>().SelectMany(Descendants).OfType<Label>().Any(x=>x.Text.Contains(L.T("Wird neu geprüft …"))))throw new Exception("Routine progress hint still visible");
  if(WidgetItems().Any(x=>x.Status==L.T("Wird neu geprüft …")))throw new Exception("Widget shows progress hint");
  error="test";updateOverview();if(!overview.Controls.Cast<Control>().SelectMany(Descendants).OfType<Label>().Any(x=>x.Text==L.T("Abfrage fehlgeschlagen")))throw new Exception("Actual error hidden");error="";updateOverview();overview.Close();
  File.WriteAllText(AppPaths.Report("Quiet-UI-Test.txt"),"OK: no routine progress banner; real errors still visible.");
 }
 static IEnumerable<Control> Descendants(Control control){yield return control;foreach(Control child in control.Controls)foreach(var descendant in Descendants(child))yield return descendant;}
}




