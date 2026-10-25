// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

static class FullscreenWindow {
 [StructLayout(LayoutKind.Sequential)] struct RECT {public int Left,Top,Right,Bottom; public Rectangle Bounds {get{return Rectangle.FromLTRB(Left,Top,Right,Bottom);}}}
 [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr h,out RECT r);
 [DllImport("user32.dll")] static extern bool IsIconic(IntPtr h);
 [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h,out uint pid);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern int GetClassName(IntPtr h,StringBuilder name,int length);
 [DllImport("dwmapi.dll")] static extern int DwmGetWindowAttribute(IntPtr h,int attribute,out RECT value,int size);
 internal static bool Covers(Rectangle window,Rectangle monitor) {return window.Left<=monitor.Left+2 && window.Top<=monitor.Top+2 && window.Right>=monitor.Right-2 && window.Bottom>=monitor.Bottom-2;}
 internal static bool ShouldHide(bool enabled,bool manual,bool fullscreen) {return manual || (enabled && fullscreen);}
 public static bool OnScreen(Rectangle widget) {
  var h=GetForegroundWindow();if(h==IntPtr.Zero || IsIconic(h))return false;
  uint pid;GetWindowThreadProcessId(h,out pid);if(pid==(uint)Process.GetCurrentProcess().Id)return false;
  var cls=new StringBuilder(128);GetClassName(h,cls,cls.Capacity);
  if(new[]{"Progman","WorkerW","Shell_TrayWnd","Shell_SecondaryTrayWnd"}.Contains(cls.ToString()))return false;
  RECT rect;if(DwmGetWindowAttribute(h,9,out rect,Marshal.SizeOf(typeof(RECT)))!=0 && !GetWindowRect(h,out rect))return false;
  return Covers(rect.Bounds,Screen.FromRectangle(widget).Bounds);
 }
}

