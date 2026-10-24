// SPDX-License-Identifier: MIT
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
sealed class AudioHotkey : NativeWindow, IDisposable {
 [DllImport("user32.dll",SetLastError=true)] static extern bool RegisterHotKey(IntPtr window,int id,uint modifiers,uint key);
 [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr window,int id);
 readonly Action action;
 public AudioHotkey(int modifiers,int key,Action callback) {
  action=callback; CreateHandle(new CreateParams());
  if(!RegisterHotKey(Handle,1,(uint)(modifiers|0x4000),(uint)key)) { int error=Marshal.GetLastWin32Error(); DestroyHandle(); throw new InvalidOperationException(L.T("Dieses Tastenkürzel ist bereits belegt oder nicht verfügbar.")+" ("+error+")"); }
 }
 protected override void WndProc(ref Message message) { if(message.Msg==0x312 && message.WParam.ToInt32()==1) action(); base.WndProc(ref message); }
 public void Dispose() { if(Handle!=IntPtr.Zero) { UnregisterHotKey(Handle,1); DestroyHandle(); } }
 internal static int SuggestedKey(string language) { return language=="en" ? 72 : language=="es" ? 65 : language=="fr" ? 67 : language=="uk" ? 89 : 75; }
}
partial class Tray {
 readonly System.Collections.Generic.Dictionary<string,AudioHotkey> registeredKeys=new System.Collections.Generic.Dictionary<string,AudioHotkey>();
 bool hotkeyBusy;
 void RunAudioHotkey(int action) {
  if(exiting || hotkeyBusy)return; hotkeyBusy=true;
  try { UpdateAudio(); int slot=action==0 ? (selected.Count>1 && IsAudioActive(selected[0],0) ? 1 : 0) : action-1; SwitchAudio(slot); }
  finally { hotkeyBusy=false; }
 }
 System.Collections.Generic.Dictionary<string,int> DesiredKeys(Settings config) {
  var result=new System.Collections.Generic.Dictionary<string,int>();
  bool[] enabled={config.HotkeyEnabled,config.HotkeyOne,config.HotkeyTwo};
  int[] modifiers={config.HotkeyModifiers,config.HotkeyOneModifiers,config.HotkeyTwoModifiers};
  int[] keys={config.HotkeyKey==0 ? AudioHotkey.SuggestedKey(config.Language) : config.HotkeyKey,config.HotkeyOneKey,config.HotkeyTwoKey};
  for(int i=0;i<3;i++) if(enabled[i]) { string signature=modifiers[i]+":"+keys[i]; if(result.ContainsKey(signature))throw new InvalidOperationException(L.T("Bitte unterschiedliche Tastenkürzel wählen.")); result.Add(signature,i); }
  return result;
 }
 void ApplyHotkeys(Settings config,Action save) {
  var desired=DesiredKeys(config); var added=new System.Collections.Generic.Dictionary<string,AudioHotkey>();
  try {
   foreach(var pair in desired) if(!registeredKeys.ContainsKey(pair.Key)) { var values=pair.Key.Split(':'); string signature=pair.Key; added.Add(signature,new AudioHotkey(int.Parse(values[0]),int.Parse(values[1]),()=>DispatchHotkey(signature))); }
   save();
   foreach(string signature in registeredKeys.Keys.Where(x=>!desired.ContainsKey(x)).ToArray()) { registeredKeys[signature].Dispose(); registeredKeys.Remove(signature); }
   foreach(var pair in added) registeredKeys.Add(pair.Key,pair.Value); added.Clear();
  } finally { foreach(var item in added.Values)item.Dispose(); }
 }
 void DispatchHotkey(string signature) { int action; if(DesiredKeys(settings).TryGetValue(signature,out action))RunAudioHotkey(action); }
 void StartHotkey() { try { ApplyHotkeys(settings,()=>{}); } catch(Exception ex) { MessageBox.Show(ex.Message,L.T("Tastenkürzel"),MessageBoxButtons.OK,MessageBoxIcon.Information); } }
 void HotkeyOptions() {
  using(var form=new Form{Text=L.T("Tastenkürzel"),ClientSize=new Size(680,390),StartPosition=FormStartPosition.CenterScreen,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false,AutoScaleMode=AutoScaleMode.Dpi}) {
   bool[] enabledValues={settings.HotkeyEnabled,settings.HotkeyOne,settings.HotkeyTwo};
   int[] modifierValues={settings.HotkeyModifiers,settings.HotkeyOneModifiers,settings.HotkeyTwoModifiers};
   int[] keyValues={settings.HotkeyKey==0 ? AudioHotkey.SuggestedKey(settings.Language) : settings.HotkeyKey,settings.HotkeyOneKey,settings.HotkeyTwoKey};
   var enabled=new CheckBox[3]; var modifiers=new CheckBox[3,4]; var letters=new ComboBox[3]; int[] masks={2,1,4,8};
   for(int i=0;i<3;i++) {
    int top=15+i*85; enabled[i]=new CheckBox{Left=16,Top=top,Width=645,Text=L.T(i==0 ? "Zwischen beiden wechseln" : i==1 ? "Kopfhörer 1 aktivieren" : "Kopfhörer 2 aktivieren"),Checked=enabledValues[i]}; form.Controls.Add(enabled[i]);
    for(int j=0;j<4;j++) { modifiers[i,j]=new CheckBox{Left=25+j*125,Top=top+30,Width=120,Text=new[]{"Ctrl / Strg","Alt","Shift / ⇧","Win"}[j],Checked=(modifierValues[i]&masks[j])!=0}; form.Controls.Add(modifiers[i,j]); }
    letters[i]=new ComboBox{Left=535,Top=top+28,Width=115,DropDownStyle=ComboBoxStyle.DropDownList}; for(int key=65;key<=90;key++) letters[i].Items.Add(settings.Language=="uk" && key==89 ? "Y / Н" : ((char)key).ToString()); letters[i].SelectedIndex=keyValues[i]-65; form.Controls.Add(letters[i]);
   }
   form.Controls.Add(new Label{Left=18,Top=275,Width=640,Height=50,Text=L.T("Wähle mindestens eine Zusatztaste und einen Buchstaben. Windows oder andere Programme können Kombinationen belegen. Das Kürzel wechselt zwischen den beiden ausgewählten Kopfhörern.")});
   var save=new Button{Left=385,Top=345,Width=130,Height=32,Text=L.T("Speichern")}; var close=new Button{Left=530,Top=345,Width=130,Height=32,Text=L.T("Schließen"),DialogResult=DialogResult.Cancel};
   save.Click+=(s,e)=> {
    int[] values=new int[3],keys=new int[3]; for(int i=0;i<3;i++) { for(int j=0;j<4;j++) if(modifiers[i,j].Checked)values[i]|=masks[j]; keys[i]=65+letters[i].SelectedIndex; if(enabled[i].Checked && values[i]==0) { MessageBox.Show(L.T("Bitte mindestens eine Zusatztaste auswählen.")); return; } if(values[i]==0)values[i]=3; }
    try {
     var next=Settings.Read(SettingsPath); next.HotkeyEnabled=enabled[0].Checked; next.HotkeyOne=enabled[1].Checked; next.HotkeyTwo=enabled[2].Checked;
     next.HotkeyModifiers=values[0];next.HotkeyOneModifiers=values[1];next.HotkeyTwoModifiers=values[2];next.HotkeyKey=keys[0];next.HotkeyOneKey=keys[1];next.HotkeyTwoKey=keys[2];
     ApplyHotkeys(next,()=>next.Save(SettingsPath)); settings=next;form.Close();
    } catch(Exception ex) { MessageBox.Show(ex.Message,L.T("Tastenkürzel"),MessageBoxButtons.OK,MessageBoxIcon.Information); }
   };
   form.Controls.AddRange(new Control[]{save,close});form.CancelButton=close; form.ShowDialog(settingsForm);
  }
 }
}

static class HotkeyTests {
 internal static void Run() {
  if(AudioHotkey.SuggestedKey("de")!=75 || AudioHotkey.SuggestedKey("en")!=72 || AudioHotkey.SuggestedKey("es")!=65 || AudioHotkey.SuggestedKey("fr")!=67 || AudioHotkey.SuggestedKey("uk")!=89) throw new Exception("Localized hotkey defaults");
  var config=new Settings(); if(config.HotkeyEnabled) throw new Exception("Hotkeys must be opt in");
  string file=AppPaths.Report("hotkey-test.ini"); config.HotkeyEnabled=true; config.HotkeyModifiers=6; config.HotkeyKey=81; config.Save(file); var loaded=Settings.Read(file); System.IO.File.Delete(file);
  if(!loaded.HotkeyEnabled || loaded.HotkeyModifiers!=6 || loaded.HotkeyKey!=81) throw new Exception("Hotkey persistence");
  System.IO.File.WriteAllText(AppPaths.Report("Hotkey-Test.txt"),"OK: localized defaults and settings persistence. Global registration requires an interactive Windows desktop; not verified by this self-test.");
 }
}
