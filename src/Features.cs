// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Aldikiller
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

class DeviceProfile {
 public string Name="",Marker="A",AudioId="",IconFile="";
 public bool RememberVolume=false; public int SavedVolume=-1;
 public string RenderMarker { get { return Symbol==5 ? IconFile : Marker; } }
 public bool WarningOverride=false,DeviceWarnings=true,DevicePulse=true;
 public int WarningThreshold=20,PulseThreshold=20; public string PulseColor="#FF9119";
 public int Symbol=0;
 public string ColorHex;
 public DeviceProfile(string color) { ColorHex=color; }
 public Color Color { get { return ColorTranslator.FromHtml(ColorHex); } }
}
class Settings {
 public bool WheelVolume=false;
 public string Language="de";
 public WidgetOptions Widget=new WidgetOptions();
 public FontFamily FontFamily() { try { var f=new FontFamily(FontName); if(f.IsStyleAvailable(FontStyle.Bold)) return f; f.Dispose(); } catch(ArgumentException) {} return new FontFamily("Segoe UI"); }
 public bool HotkeyOne=false,HotkeyTwo=false; public int HotkeyOneModifiers=3,HotkeyTwoModifiers=3,HotkeyOneKey=74,HotkeyTwoKey=76;
 public int PulsePeriod=20,PulseStrength=75; public bool PulseSteady=false;
 public Settings Copy() { return (Settings)MemberwiseClone(); }
 public bool HotkeyEnabled=false; public int HotkeyModifiers=3,HotkeyKey=0;
 public string PulseColor="#FF9119";
 public bool PulseLow=false; public int PulseThreshold=20;
 public bool QuickAudio=true;
 public bool Warnings=true,CheckAtStartup=false,NumbersOnly=false;
 public int Threshold=20,Interval=30,TextSize=21,BatteryShape=0,BatteryRadius=4,BatteryStroke=13,BatteryWidth=28,BatteryHeight=27;
 public string FontName="Arial",TextColor="Auto";
 public Dictionary<string,DeviceProfile> Profiles=new Dictionary<string,DeviceProfile>(StringComparer.OrdinalIgnoreCase);
 public DeviceProfile Profile(string id,int slot) { DeviceProfile p; return id!=null && Profiles.TryGetValue(id,out p) ? p : new DeviceProfile(slot==0 ? "#13705B" : "#1C579F"); }
 public string Name(Device d) { DeviceProfile p; return Profiles.TryGetValue(d.Id,out p) && !string.IsNullOrWhiteSpace(p.Name) ? p.Name : d.DisplayName; }
 static string Encode(string s) { return Convert.ToBase64String(Encoding.UTF8.GetBytes(s)).TrimEnd('=').Replace('+','-').Replace('/','_'); }
 static string Decode(string s) { return Encoding.UTF8.GetString(Convert.FromBase64String(s.Replace('-','+').Replace('_','/').PadRight((s.Length+3)/4*4,'='))); }
 public static Settings Read(string file) {
  var x=new Settings(); if(!File.Exists(file)) return x;
  foreach(var line in File.ReadAllLines(file)) {
   int split=line.IndexOf('='); if(split<1) continue; string key=line.Substring(0,split),value=line.Substring(split+1); int n; bool b;
   if(key=="WheelVolume" && bool.TryParse(value,out b)) x.WheelVolume=b;
   if(key=="QuickAudio" && bool.TryParse(value,out b)) x.QuickAudio=b;
   if(key=="Language" && L.Supported(value)) x.Language=value;
   if(key.StartsWith("Widget.",StringComparison.Ordinal)) x.Widget.Read(key.Substring(7),value);
   if(key=="FontName") { try { x.FontName=Decode(value); } catch(FormatException) {} }
   if(key=="TextSize" && int.TryParse(value,out n)) x.TextSize=Math.Max(10,Math.Min(28,n));
   if(key=="BatteryShape" && int.TryParse(value,out n)) x.BatteryShape=Math.Max(0,Math.Min(2,n));
   if(key=="BatteryRadius" && int.TryParse(value,out n)) x.BatteryRadius=Math.Max(0,Math.Min(14,n));
   if(key=="BatteryStroke" && int.TryParse(value,out n)) x.BatteryStroke=Math.Max(5,Math.Min(30,n));
   if(key=="BatteryWidth" && int.TryParse(value,out n)) x.BatteryWidth=Math.Max(16,Math.Min(28,n));
   if(key=="BatteryHeight" && int.TryParse(value,out n)) x.BatteryHeight=Math.Max(16,Math.Min(27,n));
   if(key=="TextColor" && (value=="Auto" || Regex.IsMatch(value,@"\A#[0-9a-fA-F]{6}\z"))) x.TextColor=value;
   if(key=="HotkeyOne" && bool.TryParse(value,out b)) x.HotkeyOne=b;
   if(key=="HotkeyTwo" && bool.TryParse(value,out b)) x.HotkeyTwo=b;
   if(key=="PulseSteady" && bool.TryParse(value,out b)) x.PulseSteady=b;
   if(key=="HotkeyOneModifiers" && int.TryParse(value,out n)) x.HotkeyOneModifiers=Math.Max(1,Math.Min(15,n));
   if(key=="HotkeyTwoModifiers" && int.TryParse(value,out n)) x.HotkeyTwoModifiers=Math.Max(1,Math.Min(15,n));
   if(key=="HotkeyOneKey" && int.TryParse(value,out n)) x.HotkeyOneKey=Math.Max(65,Math.Min(90,n));
   if(key=="HotkeyTwoKey" && int.TryParse(value,out n)) x.HotkeyTwoKey=Math.Max(65,Math.Min(90,n));
   if(key=="PulsePeriod" && int.TryParse(value,out n)) x.PulsePeriod=Math.Max(10,Math.Min(100,n));
   if(key=="PulseStrength" && int.TryParse(value,out n)) x.PulseStrength=Math.Max(10,Math.Min(100,n));
   if(key=="HotkeyEnabled" && bool.TryParse(value,out b)) x.HotkeyEnabled=b;
   if(key=="HotkeyModifiers" && int.TryParse(value,out n) && n>0 && n<=15) x.HotkeyModifiers=n;
   if(key=="HotkeyKey" && int.TryParse(value,out n) && n>=65 && n<=90) x.HotkeyKey=n;
   if(key=="PulseColor" && Regex.IsMatch(value,@"\A#[0-9a-fA-F]{6}\z")) x.PulseColor=value;
   if(key=="PulseLow" && bool.TryParse(value,out b)) x.PulseLow=b;
   if(key=="PulseThreshold" && int.TryParse(value,out n)) x.PulseThreshold=Math.Max(1,Math.Min(90,n));
   if(key=="Warnings" && bool.TryParse(value,out b)) x.Warnings=b;
   if(key=="CheckAtStartup" && bool.TryParse(value,out b)) x.CheckAtStartup=b;
   if(key=="NumbersOnly" && bool.TryParse(value,out b)) x.NumbersOnly=b;
   if(key=="Threshold" && int.TryParse(value,out n)) x.Threshold=Math.Max(1,Math.Min(90,n));
   if(key=="Interval" && int.TryParse(value,out n)) x.Interval=Math.Max(30,Math.Min(300,n));
   if(key.StartsWith("Device.",StringComparison.Ordinal)) { try {
    int suffix=key.LastIndexOf('.'); if(suffix<=7) continue; string id=Decode(key.Substring(7,suffix-7)),field=key.Substring(suffix+1); if(id.Length==0) continue;
    DeviceProfile p; if(!x.Profiles.TryGetValue(id,out p)) { p=new DeviceProfile("#13705B"); x.Profiles[id]=p; }
    if(field=="WarningOverride" && bool.TryParse(value,out b)) p.WarningOverride=b;
    if(field=="DeviceWarnings" && bool.TryParse(value,out b)) p.DeviceWarnings=b;
    if(field=="DevicePulse" && bool.TryParse(value,out b)) p.DevicePulse=b;
    if(field=="WarningThreshold" && int.TryParse(value,out n)) p.WarningThreshold=Math.Max(1,Math.Min(90,n));
    if(field=="PulseThreshold" && int.TryParse(value,out n)) p.PulseThreshold=Math.Max(1,Math.Min(90,n));
    if(field=="PulseColor" && Regex.IsMatch(value,@"\A#[0-9a-fA-F]{6}\z")) p.PulseColor=value;
    if(field=="RememberVolume" && bool.TryParse(value,out b)) p.RememberVolume=b;
    if(field=="SavedVolume" && int.TryParse(value,out n)) p.SavedVolume=Math.Max(-1,Math.Min(100,n));
    if(field=="IconFile") p.IconFile=Decode(value);
    if(field=="AudioId") p.AudioId=Decode(value);
    if(field=="Name") { var name=Decode(value); p.Name=name.Length>40 ? name.Substring(0,40) : name; }
    if(field=="Symbol" && int.TryParse(value,out n)) p.Symbol=Math.Max(0,Math.Min(5,n));
    if(field=="Marker") { var mark=Decode(value); p.Marker=CleanMarker(mark); }
    if(field=="Color" && Regex.IsMatch(value,@"\A#[0-9a-fA-F]{6}\z")) p.ColorHex=value;
   } catch(FormatException) {} }
  }
  // Older presets used fixed radii; keep their appearance during migration.
  if(!File.ReadAllLines(file).Any(line=>line.StartsWith("BatteryRadius="))) x.BatteryRadius=x.BatteryShape==1 ? 0 : x.BatteryShape==2 ? 12 : 4;
  x.Widget.Normalize(); return x;
 }
 public static string CleanMarker(string value) { if(string.IsNullOrWhiteSpace(value)) return "A"; var e=System.Globalization.StringInfo.GetTextElementEnumerator(value.Trim()); return e.MoveNext() ? (string)e.Current : "A"; }
 public void Save(string file) {
  var lines=new List<string>{"HotkeyOne="+HotkeyOne,"HotkeyTwo="+HotkeyTwo,"HotkeyOneModifiers="+HotkeyOneModifiers,"HotkeyTwoModifiers="+HotkeyTwoModifiers,"HotkeyOneKey="+HotkeyOneKey,"HotkeyTwoKey="+HotkeyTwoKey,"PulsePeriod="+PulsePeriod,"PulseStrength="+PulseStrength,"PulseSteady="+PulseSteady,"HotkeyEnabled="+HotkeyEnabled,"HotkeyModifiers="+HotkeyModifiers,"HotkeyKey="+HotkeyKey,"PulseColor="+PulseColor,"PulseLow="+PulseLow,"PulseThreshold="+PulseThreshold,"QuickAudio="+QuickAudio,"Language="+Language,"Warnings="+Warnings,"Threshold="+Threshold,"Interval="+Interval,"CheckAtStartup="+CheckAtStartup,"NumbersOnly="+NumbersOnly,"FontName="+Encode(FontName),"TextSize="+TextSize,"BatteryShape="+BatteryShape,"TextColor="+TextColor,"BatteryRadius="+BatteryRadius,"BatteryStroke="+BatteryStroke,"BatteryWidth="+BatteryWidth,"BatteryHeight="+BatteryHeight};
  lines.Add("WheelVolume="+WheelVolume);
  lines.AddRange(Widget.Lines());
  foreach(var item in Profiles.OrderBy(x=>x.Key,StringComparer.OrdinalIgnoreCase)) { string prefix="Device."+Encode(item.Key)+"."; lines.Add(prefix+"WarningOverride="+item.Value.WarningOverride);lines.Add(prefix+"DeviceWarnings="+item.Value.DeviceWarnings);lines.Add(prefix+"DevicePulse="+item.Value.DevicePulse);lines.Add(prefix+"WarningThreshold="+item.Value.WarningThreshold);lines.Add(prefix+"PulseThreshold="+item.Value.PulseThreshold);lines.Add(prefix+"PulseColor="+item.Value.PulseColor);lines.Add(prefix+"RememberVolume="+item.Value.RememberVolume);lines.Add(prefix+"SavedVolume="+item.Value.SavedVolume);lines.Add(prefix+"IconFile="+Encode(item.Value.IconFile));lines.Add(prefix+"AudioId="+Encode(item.Value.AudioId)); lines.Add(prefix+"Name="+Encode(item.Value.Name)); lines.Add(prefix+"Color="+item.Value.ColorHex); lines.Add(prefix+"Symbol="+item.Value.Symbol); lines.Add(prefix+"Marker="+Encode(item.Value.Marker)); }
  Directory.CreateDirectory(Path.GetDirectoryName(file)); string temp=file+"."+Guid.NewGuid().ToString("N")+".tmp";
  try { File.WriteAllLines(temp,lines,Encoding.UTF8); if(File.Exists(file)) File.Replace(temp,file,null); else File.Move(temp,file); } finally { if(File.Exists(temp)) File.Delete(temp); }
 }
}

