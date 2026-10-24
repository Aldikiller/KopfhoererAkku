// SPDX-License-Identifier: MIT
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

static class StoreMode {
#if STORE
 public static readonly bool Enabled=true;
#else
 public static readonly bool Enabled=false;
#endif
 public const string UpdatesUri="ms-windows-store://downloadsandupdates";
 public const string StartupUri="ms-settings:startupapps";
 public static string PreviewData;
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]
 static extern int GetCurrentPackageFamilyName(ref uint length,StringBuilder name);
 public static string LocalData {
  get {
   if(PreviewData!=null) return PreviewData;
   uint length=0; int result=GetCurrentPackageFamilyName(ref length,null);
   if(result!=122 || length==0) throw new InvalidOperationException("This Store build must run with an installed package identity.");
   var name=new StringBuilder((int)length);
   if(GetCurrentPackageFamilyName(ref length,name)!=0) throw new InvalidOperationException("Package identity unavailable.");
   return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Packages",name.ToString(),"LocalState");
  }
 }
}
