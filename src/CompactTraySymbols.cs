// SPDX-License-Identifier: MIT
using System.Drawing;
using System.Drawing.Drawing2D;
partial class Tray {
 static void DrawCompactDeviceMark(Graphics graphics,int symbol) {
  using(var mark=new GraphicsPath{FillMode=FillMode.Winding}) {
   if(symbol==0) {
    mark.AddArc(2.5f,2.5f,15,15,180,180);
    mark.AddLine(17.5f,10,17.5f,15);
    mark.AddLine(17.5f,15,13.5f,15);
    mark.AddArc(6.5f,6.5f,7,7,0,-180);
    mark.AddLine(6.5f,10,6.5f,15);
    mark.AddLine(6.5f,15,2.5f,15);mark.CloseFigure();
    mark.StartFigure();mark.AddRectangle(new RectangleF(1.5f,10,5,6));
    mark.StartFigure();mark.AddRectangle(new RectangleF(13.5f,10,5,6));
   } else {
    // One eighth note needs fewer pixels than the two-note beam.
    mark.AddPolygon(new[]{new PointF(8,12.5f),new PointF(8,2),new PointF(17,4.5f),new PointF(17,8.5f),new PointF(11.5f,6.5f),new PointF(11.5f,12.5f)});
    mark.StartFigure();mark.AddEllipse(2,10,9.5f,6);
   }
   using(var outline=new Pen(Color.Black,2.5f){LineJoin=LineJoin.Round})graphics.DrawPath(outline,mark);
   using(var fill=new SolidBrush(Color.White))graphics.FillPath(fill,mark);
  }
 }
}