class ReleaseVersion : IComparable<ReleaseVersion> {
 public int Major,Minor,Patch; public string Pre,Tag;
 public static ReleaseVersion Parse(string tag) {
  if(tag==null) return null;
  var m=Regex.Match(tag,@"\Av?(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?\z");
  int a,b,c; if(!m.Success || !int.TryParse(m.Groups[1].Value,out a) || !int.TryParse(m.Groups[2].Value,out b) || !int.TryParse(m.Groups[3].Value,out c)) return null;
  string pre=m.Groups[4].Value; if(pre.Split('.').Any(p=>Regex.IsMatch(p,@"\A[0-9]+\z") && p.Length>1 && p[0]=='0')) return null;
  return new ReleaseVersion{Major=a,Minor=b,Patch=c,Pre=pre,Tag=tag};
 }
 public int CompareTo(ReleaseVersion v) {
  int n=Major.CompareTo(v.Major); if(n!=0) return n; n=Minor.CompareTo(v.Minor); if(n!=0) return n; n=Patch.CompareTo(v.Patch); if(n!=0) return n;
  if(Pre==v.Pre) return 0; if(Pre.Length==0) return 1; if(v.Pre.Length==0) return -1;
  var a=Pre.Split('.'); var b=v.Pre.Split('.'); for(int i=0;i<Math.Min(a.Length,b.Length);i++) {
   bool an=Regex.IsMatch(a[i],@"\A[0-9]+\z"),bn=Regex.IsMatch(b[i],@"\A[0-9]+\z");
   n=an && bn ? (a[i].Length==b[i].Length ? string.CompareOrdinal(a[i],b[i]) : a[i].Length.CompareTo(b[i].Length)) : an!=bn ? (an ? -1 : 1) : string.CompareOrdinal(a[i],b[i]); if(n!=0) return n;
  } return a.Length.CompareTo(b.Length);
 }
}
static class UpdateService {
 public const string ReleasesPage="https://github.com/Aldikiller/KopfhoererAkku/releases";
 public static ReleaseVersion Select(string json,string current) {
  var installed=ReleaseVersion.Parse(current); if(installed==null) throw new FormatException(L.T("Ungültige Programmversion."));
  var entries=new JavaScriptSerializer{MaxJsonLength=524288}.DeserializeObject(json) as object[]; if(entries==null) throw new FormatException(L.T("Ungültige GitHub-Antwort."));
  ReleaseVersion best=null; foreach(var entry in entries) {
   var row=entry as Dictionary<string,object>; if(row==null) continue; object tag,draft,pre;
   if(!row.TryGetValue("draft",out draft) || !(draft is bool) || (bool)draft || !row.TryGetValue("tag_name",out tag) || !(tag is string)) continue;
   var v=ReleaseVersion.Parse((string)tag); if(v==null || (installed.Pre.Length==0 && (v.Pre.Length>0 || (row.TryGetValue("prerelease",out pre) && pre is bool && (bool)pre)))) continue;
   if(v.CompareTo(installed)>0 && (best==null || v.CompareTo(best)>0)) best=v;
  } return best;
 }
 public static string Page(ReleaseVersion v) { return ReleasesPage+"/tag/"+Uri.EscapeDataString(v.Tag); }
 public static ReleaseVersion Check() {
  ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;
  var request=(HttpWebRequest)WebRequest.Create("https://api.github.com/repos/Aldikiller/KopfhoererAkku/releases?per_page=100");
  request.UserAgent="BleAirDock/"+AppPaths.Version; request.Accept="application/vnd.github+json"; request.Timeout=8000; request.ReadWriteTimeout=8000; request.AllowAutoRedirect=false;
  using(var response=(HttpWebResponse)request.GetResponse()) using(var stream=response.GetResponseStream()) using(var reader=new StreamReader(stream,Encoding.UTF8)) {
   if(response.StatusCode!=HttpStatusCode.OK) throw new WebException(L.T("GitHub-Abfrage fehlgeschlagen."));
   char[] buffer=new char[4096]; var text=new StringBuilder(); int n; while((n=reader.Read(buffer,0,buffer.Length))>0) { if(text.Length+n>524288) throw new FormatException(L.T("GitHub-Antwort zu groß.")); text.Append(buffer,0,n); }
   return Select(text.ToString(),AppPaths.Version);
  }
 }
}
