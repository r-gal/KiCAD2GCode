using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KiCad2Gcode
{
    internal class Drawer
    {
        PictureBox pBox;
        Panel pPanel;
        Bitmap bmp;

        float H = 600;

        int scale = 10;
        int offX = 0;
        int offY = 0;



        public Drawer(PictureBox pBox_, Panel pPanel)
        {
            this.pBox = pBox_;
            this.pPanel = pPanel;   
        }

        public void SetScale(int scale_)
        {
            scale = scale_;
        }

        public void SetPos(int x, int y)
        {
            offX = x;
            offY = y;
        }


        private void DrawDotInt(Point2D position, int size, Bitmap bmp, System.Drawing.Color color)
        {
            if (position.type == Point2D.PointType_et.CROSS_X)
            {
                color = Color.DarkGreen;
            }
            else if (position.type == Point2D.PointType_et.CROSS_X)
            {
                color = Color.Purple;
            }



            using (Graphics g = Graphics.FromImage(bmp))
            {

                g.FillEllipse(new SolidBrush(color),
                (float)((position.x) * scale + offX - size),
                H - ((float)((position.y) * scale + offY + size)),
                (float)(size * 2),
                (float)(size * 2));
            }
        }

        public void DrawDot(Point2D position, int size, System.Drawing.Color color)
        {
            DrawDotInt(position, size, bmp, color);
            pBox.Image = bmp;
            pBox.Refresh();
        }


        public void DrawElement(Point2D startPt, Point2D endPt, Arc arc)
        {
            DrawChunk(startPt, endPt, arc, bmp, System.Drawing.Color.Black);
            pBox.Image = bmp;
            pBox.Refresh();
        }

        public void SetCentre(Point2D position)
        {
            pPanel.AutoScrollPosition = new Point((int)(scale * position.x) -1000,(int) H -  (int)(scale * position.y)-600);
        }

        private void DrawChunk(Point2D startPt, Point2D endPt, Arc arc, Bitmap bmp, System.Drawing.Color color )
        {

            

            if(arc == null)
            {                
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawLine(new Pen(color), 
                        (float)startPt.x * scale + offX ,
                        H-((float)startPt.y * scale + offY ),
                        (float)endPt.x * scale + offX ,
                        H-((float)endPt.y * scale + offY ));
                } 
            }
            else
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    float angleStart;
                    float angleEnd;
                    if (arc.ccw == false)
                    {
                        angleStart = (float)(arc.startAngle * 180 / Math.PI);
                        angleEnd = (float)(arc.endAngle * 180 / Math.PI);
                    }
                    else
                    {
                        angleEnd = (float)(arc.startAngle * 180 / Math.PI);
                        angleStart = (float)(arc.endAngle * 180 / Math.PI);
                    }

                    float angleSweep = angleStart - angleEnd;
                    while (angleSweep < 0) { angleSweep += 360; }
                    while (angleSweep > 360) { angleSweep -= 360; }

                    angleStart = -angleStart;
                    while (angleStart < 0) { angleStart += 360; }
                    while (angleStart > 360) { angleStart -= 360; }

                    g.DrawArc(new Pen(color),
                        (float)((arc.centre.x -  arc.radius) * scale + offX ),
                        H - ((float)((arc.centre.y +  arc.radius) * scale + offY )),
                        (float)(arc.radius * 2* scale),
                        (float)(arc.radius * 2 *scale),
                        angleStart,
                        angleSweep);
                }

            }

        }


        private void DrawDrill(Drill drill, Bitmap bmp)
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {

                g.DrawArc(new Pen(Color.Black),
                    (float)((drill.pos.x - drill.diameter/2) * scale + offX ),
                    H - ((float)((drill.pos.y + drill.diameter/2) * scale + offY )),
                    (float)(drill.diameter  * scale),
                    (float)(drill.diameter  * scale),
                    0,
                    360);
            }


        }

        public void Redraw(Net[] netList, List<Net> zones,  List<Figure> cuts, List<Drill> drills)
        {


            /* fetch border size */

            double minX =0, maxX =0, minY = 0, maxY = 0;
            bool valid = false;

            foreach (Net net in netList)
            {
                foreach (Figure f in net.figures)
                {
                    if(valid == false)
                    {
                        minX = f.shape.extPoint[0].x;
                        maxY = f.shape.extPoint[1].y;
                        maxX = f.shape.extPoint[2].x;
                        minY = f.shape.extPoint[3].y;
                        valid = true;
                    }

                    if (f.shape.extPoint[0].x < minX) { minX = f.shape.extPoint[0].x; }
                    if (f.shape.extPoint[1].y > maxY) { maxY = f.shape.extPoint[1].y; }
                    if (f.shape.extPoint[2].x > maxX) { maxX = f.shape.extPoint[2].x; }
                    if (f.shape.extPoint[3].y < minY) { minY = f.shape.extPoint[3].y; }
                }
            }

            foreach (Net z in zones)
            {
                foreach (Figure f in z.figures)
                {
                    if (f.shape.extPoint[0].x < minX) { minX = f.shape.extPoint[0].x; }
                    if (f.shape.extPoint[1].y > maxY) { maxY = f.shape.extPoint[1].y; }
                    if (f.shape.extPoint[2].x > maxX) { maxX = f.shape.extPoint[2].x; }
                    if (f.shape.extPoint[3].y < minY) { minY = f.shape.extPoint[3].y; }
                }
            }

            foreach (Figure f in cuts)
            {
                if (f.shape.extPoint[0].x < minX) { minX = f.shape.extPoint[0].x; }
                if (f.shape.extPoint[1].y > maxY) { maxY = f.shape.extPoint[1].y; }
                if (f.shape.extPoint[2].x > maxX) { maxX = f.shape.extPoint[2].x; }
                if (f.shape.extPoint[3].y < minY) { minY = f.shape.extPoint[3].y; }
            }

            double sizeXd = maxX - minX;
            double sizeYd = maxY - minY;

            int sizeX = (int)(sizeXd * scale);
            int sizeY = (int)(sizeYd * scale);

            /* create image */

            //bmp = new Bitmap(800, 600);
            bmp = new Bitmap(sizeX, sizeY);
            pBox.Width = bmp.Width;
            pBox.Height = bmp.Height;

            offX = (int)(-minX * scale);
            offY = (int)(-minY * scale);

            H = sizeY;



            /* draw nets */

            foreach (Net net in netList)
            {
                foreach (Figure f in net.figures)
                {
                    LinkedListNode<Node> n = f.shape.points.First;

                    bool first = true;

                    while (n != null)
                    {
                        LinkedListNode<Node> nPrev = n.Previous ?? f.shape.points.Last;

                        Color color = Color.Red;
                        if(f.shape.selected == 1)
                        {
                            color = Color.Green;
                        }
                        else if(f.shape.selected == 2)
                        {
                            color = Color.LightBlue;
                        }
                        else if (n.Value.pt.type == Point2D.PointType_et.BRIDGE)
                        {
                            color = Color.Cyan;
                        }

                        DrawChunk(nPrev.Value.pt, n.Value.pt, n.Value.arc, bmp,color);



                        DrawDotInt(n.Value.pt, 2, bmp, first ? Color.DarkOrange : Color.Black);
                        if (first)
                        {
                            first = false;
                        }


                        n = n.Next;
                    }

                    foreach (Polygon p in f.holes)
                    {
                        n = p.points.First;

                        while (n != null)
                        {
                            LinkedListNode<Node> nPrev = n.Previous ?? p.points.Last;

                            Color color = Color.Blue;
                            if( p.selected == 1)
                            {
                                color = Color.Green;
                            }
                            else if (p.selected == 2)
                            {
                                color = Color.LightBlue;
                            }




                            DrawChunk(nPrev.Value.pt, n.Value.pt, n.Value.arc, bmp,color);

                            DrawDotInt(n.Value.pt, 3, bmp, first ? Color.DarkOrange : Color.Black);
                            if (first)
                            {
                                first = false;
                            }

                            n = n.Next;
                        }
                    }
                }
            }

            foreach (Net z in zones)
            {
                foreach (Figure f in z.figures)
                {
                    LinkedListNode<Node> n = f.shape.points.First;

                    bool first = true;

                    while (n != null)
                    {
                        LinkedListNode<Node> nPrev = n.Previous ?? f.shape.points.Last;

                        Color color = Color.Red;
                        if (f.shape.selected == 1)
                        {
                            color = Color.Green;
                        }
                        else if (f.shape.selected == 2)
                        {
                            color = Color.LightBlue;
                        }
                        else if (n.Value.pt.type == Point2D.PointType_et.BRIDGE)
                        {
                            color = Color.Cyan;
                        }

                        DrawChunk(nPrev.Value.pt, n.Value.pt, n.Value.arc, bmp,color);

                        DrawDotInt(n.Value.pt, 3, bmp, first ? Color.DarkOrange : Color.Black);
                        if (first)
                        {
                            first = false;
                        }


                        n = n.Next;
                    }

                    foreach (Polygon p in f.holes)
                    {
                        n = p.points.First;

                        while (n != null)
                        {
                            LinkedListNode<Node> nPrev = n.Previous ?? p.points.Last;

                            Color color = Color.Blue;
                            if (p.selected == 1)
                            {
                                color = Color.Green;
                            }
                            else if (p.selected == 2)
                            {
                                color = Color.LightBlue;
                            }

                            DrawChunk(nPrev.Value.pt, n.Value.pt, n.Value.arc, bmp, color);

                            DrawDotInt(n.Value.pt, 2, bmp, first ? Color.DarkOrange : Color.Black);
                            if (first)
                            {
                                first = false;
                            }

                            n = n.Next;
                        }
                    }
                }
            }

            foreach (Figure f in cuts)
            {
                LinkedListNode<Node> n = f.shape.points.First;

                while (n != null)
                {
                    LinkedListNode<Node> nPrev = n.Previous ?? f.shape.points.Last;
                    DrawChunk(nPrev.Value.pt, n.Value.pt, n.Value.arc, bmp, Color.Black);
                    n = n.Next;
                }
                
                foreach (Polygon p in f.holes)
                {
                    n = p.points.First;

                    while (n != null)
                    {
                        LinkedListNode<Node> nPrev = n.Previous ?? p.points.Last;
                        DrawChunk(nPrev.Value.pt, n.Value.pt, n.Value.arc, bmp, Color.Violet);
                        n = n.Next;
                    }
                }
                /*Polygon p = f.holes[1];
                {
                    n = p.points.First;

                    while (n != null)
                    {
                        LinkedListNode<Node> nPrev = n.Previous ?? p.points.Last;
                        DrawChunk(nPrev.Value.pt, n.Value.pt, n.Value.arc, bmp, Color.Violet);
                        n = n.Next;
                    }

                }*/
            }

            foreach (Drill drill in drills)
            {
                DrawDrill(drill, bmp);
            }

            pBox.Image = bmp;
            pBox.Refresh();
        }

    }
}
