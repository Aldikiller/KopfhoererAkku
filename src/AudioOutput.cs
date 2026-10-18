// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

sealed class AudioOutput {
 public string Id,Name;public bool Active;public Guid ContainerId;
 public override string ToString(){return Name+(Active ? "" : " · "+L.T("Nicht verfügbar / getrennt"));}
}
// MMDevice interfaces are Windows SDK contracts. Endpoint IDs are opaque and are
// associated with a headphone only after the user explicitly chooses an output.
static class AudioApi {
 [StructLayout(LayoutKind.Sequential)] public struct Key {public Guid Format;public uint Id;public Key(string format,uint id){Format=new Guid(format);Id=id;}}
 [StructLayout(LayoutKind.Explicit,Size=24)] public struct Value {[FieldOffset(0)]public ushort Type;[FieldOffset(8)]public IntPtr Pointer;}
 [DllImport("ole32.dll")] static extern int PropVariantClear(ref Value value);
 [ComImport,Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] public interface Enumerator {
  [PreserveSig]int EnumAudioEndpoints(int flow,uint state,out Collection collection);
  [PreserveSig]int GetDefaultAudioEndpoint(int flow,int role,out Endpoint endpoint);
  [PreserveSig]int GetDevice([MarshalAs(UnmanagedType.LPWStr)]string id,out Endpoint endpoint);
  [PreserveSig]int RegisterEndpointNotificationCallback(Notifications sink);
  [PreserveSig]int UnregisterEndpointNotificationCallback(Notifications sink);
 }
 [ComImport,Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] public interface Collection {
  [PreserveSig]int GetCount(out uint count);[PreserveSig]int Item(uint index,out Endpoint endpoint);
 }
 [ComImport,Guid("D666063F-1587-4E43-81F1-B948E807363F"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] public interface Endpoint {
  [PreserveSig]int Activate(ref Guid iid,uint context,IntPtr parameters,out IntPtr result);
  [PreserveSig]int OpenPropertyStore(uint access,out Properties properties);
  [PreserveSig]int GetId([MarshalAs(UnmanagedType.LPWStr)]out string id);
  [PreserveSig]int GetState(out uint state);
 }
 [ComImport,Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] public interface Properties {
  [PreserveSig]int GetCount(out uint count);[PreserveSig]int GetAt(uint index,out Key key);
  [PreserveSig]int GetValue(ref Key key,out Value value);
  [PreserveSig]int SetValue(ref Key key,ref Value value);[PreserveSig]int Commit();
 }
 [Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] public interface Notifications {
  [PreserveSig]int OnDeviceStateChanged([MarshalAs(UnmanagedType.LPWStr)]string id,uint state);
  [PreserveSig]int OnDeviceAdded([MarshalAs(UnmanagedType.LPWStr)]string id);
  [PreserveSig]int OnDeviceRemoved([MarshalAs(UnmanagedType.LPWStr)]string id);
  [PreserveSig]int OnDefaultDeviceChanged(int flow,int role,[MarshalAs(UnmanagedType.LPWStr)]string id);
  [PreserveSig]int OnPropertyValueChanged([MarshalAs(UnmanagedType.LPWStr)]string id,Key key);
 }
 // Windows supplies no public desktop setter for the default endpoint. Keep the
 // optional PolicyConfig COM call isolated; failed calls never mark an icon active.
 [ComImport,Guid("F8679F50-850A-41CF-9C72-430F290290C8"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface Policy {
  [PreserveSig]int GetMixFormat(string id,IntPtr format);
  [PreserveSig]int GetDeviceFormat(string id,int mode,IntPtr format);
  [PreserveSig]int ResetDeviceFormat(string id);
  [PreserveSig]int SetDeviceFormat(string id,IntPtr format,IntPtr mix);
  [PreserveSig]int GetProcessingPeriod(string id,int mode,IntPtr period,IntPtr minimum);
  [PreserveSig]int SetProcessingPeriod(string id,IntPtr period);
  [PreserveSig]int GetShareMode(string id,IntPtr mode);
  [PreserveSig]int SetShareMode(string id,IntPtr mode);
  [PreserveSig]int GetPropertyValue(string id,ref Key key,out Value value);
  [PreserveSig]int SetPropertyValue(string id,ref Key key,ref Value value);
  [PreserveSig]int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)]string id,int role);
  [PreserveSig]int SetEndpointVisibility(string id,int visible);
 }
 public static void Free(object value){if(value!=null && Marshal.IsComObject(value))Marshal.ReleaseComObject(value);}
 public static Enumerator Create(){return (Enumerator)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")));}
 static string Default(Enumerator api,int role){Endpoint device=null;try{if(api.GetDefaultAudioEndpoint(0,role,out device)<0)return "";string id;Marshal.ThrowExceptionForHR(device.GetId(out id));return id;}finally{Free(device);}}
 public static string Default(){var api=Create();try{return Default(api,1);}finally{Free(api);}}
 static Guid Container(Properties props){var key=new Key("8C7ED206-3F8A-4827-B3AB-AE9E1FAEFC6C",2);Value value;if(props.GetValue(ref key,out value)<0)return Guid.Empty;try{return value.Type==72 && value.Pointer!=IntPtr.Zero ? (Guid)Marshal.PtrToStructure(value.Pointer,typeof(Guid)) : Guid.Empty;}finally{PropVariantClear(ref value);}}
 public static List<AudioOutput> List(){
  var api=Create();Collection collection=null;var result=new List<AudioOutput>();
  try{Marshal.ThrowExceptionForHR(api.EnumAudioEndpoints(0,15,out collection));uint count;Marshal.ThrowExceptionForHR(collection.GetCount(out count));
   for(uint i=0;i<count;i++){Endpoint device=null;Properties props=null;try{Marshal.ThrowExceptionForHR(collection.Item(i,out device));string id;uint state;Marshal.ThrowExceptionForHR(device.GetId(out id));Marshal.ThrowExceptionForHR(device.GetState(out state));Marshal.ThrowExceptionForHR(device.OpenPropertyStore(0,out props));
    var key=new Key("A45C254E-DF1C-4EFD-8020-67D146A850E0",14);Value value;Marshal.ThrowExceptionForHR(props.GetValue(ref key,out value));string name;try{name=value.Type==31 ? Marshal.PtrToStringUni(value.Pointer) : id;}finally{PropVariantClear(ref value);}result.Add(new AudioOutput{Id=id,Name=name,Active=(state&1)!=0,ContainerId=Container(props)});
   }finally{Free(props);Free(device);}}
   return result.OrderByDescending(x=>x.Active).ThenBy(x=>x.Name).ToList();
  }finally{Free(collection);Free(api);}
 }
 public static void Switch(string id){
  if(!List().Any(x=>x.Id==id && x.Active))throw new InvalidOperationException(L.T("Audioausgabe nicht verbunden."));
  var api=Create();object instance=null;try{string oldConsole=Default(api,0),oldMedia=Default(api,1);instance=Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")));var policy=(Policy)instance;
   try{Marshal.ThrowExceptionForHR(policy.SetDefaultEndpoint(id,0));Marshal.ThrowExceptionForHR(policy.SetDefaultEndpoint(id,1));if(Default(api,1)!=id)throw new InvalidOperationException(L.T("Audioausgabe wurde nicht übernommen."));}
   catch{if(oldConsole.Length>0)policy.SetDefaultEndpoint(oldConsole,0);if(oldMedia.Length>0)policy.SetDefaultEndpoint(oldMedia,1);throw;}
  }finally{Free(instance);Free(api);}
 }
}
[ComVisible(true),ClassInterface(ClassInterfaceType.None)] sealed class AudioEvents : AudioApi.Notifications,IDisposable {
 AudioApi.Enumerator api;readonly Action changed;
 public AudioEvents(Action action){changed=action;api=AudioApi.Create();try{Marshal.ThrowExceptionForHR(api.RegisterEndpointNotificationCallback(this));}catch{AudioApi.Free(api);api=null;throw;}}
 int Signal(){try{changed();}catch{}return 0;}
 public int OnDeviceStateChanged(string id,uint state){return Signal();}public int OnDeviceAdded(string id){return Signal();}public int OnDeviceRemoved(string id){return Signal();}
 public int OnDefaultDeviceChanged(int flow,int role,string id){return flow==0 && role==1 ? Signal() : 0;}
 public int OnPropertyValueChanged(string id,AudioApi.Key key){return 0;}
 public void Dispose(){if(api!=null){api.UnregisterEndpointNotificationCallback(this);AudioApi.Free(api);api=null;}GC.KeepAlive(this);}
}
