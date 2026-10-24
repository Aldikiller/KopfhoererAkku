// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

partial class Tray {
 AudioEvents audioEvents;string activeOutput="",audioError="";List<AudioOutput> audioOutputs=new List<AudioOutput>();bool audioQueued;int clickSlot;
 void UpdateAudio(){try{audioOutputs=AudioApi.List();activeOutput=AudioApi.Default();AutoAssignAudio();audioError="";}catch(Exception ex){audioOutputs.Clear();activeOutput="";audioError=ex.GetType().Name+" (0x"+ex.HResult.ToString("X8")+"): "+ex.Message;}}
 void StartAudioEvents(){UpdateAudio();try{audioEvents=new AudioEvents(()=>{var ui=dispatcher;if(ui==null || ui.IsDisposed)return;try{ui.BeginInvoke((Action)(()=>{if(exiting || audioQueued)return;audioQueued=true;try{UpdateAudio();Render();if(updateOverview!=null)updateOverview();}finally{audioQueued=false;}}));}catch(InvalidOperationException){}});}catch{}Render();}
 internal static bool AudioMatch(string output,string current,IEnumerable<AudioOutput> outputs){return !string.IsNullOrEmpty(output) && output==current && outputs.Any(x=>x.Id==output && x.Active);}
 bool IsAudioActive(string id,int slot){return AudioMatch(settings.Profile(id,slot).AudioId,activeOutput,audioOutputs);}
 void SwitchAudio(int slot){
  if(slot<0 || slot>=selected.Count)return;UpdateAudio();string id=selected[slot];var profile=settings.Profile(id,slot);
  if(profile.AudioId.Length==0){AudioOptions(id);if(settings.QuickAudio && settings.Profile(id,slot).AudioId.Length>0)SwitchAudio(slot);return;}
  try{RememberCurrentVolume(profile.AudioId); if(profile.RememberVolume && profile.SavedVolume>=0) EndpointVolume.Set(profile.AudioId,profile.SavedVolume); AudioApi.Switch(profile.AudioId); UpdateAudio();Render();if(updateOverview!=null)updateOverview();}
  catch(Exception ex){UpdateAudio();Render();MessageBox.Show(L.T("Audioausgabe konnte nicht umgeschaltet werden.")+"\n"+ex.Message,"Kopfhörer-Akku",MessageBoxButtons.OK,MessageBoxIcon.Information);}
 }
 internal static void AudioBadge(Graphics g){
  using(var shadow=new SolidBrush(Color.FromArgb(25,35,30)))g.FillEllipse(shadow,17,0,15,15);
  using(var green=new SolidBrush(Color.FromArgb(18,151,76)))g.FillEllipse(green,18,1,13,13);
  using(var pen=new Pen(Color.White,2)){pen.StartCap=System.Drawing.Drawing2D.LineCap.Round;pen.EndCap=System.Drawing.Drawing2D.LineCap.Round;g.DrawLines(pen,new[]{new PointF(21,7),new PointF(24,10),new PointF(29,4)});}
 }
 public void AudioOptionsPreview(){AudioOptions(null,true);}
 void AudioOptions(string requested=null,bool preview=false){
  UpdateAudio();using(var form=new Form{Text=L.T("Audio-Schnellauswahl")+" · "+AppPaths.Version,Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath),ClientSize=new Size(650,390),FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false,StartPosition=FormStartPosition.CenterScreen,AutoScaleMode=AutoScaleMode.Dpi}){
   var quick=new CheckBox{Left=18,Top=16,Width=615,Height=38,Text=L.T("Ein Klick auf das Akkusymbol wechselt die Audioausgabe"),Checked=settings.QuickAudio};
   var note=new Label{Left=18,Top=57,Width=615,Height=48,Text=L.T("Eindeutige Geräte werden automatisch zugeordnet. Bei mehreren Ausgängen bitte auswählen. Grün mit Häkchen: aktive Ausgabe.")};
   if(audioError.Length>0)note.Text=L.T("Audioausgaben konnten nicht gelesen werden.")+" "+audioError;
   else if(audioOutputs.Count==0)note.Text=L.T("Keine Audioausgabe gefunden. Kopfhörer verbinden und dieses Fenster erneut öffnen.");
   var boxes=new List<ComboBox>();var values=new List<AudioOutput>{new AudioOutput{Id="",Name=L.T("Nicht zugeordnet"),Active=true}};values.AddRange(audioOutputs);
   for(int i=0;i<selected.Count;i++){string saved=settings.Profile(selected[i],i).AudioId;if(saved.Length>0 && !values.Any(x=>x.Id==saved))values.Add(new AudioOutput{Id=saved,Name=L.T("Nicht verfügbar / getrennt"),Active=false});}
   for(int i=0;i<selected.Count;i++){string deviceId=selected[i];var device=devices.FirstOrDefault(x=>x.Id==deviceId);var label=new Label{Left=18,Top=113+i*69,Width=610,Height=23,Text=device==null ? settings.Profile(deviceId,i).Name : settings.Name(device)};
    var box=new ComboBox{AccessibleName="AudioOutput"+i,Left=18,Top=137+i*69,Width=610,DropDownStyle=ComboBoxStyle.DropDownList};box.Items.AddRange(values.Cast<object>().ToArray());string saved=settings.Profile(deviceId,i).AudioId;box.SelectedIndex=Math.Max(0,values.FindIndex(x=>x.Id==saved));boxes.Add(box);form.Controls.AddRange(new Control[]{label,box});
   }
   var interval=new NumericUpDown{Left=405,Top=271,Width=110,Minimum=30,Maximum=300,Increment=5,Value=settings.Interval};
   var intervalLabel=new Label{Left=18,Top=274,Width=375,Text=L.T("Abfrage alle (Sekunden)")};
   var intervalNote=new Label{Left=18,Top=307,Width=610,Height=28,Text=L.T("Keine kurzen Zusatzabfragen nach Standby. Manuelles Aktualisieren bleibt möglich.")};
   var save=new Button{Text=L.T("Speichern"),Left=349,Top=344,Width=135,Height=32};var close=new Button{Text=L.T("Schließen"),Left=494,Top=344,Width=135,Height=32,DialogResult=DialogResult.Cancel};
   var reload=new Button{Text=L.T("Neu laden"),Left=18,Top=344,Width=145,Height=32};
   reload.Click+=(s,e)=>{
    var previous=boxes.Select(x=>(AudioOutput)x.SelectedItem).ToArray();UpdateAudio();values.Clear();values.Add(new AudioOutput{Id="",Name=L.T("Nicht zugeordnet"),Active=true});values.AddRange(audioOutputs);
    foreach(var old in previous)if(old.Id.Length>0 && !values.Any(x=>x.Id==old.Id))values.Add(new AudioOutput{Id=old.Id,Name=old.Name,Active=false});
    for(int i=0;i<boxes.Count;i++){boxes[i].BeginUpdate();boxes[i].Items.Clear();boxes[i].Items.AddRange(values.Cast<object>().ToArray());boxes[i].SelectedIndex=Math.Max(0,values.FindIndex(x=>x.Id==previous[i].Id));boxes[i].EndUpdate();}
    note.Text=audioError.Length>0 ? L.T("Audioausgaben konnten nicht gelesen werden.")+" "+audioError : audioOutputs.Count==0 ? L.T("Keine Audioausgabe gefunden. Kopfhörer verbinden und dieses Fenster erneut öffnen.") : L.T("Windows-Audioausgaben gefunden: ")+audioOutputs.Count+". "+L.T("Bitte die passende Ausgabe auswählen.");
   };
   var diagnostic=new Button{Text=L.T("Diagnose"),Left=174,Top=344,Width=155,Height=32};diagnostic.Click+=(s,e)=>MessageBox.Show("Kopfhörer-Akku "+AppPaths.Version+"\nWindows: "+Environment.OSVersion.Version+"\n"+L.T("Windows-Audioausgaben gefunden: ")+audioOutputs.Count+"\nActive: "+audioOutputs.Count(x=>x.Active)+"\nDefault: "+(activeOutput.Length>0)+"\n"+audioError,L.T("Diagnose"));
   save.Click+=(s,e)=>{try{var next=Settings.Read(SettingsPath);next.QuickAudio=quick.Checked;next.Interval=(int)interval.Value;
    for(int i=0;i<selected.Count;i++){DeviceProfile profile;if(!next.Profiles.TryGetValue(selected[i],out profile)){profile=new DeviceProfile(i==0 ? "#13705B" : "#1C579F");next.Profiles[selected[i]]=profile;}profile.AudioId=values[boxes[i].SelectedIndex].Id;}
    if(selected.Count==2 && next.Profile(selected[0],0).AudioId.Length>0 && next.Profile(selected[0],0).AudioId==next.Profile(selected[1],1).AudioId){MessageBox.Show(L.T("Bitte unterschiedliche Audioausgaben zuordnen."));return;}
    next.Save(SettingsPath);settings=next;Render();if(updateOverview!=null)updateOverview();form.Close();
   }catch(Exception ex){MessageBox.Show(L.T("Speichern fehlgeschlagen: ")+ex.Message);}};
   form.Controls.AddRange(new Control[]{quick,note,intervalLabel,interval,intervalNote,save,close,reload,diagnostic});form.CancelButton=close;
   if(preview){form.Show();Application.DoEvents();using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(0,0,form.Width,form.Height));bitmap.Save(AppPaths.Report("Audio-Schnellauswahl.png"));}form.Close();}else form.ShowDialog();
  }
 }
 public void AudioCheck(bool switchSame){
  audioOutputs=AudioApi.List();activeOutput=AudioApi.Default();using(var events=new AudioEvents(()=>{})){if(switchSame){if(activeOutput.Length==0)throw new Exception("No default audio output");AudioApi.Switch(activeOutput);if(AudioApi.Default()!=activeOutput)throw new Exception("Default output verification failed");}}
  var bluetooth=Native.Scan().Where(x=>x.ContainerId!=Guid.Empty).Select(x=>x.ContainerId).Distinct().ToArray();var containers=audioOutputs.Where(x=>x.ContainerId!=Guid.Empty).Select(x=>x.ContainerId).Distinct().ToArray();
  File.WriteAllText(AppPaths.Report("Audio-Test.txt"),"OK: "+audioOutputs.Count+" outputs; default present: "+(activeOutput.Length>0)+"; notifications registered and disposed; same-output setter test: "+switchSame+"; Bluetooth containers: "+bluetooth.Length+"; audio containers: "+containers.Length+"; shared containers: "+bluetooth.Intersect(containers).Count());
  using(var bitmap=new Bitmap(320,120))using(var graphics=Graphics.FromImage(bitmap)){graphics.Clear(Color.FromArgb(32,35,39));for(int i=0;i<2;i++)using(var icon=BatteryIcon(i==0?75:65,i,true,null,false,new Settings(),0,"A",i==0)){graphics.DrawIcon(icon,new Rectangle(20+i*150,15,80,80));using(var native=icon.ToBitmap())graphics.DrawImage(native,112+i*150,76,16,16);}bitmap.Save(AppPaths.Report("Audio-Symbole.png"));}
 }
}
static class AudioTests {
 public static void Run(){
  if(typeof(AudioApi.Collection).GUID!=new Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"))throw new Exception("Windows SDK audio collection contract mismatch");
  var outputs=new[]{new AudioOutput{Id="a",Active=true},new AudioOutput{Id="b",Active=false}};
  if(!Tray.AudioMatch("a","a",outputs) || Tray.AudioMatch("b","b",outputs) || Tray.AudioMatch("","",outputs) || Tray.AudioMatch("missing","missing",outputs) || Tray.AudioMatch("b","a",outputs))throw new Exception("Incorrect active output marker");
  using(var icon=Tray.BatteryIcon(75,0,true,null,false,new Settings(),0,"A",true))using(var full=icon.ToBitmap())using(var small=new Bitmap(full,16,16)){int green=0;for(int y=0;y<8;y++)for(int x=8;x<16;x++){var pixel=small.GetPixel(x,y);if(pixel.G>pixel.R*1.6 && pixel.G>pixel.B*1.3)green++;}if(green<5)throw new Exception("Active badge not visible at 16px");}
  string file=AppPaths.Report("audio-settings-test-"+Guid.NewGuid().ToString("N")+".ini");
  try{var settings=new Settings();if(settings.Interval!=30)throw new Exception("Polling default is not 30s");settings.QuickAudio=false;settings.Interval=60;settings.Profiles["Bluetooth test"]=new DeviceProfile("#13705B"){AudioId="{endpoint}=ä",Name="Test"};settings.Save(file);var saved=Settings.Read(file);if(saved.QuickAudio || saved.Interval!=60 || saved.Profile("Bluetooth test",0).AudioId!="{endpoint}=ä")throw new Exception("Audio settings persistence");File.WriteAllText(file,"Interval=5\n");if(Settings.Read(file).Interval!=30)throw new Exception("Old short interval was not migrated");}finally{if(File.Exists(file))File.Delete(file);}
 }
}

