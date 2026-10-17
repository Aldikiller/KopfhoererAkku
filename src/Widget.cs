// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

enum DisplayMode { Infobereich, Widget, Beides }
enum WidgetTextAlignment { Links, Zentriert, Rechts }
class InstalledFontNames : StringConverter {
 public override bool GetStandardValuesSupported(ITypeDescriptorContext c) { return true; }
 public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext c) { var names=new List<string>(); foreach(var f in FontFamily.Families) { names.Add(f.Name); f.Dispose(); } return new StandardValuesCollection(names); }
}
class WidgetOptions {
 [Browsable(false),Category("1. Anzeige"),DisplayName("Anzeigemodus")] public DisplayMode Mode {get;set;}
 [Category("1. Anzeige"),DisplayName("Immer im Vordergrund")] public bool Topmost {get;set;}
 [Category("1. Anzeige"),DisplayName("Position sperren")] public bool Locked {get;set;}
 [Category("1. Anzeige"),DisplayName("Nebeneinander statt untereinander")] public bool Horizontal {get;set;}
 [Category("2. Größe und Position"),DisplayName("Breite (Pixel)")] public int Width {get;set;}
 [Category("2. Größe und Position"),DisplayName("Höhe (Pixel)")] public int Height {get;set;}
 [Category("2. Größe und Position"),DisplayName("Position X"),Description("-32000 setzt das Widget beim nächsten Anzeigen mittig auf den Bildschirm.")] public int X {get;set;}
 [Category("2. Größe und Position"),DisplayName("Position Y")] public int Y {get;set;}
 [Category("2. Größe und Position"),DisplayName("Innenabstand (Pixel)")] public int Padding {get;set;}
 [Category("2. Größe und Position"),DisplayName("Abstand der Geräte (Pixel)")] public int Gap {get;set;}
 [Category("2. Größe und Position"),DisplayName("Symbolgröße (Pixel)")] public int IconSize {get;set;}
 [Category("3. Farben und Transparenz"),DisplayName("Gesamte Deckkraft (%)"),Description("10 bis 100. Niedrigere Werte machen das gesamte Widget durchsichtiger.")] public int Opacity {get;set;}
 [Category("3. Farben und Transparenz"),DisplayName("Hintergrund-Deckkraft (%)"),Description("0: Hintergrund vollständig transparent, Schrift und Symbole bleiben sichtbar.")] public int BackgroundOpacity {get;set;}
 [Category("3. Farben und Transparenz"),DisplayName("Hintergrundfarbe")] public Color Background {get;set;}
 [Category("3. Farben und Transparenz"),DisplayName("Schriftfarbe")] public Color Foreground {get;set;}
 [Category("3. Farben und Transparenz"),DisplayName("Rahmenfarbe")] public Color Border {get;set;}
 [Category("3. Farben und Transparenz"),DisplayName("Rahmenstärke (Pixel)")] public int BorderWidth {get;set;}
 [Category("3. Farben und Transparenz"),DisplayName("Eckenrundung (Pixel)")] public int Radius {get;set;}
 [Category("4. Schrift und Inhalt"),DisplayName("Schriftart"),TypeConverter(typeof(InstalledFontNames))] public string FontName {get;set;}
 [Category("4. Schrift und Inhalt"),DisplayName("Schriftart für Prozentwerte"),TypeConverter(typeof(InstalledFontNames))] public string ValueFontName {get;set;}
 [Category("4. Schrift und Inhalt"),DisplayName("Textausrichtung")] public WidgetTextAlignment Alignment {get;set;}
 [Category("4. Schrift und Inhalt"),DisplayName("Fette Gerätenamen")] public bool NameBold {get;set;}
 [Category("4. Schrift und Inhalt"),DisplayName("Schriftgröße für Namen")] public int FontSize {get;set;}
 [Category("4. Schrift und Inhalt"),DisplayName("Schriftgröße für Prozentwert")] public int ValueSize {get;set;}
 [Category("4. Schrift und Inhalt"),DisplayName("Fette Prozentzahl")] public bool Bold {get;set;}
 [Category("4. Schrift und Inhalt"),DisplayName("Gerätenamen anzeigen")] public bool Names {get;set;}
 [Category("4. Schrift und Inhalt"),DisplayName("Verbindungsstatus anzeigen")] public bool Status {get;set;}
 [Category("4. Schrift und Inhalt"),DisplayName("Gerätesymbole anzeigen")] public bool Icons {get;set;}
 public WidgetOptions() { Mode=DisplayMode.Infobereich; Topmost=true; Width=380; Height=200; X=-32000; Y=-32000; Padding=16; Gap=12; IconSize=48; Opacity=100; BackgroundOpacity=82; Background=Color.FromArgb(28,32,40); Foreground=Color.White; Border=Color.FromArgb(80,90,105); BorderWidth=1; Radius=18; FontName="Segoe UI"; ValueFontName="Arial"; FontSize=12; ValueSize=26; Bold=true; Names=true; Status=true; Icons=true; }
 public WidgetOptions Copy() { return (WidgetOptions)MemberwiseClone(); }
 static int Clamp(int x,int lo,int hi) { return Math.Max(lo,Math.Min(hi,x)); }
 public void Normalize() { if(!Enum.IsDefined(typeof(DisplayMode),Mode)) Mode=DisplayMode.Infobereich; Width=Clamp(Width,160,1600); Height=Clamp(Height,90,1000); Padding=Clamp(Padding,0,80); Gap=Clamp(Gap,0,80); IconSize=Clamp(IconSize,16,160); Opacity=Clamp(Opacity,10,100); BackgroundOpacity=Clamp(BackgroundOpacity,0,100); BorderWidth=Clamp(BorderWidth,0,12); Radius=Clamp(Radius,0,200); FontSize=Clamp(FontSize,6,72); ValueSize=Clamp(ValueSize,8,120); if(string.IsNullOrWhiteSpace(FontName)) FontName="Segoe UI"; if(string.IsNullOrWhiteSpace(ValueFontName)) ValueFontName="Arial"; if(!Enum.IsDefined(typeof(WidgetTextAlignment),Alignment)) Alignment=WidgetTextAlignment.Links; Background=Color.FromArgb(Background.R,Background.G,Background.B); Foreground=Color.FromArgb(Foreground.R,Foreground.G,Foreground.B); Border=Color.FromArgb(Border.R,Border.G,Border.B); }
 public IEnumerable<string> Lines() { foreach(var p in GetType().GetProperties()) { var value=p.GetValue(this,null); string text=p.PropertyType==typeof(Color) ? ColorTranslator.ToHtml((Color)value) : Convert.ToString(value,CultureInfo.InvariantCulture); yield return "Widget."+p.Name+"="+Uri.EscapeDataString(text); } }
 public void Read(string key,string value) { var p=GetType().GetProperty(key); if(p==null || !p.CanWrite) return; try { value=Uri.UnescapeDataString(value); object parsed=p.PropertyType==typeof(Color) ? (object)ColorTranslator.FromHtml(value) : p.PropertyType.IsEnum ? Enum.Parse(p.PropertyType,value) : Convert.ChangeType(value,p.PropertyType,CultureInfo.InvariantCulture); p.SetValue(this,parsed,null); } catch(ArgumentException) {} catch(FormatException) {} catch(OverflowException) {} }
}
class WidgetItem { public string Name,Status; public int? Battery; public bool? Connected; public DeviceProfile Profile; }
static class WidgetDrawing {
 static GraphicsPath Round(float x,float y,float w,float h,float radius) { var p=new GraphicsPath(); float d=Math.Min(radius*2,Math.Min(w,h)); if(d<1) p.AddRectangle(new RectangleF(x,y,w,h)); else { p.AddArc(x,y,d,d,180,90); p.AddArc(x+w-d,y,d,d,270,90); p.AddArc(x+w-d,y+h-d,d,d,0,90); p.AddArc(x,y+h-d,d,d,90,90); p.CloseFigure(); } return p; }
 static void Text(Graphics g,string text,string fontName,int size,FontStyle style,Brush brush,RectangleF area,WidgetTextAlignment alignment=WidgetTextAlignment.Links) { if(area.Width<4 || area.Height<4) return; Font font; try { font=new Font(fontName,size,style,GraphicsUnit.Pixel); } catch(ArgumentException) { font=new Font("Segoe UI",size,style,GraphicsUnit.Pixel); } using(font) using(var format=new StringFormat{Trimming=StringTrimming.EllipsisCharacter,FormatFlags=StringFormatFlags.NoWrap,LineAlignment=StringAlignment.Center,Alignment=alignment==WidgetTextAlignment.Zentriert ? StringAlignment.Center : alignment==WidgetTextAlignment.Rechts ? StringAlignment.Far : StringAlignment.Near}) g.DrawString(text,font,brush,area,format); }
 public static Bitmap Render(WidgetOptions o,Settings settings,IList<WidgetItem> items) {
  var bitmap=new Bitmap(o.Width,o.Height,PixelFormat.Format32bppArgb);
  using(var g=Graphics.FromImage(bitmap)) { g.Clear(Color.Transparent); g.SmoothingMode=SmoothingMode.AntiAlias; g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
   float inset=Math.Max(0.5f,o.BorderWidth/2f); using(var body=Round(inset,inset,o.Width-2*inset,o.Height-2*inset,o.Radius)) { using(var brush=new SolidBrush(Color.FromArgb(o.BackgroundOpacity*255/100,o.Background))) g.FillPath(brush,body); if(o.BorderWidth>0) using(var pen=new Pen(o.Border,o.BorderWidth)) g.DrawPath(pen,body); }
   int count=Math.Max(1,items.Count); float pad=Math.Min(o.Padding,Math.Min(o.Width,o.Height)/5f),gap=Math.Min(o.Gap,(o.Horizontal ? o.Width : o.Height)/8f); float cellW=o.Horizontal ? (o.Width-2*pad-gap*(count-1))/count : o.Width-2*pad; float cellH=o.Horizontal ? o.Height-2*pad : (o.Height-2*pad-gap*(count-1))/count;
   for(int i=0;i<count;i++) { var item=i<items.Count ? items[i] : new WidgetItem{Name="Keine Kopfhörer ausgewählt",Status="Doppelklick: Einstellungen",Profile=new DeviceProfile("#13705B")}; float x=pad+(o.Horizontal ? i*(cellW+gap) : 0),y=pad+(o.Horizontal ? 0 : i*(cellH+gap)); float iconSize=o.Icons ? Math.Min(o.IconSize,Math.Min(cellH,cellW*0.4f)) : 0;
    if(iconSize>0) using(var icon=Tray.BatteryIcon(item.Battery,i,item.Connected,item.Profile.Color,settings.NumbersOnly,settings,item.Profile.Symbol,item.Profile.Marker)) using(var image=icon.ToBitmap()) { g.InterpolationMode=InterpolationMode.HighQualityBicubic; g.DrawImage(image,new RectangleF(x,y+(cellH-iconSize)/2,iconSize,iconSize)); }
    float textX=x+(iconSize>0 ? iconSize+Math.Min(12,gap+4) : 0),textW=cellW-(textX-x); float nameH=o.Names ? Math.Min(o.FontSize*1.5f,cellH*0.3f) : 0,statusH=o.Status ? Math.Min(o.FontSize*1.4f,cellH*0.25f) : 0,valueH=cellH-nameH-statusH;
    using(var brush=new SolidBrush(o.Foreground)) { if(o.Names) Text(g,item.Name,o.FontName,Math.Min(o.FontSize,(int)(nameH*0.85f)),o.NameBold ? FontStyle.Bold : FontStyle.Regular,brush,new RectangleF(textX,y,textW,nameH),o.Alignment); string value=item.Connected==false ? "–" : item.Battery.HasValue ? item.Battery+" %" : "?"; Text(g,value,o.ValueFontName,Math.Max(6,Math.Min(o.ValueSize,(int)(valueH*0.85f))),o.Bold ? FontStyle.Bold : FontStyle.Regular,brush,new RectangleF(textX,y+nameH,textW,valueH),o.Alignment); if(o.Status) Text(g,item.Status ?? "Status unbekannt",o.FontName,Math.Max(6,Math.Min(o.FontSize,(int)(statusH*0.85f))),FontStyle.Regular,brush,new RectangleF(textX,y+nameH+valueH,textW,statusH),o.Alignment); }
   }
  } return bitmap;
 }
 public static Bitmap Preview(WidgetOptions options,Settings settings,IList<WidgetItem> items,int width,int height) { using(var raw=Render(options,settings,items)) { var result=new Bitmap(width,height); using(var g=Graphics.FromImage(result)) { for(int x=0;x<width;x+=12) for(int y=0;y<height;y+=12) using(var b=new SolidBrush((x/12+y/12)%2==0 ? Color.FromArgb(215,215,215) : Color.FromArgb(245,245,245))) g.FillRectangle(b,x,y,12,12); float scale=Math.Min((float)(width-8)/raw.Width,(float)(height-8)/raw.Height); var area=new RectangleF((width-raw.Width*scale)/2,(height-raw.Height*scale)/2,raw.Width*scale,raw.Height*scale); using(var attributes=new ImageAttributes()) { var matrix=new ColorMatrix(); matrix.Matrix33=options.Opacity/100f; attributes.SetColorMatrix(matrix); g.DrawImage(raw,Rectangle.Round(area),0,0,raw.Width,raw.Height,GraphicsUnit.Pixel,attributes); } } return result; } }
}
class BatteryWidget : Form {
 WidgetOptions options; Action<Point> moved; bool dragging; Point dragOrigin,windowOrigin;
 protected override CreateParams CreateParams { get { var p=base.CreateParams; p.ExStyle|=0x80000|0x80; return p; } }
 protected override bool ShowWithoutActivation {get {return true;}}
 public BatteryWidget(Action open,Action pick,Action quit,Action<Point> onMove) { moved=onMove; FormBorderStyle=FormBorderStyle.None; ShowInTaskbar=false; StartPosition=FormStartPosition.Manual; Text="Kopfhörer-Akku Widget"; var menu=new ContextMenuStrip(); menu.Items.Add("Einstellungen …",null,(s,e)=>open()); menu.Items.Add("Geräte auswählen …",null,(s,e)=>pick()); menu.Items.Add("Beenden",null,(s,e)=>quit()); ContextMenuStrip=menu; DoubleClick+=(s,e)=>open(); }
 public void UpdateContent(WidgetOptions value,Settings settings,IList<WidgetItem> items) { options=value; TopMost=value.Topmost; var desired=new Rectangle(value.X,value.Y,value.Width,value.Height); if(value.X==-32000 || !Screen.AllScreens.Any(s=>Rectangle.Intersect(s.WorkingArea,desired).Width>=60 && Rectangle.Intersect(s.WorkingArea,desired).Height>=40)) { var area=Screen.PrimaryScreen.WorkingArea; desired.Location=new Point(area.Left+Math.Max(0,(area.Width-value.Width)/2),area.Top+Math.Max(0,(area.Height-value.Height)/2)); } if(!dragging) { Location=desired.Location; Size=desired.Size; } if(!Visible) Show(); using(var bitmap=WidgetDrawing.Render(value,settings,items)) Present(bitmap,(byte)(value.Opacity*255/100)); }
 protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); if(e.Button==MouseButtons.Left && options!=null && !options.Locked) { dragging=true; dragOrigin=Cursor.Position; windowOrigin=Location; Capture=true; } }
 protected override void OnMouseMove(MouseEventArgs e) { base.OnMouseMove(e); if(dragging) Location=new Point(windowOrigin.X+Cursor.Position.X-dragOrigin.X,windowOrigin.Y+Cursor.Position.Y-dragOrigin.Y); }
 protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); if(dragging) { dragging=false; Capture=false; moved(Location); } }
 protected override void Dispose(bool disposing) { if(disposing && ContextMenuStrip!=null) ContextMenuStrip.Dispose(); base.Dispose(disposing); }
 [StructLayout(LayoutKind.Sequential)] struct POINT { public int X,Y; public POINT(int x,int y) {X=x;Y=y;} }
 [StructLayout(LayoutKind.Sequential)] struct SIZE { public int X,Y; public SIZE(int x,int y) {X=x;Y=y;} }
 [StructLayout(LayoutKind.Sequential,Pack=1)] struct BLEND { public byte Op,Flags,Alpha,Format; }
 [DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr hwnd);
 [DllImport("user32.dll")] static extern int ReleaseDC(IntPtr hwnd,IntPtr dc);
 [DllImport("gdi32.dll")] static extern IntPtr CreateCompatibleDC(IntPtr dc);
 [DllImport("gdi32.dll")] static extern IntPtr SelectObject(IntPtr dc,IntPtr obj);
 [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr obj);
 [DllImport("gdi32.dll")] static extern bool DeleteDC(IntPtr dc);
 [DllImport("user32.dll",SetLastError=true)] static extern bool UpdateLayeredWindow(IntPtr hwnd,IntPtr dst,ref POINT pos,ref SIZE size,IntPtr src,ref POINT origin,int color,ref BLEND blend,int flags);
 void Present(Bitmap bitmap,byte alpha) { IntPtr screen=GetDC(IntPtr.Zero),memory=IntPtr.Zero,hbitmap=IntPtr.Zero,old=IntPtr.Zero; try { memory=CreateCompatibleDC(screen); hbitmap=bitmap.GetHbitmap(Color.FromArgb(0)); old=SelectObject(memory,hbitmap); var pos=new POINT(Left,Top); var size=new SIZE(bitmap.Width,bitmap.Height); var origin=new POINT(0,0); var blend=new BLEND{Alpha=alpha,Format=1}; if(!UpdateLayeredWindow(Handle,screen,ref pos,ref size,memory,ref origin,0,ref blend,2)) throw new Win32Exception(Marshal.GetLastWin32Error()); } finally { if(old!=IntPtr.Zero) SelectObject(memory,old); if(hbitmap!=IntPtr.Zero) DeleteObject(hbitmap); if(memory!=IntPtr.Zero) DeleteDC(memory); if(screen!=IntPtr.Zero) ReleaseDC(IntPtr.Zero,screen); } }
}
