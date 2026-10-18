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
[assembly: AssemblyVersion("1.3.1.0")]
[assembly: AssemblyFileVersion("1.3.1.0")]
[assembly: AssemblyInformationalVersion("1.3.1")]
[assembly: AssemblyCopyright("Copyright © 2026 Aldikiller")]

static class AppPaths {
 public const string Version="1.3.1";
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
 public string Status { get { return Connected==true ? L.T("Verbunden") : Connected==false ? L.T("Getrennt") : L.T("Verbindung unbekannt"); } }
 public override string ToString() { return DisplayName + " · " + Status + " · " + (Connected==false ? L.T("kein aktueller Akkustand") : Battery.HasValue ? Battery + (Connected==null ? L.T(" % (gespeichert)") : " %") : L.T("kein Akkustand")); }
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
  var clock=System.Diagnostics.Stopwatch.StartNew(); while(op.Status==Windows.Foundation.AsyncStatus.Started && clock.ElapsedMilliseconds<5000) Thread.Sleep(20); if(op.Status==Windows.Foundation.AsyncStatus.Started) { op.Cancel(); throw new TimeoutException(L.T("Bluetooth-Verbindungsabfrage dauert zu lange.")); }
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
  if(known!=null) foreach(var id in known) if(!result.Any(d=>d.Id==id)) result.Add(new Device{Id=id,Name=L.T("Gespeicherter Kopfhörer"),Connected=Connection(id,links)});
  return result.OrderByDescending(x=>x.Battery.HasValue).ThenBy(x=>x.Name).ToList();
 }
}
class Tray : ApplicationContext {
 BatteryWidget widget; DateTime noticeUntil=DateTime.MinValue;
 List<NotifyIcon> icons=new List<NotifyIcon>(); List<Device> devices=new List<Device>(); List<string> selected=new List<string>();
 System.Windows.Forms.Timer timer=new System.Windows.Forms.Timer(); bool busy; string error="";
 string folder=AppPaths.Data;
 bool reloadPicker,exiting,firstRun; EventWaitHandle stopEvent; Form picker,settingsForm; Action updatePicker,updateStatus; Settings settings; WarningGate warnings=new WarningGate(); DateTime? lastScan; bool updateBusy; string updateStatusText=L.T("Noch nicht geprüft");
 string SettingsPath { get { return Path.Combine(folder,"options.ini"); } }
 public Tray(bool demo=false,string languageOverride=null,string folderOverride=null) {
  if(folderOverride!=null) folder=folderOverride;
  stopEvent=new EventWaitHandle(false,EventResetMode.ManualReset,AppPaths.StopEventName);
  try { selected=File.ReadAllLines(Path.Combine(folder,"devices.txt")).Where(x=>x.Length>0).Take(2).ToList(); } catch(IOException) {} catch(UnauthorizedAccessException) {}
  try { settings=Settings.Read(SettingsPath); } catch { settings=new Settings(); }
  if(languageOverride!=null) settings.Language=languageOverride; L.Language=settings.Language; updateStatusText=L.T("Noch nicht geprüft");
  if(demo) { selected=new List<string>{"demo-1","demo-2"}; devices=new List<Device>{new Device{Id="demo-1",Name="Kopfhörer 1",Battery=75,Connected=true},new Device{Id="demo-2",Name="Kopfhörer 2",Battery=65,Connected=true}}; lastScan=DateTime.Now; }
  firstRun=selected.Count==0;
  Render(); timer.Interval=1000; int elapsed=0; timer.Tick+=(s,e)=> { if(stopEvent.WaitOne(0)) { ExitThread(); return; } if(noticeUntil!=DateTime.MinValue && DateTime.Now>noticeUntil) { noticeUntil=DateTime.MinValue; Render(); } elapsed++; if(elapsed>=settings.Interval) { elapsed=0; Refresh(); } }; timer.Start(); if(!demo) { Refresh(); if(settings.CheckAtStartup) CheckUpdates(false); }
 }
 static GraphicsPath Rounded(float x,float y,float w,float h,float r) {
  var p=new GraphicsPath(); r=Math.Max(0,Math.Min(r,Math.Min(w,h)/2)); if(r<0.1f) { p.AddRectangle(new RectangleF(x,y,w,h)); return p; } p.AddArc(x,y,r*2,r*2,180,90); p.AddArc(x+w-r*2,y,r*2,r*2,270,90); p.AddArc(x+w-r*2,y+h-r*2,r*2,r*2,0,90); p.AddArc(x,y+h-r*2,r*2,r*2,90,90); p.CloseFigure(); return p;
 }
 static Icon CopyIcon(Bitmap b) { var h=b.GetHicon(); try { return (Icon)Icon.FromHandle(h).Clone(); } finally { Native.DestroyIcon(h); } }
 public static Icon BatteryIcon(int? value,int slot,bool? connected=true,Color? custom=null,bool numbersOnly=false,Settings look=null,int symbol=0,string marker="A") {
  using(var b=new Bitmap(32,32)) using(var g=Graphics.FromImage(b)) {
   g.SmoothingMode=SmoothingMode.AntiAlias; g.Clear(Color.Transparent);
   Color bg=custom ?? (slot==0 ? Color.FromArgb(19,112,91) : Color.FromArgb(28,87,159));
   if(value.HasValue && value<=20) bg=Color.FromArgb(166,35,49);
   if(connected!=true) bg=connected==false ? Color.FromArgb(75,75,75) : Color.FromArgb(103,86,39);
   Color fg=0.2126*bg.R+0.7152*bg.G+0.0722*bg.B>155 ? Color.Black : Color.White;
   float stroke=look==null ? 1.3f : look.BatteryStroke/10f;
   float width=look==null ? 28 : look.BatteryWidth, height=look==null ? 27 : look.BatteryHeight;
   // Leave room for the outline and terminal inside the Windows icon canvas.
   float left=(29-width)/2f, top=32-height-stroke/2f;
   float bodyWidth=width-stroke, bodyHeight=height-stroke;
   if(!numbersOnly) {
   using(var body=Rounded(left+stroke/2f,top,bodyWidth,bodyHeight,look==null ? 4 : look.BatteryRadius)) {
    using(var brush=new SolidBrush(bg)) g.FillPath(brush,body);
    using(var pen=new Pen(Color.FromArgb(230,245,245),stroke)) g.DrawPath(pen,body);
   }
   using(var brush=new SolidBrush(Color.FromArgb(230,245,245))) g.FillRectangle(brush,left+width,top+bodyHeight*0.32f,2,bodyHeight*0.36f);
   }
   // Draw only the mark's strokes: gaps retain the original battery pixels.
   using(var mark=new GraphicsPath()) {
    bool filled=false;
    if(symbol==0) {
     mark.AddArc(2.5f,1.8f,12,12,180,180);
     mark.StartFigure(); mark.AddLine(2.5f,7.4f,2.5f,12);
     mark.StartFigure(); mark.AddLine(14.5f,7.4f,14.5f,12);
     mark.StartFigure(); mark.AddLine(5,8.2f,5,13);
     mark.StartFigure(); mark.AddLine(12,8.2f,12,13);
    } else if(symbol==1) {
     mark.AddLine(7,11,7,2); mark.AddLine(7,2,14,1); mark.AddLine(14,1,14,10);
     mark.StartFigure(); mark.AddEllipse(2,9,5,4); mark.StartFigure(); mark.AddEllipse(9,8,5,4);
    } else if(symbol==2) {
     mark.AddPolygon(new[]{new PointF(2,5),new PointF(6,5),new PointF(10,2),new PointF(10,13),new PointF(6,10),new PointF(2,10)});
     mark.StartFigure(); mark.AddArc(8,3,8,9,-65,130);
    } else if(symbol==3) {
     using(var family=look==null ? new FontFamily("Arial") : look.FontFamily()) mark.AddString(Settings.CleanMarker(marker),family,(int)FontStyle.Bold,15,PointF.Empty,StringFormat.GenericTypographic);
     var box=mark.GetBounds(); if(box.Width>0 && box.Height>0) using(var matrix=new Matrix()) { float scale=Math.Min(14/box.Width,12/box.Height); matrix.Translate(2,1); matrix.Scale(scale,scale); matrix.Translate(-box.X,-box.Y); mark.Transform(matrix); } filled=true;
    }
    if(symbol!=4) { using(var outline=new Pen(fg==Color.Black ? Color.White : Color.Black,filled ? 1.2f : 3.3f)) g.DrawPath(outline,mark);
     if(filled) { using(var brush=new SolidBrush(fg)) g.FillPath(brush,mark); } else using(var pen=new Pen(fg,2.1f)) g.DrawPath(pen,mark);
    }
   }
   string text=connected==false ? "×" : value.HasValue ? value.Value.ToString() : connected==null ? "?" : "–";
   using(var path=new GraphicsPath()) using(var family=(look==null ? new FontFamily("Segoe UI") : look.FontFamily())) {
    path.AddString(text,family,(int)FontStyle.Bold,(look==null ? 21f : look.TextSize)*(value==100 ? 16f/21f : 1f),PointF.Empty,StringFormat.GenericTypographic);
    var bounds=path.GetBounds(); float availableWidth=numbersOnly ? 26.2f : bodyWidth-2, availableHeight=numbersOnly ? 16.5f : Math.Min(16.5f,bodyHeight-3); float scale=Math.Min(1f,Math.Min(availableWidth/bounds.Width,availableHeight/bounds.Height)); using(var matrix=new Matrix()) { matrix.Translate((numbersOnly ? 14.6f : left+width/2)-bounds.Width*scale/2,(numbersOnly ? 22f : top+bodyHeight-availableHeight/2-1.5f)-bounds.Height*scale/2); matrix.Scale(scale,scale); matrix.Translate(-bounds.Left,-bounds.Top); path.Transform(matrix); }
    if(numbersOnly) using(var outline=new Pen(fg,1.5f)) g.DrawPath(outline,path);
    using(var brush=new SolidBrush(look!=null && look.TextColor!="Auto" ? ColorTranslator.FromHtml(look.TextColor) : numbersOnly ? bg : fg)) g.FillPath(brush,path);
   }
   return CopyIcon(b);
  }
 }
 void Refresh() {
  if(busy || exiting) return; busy=true;
  var known=selected.ToArray(); var worker=new System.ComponentModel.BackgroundWorker(); worker.DoWork+=(s,e)=>e.Result=Native.Scan(known);
  worker.RunWorkerCompleted+=(s,e)=> { if(exiting) { worker.Dispose(); return; } busy=false; error=e.Error==null ? "" : e.Error.Message; if(e.Error==null) { var fresh=(List<Device>)e.Result; foreach(var d in fresh.Where(x=>x.Name==L.T("Gespeicherter Kopfhörer"))) { var old=devices.FirstOrDefault(x=>x.Id==d.Id); if(old!=null) d.Name=old.Name; } devices=fresh; lastScan=DateTime.Now; } Render();
   if(e.Error==null) { var alerts=new List<string>(); foreach(var id in selected) { var d=devices.FirstOrDefault(x=>x.Id==id); if(warnings.Check(d,settings)) alerts.Add(settings.Name(d)+L.T(": noch ")+d.Battery+L.T(" % Akku.")); } if(alerts.Count>0 && icons.Count>0) Notify(8000,L.T("Kopfhörer-Akku niedrig"),string.Join("\n",alerts.ToArray()),ToolTipIcon.Warning); }
   if(firstRun && e.Error==null) { firstRun=false; Pick(); }
   if(reloadPicker && updatePicker!=null) updatePicker(); reloadPicker=false; if(updateStatus!=null) updateStatus(); worker.Dispose(); }; worker.RunWorkerAsync();
 }
 ContextMenuStrip Menu() {
  var m=new ContextMenuStrip(); m.Items.Add("Kopfhörer-Akku").Enabled=false;
  foreach(var id in selected) { var d=devices.FirstOrDefault(x=>x.Id==id); m.Items.Add(d==null ? L.T("Nicht verfügbar / getrennt") : Describe(d)).Enabled=false; }
  if(error.Length>0) m.Items.Add(L.T("Abfrage fehlgeschlagen: ")+error).Enabled=false;
  m.Items.Add(updateBusy ? L.T("Update-Prüfung läuft …") : L.T("Nach Updates suchen …"),null,(s,e)=>CheckUpdates(true)).Enabled=!updateBusy;
  m.Items.Add(L.T("Einstellungen …"),null,(s,e)=>Options()); m.Items.Add(L.T("Geräte auswählen …"),null,(s,e)=>Pick()); m.Items.Add(L.T("Jetzt aktualisieren"),null,(s,e)=>Refresh());
  string startup=AppPaths.Startup;
  var auto=new ToolStripMenuItem(L.T("Beim Windows-Start ausführen")); auto.Checked=File.Exists(startup);
  auto.Click+=(s,e)=> { try { if(File.Exists(startup)) File.Delete(startup); else Program.EnableAutostart(); Render(); } catch(Exception ex) { MessageBox.Show(L.T("Autostart konnte nicht geändert werden: ")+ex.Message); } };
  var modes=new ToolStripMenuItem(L.T("Anzeigemodus")); foreach(DisplayMode mode in Enum.GetValues(typeof(DisplayMode))) { var choice=mode; var item=new ToolStripMenuItem(mode==DisplayMode.Infobereich ? L.T("Nur Infobereich") : mode==DisplayMode.Widget ? L.T("Nur Desktop-Widget") : L.T("Beides")){Checked=settings.Widget.Mode==mode}; item.Click+=(s,e)=> { var previous=settings.Widget.Mode; settings.Widget.Mode=choice; try { settings.Save(SettingsPath); Render(); } catch(Exception ex) { settings.Widget.Mode=previous; Render(); MessageBox.Show(L.T("Anzeigemodus konnte nicht gespeichert werden: ")+ex.Message); } }; modes.DropDownItems.Add(item); } m.Items.Add(modes);
  m.Items.Add(auto); m.Items.Add(L.T("Über Kopfhörer-Akku …"),null,(s,e)=>About()); m.Items.Add(L.T("Beenden"),null,(s,e)=>ExitThread()); return m;
 }
 void Notify(int duration,string title,string text,ToolTipIcon kind) { if(icons.Count==0) return; noticeUntil=DateTime.Now.AddSeconds(15); icons[0].Visible=true; icons[0].ShowBalloonTip(duration,title,text,kind); }
 List<WidgetItem> WidgetItems() { return selected.Select((id,i)=> { var d=devices.FirstOrDefault(x=>x.Id==id); return new WidgetItem{Name=d!=null ? settings.Name(d) : settings.Profile(id,i).Name.Length>0 ? settings.Profile(id,i).Name : L.T("Gespeicherter Kopfhörer"),Status=error.Length>0 ? L.T("Abfrage fehlgeschlagen") : d!=null ? d.Status : L.T("Verbindung unbekannt"),Battery=error.Length==0 && d!=null ? d.Battery : null,Connected=error.Length==0 && d!=null ? d.Connected : null,Profile=settings.Profile(id,i)}; }).ToList(); }
 void Render() {
  if(settings.Widget.Mode!=DisplayMode.Infobereich) {
   if(widget==null || widget.IsDisposed) widget=new BatteryWidget(()=>Options(),()=>Pick(),()=>ExitThread(),point=> { settings.Widget.X=point.X; settings.Widget.Y=point.Y; try { settings.Save(SettingsPath); } catch(Exception ex) { MessageBox.Show(L.T("Widget-Position konnte nicht gespeichert werden: ")+ex.Message); } });
   widget.UpdateContent(settings.Widget,settings,WidgetItems());
  } else if(widget!=null) { widget.Dispose(); widget=null; }
  int count=Math.Max(1,selected.Count);
  while(icons.Count<count) { var ni=new NotifyIcon(); ni.DoubleClick+=(s,e)=>Options(); icons.Add(ni); }
  while(icons.Count>count) { var ni=icons[icons.Count-1]; ni.Visible=false; if(ni.Icon!=null) ni.Icon.Dispose(); ni.Dispose(); icons.RemoveAt(icons.Count-1); }
  for(int i=0;i<count;i++) { var ni=icons[i]; var d=selected.Count>i ? devices.FirstOrDefault(x=>x.Id==selected[i]) : null;
   var old=ni.Icon; ni.Icon=BatteryIcon(error.Length==0 && d!=null ? d.Battery : null,i,error.Length==0 && d!=null ? d.Connected : null,settings.Profile(selected.Count>i ? selected[i] : null,i).Color,settings.NumbersOnly,settings,settings.Profile(selected.Count>i ? selected[i] : null,i).Symbol,settings.Profile(selected.Count>i ? selected[i] : null,i).Marker); if(old!=null) old.Dispose();
   string tip=d!=null ? Describe(d) : selected.Count==0 ? L.T("Kopfhörer-Akku · Doppelklick: Einstellungen") : L.T("Kopfhörer ")+(i+1)+L.T(" · Verbindung unbekannt");
   if(error.Length>0) tip=L.T("Kopfhörer ")+(i+1)+L.T(" · Abfrage fehlgeschlagen"); else if(lastScan.HasValue) tip+=" · "+lastScan.Value.ToString("HH:mm:ss"); ni.Text=tip.Length>63 ? tip.Substring(0,63) : tip;
   var menu=ni.ContextMenuStrip; ni.ContextMenuStrip=Menu(); if(menu!=null) menu.Dispose(); ni.Visible=settings.Widget.Mode!=DisplayMode.Widget || (i==0 && DateTime.Now<noticeUntil);
  }
 }
 public void CheckLanguages() {
  foreach(string target in new[]{"es","fr","uk","en","de","en"}) {
   Options(); var tab=settingsForm.Controls.OfType<TabControl>().Single().TabPages[0];
   var language=tab.Controls.OfType<ComboBox>().Single(x=>x.AccessibleName=="Language");
   var mode=settingsForm.Controls.OfType<CheckBox>().Single(x=>x.AccessibleName=="AdvancedSettings");
   var threshold=tab.Controls.OfType<NumericUpDown>().Single(); decimal previous=threshold.Value; threshold.Value=37;
   mode.Checked=true; mode.Checked=false;
   if(threshold.Value!=37 || language.Parent!=tab || settingsForm.Controls.OfType<TabControl>().Single().TabPages.Count!=2) throw new Exception("Settings view lost draft values");
   threshold.Value=previous; language.SelectedIndex=Array.IndexOf(L.Codes,target);
   settingsForm.Controls.OfType<Button>().Single(x=>x.Text==L.T("Speichern")).PerformClick(); Application.DoEvents();
   if(Settings.Read(SettingsPath).Language!=target || L.Language!=target || settingsForm==null || settingsForm.Text!=L.T("Kopfhörer-Akku · Einstellungen")) throw new Exception("Language switch failed: "+target);
   settingsForm.Close();
  }
  ExitThread();
 }
 public void CheckWidgetModes() { foreach(DisplayMode mode in Enum.GetValues(typeof(DisplayMode))) { settings.Widget.Mode=mode; Render(); Application.DoEvents(); if(icons.Any(x=>x.Visible)!=(mode!=DisplayMode.Widget) || (widget!=null && widget.Visible)!=(mode!=DisplayMode.Infobereich)) throw new Exception("Anzeigemodus fehlerhaft: "+mode); } settings.Widget.Mode=DisplayMode.Infobereich; Render(); ExitThread(); }
 void About() {
  MessageBox.Show("Kopfhörer-Akku "+AppPaths.Version+"\nAldikiller · MIT\n\n"+L.T("GitHub-Downloads öffnen")+": github.com/Aldikiller/KopfhoererAkku", L.T("Über Kopfhörer-Akku"),MessageBoxButtons.OK,MessageBoxIcon.Information);
 }
 string Describe(Device d) { return settings.Name(d)+" · "+d.Status+" · "+(d.Connected==false ? L.T("kein aktueller Akkustand") : d.Battery.HasValue ? d.Battery+(d.Connected==null ? L.T(" % (gespeichert)") : " %") : L.T("kein Akkustand")); }
 void CheckUpdates(bool manual) {
  if(updateBusy || exiting) return; updateBusy=true; updateStatusText=L.T("GitHub wird abgefragt …"); if(updateStatus!=null) updateStatus(); Render();
  var worker=new System.ComponentModel.BackgroundWorker(); worker.DoWork+=(a,b)=>b.Result=UpdateService.Check();
  worker.RunWorkerCompleted+=(a,b)=> { if(exiting) { worker.Dispose(); return; } updateBusy=false;
   if(b.Error!=null) { updateStatusText=L.T("Prüfung fehlgeschlagen. Internetverbindung oder GitHub-Limit prüfen."); if(manual) MessageBox.Show(updateStatusText,"Updates",MessageBoxButtons.OK,MessageBoxIcon.Information); }
   else { var latest=b.Result as ReleaseVersion; updateStatusText=latest==null ? L.T("Keine neuere passende Version gefunden (")+DateTime.Now.ToString("HH:mm")+")." : L.T("Neue Version verfügbar: ")+latest.Tag;
    if(latest!=null && manual) { if(MessageBox.Show(L.T("Version ")+latest.Tag+L.T(" ist verfügbar.\nInstalliert: ")+AppPaths.Version+L.T("\n\nGitHub-Downloadseite im Browser öffnen?"),L.T("Update verfügbar"),MessageBoxButtons.YesNo,MessageBoxIcon.Information)==DialogResult.Yes) OpenPage(UpdateService.Page(latest)); }
    else if(latest!=null && icons.Count>0) Notify(8000,L.T("Update verfügbar"),L.T("Neue Version verfügbar: ")+latest.Tag+" · "+L.T("Nach Updates suchen …"),ToolTipIcon.Info);
    else if(manual) MessageBox.Show(updateStatusText,"Updates",MessageBoxButtons.OK,MessageBoxIcon.Information);
   }
   if(updateStatus!=null) updateStatus(); Render(); worker.Dispose();
  }; worker.RunWorkerAsync();
 }
 void OpenPage(string page) { try { Process.Start(new ProcessStartInfo(page){UseShellExecute=true}); } catch(Exception ex) { MessageBox.Show(L.T("Browser konnte nicht geöffnet werden: ")+ex.Message); } }
 public void Options(bool preview=false) {
  if(settingsForm!=null) { settingsForm.Activate(); return; }
  var f=new Form{Text=L.T("Kopfhörer-Akku · Einstellungen"),ClientSize=new Size(870,590),FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,StartPosition=FormStartPosition.CenterScreen,AutoScaleMode=AutoScaleMode.Dpi}; settingsForm=f; Action liveRefresh=null;
  var tabs=new TabControl{Left=12,Top=12,Width=586,Height=478}; var general=new TabPage(L.T("Allgemein")); var appearance=new TabPage(L.T("Namen & Anzeige")); var updates=new TabPage(L.T("Updates")); var design=new TabPage(L.T("Schrift & Batterie")); var widgetTab=new TabPage(L.T("Anzeige & Widget")); tabs.TabPages.AddRange(new[]{general,appearance,updates,design,widgetTab});
  var status=new Label{Left=14,Top=14,Width=544,Height=112}; var updateLabel=new Label{Left=14,Top=190,Width=538,Height=60};
  updateStatus=()=> { status.Text=string.Join("\n",selected.Select(id=> { var d=devices.FirstOrDefault(x=>x.Id==id); return d==null ? L.T("Gerät nicht gefunden · Verbindung unbekannt") : Describe(d); }).ToArray())+L.T("\n\nLetzte Abfrage: ")+(lastScan.HasValue ? lastScan.Value.ToString("HH:mm:ss") : L.T("noch keine"))+(error.Length>0 ? L.T("\nFehler: ")+error : ""); updateLabel.Text=updateStatusText; if(liveRefresh!=null) liveRefresh(); }; updateStatus();
  var warn=new CheckBox{Left=14,Top=136,Width=530,Text=L.T("Bei niedrigem Akku benachrichtigen"),Checked=settings.Warnings};
  var threshold=new NumericUpDown{Left=245,Top=175,Width=75,Minimum=1,Maximum=90,Value=settings.Threshold};
  var interval=new NumericUpDown{Left=245,Top=217,Width=75,Minimum=5,Maximum=300,Value=settings.Interval};
  var hint=new Label{Left=14,Top=268,Width=538,Height=92,Text=L.T("Beide ausgewählten Kopfhörer behalten ein eigenes Symbol.\nRot: niedriger Akku · Grau ×: getrennt · Gelb: Status unbekannt.\nWarnungen erscheinen nur bei bestätigter Verbindung.\nEigene Namen ändern den Bluetooth-Namen in Windows nicht.")};
  var test=new Button{Left=14,Top=376,Width=210,Height=32,Text=L.T("Benachrichtigung testen")}; test.Click+=(a,b)=> { if(icons.Count>0) Notify(5000,"Kopfhörer-Akku · Test",L.T("So erscheint eine Akkuwarnung."),ToolTipIcon.Info); };
  var language=new ComboBox{AccessibleName="Language",Left=14,Top=438,Width=240,DropDownStyle=ComboBoxStyle.DropDownList}; language.Items.AddRange(new[]{"Deutsch","English","Español","Français","Українська"}); language.SelectedIndex=Array.IndexOf(L.Codes,settings.Language);
  general.Controls.Add(new Label{Left=14,Top=414,Width=530,Text="Sprache / Language"}); general.Controls.Add(language);
  general.Controls.AddRange(new Control[]{status,warn,new Label{Left=14,Top=178,Width=225,Text=L.T("Warnung bei / unter (%)")},threshold,new Label{Left=14,Top=220,Width=225,Text=L.T("Abfrage alle (Sekunden)")},interval,hint,test});
  var symbolBoxes=new Dictionary<string,ComboBox>(); var markers=new Dictionary<string,TextBox>();
  var names=new Dictionary<string,TextBox>(); var colors=new Dictionary<string,Color>(); var previews=new List<PictureBox>(); var ids=selected.ToArray();
  var style=new ComboBox{Left=14,Top=352,Width=340,DropDownStyle=ComboBoxStyle.DropDownList}; style.Items.AddRange(new[]{L.T("Batterie mit Zahl und Zeichen"),L.T("Nur Zahl mit Zeichen")}); style.SelectedIndex=settings.NumbersOnly ? 1 : 0;
  var look=new Settings{FontName=settings.FontName,TextSize=settings.TextSize,BatteryShape=settings.BatteryShape,TextColor=settings.TextColor,BatteryRadius=settings.BatteryRadius,BatteryStroke=settings.BatteryStroke,BatteryWidth=settings.BatteryWidth,BatteryHeight=settings.BatteryHeight};
  var livePicture=new PictureBox{Left=620,Top=46,Width=236,Height=350};
  var useSample=new CheckBox{Left=620,Top=405,Width=235,Text=L.T("Beispiel-Akkustand"),Checked=false};
  var sample=new NumericUpDown{Left=620,Top=436,Width=80,Minimum=0,Maximum=100,Value=75,Enabled=false};
  f.Controls.AddRange(new Control[]{new Label{Left=620,Top=16,Width=235,Text=L.T("Livevorschau · hell und dunkel")},livePicture,useSample,sample,new Label{Left=620,Top=473,Width=236,Height=60,Text=L.T("Änderungen siehst du sofort.\nErst Speichern übernimmt sie.\nKlein: 32-Pixel-Zeichenfläche.")}});
  var widgetLook=settings.Widget.Copy(); int originalWidgetX=widgetLook.X,originalWidgetY=widgetLook.Y;
  var widgetMode=new ComboBox{Left=180,Top=8,Width=382,DropDownStyle=ComboBoxStyle.DropDownList}; widgetMode.Items.AddRange(new[]{L.T("Nur Infobereich (neben der Uhr)"),L.T("Nur Desktop-Widget"),L.T("Infobereich und Widget")}); widgetMode.SelectedIndex=(int)widgetLook.Mode;
  var widgetGrid=new PropertyGrid{Left=8,Top=42,Width=554,Height=244,SelectedObject=widgetLook,PropertySort=PropertySort.Categorized,ToolbarVisible=false,HelpVisible=true};
  var widgetPreview=new PictureBox{Left=8,Top=310,Width=554,Height=128};
  widgetTab.Controls.AddRange(new Control[]{new Label{Left=8,Top=12,Width=160,Text=L.T("Wo soll die Anzeige stehen?")},widgetMode,widgetGrid,new Label{Left=8,Top=289,Width=554,Text=L.T("Livevorschau · Schachbrett zeigt transparente Bereiche")},widgetPreview});
  var fontName=new ComboBox{Left=14,Top=40,Width=300,DropDownStyle=ComboBoxStyle.DropDownList};
  foreach(var family in FontFamily.Families) { if(family.IsStyleAvailable(FontStyle.Bold)) fontName.Items.Add(family.Name); family.Dispose(); }
  fontName.SelectedItem=look.FontName; if(fontName.SelectedIndex<0) fontName.SelectedItem="Segoe UI";
  var textSize=new NumericUpDown{Left=340,Top=40,Width=100,Minimum=10,Maximum=28,Value=look.TextSize};
  var shape=new ComboBox{Left=14,Top=120,Width=300,DropDownStyle=ComboBoxStyle.DropDownList}; shape.Items.AddRange(new[]{L.T("Abgerundet"),L.T("Eckig"),L.T("Stark abgerundet")}); shape.SelectedIndex=look.BatteryShape;
  var autoText=new CheckBox{Left=14,Top=180,Width=300,Text=L.T("Schriftfarbe automatisch"),Checked=look.TextColor=="Auto"};
  var textColor=new Button{Left=340,Top=176,Width=170,Height=30,Text=L.T("Schriftfarbe wählen …")};
  var designPreview=new PictureBox{Left=14,Top=320,Width=96,Height=96,BackColor=Color.FromArgb(32,32,32),SizeMode=PictureBoxSizeMode.Zoom}; previews.Add(designPreview);
  design.Controls.AddRange(new Control[]{new Label{Left=14,Top=16,Width=300,Text=L.T("Schriftart")},fontName,new Label{Left=340,Top=16,Width=180,Text=L.T("Schriftgröße (10–28)")},textSize,new Label{Left=14,Top=96,Width=300,Text=L.T("Batterieform")},shape,autoText,textColor,designPreview,new Label{Left=125,Top=320,Width=390,Height=100,Text=L.T("Diese Einstellungen gelten für beide Kopfhörer.\nDie Batteriefarbe wählst du unter Namen & Anzeige.\nBreite, Höhe und Rahmen beziehen sich auf die 32-Pixel-Zeichenfläche. Windows legt die Größe des Symbols im Infobereich fest. Sehr große Zahlen werden passend verkleinert.")}});
  var radius=new NumericUpDown{Left=14,Top=246,Width=110,Minimum=0,Maximum=14,Value=look.BatteryRadius};
  var strokeControl=new NumericUpDown{Left=150,Top=246,Width=110,Minimum=0.5m,Maximum=3m,DecimalPlaces=1,Increment=0.1m,Value=look.BatteryStroke/10m};
  var batteryWidth=new NumericUpDown{Left=286,Top=246,Width=110,Minimum=16,Maximum=28,Value=look.BatteryWidth};
  var batteryHeight=new NumericUpDown{Left=422,Top=246,Width=110,Minimum=16,Maximum=27,Value=look.BatteryHeight};
  design.Controls.AddRange(new Control[]{new Label{Left=14,Top=220,Width=120,Text=L.T("Rundung (0–14)")},radius,new Label{Left=150,Top=220,Width=120,Text=L.T("Rahmen (0,5–3)")},strokeControl,new Label{Left=286,Top=220,Width=120,Text=L.T("Breite (16–28)")},batteryWidth,new Label{Left=422,Top=220,Width=120,Text=L.T("Höhe (16–27)")},batteryHeight});
  Action refreshPreview=()=> {
   using(var canvas=new Bitmap(236,350)) using(var g=Graphics.FromImage(canvas)) using(var labelFont=new Font("Segoe UI",9)) {
    g.Clear(SystemColors.Control);
    for(int i=0;i<Math.Max(1,ids.Length);i++) {
     string id=i<ids.Length ? ids[i] : null; var d=devices.FirstOrDefault(x=>x.Id==id);
     int? level=useSample.Checked ? (int?)sample.Value : id==null ? (int?)75 : d!=null && error.Length==0 ? d.Battery : null;
     bool? connected=useSample.Checked || id==null ? (bool?)true : d!=null && error.Length==0 ? d.Connected : null;
     Color color=id!=null && colors.ContainsKey(id) ? colors[id] : Color.Teal;
     int symbol=id!=null && symbolBoxes.ContainsKey(id) ? symbolBoxes[id].SelectedIndex : 0;
     string marker=id!=null && markers.ContainsKey(id) ? markers[id].Text : "A";
     using(var icon=BatteryIcon(level,i,connected,color,style.SelectedIndex==1,look,symbol,marker)) {
      if(i<ids.Length) { var picture=previews[i]; var old=picture.Image; picture.Image=icon.ToBitmap(); if(old!=null) old.Dispose(); }
      if(i==0) { var old=designPreview.Image; designPreview.Image=icon.ToBitmap(); if(old!=null) old.Dispose(); }
      int y=i*174; string title=id!=null && names.ContainsKey(id) && names[id].Text.Trim().Length>0 ? names[id].Text.Trim() : d!=null ? d.DisplayName : "Beispiel";
      using(var format=new StringFormat{Trimming=StringTrimming.EllipsisCharacter,FormatFlags=StringFormatFlags.NoWrap}) g.DrawString(title,labelFont,SystemBrushes.ControlText,new RectangleF(0,y,236,22),format);
      using(var dark=new SolidBrush(Color.FromArgb(30,30,30))) g.FillRectangle(dark,0,y+26,114,132);
      using(var light=new SolidBrush(Color.FromArgb(240,240,240))) g.FillRectangle(light,120,y+26,114,132);
      using(var image=icon.ToBitmap()) { g.InterpolationMode=InterpolationMode.NearestNeighbor; g.PixelOffsetMode=PixelOffsetMode.Half; g.DrawImage(image,new Rectangle(25,y+34,64,64)); g.DrawImage(image,new Rectangle(145,y+34,64,64)); g.DrawImageUnscaled(image,41,y+114); g.DrawImageUnscaled(image,161,y+114); }
     }
    }
    var previous=livePicture.Image; livePicture.Image=(Bitmap)canvas.Clone(); if(previous!=null) previous.Dispose();
    var widgetItems=new List<WidgetItem>();
    for(int slot=0;slot<ids.Length;slot++) { string id=ids[slot]; var d=devices.FirstOrDefault(x=>x.Id==id); var profile=new DeviceProfile("#"+colors[id].R.ToString("X2")+colors[id].G.ToString("X2")+colors[id].B.ToString("X2")){Name=names[id].Text,Symbol=symbolBoxes[id].SelectedIndex,Marker=Settings.CleanMarker(markers[id].Text)}; widgetItems.Add(new WidgetItem{Name=profile.Name.Trim().Length>0 ? profile.Name : d!=null ? d.DisplayName : L.T("Gespeicherter Kopfhörer"),Status=useSample.Checked ? L.T("Beispiel · verbunden") : d!=null && error.Length==0 ? d.Status : L.T("Status unbekannt"),Battery=useSample.Checked ? (int?)sample.Value : d!=null && error.Length==0 ? d.Battery : null,Connected=useSample.Checked ? (bool?)true : d!=null && error.Length==0 ? d.Connected : null,Profile=profile}); }
    var oldWidget=widgetPreview.Image; widgetPreview.Image=WidgetDrawing.Preview(widgetLook,look,widgetItems,widgetPreview.Width,widgetPreview.Height); if(oldWidget!=null) oldWidget.Dispose();
   }
  }; liveRefresh=refreshPreview;
  for(int i=0;i<ids.Length;i++) {
   string id=ids[i]; int slot=i; var d=devices.FirstOrDefault(x=>x.Id==id); var profile=settings.Profile(id,i); colors[id]=profile.Color;
   int top=16+i*137; var name=new TextBox{Left=14,Top=top+27,Width=335,MaxLength=40,Text=profile.Name}; names[id]=name;
   var colorButton=new Button{Left=365,Top=top+25,Width=105,Height=29,Text=L.T("Akku-Farbe …"),BackColor=colors[id],ForeColor=Color.White,UseVisualStyleBackColor=false};
   colorButton.Click+=(a,b)=> { using(var dialog=new ColorDialog{Color=colors[id],FullOpen=true}) if(dialog.ShowDialog(f)==DialogResult.OK) { colors[id]=dialog.Color; colorButton.BackColor=dialog.Color; colorButton.ForeColor=dialog.Color.GetBrightness()>0.55f ? Color.Black : Color.White; refreshPreview(); } };
   var reset=new Button{Left=365,Top=top+60,Width=105,Height=28,Text=L.T("Standard")}; reset.Click+=(a,b)=> { colors[id]=slot==0 ? ColorTranslator.FromHtml("#13705B") : ColorTranslator.FromHtml("#1C579F"); name.Text=""; colorButton.BackColor=colors[id]; colorButton.ForeColor=Color.White; refreshPreview(); };
   var symbolBox=new ComboBox{AccessibleName=L.T("Zeichen für Kopfhörer ")+(i+1),Left=14,Top=top+103,Width=200,DropDownStyle=ComboBoxStyle.DropDownList}; symbolBox.Items.AddRange(new[]{L.T("Kopfhörer"),L.T("Musiknote"),L.T("Lautsprecher"),L.T("Eigener Buchstabe / Zeichen"),L.T("Kein Zeichen")}); symbolBox.SelectedIndex=profile.Symbol; symbolBoxes[id]=symbolBox;
   var markerBox=new TextBox{AccessibleName=L.T("Eigenes Zeichen für Kopfhörer ")+(i+1),Left=226,Top=top+103,Width=65,MaxLength=8,Text=profile.Marker,Enabled=profile.Symbol==3}; markers[id]=markerBox;
   symbolBox.SelectedIndexChanged+=(a,b)=> { markerBox.Enabled=symbolBox.SelectedIndex==3; refreshPreview(); }; markerBox.TextChanged+=(a,b)=>refreshPreview(); name.TextChanged+=(a,b)=>refreshPreview();
   appearance.Controls.AddRange(new Control[]{symbolBox,markerBox});
   var picture=new PictureBox{Left=492,Top=top+25,Width=48,Height=48,SizeMode=PictureBoxSizeMode.Zoom,BackColor=Color.FromArgb(32,32,32)}; previews.Insert(i,picture);
   appearance.Controls.AddRange(new Control[]{new Label{Left=14,Top=top,Width=535,Text=(i+1)+". "+(d==null ? L.T("Gespeicherter Kopfhörer") : d.DisplayName)},name,colorButton,reset,picture,new Label{Left=14,Top=top+63,Width=335,Height=38,Text=L.T("Eigener Name; leer lassen für den Gerätenamen.")}});
  }
  if(ids.Length==0) appearance.Controls.Add(new Label{Left=14,Top=20,Width=535,Height=50,Text=L.T("Zuerst unter Geräte auswählen einen oder zwei Kopfhörer festlegen.")});
  appearance.Controls.AddRange(new Control[]{new Label{Left=14,Top=322,Width=530,Text=L.T("Darstellung für beide Symbole")},style,new Label{Left=14,Top=392,Width=535,Height=42,Text=L.T("Die Livevorschau rechts zeigt Änderungen sofort. Statusfarben haben Vorrang.")}}); style.SelectedIndexChanged+=(a,b)=>refreshPreview();
  widgetMode.SelectedIndexChanged+=(a,b)=> { widgetLook.Mode=(DisplayMode)widgetMode.SelectedIndex; refreshPreview(); };
  widgetGrid.PropertyValueChanged+=(a,b)=> { widgetLook.Normalize(); widgetGrid.Refresh(); refreshPreview(); };
  fontName.SelectedIndexChanged+=(a,b)=> { look.FontName=(string)fontName.SelectedItem; refreshPreview(); };
  textSize.ValueChanged+=(a,b)=> { look.TextSize=(int)textSize.Value; refreshPreview(); };
  shape.SelectedIndexChanged+=(a,b)=> { look.BatteryShape=shape.SelectedIndex; radius.Value=shape.SelectedIndex==1 ? 0 : shape.SelectedIndex==2 ? 12 : 4; refreshPreview(); };
  radius.ValueChanged+=(a,b)=> { look.BatteryRadius=(int)radius.Value; refreshPreview(); };
  strokeControl.ValueChanged+=(a,b)=> { look.BatteryStroke=(int)(strokeControl.Value*10); refreshPreview(); };
  batteryWidth.ValueChanged+=(a,b)=> { look.BatteryWidth=(int)batteryWidth.Value; refreshPreview(); };
  batteryHeight.ValueChanged+=(a,b)=> { look.BatteryHeight=(int)batteryHeight.Value; refreshPreview(); };
  autoText.CheckedChanged+=(a,b)=> { look.TextColor=autoText.Checked ? "Auto" : "#FFFFFF"; refreshPreview(); };
  textColor.Click+=(a,b)=> { using(var dialog=new ColorDialog{Color=look.TextColor=="Auto" ? Color.White : ColorTranslator.FromHtml(look.TextColor),FullOpen=true}) if(dialog.ShowDialog(f)==DialogResult.OK) { autoText.Checked=false; look.TextColor=ColorTranslator.ToHtml(Color.FromArgb(dialog.Color.R,dialog.Color.G,dialog.Color.B)); if(!look.TextColor.StartsWith("#")) look.TextColor="#"+dialog.Color.R.ToString("X2")+dialog.Color.G.ToString("X2")+dialog.Color.B.ToString("X2"); refreshPreview(); } }; useSample.CheckedChanged+=(a,b)=> { sample.Enabled=useSample.Checked; refreshPreview(); }; sample.ValueChanged+=(a,b)=>refreshPreview(); refreshPreview();
  var autoCheck=new CheckBox{Left=14,Top=20,Width=535,Text=L.T("Beim Programmstart nach neuen Versionen suchen"),Checked=settings.CheckAtStartup};
  var privacy=new Label{Left=14,Top=62,Width=535,Height=98,Text=L.T("Freiwillig und zunächst ausgeschaltet. Die Prüfung ruft öffentliche\nRelease-Daten von GitHub ab. GitHub erhält dabei deine IP-Adresse\nund die Programmversion. Gerätenamen und Akkudaten werden nicht gesendet.\nEs werden keine Updates automatisch heruntergeladen oder installiert.")};
  var check=new Button{Left=14,Top=264,Width=170,Height=32,Text=L.T("Jetzt nach Updates suchen")}; check.Click+=(a,b)=>CheckUpdates(true);
  var downloads=new Button{Left=200,Top=264,Width=170,Height=32,Text=L.T("GitHub-Downloads öffnen")}; downloads.Click+=(a,b)=>OpenPage(UpdateService.ReleasesPage);
  updates.Controls.AddRange(new Control[]{autoCheck,privacy,new Label{Left=14,Top=169,Width=530,Text=L.T("Installiert: ")+AppPaths.Version},updateLabel,check,downloads});
  var save=new Button{Left=14,Top=506,Width=125,Height=32,Text=L.T("Speichern")}; save.Click+=(a,b)=> {
   widgetGrid.Focus(); widgetLook.Normalize(); if(widgetLook.X==originalWidgetX) widgetLook.X=settings.Widget.X; if(widgetLook.Y==originalWidgetY) widgetLook.Y=settings.Widget.Y;
   var next=new Settings{Language=L.Codes[Math.Max(0,language.SelectedIndex)],Widget=widgetLook.Copy(),Warnings=warn.Checked,Threshold=(int)threshold.Value,Interval=(int)interval.Value,CheckAtStartup=autoCheck.Checked,NumbersOnly=style.SelectedIndex==1,FontName=look.FontName,TextSize=look.TextSize,BatteryShape=look.BatteryShape,TextColor=look.TextColor,BatteryRadius=look.BatteryRadius,BatteryStroke=look.BatteryStroke,BatteryWidth=look.BatteryWidth,BatteryHeight=look.BatteryHeight};
   foreach(var item in settings.Profiles) next.Profiles[item.Key]=new DeviceProfile(item.Value.ColorHex){Name=item.Value.Name,Symbol=item.Value.Symbol,Marker=item.Value.Marker};
   foreach(var id in ids) next.Profiles[id]=new DeviceProfile("#"+colors[id].R.ToString("X2")+colors[id].G.ToString("X2")+colors[id].B.ToString("X2")){Name=names[id].Text.Trim(),Symbol=symbolBoxes[id].SelectedIndex,Marker=Settings.CleanMarker(markers[id].Text)};
   try { next.Save(SettingsPath); if(next.Threshold!=settings.Threshold || next.Warnings!=settings.Warnings) warnings.Reset(); bool changedLanguage=settings.Language!=next.Language; settings=next; L.Language=settings.Language; if(changedLanguage) { updateStatusText=L.T("Noch nicht geprüft"); if(widget!=null) { widget.Close(); widget.Dispose(); widget=null; } if(picker!=null) picker.Close(); } Render(); Refresh(); f.Close(); if(changedLanguage) Options(); } catch(Exception ex) { MessageBox.Show(L.T("Speichern fehlgeschlagen: ")+ex.Message); }
  };
  var choose=new Button{Left=151,Top=506,Width=145,Height=32,Text=L.T("Geräte auswählen")}; choose.Click+=(a,b)=>Pick();
  var refresh=new Button{Left=308,Top=506,Width=125,Height=32,Text=L.T("Aktualisieren")}; refresh.Click+=(a,b)=>Refresh();
  var cancel=new Button{Left=445,Top=506,Width=145,Height=32,Text=L.T("Schließen")}; cancel.Click+=(a,b)=>f.Close();
  var simpleTab=new TabPage(L.T("Einfach"));
  var advanced=new CheckBox{AccessibleName="AdvancedSettings",Left=16,Top=10,Width=570,Height=24,Text=L.T("Erweiterte Einstellungen")};
  var shared=new Control[]{status,warn,threshold,widgetMode,style,language};
  var originalParents=shared.Select(c=>c.Parent).ToArray();
  var originalBounds=shared.Select(c=>c.Bounds).ToArray();
  simpleTab.Controls.AddRange(new Control[]{
   new Label{Left=14,Top=10,Width=535,Height=38,Text=L.T("Die wichtigsten Einstellungen. Weitere Optionen findest du unter Erweiterte Einstellungen.")},
   new Label{Left=14,Top=178,Width=300,Text=L.T("Warnung bei / unter (%)")},
   new Label{Left=14,Top=227,Width=535,Text=L.T("Wo soll die Anzeige stehen?")},
   new Label{Left=14,Top=295,Width=535,Text=L.T("Darstellung für beide Symbole")},
   new Label{Left=14,Top=363,Width=535,Text="Sprache / Language"}});
  Action setSettingsMode=()=> {
   tabs.TabPages.Clear();
   if(advanced.Checked) {
    for(int i=0;i<shared.Length;i++) { originalParents[i].Controls.Add(shared[i]); shared[i].Bounds=originalBounds[i]; }
    tabs.TabPages.AddRange(new[]{general,appearance,updates,design,widgetTab});
   } else {
    foreach(var control in shared) simpleTab.Controls.Add(control);
    status.SetBounds(14,52,535,78); warn.SetBounds(14,141,535,28); threshold.SetBounds(340,175,90,26);
    widgetMode.SetBounds(14,252,535,28); style.SetBounds(14,320,535,28); language.SetBounds(14,388,310,28);
    tabs.TabPages.AddRange(new[]{simpleTab,updates});
   }
  };
  advanced.CheckedChanged+=(a,b)=>setSettingsMode(); setSettingsMode();
  f.Controls.AddRange(new Control[]{tabs,save,choose,refresh,cancel}); f.FormClosed+=(a,b)=> { liveRefresh=null; if(widgetPreview.Image!=null) widgetPreview.Image.Dispose(); if(livePicture.Image!=null) livePicture.Image.Dispose(); updateStatus=null; settingsForm=null; foreach(var picture in previews) if(picture.Image!=null) picture.Image.Dispose(); f.Dispose(); }; foreach(Control control in f.Controls) control.Top+=32; f.Controls.Add(advanced); f.Show();
  if(widgetGrid.SelectedGridItem!=null) { var root=widgetGrid.SelectedGridItem; while(root.Parent!=null) root=root.Parent; foreach(GridItem group in root.GridItems) if(group.GridItemType==GridItemType.Category && group.Label==L.T("1. Anzeige") && group.GridItems.Count>0) { group.Expanded=true; widgetGrid.SelectedGridItem=group.GridItems[0]; break; } }
  if(preview) { using(var basic=new Bitmap(f.Width,f.Height)) { f.DrawToBitmap(basic,new Rectangle(0,0,f.Width,f.Height)); basic.Save(AppPaths.Report("Einstellungen-Einfach.png")); } advanced.Checked=true; File.WriteAllText(AppPaths.Report("Symbol-Check.txt"),"Ausgewählte Geräte: "+selected.Count+"; angelegte Symbole: "+icons.Count+"; sichtbar an Windows übergeben: "+icons.Count(x=>x.Visible)); foreach(TabPage tab in tabs.TabPages) { tabs.SelectedTab=tab; using(var bitmap=new Bitmap(f.Width,f.Height)) { f.DrawToBitmap(bitmap,new Rectangle(0,0,f.Width,f.Height)); bitmap.Save(AppPaths.Report("Einstellungen-"+tabs.SelectedIndex+".png")); } }
   if(ids.Length>0) { symbolBoxes[ids[0]].SelectedIndex=3; markers[ids[0]].Text="A"; if(ids.Length>1) symbolBoxes[ids[1]].SelectedIndex=1; useSample.Checked=true; sample.Value=15; tabs.SelectedTab=appearance; using(var bitmap=new Bitmap(f.Width,f.Height)) { f.DrawToBitmap(bitmap,new Rectangle(0,0,f.Width,f.Height)); bitmap.Save(AppPaths.Report("Livevorschau-Zeichen.png")); } }
   f.Close(); }
 }
 void Pick() {
  if(picker!=null) { picker.Activate(); return; }
  var f=new Form{Text=L.T("Kopfhörer-Akku – Geräte auswählen"),ClientSize=new Size(560,350),StartPosition=FormStartPosition.CenterScreen,MinimumSize=new Size(576,389)}; picker=f;
  var label=new Label{Text=L.T("Bis zu zwei Kopfhörer auswählen. Namen und Farben lassen sich in den Einstellungen ändern.\nEin Strich im Symbol bedeutet: Windows liefert gerade keinen Akkustand."),Dock=DockStyle.Top,Height=55,Padding=new Padding(10)};
  var list=new CheckedListBox{Dock=DockStyle.Fill,CheckOnClick=true,HorizontalScrollbar=true};
  Action populate=()=> { list.Items.Clear(); foreach(var d in devices) list.Items.Add(d,selected.Contains(d.Id)); foreach(var id in selected.Where(id=>!devices.Any(d=>d.Id==id))) list.Items.Add(new Device{Id=id,Name=L.T("Gespeichertes Gerät (nicht verfügbar)")},true); };
  updatePicker=populate; populate(); var panel=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=75,Padding=new Padding(8)};
  var save=new Button{Text=L.T("Übernehmen"),Width=120,Height=30}; save.Click+=(s,e)=> { if(list.CheckedItems.Count>2) { MessageBox.Show(L.T("Bitte höchstens zwei Geräte auswählen.")); return; }
   var ids=list.CheckedItems.Cast<Device>().Select(d=>d.Id).ToList(); try { Directory.CreateDirectory(folder); File.WriteAllLines(Path.Combine(folder,"devices.txt"),ids); selected=ids; Render(); f.Close(); } catch(Exception ex) { MessageBox.Show(L.T("Speichern fehlgeschlagen: ")+ex.Message); } };
  var reload=new Button{Text=L.T("Neu suchen"),Width=120,Height=30}; reload.Click+=(s,e)=> { reloadPicker=true; Refresh(); };
  var hint=new Label{Text=L.T("Kopfhörer vor der Suche einschalten und mit Windows verbinden."),AutoSize=true}; panel.Controls.Add(save); panel.Controls.Add(reload); panel.Controls.Add(hint);
  f.Controls.Add(list); f.Controls.Add(label); f.Controls.Add(panel); f.FormClosed+=(s,e)=> { reloadPicker=false; picker=null; updatePicker=null; f.Dispose(); }; f.Show();
 }
 protected override void ExitThreadCore() { if(exiting) return; exiting=true; timer.Stop(); stopEvent.Dispose(); timer.Dispose(); if(widget!=null) widget.Dispose(); if(settingsForm!=null) settingsForm.Close(); foreach(var ni in icons) { ni.Visible=false; if(ni.Icon!=null) ni.Icon.Dispose(); if(ni.ContextMenuStrip!=null) ni.ContextMenuStrip.Dispose(); ni.Dispose(); } if(picker!=null) picker.Close(); base.ExitThreadCore(); }
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
  string languageFile=AppPaths.Report("language-test.ini"); var languageSettings=new Settings{Language="en"}; languageSettings.Save(languageFile); if(Settings.Read(languageFile).Language!="en") throw new Exception("Language persistence"); File.Delete(languageFile);
  L.Language="en"; if(L.T("Speichern")!="Save" || new Device{Connected=true}.Status!="Connected") throw new Exception("English UI text");
  var properties=System.ComponentModel.TypeDescriptor.GetProperties(typeof(WidgetOptions)); if(properties["Width"].DisplayName!="Width (pixels)") throw new Exception("Widget property translation");
  var alignment=new LocalizedAlignmentConverter(); if((string)alignment.ConvertTo(WidgetTextAlignment.Links,typeof(string))!="Left" || (WidgetTextAlignment)alignment.ConvertFrom("Right")!=WidgetTextAlignment.Rechts) throw new Exception("Alignment translation");
  L.Language="de"; if(L.T("Speichern")!="Speichern") throw new Exception("German UI text");

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
  config.BatteryRadius=9; config.BatteryStroke=25; config.BatteryWidth=23; config.BatteryHeight=19; config.FontName="Arial"; config.TextSize=14; config.BatteryShape=2; config.TextColor="#ABCDEF"; config.NumbersOnly=true; config.CheckAtStartup=true; config.Profiles["BTH\\TEST=ä"]=new DeviceProfile("#FFEE00"){Name="Büro = Musik 🎧",Symbol=3,Marker="A"}; config.Save(profileFile); config.Save(profileFile); loaded=Settings.Read(profileFile); File.Delete(profileFile);
  check(loaded.BatteryRadius==9 && loaded.BatteryStroke==25 && loaded.BatteryWidth==23 && loaded.BatteryHeight==19,"battery geometry persistence");
  check(loaded.FontName=="Arial" && loaded.TextSize==14 && loaded.BatteryShape==2 && loaded.TextColor=="#ABCDEF","appearance roundtrip");
  foreach(int radiusTest in new[]{0,14}) foreach(int lineTest in new[]{5,30}) foreach(int widthTest in new[]{16,28}) foreach(int heightTest in new[]{16,27}) foreach(int levelTest in new[]{1,75,100}) using(var icon=Tray.BatteryIcon(levelTest,0,true,Color.Teal,false,new Settings{BatteryRadius=radiusTest,BatteryStroke=lineTest,BatteryWidth=widthTest,BatteryHeight=heightTest})) check(icon.Width==32,"geometry extremes");
  File.WriteAllLines(profileFile,new[]{"BatteryShape=2","BatteryWidth=999","BatteryHeight=-3","BatteryStroke=999","BatteryRadius=999"}); var limited=Settings.Read(profileFile); check(limited.BatteryWidth==28 && limited.BatteryHeight==16 && limited.BatteryStroke==30 && limited.BatteryRadius==14,"geometry limits");
  File.WriteAllLines(profileFile,new[]{"BatteryShape=2"}); check(Settings.Read(profileFile).BatteryRadius==12,"legacy shape migration"); File.Delete(profileFile);
  foreach(var form in new[]{0,1,2}) foreach(var size in new[]{10,21,28}) using(var icon=Tray.BatteryIcon(100,0,true,Color.Teal,false,new Settings{FontName="missing font xyz",TextSize=size,BatteryShape=form})) check(icon.Width==32,"font fallback and shapes");
  check(loaded.NumbersOnly && loaded.CheckAtStartup && loaded.Profiles["BTH\\TEST=ä"].Name=="Büro = Musik 🎧" && loaded.Profiles["BTH\\TEST=ä"].ColorHex=="#FFEE00","profile roundtrip and atomic replacement");
  check(loaded.Profiles["BTH\\TEST=ä"].Symbol==3 && loaded.Profiles["BTH\\TEST=ä"].Marker=="A","device marker roundtrip");
  check(Settings.CleanMarker("AB")=="A" && Settings.CleanMarker(" ")=="A" && Settings.CleanMarker("Ä")=="Ä","single marker normalization");
  foreach(int symbolTest in new[]{0,1,2,3,4}) foreach(bool numericTest in new[]{false,true}) using(var icon=Tray.BatteryIcon(75,0,true,Color.Teal,numericTest,new Settings(),symbolTest,"A")) check(icon.Width==32,"all marker variants");
  using(var markIcon=Tray.BatteryIcon(75,0,true,Color.Teal,false,new Settings(),0)) using(var plainIcon=Tray.BatteryIcon(75,0,true,Color.Teal,false,new Settings(),4)) using(var markImage=markIcon.ToBitmap()) using(var plainImage=plainIcon.ToBitmap()) {
   check(markImage.GetPixel(8,10).ToArgb()==plainImage.GetPixel(8,10).ToArgb() && markImage.GetPixel(8,10).A>0,"battery remains visible through headphone gaps");
   check(markImage.GetPixel(0,0).A==0,"mark background remains transparent");
  }
  var widgetFile=AppPaths.Report("widget-test.ini"); var widgetConfig=new Settings(); widgetConfig.Widget.Mode=DisplayMode.Beides; widgetConfig.Widget.BackgroundOpacity=0; widgetConfig.Widget.Opacity=65; widgetConfig.Widget.Width=610; widgetConfig.Widget.Height=180; widgetConfig.Widget.X=-450; widgetConfig.Widget.Y=140; widgetConfig.Widget.Horizontal=true; widgetConfig.Widget.FontName="Arial"; widgetConfig.Widget.Foreground=Color.Gold; widgetConfig.Save(widgetFile); var widgetLoaded=Settings.Read(widgetFile).Widget; File.Delete(widgetFile);
  check(widgetLoaded.Mode==DisplayMode.Beides && widgetLoaded.BackgroundOpacity==0 && widgetLoaded.Opacity==65 && widgetLoaded.Width==610 && widgetLoaded.Height==180 && widgetLoaded.X==-450 && widgetLoaded.Y==140 && widgetLoaded.Horizontal && widgetLoaded.FontName=="Arial" && widgetLoaded.Foreground.ToArgb()==Color.Gold.ToArgb(),"widget settings roundtrip");
  var limits=new WidgetOptions{Width=-1,Height=99999,Opacity=0,BackgroundOpacity=-1,FontSize=0,ValueSize=999,Padding=999,Gap=999}; limits.Normalize(); check(limits.Width==160 && limits.Height==1000 && limits.Opacity==10 && limits.BackgroundOpacity==0 && limits.FontSize==6 && limits.ValueSize==120,"widget limits");
  var widgetItems=new List<WidgetItem>{new WidgetItem{Name="Langer Kopfhörername",Status=L.T("Verbunden"),Battery=100,Connected=true,Profile=new DeviceProfile("#13705B")},new WidgetItem{Name="Zweites Gerät",Status=L.T("Unbekannt"),Profile=new DeviceProfile("#1C579F")}};
  foreach(bool horizontal in new[]{false,true}) foreach(bool namesVisible in new[]{false,true}) foreach(bool statusVisible in new[]{false,true}) foreach(bool iconsVisible in new[]{false,true}) using(var bitmap=WidgetDrawing.Render(new WidgetOptions{Width=160,Height=90,Horizontal=horizontal,Names=namesVisible,Status=statusVisible,Icons=iconsVisible,Padding=80,Gap=80,ValueSize=120,FontSize=72},new Settings(),widgetItems)) check(bitmap.Width==160,"widget layout extremes");
  check(!new Settings().CheckAtStartup,"updates opt in");
  check(ReleaseVersion.Parse("v1.0.0-beta.10").CompareTo(ReleaseVersion.Parse("1.0.0-beta.2"))>0,"numeric prerelease comparison");
  check(ReleaseVersion.Parse("1.0.0").CompareTo(ReleaseVersion.Parse("1.0.0-rc.1"))>0,"stable newer than prerelease");
  check(ReleaseVersion.Parse("1.0.0-beta.01")==null && ReleaseVersion.Parse("https://evil.example/")==null,"invalid versions rejected");
  string json="[{\"draft\":false,\"tag_name\":\"v0.1.0-beta.10\",\"prerelease\":true},{\"draft\":true,\"tag_name\":\"v99.0.0\"},{\"draft\":false,\"tag_name\":\"v0.1.0-beta.2\"}]";
  check(UpdateService.Select(json,"0.1.0-beta.2").Tag=="v0.1.0-beta.10","latest beta selection");
  check(UpdateService.Select(json,"0.1.0")==null,"stable excludes previews");
  check(UpdateService.Select("[{\"draft\":false,\"tag_name\":\"v0.1.0-beta.1\"}]","0.1.0-beta.2")==null,"no downgrade");
  foreach(bool? state in new bool?[]{true,false,null}) foreach(int? level in new int?[]{0,1,20,70,100,null}) foreach(bool numeric in new[]{false,true}) using(var icon=Tray.BatteryIcon(level,1,state,Color.Yellow,numeric)) check(icon.Width==32,"custom style/status icons");
  File.WriteAllText(AppPaths.Report("Testbericht.txt"),"OK: Warnschwelle, Wiederholschutz, Wiederverbindung, fehlende Daten, deaktivierte Warnungen, Warnlogik, 54 Symbolvarianten, Namen/Farben/Anzeige speichern, Schrift und Batteriegeometrie speichern, 48 Geometrie-Grenzfälle, Zeichenwahl und Transparenz, Widget-Einstellungen und Layouts, Migration und Begrenzung, Update-Versionsvergleich, Beta-Kanal, kein Downgrade, Updates standardmäßig aus.");
 }
 [STAThread] static void Main(string[] args) {
  var output=args.FirstOrDefault(x=>x.StartsWith("--output-dir=",StringComparison.Ordinal)); if(output!=null) AppPaths.Output=Path.GetFullPath(output.Substring(13));
  if(args.Contains("--shutdown")) { if(!StopRunning()) Environment.ExitCode=2; return; }
  if(args.Contains("--version")) { File.WriteAllText(AppPaths.Report("Version.txt"),AppPaths.Version); return; }
  if(args.Contains("--disable-autostart")) { try { if(File.Exists(AppPaths.Startup)) File.Delete(AppPaths.Startup); } catch { Environment.ExitCode=1; } return; }
  if(args.Contains("--export-icon")) { using(var icon=Tray.BatteryIcon(75,0)) using(var stream=File.Create(AppPaths.Report("app.ico"))) icon.Save(stream); return; }
  if(args.Contains("--selftest")) { try { SelfTest(); } catch(Exception e) { File.WriteAllText(AppPaths.Report("Testbericht.txt"),"FEHLER: "+e); Environment.ExitCode=1; } return; }
  Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
  if(args.Contains("--language-check")) { string dir=AppPaths.Report("language-ui"); Directory.CreateDirectory(dir); using(var context=new Tray(true,"de",dir)) context.CheckLanguages(); using(var context=new Tray(true,null,dir)) { if(L.Language!="en") throw new Exception("Language restart failed"); context.ExitThread(); } File.WriteAllText(AppPaths.Report("Language-Test.txt"),"OK: German/English/Spanish/French/Ukrainian switching, settings windows rebuilt, persistence and restart."); return; }
  if(args.Contains("--ui-check")) { var context=new Tray(true,args.Contains("--spanish") ? "es" : args.Contains("--french") ? "fr" : args.Contains("--ukrainian") ? "uk" : args.Contains("--english") ? "en" : "de"); context.Options(true); context.ExitThread(); context.Dispose(); return; }
  if(args.Contains("--diagnose")) { try { File.WriteAllLines(AppPaths.Report("Diagnose.txt"),Native.Scan().Select(d=>d.ToString()+"\t"+d.Id).Concat(new[]{Native.LinkDiagnostics})); } catch(Exception e) { File.WriteAllText(AppPaths.Report("Diagnose.txt"),e.ToString()); } return; }
  if(args.Contains("--widget-check")) {
   Application.EnableVisualStyles();
   var items=new List<WidgetItem>{new WidgetItem{Name="JBL · Büro",Status=L.T("Verbunden"),Battery=75,Connected=true,Profile=new DeviceProfile("#13705B")},new WidgetItem{Name="JLab · Musik",Status=L.T("Getrennt"),Battery=15,Connected=false,Profile=new DeviceProfile("#1C579F"){Symbol=3,Marker="B"}}};
   var settings=new Settings(); var options=new WidgetOptions();
   using(var bitmap=WidgetDrawing.Preview(options,settings,items,570,300)) bitmap.Save(AppPaths.Report("Widget-Standard.png"));
   options.BackgroundOpacity=0; options.BorderWidth=0;
   using(var bitmap=WidgetDrawing.Preview(options,settings,items,570,300)) bitmap.Save(AppPaths.Report("Widget-Transparent.png"));
   using(var bitmap=WidgetDrawing.Render(options,settings,items)) if(bitmap.GetPixel(190,4).A!=0) throw new Exception("Widget background not transparent");
   options.Horizontal=true; options.Width=620; options.Height=140; options.BackgroundOpacity=60; options.Opacity=70;
   using(var bitmap=WidgetDrawing.Preview(options,settings,items,660,190)) bitmap.Save(AppPaths.Report("Widget-Horizontal.png"));
   using(var widget=new BatteryWidget(()=>{},()=>{},()=>{},point=>{})) { widget.UpdateContent(options,settings,items); Application.DoEvents(); widget.Hide(); options.Width=420; options.Height=240; widget.UpdateContent(options,settings,items); Application.DoEvents(); widget.Close(); }
   using(var tray=new Tray(true)) tray.CheckWidgetModes();
   File.WriteAllText(AppPaths.Report("Widget-Test.txt"),"OK: Alpha-Transparenz, Layouts und echtes Windows-Layered-Widget einschließlich Größenwechsel sowie alle drei Anzeigemodi."); return;
  }
  if(args.Contains("--geometry-preview")) { using(var bitmap=new Bitmap(576,240)) using(var g=Graphics.FromImage(bitmap)) { g.Clear(Color.FromArgb(30,30,30)); int i=0; foreach(var look in new[]{new Settings(),new Settings{BatteryRadius=0,BatteryStroke=5},new Settings{BatteryRadius=14,BatteryStroke=30},new Settings{BatteryWidth=16,BatteryHeight=16,BatteryStroke=30},new Settings{BatteryWidth=28,BatteryHeight=16,BatteryRadius=8},new Settings{BatteryWidth=16,BatteryHeight=27,BatteryRadius=6}}) { using(var icon=Tray.BatteryIcon(100,0,true,Color.Teal,false,look)) using(var image=icon.ToBitmap()) { g.DrawImage(image,new Rectangle(i*96+8,12,80,80)); g.DrawImage(image,new Rectangle(i*96+32,110,32,32)); g.DrawImage(image,new Rectangle(i*96+40,158,16,16)); } i++; } bitmap.Save(AppPaths.Report("Batterieformen.png")); } return; }
  if(args.Contains("--preview")) { using(var b=new Bitmap(440,180)) using(var g=Graphics.FromImage(b)) { g.Clear(Color.FromArgb(28,28,28)); int row=0; foreach(bool numeric in new[]{false,true}) { int i=0; foreach(int? n in new int?[]{90,70,1,100,null}) { using(var icon=Tray.BatteryIcon(n,i%2,true,i%2==0 ? Color.FromArgb(19,112,91) : Color.FromArgb(28,87,159),numeric)) { g.DrawIcon(icon,12+i*84,12+row*85); using(var image=icon.ToBitmap()) g.DrawImage(image,new Rectangle(20+i*84,58+row*85,16,16)); } i++; } row++; } b.Save(AppPaths.Report("Vorschau.png")); } return; }
  if(args.Contains("--update-check")) { try { var latest=UpdateService.Check(); File.WriteAllText(AppPaths.Report("Update-Check.txt"),latest==null ? "OK: keine neuere Version" : "OK: "+latest.Tag); } catch(Exception e) { File.WriteAllText(AppPaths.Report("Update-Check.txt"),"FEHLER: "+e); Environment.ExitCode=1; } return; }

  if(args.Contains("--enable-autostart")) { try { EnableAutostart(); File.WriteAllText(AppPaths.Report("Autostart-Status.txt"),"Aktiv: Windows-Autostartverknüpfung"); } catch(Exception e) { File.WriteAllText(AppPaths.Report("Autostart-Status.txt"),e.ToString()); Environment.ExitCode=1; } return; }

  if(args.Contains("--replace") && !StopRunning()) { MessageBox.Show(L.T("Die laufende Version konnte nicht beendet werden. Bitte über das Symbol beenden.")); return; }
  bool created; using(var mutex=new Mutex(true,AppPaths.MutexName,out created)) {
   if(!created) { MessageBox.Show(L.T("Kopfhörer-Akku läuft bereits. Die Symbole stehen ggf. unter ^ neben der Uhr."),"Kopfhörer-Akku"); return; }
   try { Application.Run(new Tray()); } catch(Exception ex) { MessageBox.Show(L.T("Kopfhörer-Akku konnte nicht gestartet werden: ")+ex.Message,"Kopfhörer-Akku",MessageBoxButtons.OK,MessageBoxIcon.Error); Environment.ExitCode=1; }
  }
 }
}
