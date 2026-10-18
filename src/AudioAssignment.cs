// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;

static class AudioAssignment {
 // Device labels are editable. Never infer an association from names or aliases.
 public static Dictionary<string,string> Plan(IEnumerable<Device> selected,IEnumerable<AudioOutput> outputs,Settings settings){
  var devices=selected.ToArray();var endpoints=outputs.ToArray();var result=new Dictionary<string,string>();
  var reserved=new HashSet<string>(settings.Profiles.Values.Where(x=>x.AudioId.Length>0).Select(x=>x.AudioId),StringComparer.Ordinal);
  foreach(var device in devices){
   if(device.ContainerId==Guid.Empty || settings.Profile(device.Id,0).AudioId.Length>0 || devices.Count(x=>x.ContainerId==device.ContainerId)!=1)continue;
   var matches=endpoints.Where(x=>x.Active && x.ContainerId==device.ContainerId).GroupBy(x=>x.Id,StringComparer.Ordinal).Select(x=>x.First()).ToArray();
   if(matches.Length!=1 || reserved.Contains(matches[0].Id))continue;
   result.Add(device.Id,matches[0].Id);reserved.Add(matches[0].Id);
  }
  return result;
 }
 public static void Test(){
  var first=Guid.NewGuid();var second=Guid.NewGuid();var settings=new Settings();
  var a=new Device{Id="a",Name="Big",ContainerId=first};var b=new Device{Id="b",Name="Retro",ContainerId=second};
  var endpoints=new[]{new AudioOutput{Id="out-a",Name="Other label",ContainerId=first,Active=true},new AudioOutput{Id="out-b",Name="Other label",ContainerId=second,Active=true}};
  var plan=Plan(new[]{a,b},endpoints,settings);if(plan.Count!=2 || plan["a"]!="out-a" || plan["b"]!="out-b")throw new Exception("Container association failed");
  settings.Profiles["a"]=new DeviceProfile("#13705B"){AudioId="manual"};if(Plan(new[]{a,b},endpoints,settings).ContainsKey("a"))throw new Exception("Manual choice replaced");settings.Profiles.Clear();
  if(Plan(new[]{a},endpoints.Concat(new[]{new AudioOutput{Id="handsfree",ContainerId=first,Active=true}}),settings).Count!=0)throw new Exception("Ambiguous output guessed");
  if(Plan(new[]{a},new[]{new AudioOutput{Id="off",ContainerId=first,Active=false}},settings).Count!=0)throw new Exception("Inactive output auto-assigned");
  if(Plan(new[]{new Device{Id="unknown",Name="Other label"}},endpoints,settings).Count!=0)throw new Exception("Missing identity inferred from name");
  b.ContainerId=first;if(Plan(new[]{a,b},endpoints,settings).Count!=0)throw new Exception("Shared container assigned arbitrarily");
 }
}

partial class Tray {
 void AutoAssignAudio(){
  var selectedDevices=devices.Where(x=>selected.Contains(x.Id)).ToArray();var plan=AudioAssignment.Plan(selectedDevices,audioOutputs,settings);if(plan.Count==0)return;
  var old=settings.Profiles;settings.Profiles=new Dictionary<string,DeviceProfile>(old,StringComparer.OrdinalIgnoreCase);
  try{foreach(var pair in plan){int slot=selected.IndexOf(pair.Key);var prior=settings.Profile(pair.Key,slot);settings.Profiles[pair.Key]=new DeviceProfile(prior.ColorHex){Name=prior.Name,Symbol=prior.Symbol,Marker=prior.Marker,AudioId=pair.Value};}settings.Save(SettingsPath);}
  catch{settings.Profiles=old;throw;}
 }
}
