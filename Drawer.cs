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

        float H = 600;
        /*
        int scale = 10;
        int offX = 0;
        int offY = 0;
        */
        int scale = 20;
        int offX = -100;
        int offY = -800;
        

        public Drawer(PictureBox pBox_) 
        { 
            this.pBox = pBox_;
        }

        public void SetScale(int scale_)
        {
            scale = scale_;
        }

        public void SetPos(int x,int y)
        {
            offX = x;
            offY = y;
        }


        private void DrawDot(Point2D position, int size, Bitmap bmp, System.Drawing.Color color)
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawArc(new Pen(color),
                (float)((position.x ) * scale + offX * scale - size),
                H - ((float)((position.y ) * scale + offY * scale + size)),
                (float)(size * 2 ),
                (float)(size * 2 ),
                0,
                360);
            }
        }

        private void DrawChunk(Point2D startPt, Point2D endPt, Arc arc, Bitmap bmp, System.Drawing.Color color )
        {
            if(arc == null)
            {                
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawLine(new Pen(color), 
                        (float)startPt.x * scale + offX * scale,
                        H-((float)startPt.y * scale + offY * scale),
                        (float)endPt.x * scale + offX * scale,
                        H-((float)endPt.y * scale + offY * scale));
                } 
            }
            else
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    float angleStart = (float)(arc.startAngle *180 / Math.PI);
                    float angleEnd = (float)(arc.endAngle * 180 / Math.PI);

                    float angleSweep = angleStart - angleEnd;
                    while (angleSweep < 0) { angleSweep += 360; }
                    while (angleSweep > 360) { angleSweep -= 360; }

                    angleStart = -angleStart;
                    while (angleStart < 0) { angleStart += 360; }
                    while (angleStart > 360) { angleStart -= 360; }

                    g.DrawArc(new Pen(color),
                        (float)((arc.centre.x -  arc.radius) * scale + offX * scale),
                        H - ((float)((arc.centre.y +  arc.radius) * scale + offY * scale)),
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
                    (float)((drill.pos.x - drill.diameter/2) * scale + offX * scale),
                    H - ((float)((drill.pos.y + drill.diameter/2) * scale + offY * scale)),
                    (float)(drill.diameter  * scale),
                    (float)(drill.diameter  * scale),
                    0,
                    360);
            }


        }

        public void Redraw(List<Figure> figures, List<Figure> cuts, List<Drill> drills)
        {
            Bitmap bmp = new Bitmap(800, 600);
            pBox.Width = bmp.Width;
            pBox.Height = bmp.Height;

            foreach (Figure f in figures)
            {
                LinkedListNode<Node> n = f.shape.points.First;

                bool first = true;

                while (n != null)
                {
                    LinkedListNode<Node> nPrev = n.Previous ?? f.shape.points.Last;
                    DrawChunk(nPrev.Value.pt, n.Value.pt, n.Value.arc, bmp, f.selected ? Color.Green : Color.Red);

                    DrawDot(n.Value.pt, 3, bmp, first ? Color.DarkOrange :  Color.Black);
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
                        DrawChunk(nPrev.Value.pt, n.Value.pt, n.Value.arc, bmp, Color.Blue);
                        n = n.Next;
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
        }

    }
}
