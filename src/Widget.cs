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
[TypeConverter(typeof(LocalizedAlignmentConverter))]
enum WidgetTextAlignment { Links, Zentriert, Rechts }
class InstalledFontNames : StringConverter {
 public override bool GetStandardValuesSupported(ITypeDescriptorContext c) { return true; }
 public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext c) { var names=new List<string>(); foreach(var f in FontFamily.Families) { names.Add(f.Name); f.Dispose(); } return new StandardValuesCollection(names); }
}
class WidgetOptions {
 [Browsable(false)] public bool HideFullscreen {get;set;}
 [LocalizedCategory("1. Anzeige"),LocalizedName("Tiny: Bildsymbol ohne Akku")] public bool TinyImageOnly {get;set;}
 [LocalizedCategory("1. Anzeige"),LocalizedName("Am Bildschirmrand einrasten")] public bool Snap {get;set;}
 [LocalizedCategory("2. Größe und Position"),LocalizedName("Einrastabstand (Pixel)")] public int SnapDistance {get;set;}
 [LocalizedCategory("1. Anzeige"),LocalizedName("Tiny: nur Akkusymbole"),LocalizedDescription("Kompakte Ansicht ohne zusätzliche Prozentzahl; jedes Symbol bleibt anklickbar.")] public bool Tiny {get;set;}
 [Browsable(false),LocalizedCategory("1. Anzeige"),LocalizedName("Anzeigemodus")] public DisplayMode Mode {get;set;}
 [LocalizedCategory("1. Anzeige"),LocalizedName("Immer im Vordergrund")] public bool Topmost {get;set;}
 [LocalizedCategory("1. Anzeige"),LocalizedName("Position sperren")] public bool Locked {get;set;}
 [LocalizedCategory("1. Anzeige"),LocalizedName("Nebeneinander statt untereinander")] public bool Horizontal {get;set;}
 [LocalizedCategory("2. Größe und Position"),LocalizedName("Breite (Pixel)")] public int Width {get;set;}
 [LocalizedCategory("2. Größe und Position"),LocalizedName("Höhe (Pixel)")] public int Height {get;set;}
 [LocalizedCategory("2. Größe und Position"),LocalizedName("Position X"),LocalizedDescription("-32000 setzt das Widget beim nächsten Anzeigen mittig auf den Bildschirm.")] public int X {get;set;}
 [LocalizedCategory("2. Größe und Position"),LocalizedName("Position Y")] public int Y {get;set;}
 [LocalizedCategory("2. Größe und Position"),LocalizedName("Innenabstand (Pixel)")] public int Padding {get;set;}
 [LocalizedCategory("2. Größe und Position"),LocalizedName("Abstand der Geräte (Pixel)")] public int Gap {get;set;}
 [LocalizedCategory("2. Größe und Position"),LocalizedName("Symbolgröße (Pixel)")] public int IconSize {get;set;}
 [LocalizedCategory("3. Farben und Transparenz"),LocalizedName("Gesamte Deckkraft (%)"),LocalizedDescription("10 bis 100. Niedrigere Werte machen das gesamte Widget durchsichtiger.")] public int Opacity {get;set;}
 [LocalizedCategory("3. Farben und Transparenz"),LocalizedName("Hintergrund-Deckkraft (%)"),LocalizedDescription("0: Hintergrund vollständig transparent, Schrift und Symbole bleiben sichtbar.")] public int BackgroundOpacity {get;set;}
 [LocalizedCategory("3. Farben und Transparenz"),LocalizedName("Hintergrundfarbe")] public Color Background {get;set;}
 [LocalizedCategory("3. Farben und Transparenz"),LocalizedName("Schriftfarbe")] public Color Foreground {get;set;}
 [LocalizedCategory("3. Farben und Transparenz"),LocalizedName("Rahmenfarbe")] public Color Border {get;set;}
 [LocalizedCategory("3. Farben und Transparenz"),LocalizedName("Rahmenstärke (Pixel)")] public int BorderWidth {get;set;}
 [LocalizedCategory("3. Farben und Transparenz"),LocalizedName("Eckenrundung (Pixel)")] public int Radius {get;set;}
 [LocalizedCategory("4. Schrift und Inhalt"),LocalizedName("Schriftart"),TypeConverter(typeof(InstalledFontNames))] public string FontName {get;set;}
 [LocalizedCategory("4. Schrift und Inhalt"),LocalizedName("Schriftart für Prozentwerte"),TypeConverter(typeof(InstalledFontNames))] public string ValueFontName {get;set;}
 [LocalizedCategory("4. Schrift und Inhalt"),LocalizedName("Textausrichtung")] public WidgetTextAlignment Alignment {get;set;}
 [LocalizedCategory("4. Schrift und Inhalt"),LocalizedName("Fette Gerätenamen")] public bool NameBold {get;set;}
 [LocalizedCategory("4. Schrift und Inhalt"),LocalizedName("Schriftgröße für Namen")] public int FontSize {get;set;}
 [LocalizedCategory("4. Schrift und Inhalt"),LocalizedName("Schriftgröße für Prozentwert")] public int ValueSize {get;set;}
 [LocalizedCategory("4. Schrift und Inhalt"),LocalizedName("Fette Prozentzahl")] public bool Bold {get;set;}
 [LocalizedCategory("4. Schrift und Inhalt"),LocalizedName("Gerätenamen anzeigen")] public bool Names {get;set;}
 [LocalizedCategory("4. Schrift und Inhalt"),LocalizedName("Verbindungsstatus anzeigen")] public bool Status {get;set;}
 [LocalizedCategory("4. Schrift und Inhalt"),LocalizedName("Gerätesymbole anzeigen")] public bool Icons {get;set;}
 public WidgetOptions() { Mode=DisplayMode.Infobereich; Snap=true; SnapDistance=12; Topmost=true; Width=300; Height=300; X=-32000; Y=-32000; Padding=20; Gap=16; IconSize=64; Opacity=100; BackgroundOpacity=82; Background=Color.FromArgb(28,32,40); Foreground=Color.White; Border=Color.FromArgb(80,90,105); BorderWidth=2; Radius=28; FontName="Segoe UI"; ValueFontName="Arial"; FontSize=16; ValueSize=32; Bold=true; Names=true; Status=true; Icons=true; }
 public void ApplyPreset(bool tiny) {
  var preset=new WidgetOptions();
  if(tiny) { preset.Tiny=true; preset.Width=112; preset.Height=56; preset.Padding=6; preset.Gap=8; preset.IconSize=40; preset.Horizontal=true; preset.Radius=14; preset.Names=false; preset.Status=false; }
  var hideFullscreen=HideFullscreen; var mode=Mode; var x=X; var y=Y; var locked=Locked; var topmost=Topmost;
  foreach(var property in GetType().GetProperties()) property.SetValue(this,property.GetValue(preset,null),null);
  HideFullscreen=hideFullscreen; Mode=mode; X=x; Y=y; Locked=locked; Topmost=topmost;
 }
 public WidgetOptions Copy() { return (WidgetOptions)MemberwiseClone(); }
 static int Clamp(int x,int lo,int hi) { return Math.Max(lo,Math.Min(hi,x)); }
 public void Normalize() { SnapDistance=Clamp(SnapDistance,0,80); if(!Enum.IsDefined(typeof(DisplayMode),Mode)) Mode=DisplayMode.Infobereich; Width=Clamp(Width,Tiny ? 48 : 160,1600); Height=Clamp(Height,Tiny ? 40 : 90,1000); Padding=Clamp(Padding,0,80); Gap=Clamp(Gap,0,80); IconSize=Clamp(IconSize,16,160); Opacity=Clamp(Opacity,10,100); BackgroundOpacity=Clamp(BackgroundOpacity,0,100); BorderWidth=Clamp(BorderWidth,0,12); Radius=Clamp(Radius,0,200); FontSize=Clamp(FontSize,6,72); ValueSize=Clamp(ValueSize,8,120); if(string.IsNullOrWhiteSpace(FontName)) FontName="Segoe UI"; if(string.IsNullOrWhiteSpace(ValueFontName)) ValueFontName="Arial"; if(!Enum.IsDefined(typeof(WidgetTextAlignment),Alignment)) Alignment=WidgetTextAlignment.Links; Background=Color.FromArgb(Background.R,Background.G,Background.B); Foreground=Color.FromArgb(Foreground.R,Foreground.G,Foreground.B); Border=Color.FromArgb(Border.R,Border.G,Border.B); }
 public IEnumerable<string> Lines() { foreach(var p in GetType().GetProperties()) { var value=p.GetValue(this,null); string text=p.PropertyType==typeof(Color) ? ColorTranslator.ToHtml((Color)value) : Convert.ToString(value,CultureInfo.InvariantCulture); yield return "Widget."+p.Name+"="+Uri.EscapeDataString(text); } }
 public void Read(string key,string value) { var p=GetType().GetProperty(key); if(p==null || !p.CanWrite) return; try { value=Uri.UnescapeDataString(value); object parsed=p.PropertyType==typeof(Color) ? (object)ColorTranslator.FromHtml(value) : p.PropertyType.IsEnum ? Enum.Parse(p.PropertyType,value) : Convert.ChangeType(value,p.PropertyType,CultureInfo.InvariantCulture); p.SetValue(this,parsed,null); } catch(ArgumentException) {} catch(FormatException) {} catch(OverflowException) {} }
}
class WidgetItem { public string Name,Status; public int? Battery; public bool? Connected; public bool AudioActive; public DeviceProfile Profile; }
static class WidgetDrawing {
 static GraphicsPath Round(float x,float y,float w,float h,float radius) { var p=new GraphicsPath(); float d=Math.Min(radius*2,Math.Min(w,h)); if(d<1) p.AddRectangle(new RectangleF(x,y,w,h)); else { p.AddArc(x,y,d,d,180,90); p.AddArc(x+w-d,y,d,d,270,90); p.AddArc(x+w-d,y+h-d,d,d,0,90); p.AddArc(x,y+h-d,d,d,90,90); p.CloseFigure(); } return p; }
 static void Text(Graphics g,string text,string fontName,int size,FontStyle style,Brush brush,RectangleF area,WidgetTextAlignment alignment=WidgetTextAlignment.Links) { if(area.Width<4 || area.Height<4) return; Font font; try { font=new Font(fontName,size,style,GraphicsUnit.Pixel); } catch(ArgumentException) { font=new Font("Segoe UI",size,style,GraphicsUnit.Pixel); } using(font) using(var format=new StringFormat{Trimming=StringTrimming.EllipsisCharacter,FormatFlags=StringFormatFlags.NoWrap,LineAlignment=StringAlignment.Center,Alignment=alignment==WidgetTextAlignment.Zentriert ? StringAlignment.Center : alignment==WidgetTextAlignment.Rechts ? StringAlignment.Far : StringAlignment.Near}) g.DrawString(text,font,brush,area,format); }
 public static RectangleF Cell(WidgetOptions o,int count,int i) {
  count=Math.Max(1,count); float pad=Math.Min(o.Padding,Math.Min(o.Width,o.Height)/5f),gap=Math.Min(o.Gap,(o.Horizontal ? o.Width : o.Height)/8f);
  float w=o.Horizontal ? (o.Width-2*pad-gap*(count-1))/count : o.Width-2*pad,h=o.Horizontal ? o.Height-2*pad : (o.Height-2*pad-gap*(count-1))/count;
  return new RectangleF(pad+(o.Horizontal ? i*(w+gap) : 0),pad+(o.Horizontal ? 0 : i*(h+gap)),w,h);
 }
 public static int Hit(WidgetOptions o,int count,Point point) { for(int i=0;i<count;i++)if(Cell(o,count,i).Contains(point))return i;return -1; }
 public static Bitmap Render(WidgetOptions o,Settings settings,IList<WidgetItem> items) {
  var bitmap=new Bitmap(o.Width,o.Height,PixelFormat.Format32bppArgb);
  using(var g=Graphics.FromImage(bitmap)) { g.Clear(Color.Transparent); g.SmoothingMode=SmoothingMode.AntiAlias; g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
   float inset=Math.Max(0.5f,o.BorderWidth/2f); using(var body=Round(inset,inset,o.Width-2*inset,o.Height-2*inset,o.Radius)) { using(var brush=new SolidBrush(Color.FromArgb(o.BackgroundOpacity*255/100,o.Background))) g.FillPath(brush,body); if(o.BorderWidth>0) using(var pen=new Pen(o.Border,o.BorderWidth)) g.DrawPath(pen,body); }
   int count=Math.Max(1,items.Count); float gap=Math.Min(o.Gap,(o.Horizontal ? o.Width : o.Height)/8f);
   for(int i=0;i<count;i++) { var item=i<items.Count ? items[i] : new WidgetItem{Name=L.T("Keine Kopfhörer ausgewählt"),Status=L.T("Doppelklick: Einstellungen"),Profile=new DeviceProfile("#13705B")}; var cell=Cell(o,count,i);float x=cell.X,y=cell.Y,cellW=cell.Width,cellH=cell.Height; if(o.Tiny) {
     float size=Math.Max(1,Math.Min(o.IconSize,Math.Min(cellW,cellH)));
     if(o.Icons && o.TinyImageOnly && item.Profile.Symbol==5) { float ix=x+(cellW-size)/2,iy=y+(cellH-size)/2; CustomSymbols.Draw(g,item.Profile.IconFile,new RectangleF(ix,iy,size,size)); if(item.AudioActive){var state=g.Save();g.TranslateTransform(ix,iy);g.ScaleTransform(size/32,size/32);Tray.AudioBadge(g);g.Restore(state);} }
     else if(o.Icons) using(var icon=Tray.BatteryIcon(item.Battery,i,item.Connected,item.Profile.Color,settings.NumbersOnly,settings,item.Profile.Symbol,item.Profile.RenderMarker,item.AudioActive,0,(int)Math.Ceiling(size))) using(var image=icon.ToBitmap()) { g.InterpolationMode=InterpolationMode.HighQualityBicubic; g.DrawImage(image,new RectangleF(x+(cellW-size)/2,y+(cellH-size)/2,size,size)); }
     continue;
    }
    float iconSize=o.Icons ? Math.Min(o.IconSize,Math.Min(cellH,cellW*0.4f)) : 0;
    if(iconSize>0) using(var icon=Tray.BatteryIcon(item.Battery,i,item.Connected,item.Profile.Color,settings.NumbersOnly,settings,item.Profile.Symbol,item.Profile.RenderMarker,false,0,(int)Math.Ceiling(iconSize))) using(var image=icon.ToBitmap()) { g.InterpolationMode=InterpolationMode.HighQualityBicubic; g.DrawImage(image,new RectangleF(x,y+(cellH-iconSize)/2,iconSize,iconSize)); }
    float textX=x+(iconSize>0 ? iconSize+Math.Min(12,gap+4) : 0),textW=cellW-(textX-x)-(item.AudioActive ? 22 : 0); float nameH=o.Names ? Math.Min(o.FontSize*1.5f,cellH*0.3f) : 0,statusH=o.Status ? Math.Min(o.FontSize*1.4f,cellH*0.25f) : 0,valueH=cellH-nameH-statusH;
    if(item.AudioActive) { float badge=Math.Min(20,Math.Min(cellW,cellH));float bx=x+cellW-badge,by=y+(cellH-badge)/2;
     using(var brush=new SolidBrush(Color.FromArgb(18,151,76)))g.FillEllipse(brush,bx,by,badge,badge);
     using(var pen=new Pen(Color.White,Math.Max(1,badge/9))) {pen.StartCap=LineCap.Round;pen.EndCap=LineCap.Round;g.DrawLines(pen,new[]{new PointF(bx+badge*.23f,by+badge*.51f),new PointF(bx+badge*.43f,by+badge*.71f),new PointF(bx+badge*.77f,by+badge*.29f)});}
    }
    using(var brush=new SolidBrush(o.Foreground)) { if(o.Names) Text(g,item.Name,o.FontName,Math.Min(o.FontSize,(int)(nameH*0.85f)),o.NameBold ? FontStyle.Bold : FontStyle.Regular,brush,new RectangleF(textX,y,textW,nameH),o.Alignment); string value=item.Connected==false ? "–" : item.Connected==true && item.Battery.HasValue ? item.Battery+" %" : "?"; Text(g,value,o.ValueFontName,Math.Max(6,Math.Min(o.ValueSize,(int)(valueH*0.85f))),o.Bold ? FontStyle.Bold : FontStyle.Regular,brush,new RectangleF(textX,y+nameH,textW,valueH),o.Alignment); if(o.Status) Text(g,item.Status ?? L.T("Status unbekannt"),o.FontName,Math.Max(6,Math.Min(o.FontSize,(int)(statusH*0.85f))),FontStyle.Regular,brush,new RectangleF(textX,y+nameH+valueH,textW,statusH),o.Alignment); }
   }
  } return bitmap;
 }
 public static Bitmap Preview(WidgetOptions options,Settings settings,IList<WidgetItem> items,int width,int height) { using(var raw=Render(options,settings,items)) { var result=new Bitmap(width,height); using(var g=Graphics.FromImage(result)) { for(int x=0;x<width;x+=12) for(int y=0;y<height;y+=12) using(var b=new SolidBrush((x/12+y/12)%2==0 ? Color.FromArgb(215,215,215) : Color.FromArgb(245,245,245))) g.FillRectangle(b,x,y,12,12); float scale=Math.Min((float)(width-8)/raw.Width,(float)(height-8)/raw.Height); var area=new RectangleF((width-raw.Width*scale)/2,(height-raw.Height*scale)/2,raw.Width*scale,raw.Height*scale); using(var attributes=new ImageAttributes()) { var matrix=new ColorMatrix(); matrix.Matrix33=options.Opacity/100f; attributes.SetColorMatrix(matrix); g.DrawImage(raw,Rectangle.Round(area),0,0,raw.Width,raw.Height,GraphicsUnit.Pixel,attributes); } } return result; } }
}
class BatteryWidget : Form {
 readonly ToolTip deviceTip=new ToolTip{ShowAlways=true}; System.Collections.Generic.IList<WidgetItem> tooltipItems=new System.Collections.Generic.List<WidgetItem>(); int tooltipSlot=-1;
 Settings contentSettings;
 WidgetOptions options; Action<Point> moved; Action<int> chooseAudio; int itemCount,pendingSlot=-1; bool quickAudio;
 readonly WidgetGesture gesture=new WidgetGesture(); readonly System.Windows.Forms.Timer clickTimer=new System.Windows.Forms.Timer{Interval=SystemInformation.DoubleClickTime};
 void CancelClick(){clickTimer.Stop();pendingSlot=-1;}
 void CommitClick(){clickTimer.Stop();int slot=pendingSlot;pendingSlot=-1;if(quickAudio && slot>=0 && slot<itemCount && chooseAudio!=null)chooseAudio(slot);}
 internal void FlushClickForTest(){CommitClick();}

