// SPDX-License-Identifier: MIT
using System; using System.Drawing; using System.IO;
static class Release165Tests {
 internal static void Run() {
  if(Tray.StatusText(true,true,90)!=L.T("Abfrage fehlgeschlagen") || Tray.StatusText(false,false,90)!=L.T("Nicht verbunden") || Tray.StatusText(false,null,90)!=L.T("Verbindung unbekannt") || Tray.StatusText(false,true,null)!=L.T("Windows liefert keinen Akkustand"))throw new Exception("Status distinction");
  var settings=new Settings(); settings.Profiles["test"]=new DeviceProfile("#123456"){Symbol=5,IconFile="C:\\Test\\icon.png",RememberVolume=true,SavedVolume=32};
  string file=AppPaths.Report("release165-test.ini");settings.Save(file);var loaded=Settings.Read(file).Profiles["test"];File.Delete(file);if(loaded.Symbol!=5 || loaded.IconFile!=settings.Profiles["test"].IconFile || !loaded.RememberVolume || loaded.SavedVolume!=32)throw new Exception("Image and volume persistence");
  string dir=AppPaths.Report("symbols");CustomSymbols.Generate(dir);
  foreach(string name in new[]{"over-ear","on-ear","in-ear","headset","speaker"}) using(var image=new Bitmap(Path.Combine(dir,name+".png"))) { if(image.Width!=128 || image.GetPixel(0,0).A!=0)throw new Exception("Transparent symbol template"); }
  using(var sheet=new Bitmap(500,150))using(var g=Graphics.FromImage(sheet)) {
   g.Clear(Color.FromArgb(35,35,35));int col=0;
   foreach(string name in new[]{"over-ear","on-ear","in-ear","headset","speaker"}) {
    string symbol=Path.Combine(dir,name+".png");
    using(var before=Tray.BatteryIcon(90,0,true,null,false,null,5,symbol,false,0,16))using(var after=Tray.BatteryIcon(90,0,true,null,false,null,5,symbol,false,0,16,true))using(var a=before.ToBitmap())using(var b=after.ToBitmap()) {
     g.DrawImageUnscaled(a,col*100+20,12);g.DrawImageUnscaled(b,col*100+60,12);
     g.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;g.DrawImage(a,new Rectangle(col*100+2,45,40,40));g.DrawImage(b,new Rectangle(col*100+52,45,40,40));
     int changed=0;for(int y=0;y<16;y++)for(int x=0;x<16;x++)if(a.GetPixel(x,y)!=b.GetPixel(x,y))changed++;if(changed<5)throw new Exception("Tray-specific symbol rendering");
    }
    col++;
   }
   sheet.Save(AppPaths.Report("Tray-Symbols-Before-After.png"));
  }
  using(var bitmap=new Bitmap(128,128))using(var graphics=Graphics.FromImage(bitmap)){graphics.Clear(Color.Transparent);CustomSymbols.Draw(graphics,Path.Combine(dir,"headset.png"),new RectangleF(0,0,128,128));bitmap.Save(AppPaths.Report("Custom-Symbol-Preview.png"));}
 }
}
