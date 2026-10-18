// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;

// A successful Windows query is not proof that the device measured a new value.
// Keep query timestamps separate from the last usable battery reading.
sealed class DeviceReading {
 public DateTime? CheckedUtc,ValueReadUtc;
 public int? LastValue;
 public bool NeedsCheck=true;
}
sealed class ReadingBook {
 readonly Dictionary<string,DeviceReading> readings=new Dictionary<string,DeviceReading>();
 public DeviceReading Get(string id) { DeviceReading entry; if(!readings.TryGetValue(id,out entry)) readings[id]=entry=new DeviceReading(); return entry; }
 public void Observe(IEnumerable<Device> devices,DateTime now) {
  foreach(var device in devices) {
   var entry=Get(device.Id);entry.CheckedUtc=now;entry.NeedsCheck=false;
   if(device.Connected==true && device.Battery.HasValue) {entry.LastValue=device.Battery;entry.ValueReadUtc=now;}
  }
 }
 public void Invalidate(IEnumerable<string> ids) {foreach(var id in ids) Get(id).NeedsCheck=true;}
 public static string Age(DateTime? utc,DateTime now) {
  if(!utc.HasValue) return "–";
  var elapsed=now-utc.Value;long seconds=Math.Max(0,(long)elapsed.TotalSeconds);
  if(seconds<60) return seconds+" s";
  if(seconds<3600) return seconds/60+" min";
  if(seconds<86400) return seconds/3600+" h";
  return seconds/86400+" d";
 }
}
static class ReadingTests {
 public static void Run() {
  Action<bool,string> check=(ok,text)=>{if(!ok)throw new Exception(text);};var time=new DateTime(2026,1,1,12,0,0,DateTimeKind.Utc);
  var book=new ReadingBook();var a=new Device{Id="a",Connected=true,Battery=75};var b=new Device{Id="b",Connected=true,Battery=60};book.Observe(new[]{a,b},time);
  a.Battery=70;book.Observe(new[]{a},time.AddSeconds(20));check(book.Get("b").ValueReadUtc==time,"Other device timestamp changed");
  a.Connected=false;a.Battery=5;book.Observe(new[]{a},time.AddSeconds(30));check(book.Get("a").LastValue==70 && book.Get("a").ValueReadUtc==time.AddSeconds(20),"Disconnected value treated as current");
  a.Connected=true;a.Battery=null;book.Observe(new[]{a},time.AddSeconds(40));check(book.Get("a").LastValue==70,"Missing data replaced known value");
  book.Invalidate(new[]{"a","b"});check(book.Get("a").NeedsCheck && book.Get("b").NeedsCheck,"Resume did not invalidate readings");book.Observe(new[]{a},time.AddSeconds(50));check(!book.Get("a").NeedsCheck && book.Get("b").NeedsCheck,"Unverified device incorrectly refreshed");
  check(ReadingBook.Age(time,time.AddSeconds(-5))=="0 s","Clock rollback");check(ReadingBook.Age(time,time.AddSeconds(61))=="1 min","Relative age");
 }
}
