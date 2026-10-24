// SPDX-License-Identifier: MIT
using System; using System.Drawing; using System.Linq; using System.Windows.Forms;
partial class Tray {
 public void AdvancedUiCheck() {
  foreach(string language in L.Codes) {
   L.Language=language; settings.Language=language;
   foreach(bool hotkeys in new[]{false,true}) {
    using(var closer=new System.Windows.Forms.Timer{Interval=250}) {
     closer.Tick+=(sender,args)=> {
      closer.Stop(); var dialog=Application.OpenForms.Cast<Form>().FirstOrDefault(x=>x.Text==L.T(hotkeys ? "Tastenkürzel" : "Warnungen je Kopfhörer …"));
      if(dialog==null)throw new Exception("Advanced dialog missing");
      using(var bitmap=new Bitmap(dialog.Width,dialog.Height)) { dialog.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height)); bitmap.Save(AppPaths.Report((hotkeys ? "Hotkeys-" : "Warnings-")+language+".png")); }
      dialog.Close();
     };
     closer.Start(); if(hotkeys)HotkeyOptions();else DeviceWarningOptions();
    }
   }
  }
  L.Language="de"; settings.Language="de";
  Options(true);
  warningsPausedUntil=DateTime.UtcNow.AddMinutes(30);
  if(warningsPausedUntil<=DateTime.UtcNow)throw new Exception("Warning pause");
  warningsPausedUntil=DateTime.MinValue;
  settings.Profiles[selected[0]]=new DeviceProfile("#123456"){WarningOverride=true,DeviceWarnings=false,DevicePulse=true,WarningThreshold=40,PulseThreshold=15,PulseColor="#2244CC"};
  var config=WarningSettings(selected[0]); if(config.Warnings || !config.PulseLow || config.Threshold!=40 || config.PulseThreshold!=15 || config.PulseColor!="#2244CC")throw new Exception("Device override application");
  settings.HotkeyEnabled=true;settings.HotkeyOne=true;settings.HotkeyKey=75;settings.HotkeyOneKey=75;settings.HotkeyOneModifiers=settings.HotkeyModifiers;
  bool duplicate=false;try{DesiredKeys(settings);}catch(InvalidOperationException){duplicate=true;}
  if(!duplicate)throw new Exception("Duplicate shortcut configuration");
  ExitThread(); System.IO.File.WriteAllText(AppPaths.Report("Advanced-UI-Test.txt"),"OK: five-language warning and hotkey dialogs rendered, pulse controls, device overrides, duplicate shortcut validation. Global hotkey registration requires interactive desktop testing.");
 }
 DateTime warningsPausedUntil=DateTime.MinValue;
 Settings WarningSettings(string id) {
  var result=settings.Copy(); var profile=settings.Profile(id,0);
  if(profile.WarningOverride) { result.Warnings=profile.DeviceWarnings; result.Threshold=profile.WarningThreshold; result.PulseLow=profile.DevicePulse; result.PulseThreshold=profile.PulseThreshold; result.PulseColor=profile.PulseColor; }
  return result;
 }
 void AddWarningPause(ContextMenuStrip menu) {
  var pause=new ToolStripMenuItem(L.T("Akkuwarnungen pausieren"));
  foreach(int minutes in new[]{30,60,120}) { int duration=minutes; pause.DropDownItems.Add(minutes+" min",null,(s,e)=> { warningsPausedUntil=DateTime.UtcNow.AddMinutes(duration); warnings.Reset(); Render(); }); }
  if(warningsPausedUntil>DateTime.UtcNow) pause.Text+=" · "+warningsPausedUntil.ToLocalTime().ToString("HH:mm");
  pause.DropDownItems.Add(L.T("Warnungen fortsetzen"),null,(s,e)=> { warningsPausedUntil=DateTime.MinValue; warnings.Reset(); Render(); }); menu.Items.Add(pause);
 }
 void DeviceWarningOptions() {
  using(var form=new Form{Text=L.T("Warnungen je Kopfhörer …"),ClientSize=new Size(640,410),StartPosition=FormStartPosition.CenterScreen,AutoScaleMode=AutoScaleMode.Dpi}) {
   var tabs=new TabControl{Left=12,Top=12,Width=616,Height=335}; form.Controls.Add(tabs);
   var saves=new System.Collections.Generic.List<Action<Settings>>();
   for(int i=0;i<selected.Count;i++) {
    string id=selected[i]; var profile=settings.Profile(id,i); var config=WarningSettings(id); var device=devices.FirstOrDefault(x=>x.Id==id);
    var tab=new TabPage(device==null ? (i+1).ToString() : settings.Name(device)); tabs.TabPages.Add(tab);
    var own=new CheckBox{Left=16,Top=16,Width=550,Text=L.T("Eigene Warnungen für dieses Gerät"),Checked=profile.WarningOverride};
    var notify=new CheckBox{Left=16,Top=58,Width=390,Text=L.T("Bei niedrigem Akku benachrichtigen"),Checked=config.Warnings};
    var threshold=new NumericUpDown{Left=440,Top=58,Width=75,Minimum=1,Maximum=90,Value=config.Threshold};
    var pulse=new CheckBox{Left=16,Top=105,Width=390,Text=L.T("Leuchtwarnung"),Checked=config.PulseLow};
    var pulseThreshold=new NumericUpDown{Left=440,Top=105,Width=75,Minimum=1,Maximum=90,Value=config.PulseThreshold};
    string color=config.PulseColor; var colorButton=new Button{Left=16,Top=152,Width=290,Height=34,Text=L.T("Pulsfarbe …"),BackColor=ColorTranslator.FromHtml(color)};
    colorButton.Click+=(s,e)=> { using(var dialog=new ColorDialog{Color=ColorTranslator.FromHtml(color),FullOpen=true}) if(dialog.ShowDialog(form)==DialogResult.OK) { color="#"+dialog.Color.R.ToString("X2")+dialog.Color.G.ToString("X2")+dialog.Color.B.ToString("X2"); colorButton.BackColor=dialog.Color; } };
    tab.Controls.AddRange(new Control[]{own,notify,threshold,pulse,pulseThreshold,colorButton,new Label{Left=16,Top=210,Width=550,Height=65,Text=L.T("Zahlen: Warnung bei / unter (%). Ohne eigene Warnungen gelten die allgemeinen Einstellungen.")}});
    Action enable=()=> { foreach(var control in new Control[]{notify,threshold,pulse,pulseThreshold,colorButton}) control.Enabled=own.Checked; }; own.CheckedChanged+=(s,e)=>enable(); enable();
    saves.Add(next=> { DeviceProfile target; if(!next.Profiles.TryGetValue(id,out target)) { target=new DeviceProfile(profile.ColorHex){Name=profile.Name,Marker=profile.Marker,AudioId=profile.AudioId,Symbol=profile.Symbol}; next.Profiles[id]=target; } target.WarningOverride=own.Checked; target.DeviceWarnings=notify.Checked; target.WarningThreshold=(int)threshold.Value; target.DevicePulse=pulse.Checked; target.PulseThreshold=(int)pulseThreshold.Value; target.PulseColor=color; });
   }
   var save=new Button{Left=350,Top=365,Width=125,Height=32,Text=L.T("Speichern")}; save.Click+=(s,e)=> { try { var next=Settings.Read(SettingsPath); foreach(var apply in saves) apply(next); next.Save(SettingsPath); settings=next; warnings.Reset(); Render(); form.Close(); } catch(Exception ex) { MessageBox.Show(ex.Message); } };
   var close=new Button{Left=490,Top=365,Width=125,Height=32,Text=L.T("Schließen"),DialogResult=DialogResult.Cancel}; form.Controls.AddRange(new Control[]{save,close}); form.CancelButton=close; form.ShowDialog(settingsForm);
  }
 }
}
