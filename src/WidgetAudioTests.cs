// SPDX-License-Identifier: MIT
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

static class WidgetAudioTests {
 static void Check(bool value,string label){if(!value)throw new Exception("Widget audio: "+label);}
 class Probe : BatteryWidget {
  public Probe(Action open,Action<int> audio,Action<Point> moved):base(open,()=>{},()=>{},moved,audio){}
  public void Down(Point p,int clicks=1){OnMouseDown(new MouseEventArgs(MouseButtons.Left,clicks,p.X,p.Y,0));}
  public void MoveTo(Point p){OnMouseMove(new MouseEventArgs(MouseButtons.Left,0,p.X,p.Y,0));}
  public void Up(Point p){OnMouseUp(new MouseEventArgs(MouseButtons.Left,1,p.X,p.Y,0));}
  public void Double(){OnDoubleClick(EventArgs.Empty);}
 }
 public static void Run(){
  var options=new WidgetOptions{X=100,Y=100};
  foreach(bool horizontal in new[]{false,true}){
   options.Horizontal=horizontal;
   for(int i=0;i<2;i++){var c=WidgetDrawing.Cell(options,2,i);Check(WidgetDrawing.Hit(options,2,new Point((int)(c.X+c.Width/2),(int)(c.Y+c.Height/2)))==i,"layout selection");}
   Check(WidgetDrawing.Hit(options,2,Point.Empty)==-1,"padding");
  }
  options.Horizontal=false;
  var items=new List<WidgetItem>{new WidgetItem{Name="One",Battery=75,Connected=true,AudioActive=true,Profile=new DeviceProfile("#13705B")},new WidgetItem{Name="Two",Battery=65,Connected=true,Profile=new DeviceProfile("#1C579F")}};
  int selected=-1,opened=0,moves=0;
  using(var widget=new Probe(()=>opened++,i=>selected=i,p=>moves++)){
   var settings=new Settings();widget.UpdateContent(options,settings,items);
   var c=WidgetDrawing.Cell(options,2,1);var hit=new Point((int)c.X+30,(int)c.Y+20);
   widget.Down(hit);widget.Up(hit);widget.FlushClickForTest();Check(selected==1 && moves==0,"single click second device");
   selected=-1;widget.Down(hit);widget.MoveTo(new Point(hit.X+30,hit.Y+20));widget.Up(hit);widget.FlushClickForTest();Check(selected==-1 && moves==1,"drag never switches");
   options.Locked=true;widget.UpdateContent(options,settings,items);widget.Down(hit);widget.Up(hit);widget.FlushClickForTest();Check(selected==1,"locked widget remains clickable");
   selected=-1;widget.Down(hit);widget.MoveTo(new Point(hit.X+30,hit.Y));widget.Up(hit);widget.FlushClickForTest();Check(selected==-1 && moves==1,"locked movement does not click");
   widget.Down(hit);widget.Up(hit);widget.Down(hit,2);widget.Double();widget.Up(hit);widget.FlushClickForTest();Check(selected==-1 && opened==1,"double click only opens settings");
   settings.QuickAudio=false;widget.UpdateContent(options,settings,items);widget.Down(hit);widget.Up(hit);widget.FlushClickForTest();Check(selected==-1,"disabled quick selection");
   settings.QuickAudio=true;widget.UpdateContent(options,settings,new List<WidgetItem>());widget.Down(hit);widget.Up(hit);widget.FlushClickForTest();Check(selected==-1,"empty widget");
  }
  options.Locked=false;options.Icons=false;
  using(var image=WidgetDrawing.Render(options,new Settings(),items)){var cell=WidgetDrawing.Cell(options,2,0);var green=image.GetPixel((int)(cell.Right-10),(int)(cell.Y+cell.Height/2+7));Check(green.G>green.R+50,"active badge without icons");}
 }
}
