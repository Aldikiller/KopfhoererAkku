// SPDX-License-Identifier: MIT
using System; using System.Drawing; using System.Drawing.Drawing2D; using System.Drawing.Imaging; using System.IO; using System.Windows.Forms;
static class CustomSymbols {
 static readonly System.Collections.Generic.Dictionary<string,Bitmap> cache=new System.Collections.Generic.Dictionary<string,Bitmap>();
 public static string Import(IWin32Window owner,string folder) {
  using(var dialog=new OpenFileDialog{Filter="PNG / ICO|*.png;*.ico",InitialDirectory=Path.Combine(AppPaths.Base,"docs","symbols")}) {
   if(dialog.ShowDialog(owner)!=DialogResult.OK)return null;
   try {
    var info=new FileInfo(dialog.FileName); if(info.Length>4*1024*1024)throw new InvalidOperationException(L.T("Bild zu groß (max. 4 MB)."));
    using(var image=Load(dialog.FileName)) { if(image.Width>2048 || image.Height>2048)throw new InvalidOperationException(L.T("Bild zu groß (max. 2048 Pixel)."));
     string directory=Path.Combine(folder,"symbols");Directory.CreateDirectory(directory);string target=Path.Combine(directory,Guid.NewGuid().ToString("N")+".png");
     using(var bitmap=new Bitmap(128,128)) using(var g=Graphics.FromImage(bitmap)) {g.Clear(Color.Transparent);g.InterpolationMode=InterpolationMode.HighQualityBicubic;float scale=Math.Min(128f/image.Width,128f/image.Height);g.DrawImage(image,(128-image.Width*scale)/2,(128-image.Height*scale)/2,image.Width*scale,image.Height*scale);bitmap.Save(target,ImageFormat.Png);}return target;
    }
   } catch(Exception ex) { MessageBox.Show(L.T("Symbol konnte nicht geladen werden: ")+ex.Message);return null; }
  }
 }
 static Bitmap Load(string file) { if(Path.GetExtension(file).Equals(".ico",StringComparison.OrdinalIgnoreCase))using(var icon=new Icon(file,128,128))return icon.ToBitmap(); using(var source=Image.FromFile(file))return new Bitmap(source); }
 public static void Draw(Graphics g,string file,RectangleF rectangle) {
  if(string.IsNullOrEmpty(file))return;
  Bitmap bitmap; if(!cache.TryGetValue(file,out bitmap)) { try {bitmap=Load(file);}catch {return;}if(cache.Count>=16) {foreach(var value in cache.Values)value.Dispose();cache.Clear();}cache[file]=bitmap; }
  float scale=Math.Min(rectangle.Width/bitmap.Width,rectangle.Height/bitmap.Height);g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.DrawImage(bitmap,rectangle.X+(rectangle.Width-bitmap.Width*scale)/2,rectangle.Y+(rectangle.Height-bitmap.Height*scale)/2,bitmap.Width*scale,bitmap.Height*scale);
 }
 public static void DrawTray(Graphics g,string file,RectangleF rectangle) {
  if(string.IsNullOrEmpty(file))return;
  Bitmap source; if(!cache.TryGetValue(file,out source)) { try{source=Load(file);}catch{return;} if(cache.Count>=16){foreach(var old in cache.Values)old.Dispose();cache.Clear();}cache[file]=source; }
  int left=source.Width,top=source.Height,right=-1,bottom=-1;
  for(int y=0;y<source.Height;y++)for(int x=0;x<source.Width;x++)if(source.GetPixel(x,y).A>24){left=Math.Min(left,x);top=Math.Min(top,y);right=Math.Max(right,x);bottom=Math.Max(bottom,y);}
  if(right<left)return;
  var crop=new Rectangle(left,top,right-left+1,bottom-top+1); float scale=Math.Min(rectangle.Width/crop.Width,rectangle.Height/crop.Height);
  var dest=new RectangleF(rectangle.X+(rectangle.Width-crop.Width*scale)/2,rectangle.Y+(rectangle.Height-crop.Height*scale)/2,crop.Width*scale,crop.Height*scale);
  using(var ink=new ImageAttributes()) {
   var matrix=new ColorMatrix(new float[][]{new float[]{0,0,0,0,0},new float[]{0,0,0,0,0},new float[]{0,0,0,0,0},new float[]{0,0,0,1,0},new float[]{0,0,0,0,1}});ink.SetColorMatrix(matrix);
   for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++) {var area=new Rectangle((int)Math.Round(dest.X+x),(int)Math.Round(dest.Y+y),(int)Math.Round(dest.Width),(int)Math.Round(dest.Height));g.DrawImage(source,area,crop.X,crop.Y,crop.Width,crop.Height,GraphicsUnit.Pixel,ink);}
   matrix.Matrix40=1;matrix.Matrix41=1;matrix.Matrix42=1;ink.SetColorMatrix(matrix);
   foreach(float offset in new[]{-0.35f,0.35f})g.DrawImage(source,Rectangle.Round(new RectangleF(dest.X+offset,dest.Y,dest.Width,dest.Height)),crop.X,crop.Y,crop.Width,crop.Height,GraphicsUnit.Pixel,ink);
  }
 }
 public static void Generate(string directory) {
  Directory.CreateDirectory(directory);
  string[] names={"over-ear","on-ear","in-ear","headset","speaker"};
  for(int i=0;i<5;i++) {
   using(var image=new Bitmap(128,128))using(var g=Graphics.FromImage(image))using(var pen=new Pen(Color.White,9)) {
    g.Clear(Color.Transparent);g.SmoothingMode=SmoothingMode.AntiAlias;pen.StartCap=LineCap.Round;pen.EndCap=LineCap.Round;
    if(i==4){g.DrawPolygon(pen,new[]{new Point(20,48),new Point(42,48),new Point(66,28),new Point(66,100),new Point(42,80),new Point(20,80)});g.DrawArc(pen,56,28,48,72,-60,120);}
    else if(i==2){g.DrawEllipse(pen,22,20,25,35);g.DrawLine(pen,35,55,35,102);g.DrawEllipse(pen,81,20,25,35);g.DrawLine(pen,94,55,94,102);}
    else{g.DrawArc(pen,20,18,88,88,180,180);g.DrawRectangle(pen,20,62,i==1?14:22,38);g.DrawRectangle(pen,i==1?94:86,62,i==1?14:22,38);if(i==3){g.DrawLine(pen,108,97,108,113);g.DrawLine(pen,108,113,75,113);}}
    image.Save(Path.Combine(directory,names[i]+".png"),ImageFormat.Png);
   }
   string path=i==4 ? "M20 48H42L66 28V100L42 80H20Z M90 34Q116 64 90 94" : i==2 ? "M35 55V102 M94 55V102 M22 38a13 18 0 1 0 26 0a13 18 0 1 0 -26 0 M81 38a13 18 0 1 0 26 0a13 18 0 1 0 -26 0" : "M20 64V62a44 44 0 0 1 88 0V64 M20 62H42V100H20Z M86 62H108V100H86Z"+(i==3 ? " M108 97V113H75" : "");
   File.WriteAllText(Path.Combine(directory,names[i]+".svg"),"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 128 128\"><path d=\""+path+"\" fill=\"none\" stroke=\"white\" stroke-width=\"9\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg>");
  }
 }
}
