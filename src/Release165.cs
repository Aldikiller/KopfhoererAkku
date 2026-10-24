// SPDX-License-Identifier: MIT
using System; using System.Drawing; using System.IO; using System.Linq; using System.Runtime.InteropServices; using System.Windows.Forms;
partial class Tray {
 public void Release165UiCheck() {
  settings.Widget.Mode=DisplayMode.Beides;Render();widgetHidden=true;Render();
  if(widget.Visible || !icons[0].Visible)throw new Exception("Hidden widget restore access");
  widgetHidden=false;Render();if(!widget.Visible)throw new Exception("Widget restore");
  foreach(string language in L.Codes) {
   L.Language=language;
   using(var closer=new System.Windows.Forms.Timer{Interval=200}) {
    closer.Tick+=(sender,args)=> {closer.Stop();var dialog=Application.OpenForms.Cast<Form>().First(x=>x.Text==L.T("Lautstärke je Kopfhörer"));using(var bitmap=new Bitmap(dialog.Width,dialog.Height)){dialog.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));bitmap.Save(AppPaths.Report("Volume-"+language+".png"));}dialog.Close();};
    closer.Start();VolumeOptions();
   }
  }
  L.Language="de"; Options(true); ExitThread();File.WriteAllText(AppPaths.Report("Release165-UI-Test.txt"),"OK: temporary widget hide and restore with accessible tray, volume options in five languages, settings and image chooser layout. Real volume restoration requires hardware test.");
 }
 bool widgetHidden;
 void AddWidgetVisibility(ContextMenuStrip menu) { menu.Items.Add(L.T("Widget ausblenden / anzeigen"),null,(s,e)=> { widgetHidden=!widgetHidden; Render(); }); }
 string DeviceStatus(Device d) { return StatusText(error.Length>0,EffectiveConnection(d),d.Battery); }
 internal static string StatusText(bool failed,bool? connected,int? battery) { return failed ? L.T("Abfrage fehlgeschlagen") : connected==false ? L.T("Nicht verbunden") : connected!=true ? L.T("Verbindung unbekannt") : !battery.HasValue ? L.T("Windows liefert keinen Akkustand") : L.T("Verbunden"); }
 void RememberCurrentVolume(string target) {
  bool changed=false;
  foreach(var pair in settings.Profiles) if(pair.Value.RememberVolume && pair.Value.AudioId==activeOutput && activeOutput.Length>0 && activeOutput!=target) { try {pair.Value.SavedVolume=EndpointVolume.Get(activeOutput); changed=true;} catch(System.Runtime.InteropServices.COMException) {} }
  if(changed) settings.Save(SettingsPath);
 }
 void VolumeOptions() {
  using(var form=new Form{Text=L.T("Lautstärke je Kopfhörer"),ClientSize=new Size(580,255),StartPosition=FormStartPosition.CenterScreen,AutoScaleMode=AutoScaleMode.Dpi}) {
   var boxes=new CheckBox[selected.Count]; var levels=new NumericUpDown[selected.Count];
   for(int i=0;i<selected.Count;i++) { var profile=settings.Profile(selected[i],i); var device=devices.FirstOrDefault(x=>x.Id==selected[i]); int y=20+i*60;
    boxes[i]=new CheckBox{Left=18,Top=y,Width=400,Text=device==null ? (i+1).ToString() : settings.Name(device),Checked=profile.RememberVolume};
    levels[i]=new NumericUpDown{Left=440,Top=y,Width=90,Minimum=0,Maximum=100,Value=profile.SavedVolume>=0 ? profile.SavedVolume : 25};
    form.Controls.AddRange(new Control[]{boxes[i],levels[i]});
   }
   form.Controls.Add(new Label{Left=18,Top=145,Width=535,Height=48,Text=L.T("Optional: Lautstärke beim Wechsel merken. Die Zahl legt den ersten Zielwert (%) fest. Stummschaltung bleibt unverändert.")});
   var save=new Button{Left=285,Top=208,Width=125,Height=32,Text=L.T("Speichern")}; var close=new Button{Left=430,Top=208,Width=125,Height=32,Text=L.T("Schließen"),DialogResult=DialogResult.Cancel};
   save.Click+=(s,e)=> { try { var next=Settings.Read(SettingsPath); for(int i=0;i<selected.Count;i++) { DeviceProfile profile; if(!next.Profiles.TryGetValue(selected[i],out profile)) { var old=settings.Profile(selected[i],i); profile=new DeviceProfile(old.ColorHex){Name=old.Name,AudioId=old.AudioId,Symbol=old.Symbol,Marker=old.Marker,IconFile=old.IconFile}; next.Profiles[selected[i]]=profile; } profile.RememberVolume=boxes[i].Checked;profile.SavedVolume=(int)levels[i].Value; } next.Save(SettingsPath);settings=next;form.Close(); } catch(Exception ex) { MessageBox.Show(ex.Message); } };
   form.Controls.AddRange(new Control[]{save,close});form.CancelButton=close;form.ShowDialog(settingsForm);
  }
 }
}
static class EndpointVolume {
 [ComImport,Guid("5CDF2C82-841E-4546-9722-0CF74078229A"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface Volume {
  [PreserveSig]int RegisterControlChangeNotify(IntPtr sink); [PreserveSig]int UnregisterControlChangeNotify(IntPtr sink); [PreserveSig]int GetChannelCount(out uint channels);
  [PreserveSig]int SetMasterVolumeLevel(float level,ref Guid context); [PreserveSig]int SetMasterVolumeLevelScalar(float level,ref Guid context);
  [PreserveSig]int GetMasterVolumeLevel(out float level); [PreserveSig]int GetMasterVolumeLevelScalar(out float level);
 }
 static T With<T>(string id,Func<Volume,T> action) { var api=AudioApi.Create();AudioApi.Endpoint device=null;object instance=null;IntPtr pointer=IntPtr.Zero;
  try { Marshal.ThrowExceptionForHR(api.GetDevice(id,out device));var iid=typeof(Volume).GUID;Marshal.ThrowExceptionForHR(device.Activate(ref iid,23,IntPtr.Zero,out pointer));instance=Marshal.GetObjectForIUnknown(pointer);return action((Volume)instance); }
  finally {if(pointer!=IntPtr.Zero)Marshal.Release(pointer);AudioApi.Free(instance);AudioApi.Free(device);AudioApi.Free(api);}
 }
 public static int Get(string id) {return With(id,volume=> {float value;Marshal.ThrowExceptionForHR(volume.GetMasterVolumeLevelScalar(out value));return Math.Max(0,Math.Min(100,(int)Math.Round(value*100)));});}
 public static void Set(string id,int percent) {With(id,volume=> {var context=Guid.Empty;Marshal.ThrowExceptionForHR(volume.SetMasterVolumeLevelScalar(Math.Max(0,Math.Min(100,percent))/100f,ref context));return true;});}
}