// Windows does not expose a MouseWheel event on NotifyIcon. This hook is installed
// only while the user enables the option; no mouse history is stored. All audio
// work is queued outside the hook, and unrelated wheel events pass through.
sealed class DeviceWheel : IDisposable {
 delegate IntPtr Hook(int code,IntPtr message,IntPtr data);
 [StructLayout(LayoutKind.Sequential)] struct MouseData {public Point Point;public uint Data,Flags,Time;public UIntPtr Extra;}
 [StructLayout(LayoutKind.Sequential)] struct Identifier {public uint Size;public IntPtr Window;public uint Id;public Guid Guid;}
 [StructLayout(LayoutKind.Sequential)] struct RECT {public int Left,Top,Right,Bottom;}
 [DllImport("user32.dll",SetLastError=true)] static extern IntPtr SetWindowsHookEx(int id,Hook callback,IntPtr module,uint thread);
 [DllImport("user32.dll")] static extern bool UnhookWindowsHookEx(IntPtr hook);
 [DllImport("user32.dll")] static extern IntPtr CallNextHookEx(IntPtr hook,int code,IntPtr message,IntPtr data);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)] static extern IntPtr GetModuleHandle(string name);
 [DllImport("shell32.dll")] static extern int Shell_NotifyIconGetRect(ref Identifier id,out RECT rectangle);
 [DllImport("user32.dll")] static extern IntPtr WindowFromPoint(Point point);
 [DllImport("user32.dll")] static extern IntPtr GetAncestor(IntPtr window,uint flags);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern int GetClassName(IntPtr window,StringBuilder text,int size);
 internal static IntPtr AtPoint(Point point) {return WindowFromPoint(point);}
 internal static bool IsTrayWindow(IntPtr window) {if(window==IntPtr.Zero)return false;var name=new StringBuilder(256);GetClassName(GetAncestor(window,2),name,name.Capacity);var value=name.ToString();return value.IndexOf("Tray",StringComparison.OrdinalIgnoreCase)>=0 || value.IndexOf("Overflow",StringComparison.OrdinalIgnoreCase)>=0;}
 readonly Hook callback;readonly Func<Point,int,bool> action;IntPtr handle;
 public DeviceWheel(Func<Point,int,bool> action) {this.action=action;callback=Receive;handle=SetWindowsHookEx(14,callback,GetModuleHandle(null),0);if(handle==IntPtr.Zero)throw new Win32Exception(Marshal.GetLastWin32Error());}
 IntPtr Receive(int code,IntPtr message,IntPtr data) {
  if(code>=0 && message.ToInt32()==0x20A) {try{var m=(MouseData)Marshal.PtrToStructure(data,typeof(MouseData));if(action(m.Point,unchecked((short)(m.Data>>16))))return new IntPtr(1);}catch(Exception){/* Never block another application's input on failure. */}}
  return CallNextHookEx(handle,code,message,data);
 }
 public static Rectangle IconBounds(NotifyIcon icon) {
  if(!icon.Visible)return Rectangle.Empty;
  // .NET Framework NotifyIcon does not expose its Shell identity publicly.
  var fields=typeof(NotifyIcon).GetFields(BindingFlags.Instance|BindingFlags.NonPublic);
  var win=fields.FirstOrDefault(x=>typeof(NativeWindow).IsAssignableFrom(x.FieldType));
  var id=fields.FirstOrDefault(x=>x.Name=="id" || x.Name=="_id");
  if(win==null || id==null)return Rectangle.Empty;var window=win.GetValue(icon) as NativeWindow;if(window==null)return Rectangle.Empty;
  var ident=new Identifier{Size=(uint)Marshal.SizeOf(typeof(Identifier)),Window=window.Handle,Id=Convert.ToUInt32(id.GetValue(icon))};RECT rect;
  return Shell_NotifyIconGetRect(ref ident,out rect)==0 ? Rectangle.FromLTRB(rect.Left,rect.Top,rect.Right,rect.Bottom) : Rectangle.Empty;
 }
 public void Dispose(){if(handle!=IntPtr.Zero){UnhookWindowsHookEx(handle);handle=IntPtr.Zero;}GC.KeepAlive(callback);}
}
partial class Tray {
 Func<Point,IntPtr> windowAtPoint=DeviceWheel.AtPoint;
 Func<Rectangle,bool> fullscreenProbe=FullscreenWindow.OnScreen;
 Func<string,int> getWheelVolume=EndpointVolume.Get; Action<string,int> setWheelVolume=EndpointVolume.Set;
 DeviceWheel wheel;bool fullscreenHidden,wheelFailed;int wheelRemainder,wheelSlot=-1;
 internal static IEnumerable<string> MenuLabels(ToolStripItemCollection items) {foreach(ToolStripItem item in items){yield return item.Text;var branch=item as ToolStripMenuItem;if(branch!=null)foreach(var label in MenuLabels(branch.DropDownItems))yield return label;}}
 void FillCompactMenu(ContextMenuStrip menu) {
  var audio=new ToolStripMenuItem(L.T("Audioausgabe"));
  for(int i=0;i<selected.Count;i++) {int slot=i;var id=selected[i];var d=devices.FirstOrDefault(x=>x.Id==id);var profile=settings.Profile(id,i);var name=d!=null ? settings.Name(d) : profile.Name.Length>0 ? profile.Name : L.T("Gespeicherter Kopfhörer");
   var item=new ToolStripMenuItem(name){Checked=IsAudioActive(id,i)};item.Click+=(s,e)=>SwitchAudio(slot);audio.DropDownItems.Add(item);
  }
  if(audio.DropDownItems.Count==0)audio.Enabled=false;menu.Items.Add(audio);
  var toggle=new ToolStripMenuItem(L.T(widgetHidden ? "Widget anzeigen" : "Widget ausblenden")){Enabled=settings.Widget.Mode!=DisplayMode.Infobereich};toggle.Click+=(s,e)=>{widgetHidden=!widgetHidden;Render();};menu.Items.Add(toggle);
  menu.Items.Add(L.T("Einstellungen …"),null,(s,e)=>Options());
  var more=new ToolStripMenuItem(L.T("Weitere Aktionen"));
  more.DropDownItems.Add(L.T("Geräteübersicht"),null,(s,e)=>ShowOverview());more.DropDownItems.Add(L.T("Geräte auswählen …"),null,(s,e)=>Pick());more.DropDownItems.Add(L.T("Jetzt aktualisieren"),null,(s,e)=>Refresh());
  using(var pause=new ContextMenuStrip()){AddWarningPause(pause);while(pause.Items.Count>0)more.DropDownItems.Add(pause.Items[0]);}
  more.DropDownItems.Add(new ToolStripSeparator());more.DropDownItems.Add(L.T("Neustart"),null,(s,e)=>Restart());menu.Items.Add(more);
  menu.Items.Add(new ToolStripSeparator());menu.Items.Add(L.T("Beenden"),null,(s,e)=>ExitThread());
 }
 void UpdateFullscreen() {
  bool hidden=settings.Widget.HideFullscreen && settings.Widget.Mode!=DisplayMode.Infobereich && widget!=null && !widget.IsDisposed && fullscreenProbe(widget.Bounds);
  if(hidden!=fullscreenHidden){fullscreenHidden=hidden;Render();}
 }
 void SyncWheel() {
  if(!settings.WheelVolume){if(wheel!=null){wheel.Dispose();wheel=null;}wheelFailed=false;wheelRemainder=0;wheelSlot=-1;return;}
  if(wheel==null && !wheelFailed)try{wheel=new DeviceWheel(HandleWheel);}catch(Win32Exception){wheelFailed=true;Notify(5000,"BleAirDock",L.T("Mausradsteuerung konnte nicht aktiviert werden."),ToolTipIcon.Warning);}
 }
 internal static int WheelTarget(int current,int notches) {return (int)Math.Max(0L,Math.Min(100L,(long)current+notches*2L));}
 bool HandleWheel(Point point,int delta) {
  if(!settings.WheelVolume || exiting || delta==0)return false;
  int slot=-1;var surface=windowAtPoint(point);
  for(int i=0;i<icons.Count && i<selected.Count;i++)if(DeviceWheel.IconBounds(icons[i]).Contains(point) && DeviceWheel.IsTrayWindow(surface)){slot=i;break;}
  if(slot<0 && widget!=null && !widget.IsDisposed && surface==widget.Handle)slot=widget.WheelSlot(point);
  if(slot<0 || slot>=selected.Count){wheelRemainder=0;wheelSlot=-1;return false;}
  string id=selected[slot],endpoint=settings.Profile(id,slot).AudioId;
  if(string.IsNullOrEmpty(endpoint) || !audioOutputs.Any(x=>x.Id==endpoint && x.Active))return false;
  if(wheelSlot!=slot){wheelRemainder=0;wheelSlot=slot;}wheelRemainder+=delta;int steps=wheelRemainder/120;wheelRemainder%=120;if(steps==0)return true;
  var ui=dispatcher;if(ui==null || ui.IsDisposed || !ui.IsHandleCreated)return false;
  int targetSlot=slot;
  try{ui.BeginInvoke((Action)(()=>{
   if(exiting || !settings.WheelVolume || targetSlot>=selected.Count || selected[targetSlot]!=id || settings.Profile(id,targetSlot).AudioId!=endpoint)return;
   try{int value=WheelTarget(getWheelVolume(endpoint),steps);setWheelVolume(endpoint,value);
    var profile=settings.Profile(id,targetSlot);if(profile.RememberVolume){profile.SavedVolume=value;try{settings.Save(SettingsPath);}catch(IOException){}catch(UnauthorizedAccessException){}}
    if(targetSlot<icons.Count)icons[targetSlot].Text=ShortText((devices.FirstOrDefault(x=>x.Id==id)!=null ? settings.Name(devices.First(x=>x.Id==id)) : settings.Profile(id,targetSlot).Name)+" · "+L.T("Lautstärke")+": "+value+" %");
   }catch(COMException){/* A disconnected endpoint must not trigger repeated popups. */}
  }));return true;}catch(InvalidOperationException){return false;}
 }
 static string ShortText(string text){return text.Length>63 ? text.Substring(0,63) : text;}
 void StopInteraction(){if(wheel!=null){wheel.Dispose();wheel=null;}}
}
