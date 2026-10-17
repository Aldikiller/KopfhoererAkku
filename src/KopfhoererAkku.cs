// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Aldikiller
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using System.Text.RegularExpressions;

[assembly: AssemblyTitle("Kopfhörer-Akku")]
[assembly: AssemblyDescription("Bluetooth-Akkuanzeige im Windows-Infobereich")]
[assembly: AssemblyCompany("Aldikiller")]
[assembly: AssemblyProduct("Kopfhörer-Akku")]
[assembly: AssemblyVersion("0.1.0.1")]
[assembly: AssemblyFileVersion("0.1.0.1")]
[assembly: AssemblyInformationalVersion("0.1.0-beta.2")]
[assembly: AssemblyCopyright("Copyright © 2026 Aldikiller")]

static class AppPaths {
 public const string Version="0.1.0-beta.2";
 public const string MutexName="Local\\KopfhoererAkku.Release.Tray";
 public const string StopEventName="Local\\KopfhoererAkku.Release.Stop";
 public static readonly string Base=AppDomain.CurrentDomain.BaseDirectory;
 public static bool Portable { get { return File.Exists(Path.Combine(Base,"portable.flag")); } }
 public static string Data { get { return Portable ? Path.Combine(Base,"Einstellungen") : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"KopfhoererAkku"); } }
 public static string Output=Data;
 public static string Startup { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup),"KopfhoererAkku.lnk"); } }
 public static string Report(string name) { Directory.CreateDirectory(Output); return Path.Combine(Output,name); }
}
class Device {
 public string Id, Name; public int? Battery; public bool? Connected;
 public string DisplayName { get { return Name.Replace(" Hands-Free AG", ""); } }
 public string Status { get { return Connected==true ? "Verbunden" : Connected==false ? "Getrennt" : "Verbindung unbekannt"; } }
 public override string ToString() { return DisplayName + " · " + Status + " · " + (Connected==false ? "kein aktueller Akkustand" : Battery.HasValue ? Battery + (Connected==null ? " % (gespeichert)" : " %") : "kein Akkustand"); }
}
class WarningGate {
 readonly HashSet<string> warned=new HashSet<string>();
 public bool Check(Device d,Settings s) {
  if(d==null) return false;
  if(d.Connected==false || (d.Connected==true && d.Battery.HasValue && d.Battery.Value>s.Threshold+5)) warned.Remove(d.Id);
  if(!s.Warnings || d.Connected!=true || !d.Battery.HasValue || d.Battery.Value>s.Threshold) return false;
  return warned.Add(d.Id);
 }
 public void Reset() { warned.Clear(); }
}
static class Native {
 public static string LinkDiagnostics="";
 [StructLayout(LayoutKind.Sequential)] public struct Info { public int Size; public Guid Class; public uint Inst; public IntPtr Reserved; }
 [StructLayout(LayoutKind.Sequential)] public struct Key { public Guid Guid; public uint Pid; public Key(string g,uint p) { Guid=new Guid(g); Pid=p; } }
 [DllImport("setupapi.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern IntPtr SetupDiGetClassDevsW(IntPtr g,string e,IntPtr w,uint f);
 [DllImport("setupapi.dll",SetLastError=true)] static extern bool SetupDiEnumDeviceInfo(IntPtr h,uint i,ref Info d);
 [DllImport("setupapi.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern bool SetupDiGetDevicePropertyW(IntPtr h,ref Info d,ref Key k,out uint t,byte[] b,uint n,out uint r,uint f);
 [DllImport("setupapi.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern bool SetupDiGetDeviceInstanceIdW(IntPtr h,ref Info d,StringBuilder b,uint n,out uint r);
 [DllImport("setupapi.dll")] static extern bool SetupDiDestroyDeviceInfoList(IntPtr h);
 [DllImport("user32.dll")] public static extern bool DestroyIcon(IntPtr h);
 static byte[] Prop(IntPtr h,ref Info d,Key k,out uint type) { byte[] b=new byte[8192]; uint n; return SetupDiGetDevicePropertyW(h,ref d,ref k,out type,b,(uint)b.Length,out n,0) ? b.Take((int)n).ToArray() : null; }
 static string Str(IntPtr h,ref Info d,uint pid) { uint t; var b=Prop(h,ref d,new Key("a45c254e-df1c-4efd-8020-67d146a850e0",pid),out t); return b==null ? "" : Encoding.Unicode.GetString(b).TrimEnd('\0'); }
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] struct BtInfo {
  public uint Size; public ulong Address; public uint Class; public int Connected,Remembered,Authenticated;
  public ushort S1,S2,S3,S4,S5,S6,S7,S8,T1,T2,T3,T4,T5,T6,T7,T8;
  [MarshalAs(UnmanagedType.ByValTStr,SizeConst=248)] public string Name;
 }
 [StructLayout(LayoutKind.Sequential)] struct BtSearch { public uint Size; public int Auth,Remembered,Unknown,Connected,Inquiry; public byte Timeout; public IntPtr Radio; }
 [DllImport("BluetoothApis.dll",SetLastError=true)] static extern IntPtr BluetoothFindFirstDevice(ref BtSearch search,ref BtInfo info);
 [DllImport("BluetoothApis.dll",SetLastError=true)] static extern bool BluetoothFindNextDevice(IntPtr find,ref BtInfo info);
 [DllImport("BluetoothApis.dll")] static extern bool BluetoothFindDeviceClose(IntPtr find);
 static Dictionary<ulong,bool> EndpointConnections() {
  var op=Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(
   "System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\" OR System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\"",
   new[]{"System.Devices.Aep.DeviceAddress","System.Devices.Aep.IsConnected"},Windows.Devices.Enumeration.DeviceInformationKind.AssociationEndpoint);
  var clock=System.Diagnostics.Stopwatch.StartNew(); while(op.Status==Windows.Foundation.AsyncStatus.Started && clock.ElapsedMilliseconds<5000) Thread.Sleep(20); if(op.Status==Windows.Foundation.AsyncStatus.Started) { op.Cancel(); throw new TimeoutException("Bluetooth-Verbindungsabfrage dauert zu lange."); }
  var map=new Dictionary<ulong,bool>(); foreach(var endpoint in op.GetResults()) {
   object address,state; if(!endpoint.Properties.TryGetValue("System.Devices.Aep.DeviceAddress",out address) || !endpoint.Properties.TryGetValue("System.Devices.Aep.IsConnected",out state) || !(state is bool)) continue;
   ulong mac; if(ulong.TryParse(Convert.ToString(address).Replace(":","").Replace("-",""),System.Globalization.NumberStyles.HexNumber,null,out mac)) { bool old; map[mac]=(map.TryGetValue(mac,out old) && old) || (bool)state; }
  } return map;
 }
 static Dictionary<ulong,bool> Connections() {
  var map=new Dictionary<ulong,bool>(); var search=new BtSearch{Auth=1,Remembered=1,Unknown=1,Connected=1,Inquiry=0}; search.Size=(uint)Marshal.SizeOf(search);
  var info=new BtInfo(); info.Size=(uint)Marshal.SizeOf(info);
  var find=BluetoothFindFirstDevice(ref search,ref info);
  if(find==IntPtr.Zero) { int code=Marshal.GetLastWin32Error(); if(code!=259) throw new System.ComponentModel.Win32Exception(code); return map; }
  try { do { bool old; map[info.Address]=(map.TryGetValue(info.Address,out old) && old) || info.Connected!=0; info.Size=(uint)Marshal.SizeOf(info); } while(BluetoothFindNextDevice(find,ref info)); if(Marshal.GetLastWin32Error()!=259) throw new System.ComponentModel.Win32Exception(); } finally { BluetoothFindDeviceClose(find); }
  return map;
 }
 static bool? Connection(string id,Dictionary<ulong,bool> map) {
  var matches=Regex.Matches(id,@"(?i)(?<![0-9a-f])[0-9a-f]{12}(?![0-9a-f])");
  foreach(Match match in matches) { ulong address; bool connected; if(ulong.TryParse(match.Value,System.Globalization.NumberStyles.HexNumber,null,out address) && map.TryGetValue(address,out connected)) return connected; }
  return null;
 }
 public static List<Device> Scan(IEnumerable<string> known=null) {
  Dictionary<ulong,bool> links; try { try { links=Connections(); if(links.Count==0) links=EndpointConnections(); } catch { links=EndpointConnections(); } LinkDiagnostics="Bluetooth-Geräte: "+string.Join(", ",links.Select(x=>x.Key.ToString("X12")+"="+x.Value).ToArray()); } catch(Exception ex) { LinkDiagnostics=ex.ToString(); links=new Dictionary<ulong,bool>(); }
  var result=new List<Device>(); var h=SetupDiGetClassDevsW(IntPtr.Zero,null,IntPtr.Zero,6);
  if(h==new IntPtr(-1)) throw new System.ComponentModel.Win32Exception();
  try { for(uint i=0;;i++) { var d=new Info(); d.Size=Marshal.SizeOf(d); if(!SetupDiEnumDeviceInfo(h,i,ref d)) { if(Marshal.GetLastWin32Error()!=259) throw new System.ComponentModel.Win32Exception(); break; }
   var id=new StringBuilder(2048); uint n; if(!SetupDiGetDeviceInstanceIdW(h,ref d,id,2048,out n)) continue;
   string s=id.ToString(); if(!s.StartsWith("BTH",StringComparison.OrdinalIgnoreCase)) continue;
   uint ct; var cb=Prop(h,ref d,new Key("78c34fc8-104a-4aca-9ea4-524d52996e57",55),out ct); bool? link=(cb!=null && ct==17 && cb.Length==1) ? (bool?)(cb[0]!=0) : null;
   uint t; var b=Prop(h,ref d,new Key("104ea319-6ee2-4701-bd47-8ddbf425bbe5",2),out t);
   int? level=null; if(b!=null && t==3 && b.Length>=1 && b[0]<=100) level=b[0];
   if(b!=null && t==7 && b.Length>=4 && BitConverter.ToUInt32(b,0)<=100) level=(int)BitConverter.ToUInt32(b,0);
   string name=Str(h,ref d,14); if(name.Length==0) name=Str(h,ref d,2); if(name.Length==0) name=s;
   // Physical Bluetooth nodes and nodes that actually publish a battery value.
   if(level.HasValue || s.StartsWith("BTHENUM\\DEV_",StringComparison.OrdinalIgnoreCase) || s.StartsWith("BTHLE\\DEV_",StringComparison.OrdinalIgnoreCase)) result.Add(new Device{Id=s,Name=name,Battery=level,Connected=Connection(s,links) ?? link});
  }} finally { SetupDiDestroyDeviceInfoList(h); }
  if(known!=null) foreach(var id in known) if(!result.Any(d=>d.Id==id)) result.Add(new Device{Id=id,Name="Gespeicherter Kopfhörer",Connected=Connection(id,links)});
  return result.OrderByDescending(x=>x.Battery.HasValue).ThenBy(x=>x.Name).ToList();
 }
}
class Tray : ApplicationContext {
 List<NotifyIcon> icons=new List<NotifyIcon>(); List<Device> devices=new List<Device>(); List<string> selected=new List<string>();
 System.Windows.Forms.Timer timer=new System.Windows.Forms.Timer(); bool busy; string error="";
 string folder=AppPaths.Data;
 bool reloadPicker,exiting,firstRun; EventWaitHandle stopEvent; Form picker,settingsForm; Action updatePicker,updateStatus; Settings settings; WarningGate warnings=new WarningGate(); DateTime? lastScan; bool updateBusy; string updateStatusText="Noch nicht geprüft";
 string SettingsPath { get { return Path.Combine(folder,"options.ini"); } }
 public Tray(bool demo=false) {
  stopEvent=new EventWaitHandle(false,EventResetMode.ManualReset,AppPaths.StopEventName);
  try { selected=File.ReadAllLines(Path.Combine(folder,"devices.txt")).Where(x=>x.Length>0).Take(2).ToList(); } catch(IOException) {} catch(UnauthorizedAccessException) {}
  try { settings=Settings.Read(SettingsPath); } catch { settings=new Settings(); }
  if(demo) { selected=new List<string>{"demo-1","demo-2"}; devices=new List<Device>{new Device{Id="demo-1",Name="Kopfhörer 1",Battery=75,Connected=true},new Device{Id="demo-2",Name="Kopfhörer 2",Battery=65,Connected=true}}; lastScan=DateTime.Now; }
  firstRun=selected.Count==0;
  Render(); timer.Interval=1000; int elapsed=0; timer.Tick+=(s,e)=> { if(stopEvent.WaitOne(0)) { ExitThread(); return; } elapsed++; if(elapsed>=settings.Interval) { elapsed=0; Refresh(); } }; timer.Start(); if(!demo) { Refresh(); if(settings.CheckAtStartup) CheckUpdates(false); }
 }
 static GraphicsPath Rounded(float x,float y,float w,float h,float r) {
  var p=new GraphicsPath(); p.AddArc(x,y,r*2,r*2,180,90); p.AddArc(x+w-r*2,y,r*2,r*2,270,90); p.AddArc(x+w-r*2,y+h-r*2,r*2,r*2,0,90); p.AddArc(x,y+h-r*2,r*2,r*2,90,90); p.CloseFigure(); return p;
 }
 static Icon CopyIcon(Bitmap b) { var h=b.GetHicon(); try { return (Icon)Icon.FromHandle(h).Clone(); } finally { Native.DestroyIcon(h); } }
 public static Icon BatteryIcon(int? value,int slot,bool? connected=true,Color? custom=null,bool numbersOnly=false) {
  using(var b=new Bitmap(32,32)) using(var g=Graphics.FromImage(b)) {
   g.SmoothingMode=SmoothingMode.AntiAlias; g.Clear(Color.Transparent);
   Color bg=custom ?? (slot==0 ? Color.FromArgb(19,112,91) : Color.FromArgb(28,87,159));
   if(value.HasValue && value<=20) bg=Color.FromArgb(166,35,49);
   if(connected!=true) bg=connected==false ? Color.FromArgb(75,75,75) : Color.FromArgb(103,86,39);
   Color fg=0.2126*bg.R+0.7152*bg.G+0.0722*bg.B>155 ? Color.Black : Color.White;
   if(!numbersOnly) {
   using(var body=Rounded(0.6f,4.6f,28.2f,26.7f,4)) {
    using(var brush=new SolidBrush(bg)) g.FillPath(brush,body);
    using(var pen=new Pen(Color.FromArgb(230,245,245),1.3f)) g.DrawPath(pen,body);
   }
   using(var brush=new SolidBrush(Color.FromArgb(230,245,245))) g.FillRectangle(brush,29.4f,14,2.5f,10);
   }
   // The headphone badge is joined to the battery at its upper left corner.
   using(var badge=Rounded(0,0,18,15,3)) using(var brush=new SolidBrush(bg)) g.FillPath(brush,badge);
   using(var pen=new Pen(fg,2.1f)) {
    g.DrawArc(pen,2.5f,1.8f,12,12,180,180);
    g.DrawLine(pen,2.5f,7.4f,2.5f,12); g.DrawLine(pen,14.5f,7.4f,14.5f,12);
    g.DrawLine(pen,5,8.2f,5,13); g.DrawLine(pen,12,8.2f,12,13);
   }
   string text=connected==false ? "×" : value.HasValue ? value.Value.ToString() : connected==null ? "?" : "–";
   using(var path=new GraphicsPath()) using(var family=new FontFamily("Segoe UI")) {
    path.AddString(text,family,(int)FontStyle.Bold,value==100 ? 16f : 21f,PointF.Empty,StringFormat.GenericTypographic);
    var bounds=path.GetBounds(); float scale=Math.Min(1f,Math.Min(26.2f/bounds.Width,16.5f/bounds.Height)); using(var matrix=new Matrix()) { matrix.Translate(14.6f-bounds.Width*scale/2,22f-bounds.Height*scale/2); matrix.Scale(scale,scale); matrix.Translate(-bounds.Left,-bounds.Top); path.Transform(matrix); }
    if(numbersOnly) using(var outline=new Pen(fg,1.5f)) g.DrawPath(outline,path);
    using(var brush=new SolidBrush(numbersOnly ? bg : fg)) g.FillPath(brush,path);
   }
   return CopyIcon(b);
  }
 }
 void Refresh() {
  if(busy || exiting) return; busy=true;
  var known=selected.ToArray(); var worker=new System.ComponentModel.BackgroundWorker(); worker.DoWork+=(s,e)=>e.Result=Native.Scan(known);
  worker.RunWorkerCompleted+=(s,e)=> { if(exiting) { worker.Dispose(); return; } busy=false; error=e.Error==null ? "" : e.Error.Message; if(e.Error==null) { var fresh=(List<Device>)e.Result; foreach(var d in fresh.Where(x=>x.Name=="Gespeicherter Kopfhörer")) { var old=devices.FirstOrDefault(x=>x.Id==d.Id); if(old!=null) d.Name=old.Name; } devices=fresh; lastScan=DateTime.Now; } Render();
   if(e.Error==null) { var alerts=new List<string>(); foreach(var id in selected) { var d=devices.FirstOrDefault(x=>x.Id==id); if(warnings.Check(d,settings)) alerts.Add(settings.Name(d)+": noch "+d.Battery+" % Akku."); } if(alerts.Count>0 && icons.Count>0) icons[0].ShowBalloonTip(8000,"Kopfhörer-Akku niedrig",string.Join("\n",alerts.ToArray()),ToolTipIcon.Warning); }
   if(firstRun && e.Error==null) { firstRun=false; Pick(); }
   if(reloadPicker && updatePicker!=null) updatePicker(); reloadPicker=false; if(updateStatus!=null) updateStatus(); worker.Dispose(); }; worker.RunWorkerAsync();
 }
 ContextMenuStrip Menu() {
  var m=new ContextMenuStrip(); m.Items.Add("Kopfhörer-Akku").Enabled=false;
  foreach(var id in selected) { var d=devices.FirstOrDefault(x=>x.Id==id); m.Items.Add(d==null ? "Nicht verfügbar / getrennt" : Describe(d)).Enabled=false; }
  if(error.Length>0) m.Items.Add("Abfrage fehlgeschlagen: "+error).Enabled=false;
  m.Items.Add(updateBusy ? "Update-Prüfung läuft …" : "Nach Updates suchen …",null,(s,e)=>CheckUpdates(true)).Enabled=!updateBusy;
  m.Items.Add("Einstellungen …",null,(s,e)=>Options()); m.Items.Add("Geräte auswählen …",null,(s,e)=>Pick()); m.Items.Add("Jetzt aktualisieren",null,(s,e)=>Refresh());
  string startup=AppPaths.Startup;
  var auto=new ToolStripMenuItem("Mit Windows starten"); auto.Checked=File.Exists(startup);
  auto.Click+=(s,e)=> { try { if(File.Exists(startup)) File.Delete(startup); else Program.EnableAutostart(); Render(); } catch(Exception ex) { MessageBox.Show("Autostart konnte nicht geändert werden: "+ex.Message); } };
  m.Items.Add(auto); m.Items.Add("Über Kopfhörer-Akku …",null,(s,e)=>About()); m.Items.Add("Beenden",null,(s,e)=>ExitThread()); return m;
 }
 void Render() {
  int count=Math.Max(1,selected.Count);
  while(icons.Count<count) { var ni=new NotifyIcon(); ni.DoubleClick+=(s,e)=>Options(); icons.Add(ni); }
  while(icons.Count>count) { var ni=icons[icons.Count-1]; ni.Visible=false; if(ni.Icon!=null) ni.Icon.Dispose(); ni.Dispose(); icons.RemoveAt(icons.Count-1); }
  for(int i=0;i<count;i++) { var ni=icons[i]; var d=selected.Count>i ? devices.FirstOrDefault(x=>x.Id==selected[i]) : null;
   var old=ni.Icon; ni.Icon=BatteryIcon(error.Length==0 && d!=null ? d.Battery : null,i,error.Length==0 && d!=null ? d.Connected : null,settings.Profile(selected.Count>i ? selected[i] : null,i).Color,settings.NumbersOnly); if(old!=null) old.Dispose();
   string tip=d!=null ? Describe(d) : selected.Count==0 ? "Kopfhörer-Akku · Doppelklick: Einstellungen" : "Kopfhörer "+(i+1)+" · Verbindung unbekannt";
   if(error.Length>0) tip="Kopfhörer "+(i+1)+" · Abfrage fehlgeschlagen"; else if(lastScan.HasValue) tip+=" · "+lastScan.Value.ToString("HH:mm:ss"); ni.Text=tip.Length>63 ? tip.Substring(0,63) : tip;
   var menu=ni.ContextMenuStrip; ni.ContextMenuStrip=Menu(); if(menu!=null) menu.Dispose(); ni.Visible=true;
  }
 }
 void About() {
  MessageBox.Show("Kopfhörer-Akku "+AppPaths.Version+"\nHerausgeber: Aldikiller\n\nKostenlos und Open Source unter der MIT-Lizenz.\nOhne Gewährleistung, soweit gesetzlich zulässig.\n\nKeine Telemetrie. Freiwillige Update-Prüfung über GitHub.\nWindows-Akkuwerte können verzögert oder gespeichert sein.\n\nProjekt: github.com/Aldikiller/KopfhoererAkku", "Über Kopfhörer-Akku",MessageBoxButtons.OK,MessageBoxIcon.Information);
 }
 string Describe(Device d) { return settings.Name(d)+" · "+d.Status+" · "+(d.Connected==false ? "kein aktueller Akkustand" : d.Battery.HasValue ? d.Battery+(d.Connected==null ? " % (gespeichert)" : " %") : "kein Akkustand"); }
 void CheckUpdates(bool manual) {
  if(updateBusy || exiting) return; updateBusy=true; updateStatusText="GitHub wird abgefragt …"; if(updateStatus!=null) updateStatus(); Render();
  var worker=new System.ComponentModel.BackgroundWorker(); worker.DoWork+=(a,b)=>b.Result=UpdateService.Check();
  worker.RunWorkerCompleted+=(a,b)=> { if(exiting) { worker.Dispose(); return; } updateBusy=false;
   if(b.Error!=null) { updateStatusText="Prüfung fehlgeschlagen. Internetverbindung oder GitHub-Limit prüfen."; if(manual) MessageBox.Show(updateStatusText,"Updates",MessageBoxButtons.OK,MessageBoxIcon.Information); }
   else { var latest=b.Result as ReleaseVersion; updateStatusText=latest==null ? "Keine neuere passende Version gefunden ("+DateTime.Now.ToString("HH:mm")+")." : "Neue Version verfügbar: "+latest.Tag;
    if(latest!=null && manual) { if(MessageBox.Show("Version "+latest.Tag+" ist verfügbar.\nInstalliert: "+AppPaths.Version+"\n\nGitHub-Downloadseite im Browser öffnen?","Update verfügbar",MessageBoxButtons.YesNo,MessageBoxIcon.Information)==DialogResult.Yes) OpenPage(UpdateService.Page(latest)); }
    else if(latest!=null && icons.Count>0) icons[0].ShowBalloonTip(8000,"Update verfügbar",latest.Tag+" ist verfügbar. Rechtsklick → Nach Updates suchen.",ToolTipIcon.Info);
    else if(manual) MessageBox.Show(updateStatusText,"Updates",MessageBoxButtons.OK,MessageBoxIcon.Information);
   }
   if(updateStatus!=null) updateStatus(); Render(); worker.Dispose();
  }; worker.RunWorkerAsync();
 }
 void OpenPage(string page) { try { Process.Start(new ProcessStartInfo(page){UseShellExecute=true}); } catch(Exception ex) { MessageBox.Show("Browser konnte nicht geöffnet werden: "+ex.Message); } }
 public void Options(bool preview=false) {
  if(settingsForm!=null) { settingsForm.Activate(); return; }
  var f=new Form{Text="Kopfhörer-Akku · Einstellungen",ClientSize=new Size(610,550),FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,StartPosition=FormStartPosition.CenterScreen,AutoScaleMode=AutoScaleMode.Dpi}; settingsForm=f;
  var tabs=new TabControl{Left=12,Top=12,Width=586,Height=478}; var general=new TabPage("Allgemein"); var appearance=new TabPage("Namen & Anzeige"); var updates=new TabPage("Updates"); tabs.TabPages.AddRange(new[]{general,appearance,updates});
  var status=new Label{Left=14,Top=14,Width=544,Height=112}; var updateLabel=new Label{Left=14,Top=190,Width=538,Height=60};
  updateStatus=()=> { status.Text=string.Join("\n",selected.Select(id=> { var d=devices.FirstOrDefault(x=>x.Id==id); return d==null ? "Gerät nicht gefunden · Verbindung unbekannt" : Describe(d); }).ToArray())+"\n\nLetzte Abfrage: "+(lastScan.HasValue ? lastScan.Value.ToString("HH:mm:ss") : "noch keine")+(error.Length>0 ? "\nFehler: "+error : ""); updateLabel.Text=updateStatusText; }; updateStatus();
  var warn=new CheckBox{Left=14,Top=136,Width=530,Text="Bei niedrigem Akku benachrichtigen",Checked=settings.Warnings};
  var threshold=new NumericUpDown{Left=245,Top=175,Width=75,Minimum=1,Maximum=90,Value=settings.Threshold};
  var interval=new NumericUpDown{Left=245,Top=217,Width=75,Minimum=5,Maximum=300,Value=settings.Interval};
  var hint=new Label{Left=14,Top=268,Width=538,Height=92,Text="Beide ausgewählten Kopfhörer behalten ein eigenes Symbol.\nRot: niedriger Akku · Grau ×: getrennt · Gelb: Status unbekannt.\nWarnungen erscheinen nur bei bestätigter Verbindung.\nEigene Namen ändern den Bluetooth-Namen in Windows nicht."};
  var test=new Button{Left=14,Top=376,Width=180,Height=32,Text="Benachrichtigung testen"}; test.Click+=(a,b)=> { if(icons.Count>0) icons[0].ShowBalloonTip(5000,"Kopfhörer-Akku · Test","So erscheint eine Akkuwarnung.",ToolTipIcon.Info); };
  general.Controls.AddRange(new Control[]{status,warn,new Label{Left=14,Top=178,Width=225,Text="Warnung bei / unter (%)"},threshold,new Label{Left=14,Top=220,Width=225,Text="Abfrage alle (Sekunden)"},interval,hint,test});
  var names=new Dictionary<string,TextBox>(); var colors=new Dictionary<string,Color>(); var previews=new List<PictureBox>(); var ids=selected.ToArray();
  var style=new ComboBox{Left=14,Top=352,Width=340,DropDownStyle=ComboBoxStyle.DropDownList}; style.Items.AddRange(new[]{"Batterie mit Zahl und Kopfhörerzeichen","Nur Zahl mit Kopfhörerzeichen"}); style.SelectedIndex=settings.NumbersOnly ? 1 : 0;
  Action refreshPreview=()=> { for(int i=0;i<ids.Length;i++) { var picture=previews[i]; var old=picture.Image; using(var icon=BatteryIcon(i==0 ? 75 : 65,i,true,colors[ids[i]],style.SelectedIndex==1)) picture.Image=icon.ToBitmap(); if(old!=null) old.Dispose(); } };
  for(int i=0;i<ids.Length;i++) {
   string id=ids[i]; int slot=i; var d=devices.FirstOrDefault(x=>x.Id==id); var profile=settings.Profile(id,i); colors[id]=profile.Color;
   int top=16+i*137; var name=new TextBox{Left=14,Top=top+27,Width=335,MaxLength=40,Text=profile.Name}; names[id]=name;
   var colorButton=new Button{Left=365,Top=top+25,Width=105,Height=29,Text="Farbe …",BackColor=colors[id],ForeColor=Color.White,UseVisualStyleBackColor=false};
   colorButton.Click+=(a,b)=> { using(var dialog=new ColorDialog{Color=colors[id],FullOpen=true}) if(dialog.ShowDialog(f)==DialogResult.OK) { colors[id]=dialog.Color; colorButton.BackColor=dialog.Color; colorButton.ForeColor=dialog.Color.GetBrightness()>0.55f ? Color.Black : Color.White; refreshPreview(); } };
   var reset=new Button{Left=365,Top=top+60,Width=105,Height=28,Text="Standard"}; reset.Click+=(a,b)=> { colors[id]=slot==0 ? ColorTranslator.FromHtml("#13705B") : ColorTranslator.FromHtml("#1C579F"); name.Text=""; colorButton.BackColor=colors[id]; colorButton.ForeColor=Color.White; refreshPreview(); };
   var picture=new PictureBox{Left=492,Top=top+25,Width=48,Height=48,SizeMode=PictureBoxSizeMode.Zoom,BackColor=Color.FromArgb(32,32,32)}; previews.Add(picture);
   appearance.Controls.AddRange(new Control[]{new Label{Left=14,Top=top,Width=535,Text=(i+1)+". "+(d==null ? "Gespeicherter Kopfhörer" : d.DisplayName)},name,colorButton,reset,picture,new Label{Left=14,Top=top+63,Width=335,Height=38,Text="Eigener Name; leer lassen für den Gerätenamen."}});
  }
  if(ids.Length==0) appearance.Controls.Add(new Label{Left=14,Top=20,Width=535,Height=50,Text="Zuerst unter Geräte auswählen einen oder zwei Kopfhörer festlegen."});
  appearance.Controls.AddRange(new Control[]{new Label{Left=14,Top=322,Width=530,Text="Darstellung für beide Symbole"},style,new Label{Left=14,Top=392,Width=535,Height=42,Text="Vorschau mit Beispielwerten. Statusfarben haben Vorrang vor deiner Farbe."}}); style.SelectedIndexChanged+=(a,b)=>refreshPreview(); refreshPreview();
  var autoCheck=new CheckBox{Left=14,Top=20,Width=535,Text="Beim Programmstart nach neuen Versionen suchen",Checked=settings.CheckAtStartup};
  var privacy=new Label{Left=14,Top=62,Width=535,Height=98,Text="Freiwillig und zunächst ausgeschaltet. Die Prüfung ruft öffentliche\nRelease-Daten von GitHub ab. GitHub erhält dabei deine IP-Adresse\nund die Programmversion. Gerätenamen und Akkudaten werden nicht gesendet.\nEs werden keine Updates automatisch heruntergeladen oder installiert."};
  var check=new Button{Left=14,Top=264,Width=170,Height=32,Text="Jetzt nach Updates suchen"}; check.Click+=(a,b)=>CheckUpdates(true);
  var downloads=new Button{Left=200,Top=264,Width=170,Height=32,Text="GitHub-Downloads öffnen"}; downloads.Click+=(a,b)=>OpenPage(UpdateService.ReleasesPage);
  updates.Controls.AddRange(new Control[]{autoCheck,privacy,new Label{Left=14,Top=169,Width=530,Text="Installiert: "+AppPaths.Version},updateLabel,check,downloads});
  var save=new Button{Left=14,Top=506,Width=125,Height=32,Text="Speichern"}; save.Click+=(a,b)=> {
   var next=new Settings{Warnings=warn.Checked,Threshold=(int)threshold.Value,Interval=(int)interval.Value,CheckAtStartup=autoCheck.Checked,NumbersOnly=style.SelectedIndex==1};
   foreach(var item in settings.Profiles) next.Profiles[item.Key]=new DeviceProfile(item.Value.ColorHex){Name=item.Value.Name};
   foreach(var id in ids) next.Profiles[id]=new DeviceProfile("#"+colors[id].R.ToString("X2")+colors[id].G.ToString("X2")+colors[id].B.ToString("X2")){Name=names[id].Text.Trim()};
   try { next.Save(SettingsPath); if(next.Threshold!=settings.Threshold || next.Warnings!=settings.Warnings) warnings.Reset(); settings=next; Render(); Refresh(); f.Close(); } catch(Exception ex) { MessageBox.Show("Speichern fehlgeschlagen: "+ex.Message); }
  };
  var choose=new Button{Left=151,Top=506,Width=145,Height=32,Text="Geräte auswählen"}; choose.Click+=(a,b)=>Pick();
  var refresh=new Button{Left=308,Top=506,Width=125,Height=32,Text="Aktualisieren"}; refresh.Click+=(a,b)=>Refresh();
  var cancel=new Button{Left=445,Top=506,Width=145,Height=32,Text="Schließen"}; cancel.Click+=(a,b)=>f.Close();
  f.Controls.AddRange(new Control[]{tabs,save,choose,refresh,cancel}); f.FormClosed+=(a,b)=> { updateStatus=null; settingsForm=null; foreach(var picture in previews) if(picture.Image!=null) picture.Image.Dispose(); f.Dispose(); }; f.Show();
  if(preview) { File.WriteAllText(AppPaths.Report("Symbol-Check.txt"),"Ausgewählte Geräte: "+selected.Count+"; angelegte Symbole: "+icons.Count+"; sichtbar an Windows übergeben: "+icons.Count(x=>x.Visible)); foreach(TabPage tab in tabs.TabPages) { tabs.SelectedTab=tab; using(var bitmap=new Bitmap(f.Width,f.Height)) { f.DrawToBitmap(bitmap,new Rectangle(0,0,f.Width,f.Height)); bitmap.Save(AppPaths.Report("Einstellungen-"+tabs.SelectedIndex+".png")); } } f.Close(); }
 }
 void Pick() {
  if(picker!=null) { picker.Activate(); return; }
  var f=new Form{Text="Kopfhörer-Akku – Geräte auswählen",ClientSize=new Size(560,350),StartPosition=FormStartPosition.CenterScreen,MinimumSize=new Size(576,389)}; picker=f;
  var label=new Label{Text="Bis zu zwei Kopfhörer auswählen. Namen und Farben lassen sich in den Einstellungen ändern.\nEin Strich im Symbol bedeutet: Windows liefert gerade keinen Akkustand.",Dock=DockStyle.Top,Height=55,Padding=new Padding(10)};
  var list=new CheckedListBox{Dock=DockStyle.Fill,CheckOnClick=true,HorizontalScrollbar=true};
  Action populate=()=> { list.Items.Clear(); foreach(var d in devices) list.Items.Add(d,selected.Contains(d.Id)); foreach(var id in selected.Where(id=>!devices.Any(d=>d.Id==id))) list.Items.Add(new Device{Id=id,Name="Gespeichertes Gerät (nicht verfügbar)"},true); };
  updatePicker=populate; populate(); var panel=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=75,Padding=new Padding(8)};
  var save=new Button{Text="Übernehmen",Width=120,Height=30}; save.Click+=(s,e)=> { if(list.CheckedItems.Count>2) { MessageBox.Show("Bitte höchstens zwei Geräte auswählen."); return; }
   var ids=list.CheckedItems.Cast<Device>().Select(d=>d.Id).ToList(); try { Directory.CreateDirectory(folder); File.WriteAllLines(Path.Combine(folder,"devices.txt"),ids); selected=ids; Render(); f.Close(); } catch(Exception ex) { MessageBox.Show("Speichern fehlgeschlagen: "+ex.Message); } };
  var reload=new Button{Text="Neu suchen",Width=120,Height=30}; reload.Click+=(s,e)=> { reloadPicker=true; Refresh(); };
  var hint=new Label{Text="Kopfhörer vor der Suche einschalten und mit Windows verbinden.",AutoSize=true}; panel.Controls.Add(save); panel.Controls.Add(reload); panel.Controls.Add(hint);
  f.Controls.Add(list); f.Controls.Add(label); f.Controls.Add(panel); f.FormClosed+=(s,e)=> { reloadPicker=false; picker=null; updatePicker=null; f.Dispose(); }; f.Show();
 }
 protected override void ExitThreadCore() { if(exiting) return; exiting=true; timer.Stop(); stopEvent.Dispose(); timer.Dispose(); if(settingsForm!=null) settingsForm.Close(); foreach(var ni in icons) { ni.Visible=false; if(ni.Icon!=null) ni.Icon.Dispose(); if(ni.ContextMenuStrip!=null) ni.ContextMenuStrip.Dispose(); ni.Dispose(); } if(picker!=null) picker.Close(); base.ExitThreadCore(); }
}
static class Program {
 public static void EnableAutostart() {
  object shell=null,shortcut=null;
  try {
   shell=Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell",true));
   shortcut=shell.GetType().InvokeMember("CreateShortcut",BindingFlags.InvokeMethod,null,shell,new object[]{AppPaths.Startup});
   shortcut.GetType().InvokeMember("TargetPath",BindingFlags.SetProperty,null,shortcut,new object[]{Application.ExecutablePath});
   shortcut.GetType().InvokeMember("WorkingDirectory",BindingFlags.SetProperty,null,shortcut,new object[]{AppPaths.Base});
   shortcut.GetType().InvokeMember("Description",BindingFlags.SetProperty,null,shortcut,new object[]{"Bluetooth-Kopfhörer-Akkuanzeige"});
   shortcut.GetType().InvokeMember("Save",BindingFlags.InvokeMethod,null,shortcut,null);
  } finally { if(shortcut!=null) Marshal.FinalReleaseComObject(shortcut); if(shell!=null) Marshal.FinalReleaseComObject(shell); }
 }
 public static bool StopRunning() {
  try { using(var stop=EventWaitHandle.OpenExisting(AppPaths.StopEventName)) stop.Set(); }
  catch(WaitHandleCannotBeOpenedException) { return true; }
  using(var mutex=new Mutex(false,AppPaths.MutexName)) { bool acquired=false; try { try { acquired=mutex.WaitOne(10000); } catch(AbandonedMutexException) { acquired=true; } return acquired; } finally { if(acquired) mutex.ReleaseMutex(); } }
 }
 static void SelfTest() {
  var config=new Settings(); var gate=new WarningGate(); var d=new Device{Id="test",Name="Test",Battery=15,Connected=true};
  Action<bool,string> check=(ok,message)=> { if(!ok) throw new Exception(message); };
  check(gate.Check(d,config),"initial low warning"); check(!gate.Check(d,config),"no repeated warnings");
  d.Connected=null; check(!gate.Check(d,config),"unknown link cannot warn"); d.Connected=false; check(!gate.Check(d,config),"disconnected cannot warn");
  d.Connected=true; check(gate.Check(d,config),"reconnect warns again"); d.Battery=30; check(!gate.Check(d,config),"recovery resets");
  d.Battery=20; check(gate.Check(d,config),"inclusive threshold"); d.Battery=null; check(!gate.Check(d,config),"missing battery cannot warn");
  gate.Reset(); config.Warnings=false; d.Battery=5; check(!gate.Check(d,config),"disabled warnings");
  foreach(bool? connected in new bool?[]{true,false,null}) foreach(int? level in new int?[]{0,1,20,70,100,null}) using(var icon=Tray.BatteryIcon(level,0,connected)) check(icon.Width==32,"icon creation");
  string file=AppPaths.Report("selftest-"+Guid.NewGuid().ToString("N")+".ini"); Directory.CreateDirectory(Path.GetDirectoryName(file)); config.Threshold=35; config.Interval=42; config.Save(file); var loaded=Settings.Read(file); File.Delete(file); check(loaded.Threshold==35 && loaded.Interval==42 && !loaded.Warnings,"settings roundtrip");
  string profileFile=AppPaths.Report("profiles-test-"+Guid.NewGuid().ToString("N")+".ini");
  config.NumbersOnly=true; config.CheckAtStartup=true; config.Profiles["BTH\\TEST=ä"]=new DeviceProfile("#FFEE00"){Name="Büro = Musik 🎧"}; config.Save(profileFile); config.Save(profileFile); loaded=Settings.Read(profileFile); File.Delete(profileFile);
  check(loaded.NumbersOnly && loaded.CheckAtStartup && loaded.Profiles["BTH\\TEST=ä"].Name=="Büro = Musik 🎧" && loaded.Profiles["BTH\\TEST=ä"].ColorHex=="#FFEE00","profile roundtrip and atomic replacement");
  check(!new Settings().CheckAtStartup,"updates opt in");
  check(ReleaseVersion.Parse("v1.0.0-beta.10").CompareTo(ReleaseVersion.Parse("1.0.0-beta.2"))>0,"numeric prerelease comparison");
  check(ReleaseVersion.Parse("1.0.0").CompareTo(ReleaseVersion.Parse("1.0.0-rc.1"))>0,"stable newer than prerelease");
  check(ReleaseVersion.Parse("1.0.0-beta.01")==null && ReleaseVersion.Parse("https://evil.example/")==null,"invalid versions rejected");
  string json="[{\"draft\":false,\"tag_name\":\"v0.1.0-beta.10\",\"prerelease\":true},{\"draft\":true,\"tag_name\":\"v99.0.0\"},{\"draft\":false,\"tag_name\":\"v0.1.0-beta.2\"}]";
  check(UpdateService.Select(json,"0.1.0-beta.2").Tag=="v0.1.0-beta.10","latest beta selection");
  check(UpdateService.Select(json,"0.1.0")==null,"stable excludes previews");
  check(UpdateService.Select("[{\"draft\":false,\"tag_name\":\"v0.1.0-beta.1\"}]","0.1.0-beta.2")==null,"no downgrade");
  foreach(bool? state in new bool?[]{true,false,null}) foreach(int? level in new int?[]{0,1,20,70,100,null}) foreach(bool numeric in new[]{false,true}) using(var icon=Tray.BatteryIcon(level,1,state,Color.Yellow,numeric)) check(icon.Width==32,"custom style/status icons");
  File.WriteAllText(AppPaths.Report("Testbericht.txt"),"OK: Warnschwelle, Wiederholschutz, Wiederverbindung, fehlende Daten, deaktivierte Warnungen, Warnlogik, 54 Symbolvarianten, Namen/Farben/Anzeige speichern, Update-Versionsvergleich, Beta-Kanal, kein Downgrade, Updates standardmäßig aus.");
 }
 [STAThread] static void Main(string[] args) {
  var output=args.FirstOrDefault(x=>x.StartsWith("--output-dir=",StringComparison.Ordinal)); if(output!=null) AppPaths.Output=Path.GetFullPath(output.Substring(13));
  if(args.Contains("--shutdown")) { if(!StopRunning()) Environment.ExitCode=2; return; }
  if(args.Contains("--version")) { File.WriteAllText(AppPaths.Report("Version.txt"),AppPaths.Version); return; }
  if(args.Contains("--disable-autostart")) { try { if(File.Exists(AppPaths.Startup)) File.Delete(AppPaths.Startup); } catch { Environment.ExitCode=1; } return; }
  if(args.Contains("--export-icon")) { using(var icon=Tray.BatteryIcon(75,0)) using(var stream=File.Create(AppPaths.Report("app.ico"))) icon.Save(stream); return; }
  if(args.Contains("--selftest")) { try { SelfTest(); } catch(Exception e) { File.WriteAllText(AppPaths.Report("Testbericht.txt"),"FEHLER: "+e); Environment.ExitCode=1; } return; }
  Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
  if(args.Contains("--ui-check")) { var context=new Tray(true); context.Options(true); context.ExitThread(); context.Dispose(); return; }
  if(args.Contains("--diagnose")) { try { File.WriteAllLines(AppPaths.Report("Diagnose.txt"),Native.Scan().Select(d=>d.ToString()+"\t"+d.Id).Concat(new[]{Native.LinkDiagnostics})); } catch(Exception e) { File.WriteAllText(AppPaths.Report("Diagnose.txt"),e.ToString()); } return; }
  if(args.Contains("--preview")) { using(var b=new Bitmap(440,180)) using(var g=Graphics.FromImage(b)) { g.Clear(Color.FromArgb(28,28,28)); int row=0; foreach(bool numeric in new[]{false,true}) { int i=0; foreach(int? n in new int?[]{90,70,1,100,null}) { using(var icon=Tray.BatteryIcon(n,i%2,true,i%2==0 ? Color.FromArgb(19,112,91) : Color.FromArgb(28,87,159),numeric)) { g.DrawIcon(icon,12+i*84,12+row*85); using(var image=icon.ToBitmap()) g.DrawImage(image,new Rectangle(20+i*84,58+row*85,16,16)); } i++; } row++; } b.Save(AppPaths.Report("Vorschau.png")); } return; }
  if(args.Contains("--update-check")) { try { var latest=UpdateService.Check(); File.WriteAllText(AppPaths.Report("Update-Check.txt"),latest==null ? "OK: keine neuere Version" : "OK: "+latest.Tag); } catch(Exception e) { File.WriteAllText(AppPaths.Report("Update-Check.txt"),"FEHLER: "+e); Environment.ExitCode=1; } return; }

  if(args.Contains("--enable-autostart")) { try { EnableAutostart(); File.WriteAllText(AppPaths.Report("Autostart-Status.txt"),"Aktiv: Windows-Autostartverknüpfung"); } catch(Exception e) { File.WriteAllText(AppPaths.Report("Autostart-Status.txt"),e.ToString()); Environment.ExitCode=1; } return; }

  if(args.Contains("--replace") && !StopRunning()) { MessageBox.Show("Die laufende Version konnte nicht beendet werden. Bitte über das Symbol beenden."); return; }
  bool created; using(var mutex=new Mutex(true,AppPaths.MutexName,out created)) {
   if(!created) { MessageBox.Show("Kopfhörer-Akku läuft bereits. Die Symbole stehen ggf. unter ^ neben der Uhr.","Kopfhörer-Akku"); return; }
   try { Application.Run(new Tray()); } catch(Exception ex) { MessageBox.Show("Kopfhörer-Akku konnte nicht gestartet werden: "+ex.Message,"Kopfhörer-Akku",MessageBoxButtons.OK,MessageBoxIcon.Error); Environment.ExitCode=1; }
  }
 }
}
