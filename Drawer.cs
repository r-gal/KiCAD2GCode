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

        int scale = 10;
        /*int offX = 0;
        int offY = 1000;*/
        
        int offX = 0;
        int offY = 0;

        
        public Drawer(PictureBox pBox_) 
        { 
            this.pBox = pBox_;
        }


        private void DrawChunk(Point2D startPt, Point2D endPt, Arc arc, Bitmap bmp, System.Drawing.Color color )
        {
            if(arc == null)
            {                
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawLine(new Pen(color), 
                        (float)startPt.x * scale + offX,
                        H-((float)startPt.y * scale + offY),
                        (float)endPt.x * scale + offX,
                        H-((float)endPt.y * scale + offY));
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
                        (float)((arc.centre.x -  arc.radius) * scale + offX),
                        H - ((float)((arc.centre.y +  arc.radius) * scale + offY)),
                        (float)(arc.radius * 2* scale),
                        (float)(arc.radius * 2 *scale),
                        angleStart,
                        angleSweep);
                }

            }

        }

        private void DrawChunkOld(Chunk chunk, Bitmap bmp, System.Drawing.Color color)
        {
            if (chunk.type == Chunk.ChunkType.Line)
            {



                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawLine(new Pen(color),
                        (float)chunk.start.x * scale + offX,
                        H - ((float)chunk.start.y * scale + offY),
                        (float)chunk.end.x * scale + offX,
                        H - ((float)chunk.end.y * scale + offY));
                }




            }
            else if (chunk.type == Chunk.ChunkType.Arc)
            {
                Arc arc = (Arc)chunk;
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    float angleStart = (float)(arc.startAngle * 180 / Math.PI);
                    float angleEnd = (float)(arc.endAngle * 180 / Math.PI);

                    float angleSweep = angleStart - angleEnd;
                    while (angleSweep < 0) { angleSweep += 360; }
                    while (angleSweep > 360) { angleSweep -= 360; }

                    angleStart = -angleStart;
                    while (angleStart < 0) { angleStart += 360; }
                    while (angleStart > 360) { angleStart -= 360; }

                    g.DrawArc(new Pen(color),
                        (float)((arc.centre.x - arc.radius) * scale + offX),
                        H - ((float)((arc.centre.y + arc.radius) * scale + offY)),
                        (float)(arc.radius * 2 * scale),
                        (float)(arc.radius * 2 * scale),
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
                    (float)((drill.pos.x - drill.diameter/2) * scale + offX),
                    H - ((float)((drill.pos.y + drill.diameter/2) * scale + offY)),
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
                LinkedListNode<Node> n = f.points.First;

                while(n != null)
                {                    
                    LinkedListNode<Node> nPrev = n.Previous ?? f.points.Last;
                    DrawChunk(nPrev.Value.pt, n.Value.pt, n.Value.arc, bmp, Color.Red);
                    n = n.Next;
                }
            }

            foreach (Figure f in cuts)
            {
                LinkedListNode<Node> n = f.points.First;

                while (n != null)
                {
                    LinkedListNode<Node> nPrev = n.Previous ?? f.points.Last;
                    DrawChunk(nPrev.Value.pt, n.Value.pt, n.Value.arc, bmp, Color.Black);
                    n = n.Next;
                }
            }


            foreach (Drill drill in drills)
            {
                DrawDrill(drill, bmp);
            }

            pBox.Image = bmp;
        }

        public void RedrawOld(List<Figure> figures, List<Figure> cuts, List<Drill> drills)
        {
            Bitmap bmp = new Bitmap(800, 600);
            pBox.Width = bmp.Width;
            pBox.Height = bmp.Height;

            foreach (Figure f in figures)
            {
                foreach (Chunk chunk in f.chunks)
                {
                    DrawChunkOld(chunk, bmp, Color.Red);
                }
            }

            foreach (Figure f in cuts)
            {
                foreach (Chunk chunk in f.chunks)
                {
                    DrawChunkOld(chunk, bmp, Color.Black);
                }
            }


            foreach (Drill drill in drills)
            {
                DrawDrill(drill, bmp);
            }

            pBox.Image = bmp;
        }
    }
}