 protected override CreateParams CreateParams { get { var p=base.CreateParams; p.ExStyle|=0x80000|0x80; return p; } }
 protected override bool ShowWithoutActivation {get {return true;}}
 public BatteryWidget(Action open,Action pick,Action quit,Action<Point> onMove,Action<int> onAudio=null,Action<ContextMenuStrip> configureMenu=null) { moved=onMove;chooseAudio=onAudio; Microsoft.Win32.SystemEvents.DisplaySettingsChanged+=DisplaysChanged;clickTimer.Tick+=(s,e)=>CommitClick(); FormBorderStyle=FormBorderStyle.None; ShowInTaskbar=false; StartPosition=FormStartPosition.Manual; Text="BleAirDock Widget"; var menu=new ContextMenuStrip(); if(configureMenu!=null){configureMenu(menu);menu.Opening+=(s,e)=>{foreach(ToolStripItem old in menu.Items.Cast<ToolStripItem>().ToArray())old.Dispose();menu.Items.Clear();configureMenu(menu);};} else { menu.Items.Add(L.T("Einstellungen …"),null,(s,e)=>open()); menu.Items.Add(L.T("Geräte auswählen …"),null,(s,e)=>pick()); menu.Items.Add(L.T("Beenden"),null,(s,e)=>quit()); } ContextMenuStrip=menu; DoubleClick+=(s,e)=>{CancelClick();open();}; }
 public void UpdateContent(WidgetOptions value,Settings settings,IList<WidgetItem> items) { options=value;contentSettings=settings;tooltipItems=items;tooltipSlot=-1;itemCount=items.Count;quickAudio=settings.QuickAudio; TopMost=value.Topmost; var desired=new Rectangle(value.X,value.Y,value.Width,value.Height); if(value.X==-32000 || !Screen.AllScreens.Any(s=>Rectangle.Intersect(s.WorkingArea,desired).Width>=60 && Rectangle.Intersect(s.WorkingArea,desired).Height>=40)) { var area=Screen.PrimaryScreen.WorkingArea; desired.Location=new Point(area.Left+Math.Max(0,(area.Width-value.Width)/2),area.Top+Math.Max(0,(area.Height-value.Height)/2)); } if(!gesture.Pressed) { var screen=Screen.FromRectangle(desired).WorkingArea; desired.Location=FitPosition(desired.Location,desired.Size,screen,0); Location=desired.Location; Size=desired.Size; } if(!Visible) Show(); using(var bitmap=WidgetDrawing.Render(value,settings,items)) Present(bitmap,(byte)(value.Opacity*255/100)); }
 void DisplaysChanged(object sender,EventArgs args) { if(IsDisposed || !IsHandleCreated)return; try { BeginInvoke((Action)(()=> { if(!IsDisposed && options!=null)UpdateContent(options,contentSettings,tooltipItems); })); } catch(InvalidOperationException) {} }
 internal int WheelSlot(Point screen) { return options==null || !Visible ? -1 : WidgetDrawing.Hit(options,itemCount,PointToClient(screen)); }
 internal static string Tooltip(WidgetItem item) { return item.Name+" · "+(item.Status ?? L.T("Status unbekannt"))+" · "+(item.Connected==true && item.Battery.HasValue ? item.Battery+" %" : "–"); }
 internal static Point FitPosition(Point point,Size size,Rectangle area,int snap) {
  int maxX=Math.Max(area.Left,area.Right-size.Width),maxY=Math.Max(area.Top,area.Bottom-size.Height);
  int x=Math.Max(area.Left,Math.Min(maxX,point.X)),y=Math.Max(area.Top,Math.Min(maxY,point.Y));
  if(Math.Abs(x-area.Left)<=snap)x=area.Left; else if(Math.Abs(x-maxX)<=snap)x=maxX;
  if(Math.Abs(y-area.Top)<=snap)y=area.Top; else if(Math.Abs(y-maxY)<=snap)y=maxY;
  return new Point(x,y);
 }
 protected override void OnMouseLeave(EventArgs e) { tooltipSlot=-1; deviceTip.SetToolTip(this,""); base.OnMouseLeave(e); }
 protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e);if(e.Button!=MouseButtons.Left || options==null)return;
  if(e.Clicks>1){CancelClick();gesture.Cancel();return;}CancelClick();gesture.Begin(PointToScreen(e.Location),Location,WidgetDrawing.Hit(options,itemCount,e.Location));Capture=true;
 }
 protected override void OnMouseMove(MouseEventArgs e) { base.OnMouseMove(e);if(options==null)return;
  int hovered=WidgetDrawing.Hit(options,itemCount,e.Location); if(hovered!=tooltipSlot) { tooltipSlot=hovered; deviceTip.SetToolTip(this,hovered>=0 && hovered<tooltipItems.Count ? Tooltip(tooltipItems[hovered]) : ""); }
  var screen=PointToScreen(e.Location);gesture.Move(screen,SystemInformation.DragSize);if(gesture.Dragged && !options.Locked) { var point=gesture.Position(screen); Location=FitPosition(point,Size,Screen.FromPoint(screen).WorkingArea,options.Snap ? options.SnapDistance : 0); }
 }
 protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e);if(e.Button!=MouseButtons.Left || !gesture.Pressed || options==null)return;
  gesture.Move(PointToScreen(e.Location),SystemInformation.DragSize);bool dragged=gesture.Dragged;int slot=gesture.End();Capture=false;
  if(dragged){if(!options.Locked)moved(Location);return;}if(quickAudio && slot>=0 && WidgetDrawing.Hit(options,itemCount,e.Location)==slot){pendingSlot=slot;clickTimer.Start();}
 }
 protected override void OnMouseCaptureChanged(EventArgs e){base.OnMouseCaptureChanged(e);if(!Capture && gesture.Pressed)gesture.Cancel();}
 protected override void Dispose(bool disposing) { if(disposing){Microsoft.Win32.SystemEvents.DisplaySettingsChanged-=DisplaysChanged;CancelClick();deviceTip.Dispose();clickTimer.Dispose();if(ContextMenuStrip!=null)ContextMenuStrip.Dispose();}base.Dispose(disposing); }
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

// Uses screen coordinates so dragging remains stable while the window moves.
class WidgetGesture {
 public bool Pressed,Dragged;Point origin,window;int slot;
 public void Begin(Point point,Point location,int hit){Pressed=true;Dragged=false;origin=point;window=location;slot=hit;}
 public void Move(Point point,Size threshold){if(Pressed && (Math.Abs(point.X-origin.X)>=Math.Max(1,threshold.Width/2) || Math.Abs(point.Y-origin.Y)>=Math.Max(1,threshold.Height/2)))Dragged=true;}
 public Point Position(Point point){return new Point(window.X+point.X-origin.X,window.Y+point.Y-origin.Y);}
 public int End(){int result=Pressed && !Dragged ? slot : -1;Pressed=false;return result;}
 public void Cancel(){Pressed=false;Dragged=false;}
}
