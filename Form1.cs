using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KiCad2Gcode
{
    public partial class Form1 : Form
    {
        PcbFileParser pcbFileParser;
        Drawer drawer;

        List<Figure> figures = new List<Figure>();
        List<Figure> cuts = new List<Figure>();
        List<Drill> drills = new List<Drill>();

        int idxA = 0;
        int idxB = 1;


        public Form1()
        {
            InitializeComponent();
            pcbFileParser = new PcbFileParser(this);
            drawer = new Drawer(pictureBox1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //textBox1.Text = pcbFileParser.Parse("manipulator.kicad_pcb").ToString();
            textBox1.Text = pcbFileParser.Parse("test1.kicad_pcb").ToString();






            idxA = 0;
            idxB = 1;
        }

        private bool Step()
        {
            /*step button */
            bool result;
            bool merged = false;

            foreach (Figure f in figures)
            {
                f.selected = false;
            }
            if (idxA < figures.Count && idxB < figures.Count && idxA != idxB)
            {
                PrintText("Try " + idxA.ToString() + " vs " + idxB.ToString() + "size = " + figures.Count
                    .ToString() + "\n");
                if (idxA == 11 && idxB == 10)
                {
                    PrintText("trap\n");
                }
                merged = false;

                Merger m = new Merger(this);
                Figure mergedFigure = m.Merge(figures[idxA], figures[idxB]);

                if (mergedFigure != null)
                {
                    figures[idxA] = mergedFigure;
                    figures[idxA].selected = true;
                    figures.RemoveAt(idxB);
                    PrintText("foud\n");

                    
                    merged = true;
                }
                else
                {
                    figures[idxA].selected = true;
                    figures[idxB].selected = true;

                    PrintText("skip\n");
                    idxB++;
                    if (idxB == idxA) { idxB++; }
                }



                if (idxB >= figures.Count)
                {
                    idxA++;
                    if(merged == true)
                    {
                        idxB = 0;
                    }
                    else
                    {
                        idxB = idxA + 1;
                    }
                    idxB = 0;


                }


                result = true;
            }
            else
            {
                PrintText("Nothing to do ! \n");
                result = false;
            }

            drawer.Redraw(figures, cuts, drills);
            return result;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            bool res = false;

            idxA = 0;
            idxB = 1;

            do
            {
                res = Step();
            } while (res == true);
        }

        private void button5_Click(object sender, EventArgs e)
        {

            Step();
        }

        public void PrintText(string text)
        {
            richTextBox1.AppendText(text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            /*
            Figure figure = new Figure();
            Line line;
            Arc arc;

            line = new Line();
            line.start = new Point2D(3, 5);
            line.end = new Point2D(7, 5);
            figure.chunks.Add(line);
            line = new Line();
            line.start = new Point2D(5, 3);
            line.end = new Point2D(5, 7);
            figure.chunks.Add(line);

            arc = new Arc();            
            arc.radius = 5;
            arc.startAngle = Math.PI;
            arc.endAngle = - Math.PI;
            arc.start = new Point2D(0, 0);
            arc.end = new Point2D( 0,0);
            arc.centre = new Point2D(5, 5);
            figure.chunks.Add(arc);

            */

            //figure.Rotate(Math.PI/4);
            /*

            figures.Add(figure);*/

            


            drawer.Redraw(figures,cuts,drills);


        }

        public void AddFigure(Figure f)
        {
            figures.Add(f);
        }

        public void AddCuts(Figure f)
        {
            cuts.Add(f);
        }

        public void AddDrill(Drill drill)
        {
            drills.Add(drill);
        }

        private void AddTestForm(Point2D[] pts1, Arc[] arc1, Point2D[] pts2, Arc[] arc2, Vector position)
        {
            Figure figure1 = new Figure();
            Figure figure2 = new Figure();
            Line line;
            Arc arc;

            Node node;
            LinkedListNode<Node> lln;

            for(int i=0; i< pts1.Length;i++) 
            {
                node = new Node();
                node.pt = new Point2D(pts1[i]);
                if(arc1[i] != null)
                {
                    node.arc = new Arc();
                    node.arc.centre = new Point2D(arc1[i].centre);
                    node.arc.startAngle = arc1[i].startAngle;
                    node.arc.endAngle = arc1[i].endAngle;
                    node.arc.radius = arc1[i].radius;
                }
                
                lln = new LinkedListNode<Node>(node);
                figure1.shape.points.AddLast(lln);
            }

            for (int i = 0; i < pts2.Length; i++)
            {
                node = new Node();
                node.pt = new Point2D(pts2[i]);
                if (arc2[i] != null)
                {
                    node.arc = new Arc();
                    node.arc.centre = new Point2D(arc2[i].centre);
                    node.arc.startAngle = arc2[i].startAngle;
                    node.arc.endAngle = arc2[i].endAngle;
                    node.arc.radius = arc2[i].radius;
                }
                lln = new LinkedListNode<Node>(node);
                figure2.shape.points.AddLast(lln);
            }

            figure1.Move(position); 
            figure2.Move(position);

            Merger m = new Merger(this);
            Figure mergedFigure = m.Merge(figure1, figure2);

            figures.Add(figure1);
            figures.Add(figure2);
            cuts.Add(mergedFigure);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Point2D[] pts1 = new Point2D[4];
            Point2D[] pts2 = new Point2D[4];

            Arc[] arc1 = new Arc[4];
            Arc[] arc2 = new Arc[4];

            for(int i=0;i<4;i++)
            {
                arc1[i] = null;
                arc2[i] = null;
            }


            pts1[0] = new Point2D(10, 10);
            pts1[1] = new Point2D(15, 10);
            pts1[2] = new Point2D(15, 4);
            pts1[3] = new Point2D(10, 4);
           
            pts2[0] = new Point2D(12, 15);
            pts2[1] = new Point2D(14, 15);
            pts2[2] = new Point2D(14, 8);
            pts2[3] = new Point2D(12, 8);
            AddTestForm(pts1, arc1,  pts2, arc2, new Vector(0, 0));
            
            pts2[0] = new Point2D(12, 15);
            pts2[1] = new Point2D(18, 15);
            pts2[2] = new Point2D(18, 8);
            pts2[3] = new Point2D(12, 8);
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(0, 20));

            pts2[0] = new Point2D(10, 15);
            pts2[1] = new Point2D(15, 15);
            pts2[2] = new Point2D(15, 8);
            pts2[3] = new Point2D(10, 8);
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(0, 40));

            pts2[0] = new Point2D(11, 15);
            pts2[1] = new Point2D(14, 15);
            pts2[2] = new Point2D(14, 10);
            pts2[3] = new Point2D(11, 10);
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(10, 0));
            
            pts2[0] = new Point2D(11, 10);
            pts2[1] = new Point2D(15, 10);
            pts2[2] = new Point2D(16, 3);
            pts2[3] = new Point2D(11, 3);
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(10, 20));

            pts2[0] = new Point2D(11, 10);
            pts2[1] = new Point2D(15, 10);
            pts2[2] = new Point2D(14, 3);
            pts2[3] = new Point2D(11, 3);
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(10, 40));
            
            pts2[0] = new Point2D(12, 10);
            pts2[1] = new Point2D(15, 6);
            pts2[2] = new Point2D(12, 4);
            pts2[3] = new Point2D(10, 6);
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(20, 0));
            //AddTestForm(pts1, arc1, pts2, arc2, new Vector(0, 0));
            
            pts2[0] = new Point2D(10, 10);
            pts2[1] = new Point2D(15, 10);
            pts2[2] = new Point2D(15, 4);
            pts2[3] = new Point2D(10, 4);
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(20, 20));
            

            pts2[0] = new Point2D(12, 15);
            pts2[1] = new Point2D(16, 15);
            pts2[2] = new Point2D(16, 11);
            pts2[3] = new Point2D(12, 11);
            arc2[3] = new Arc();
            arc2[3].radius = 2;
            arc2[3].centre = new Point2D(14, 11);
            arc2[3].startAngle = 0;
            arc2[3].endAngle = -Math.PI;
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(20, 40));
            
            pts2[0] = new Point2D(10.5, 15);
            pts2[1] = new Point2D(14.5, 15);
            pts2[2] = new Point2D(14.5, 11);
            pts2[3] = new Point2D(10.5, 11);
            arc2[3] = new Arc();
            arc2[3].radius = 2;
            arc2[3].centre = new Point2D(12.5, 11);
            arc2[3].startAngle = 0;
            arc2[3].endAngle = -Math.PI;
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(30, 00));
            arc2[3] = null;
            
            pts2[0] = new Point2D(12, 2);
            pts2[1] = new Point2D(10, 8);
            pts2[2] = new Point2D(14, 12);
            pts2[3] = new Point2D(15, 7);
            arc2[2] = new Arc();
            arc2[2].radius = 2 * Math.Sqrt(2);
            arc2[2].centre = new Point2D(12, 10);
            arc2[2].startAngle = -0.75 * Math.PI;
            arc2[2].endAngle = 0.25 * Math.PI;
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(30, 20));
            //AddTestForm(pts1, arc1, pts2, arc2, new Vector(0,0));
            arc2[2] = null;
            
            pts2[0] = new Point2D(12, 6);
            pts2[1] = new Point2D(12, 8);
            pts2[2] = new Point2D(16, 8);
            pts2[3] = new Point2D(16, 6);
            arc2[2] = new Arc();
            arc2[2].radius = 2 ;
            arc2[2].centre = new Point2D(14, 8);
            arc2[2].startAngle = - Math.PI;
            arc2[2].endAngle = 0;
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(30, 40));
            arc2[2] = null;

            Point2D[] pts2tmp = new Point2D[1];
            pts2tmp[0] = new Point2D(10 , 11);
            arc2[0] = new Arc();
            arc2[0].radius = 2.5;
            arc2[0].centre = new Point2D(12.5, 11);
            arc2[0].startAngle = Math.PI;
            arc2[0].endAngle = -Math.PI;
            AddTestForm(pts1, arc1, pts2tmp, arc2, new Vector(40, 0));
            //AddTestForm(pts1, arc1, pts2tmp, arc2, new Vector(0, 0));
            arc2[0] = null;

            pts2tmp[0] = new Point2D(9, 10);
            arc2[0] = new Arc();
            arc2[0].radius = 3.5;
            arc2[0].centre = new Point2D(12.5, 10);
            arc2[0].startAngle = Math.PI;
            arc2[0].endAngle = -Math.PI;
            AddTestForm(pts1, arc1, pts2tmp, arc2, new Vector(40, 20));
            //AddTestForm(pts1, arc1, pts2tmp, arc2, new Vector(0, 0));
            arc2[0] = null;

            // change  form 1 to arc 

            arc1[1] = new Arc();
            arc1[1].radius = 2.5;
            arc1[1].startAngle = - Math.PI;
            arc1[1].endAngle = 0;
            arc1[1].centre = new Point2D(12.5, 10);
            
            
            pts2[0] = new Point2D(10, 14);
            pts2[1] = new Point2D(15, 14);
            pts2[2] = new Point2D(15, 13);
            pts2[3] = new Point2D(10, 13);
            arc2[3] = new Arc();
            arc2[3].radius = 2.5;
            arc2[3].centre = new Point2D(12.5, 13);
            arc2[3].startAngle = 0;
            arc2[3].endAngle = Math.PI;
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(40, 40));
            //AddTestForm(pts1, arc1, pts2, arc2, new Vector(0, 0));
            arc2[3] = null;
            
            pts2[0] = new Point2D(8, 7.5);
            pts2[1] = new Point2D(8, 12.5);
            pts2[2] = new Point2D(12.5, 12.5);
            pts2[3] = new Point2D(12.5, 7.5);
            arc2[3] = new Arc();
            arc2[3].radius = 2.5;
            arc2[3].centre = new Point2D(12.5, 10);
            arc2[3].startAngle = Math.PI/2;
            arc2[3].endAngle = -Math.PI/2;
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(50, 00));
            arc2[3] = null;
            
            pts2[0] = new Point2D(9, 5);
            pts2[1] = new Point2D(9, 9);
            pts2[2] = new Point2D(16, 9);
            pts2[3] = new Point2D(16, 6);
            arc2[2] = new Arc();
            arc2[2].radius = 3.5;
            arc2[2].centre = new Point2D(12.5, 9);
            arc2[2].startAngle = -Math.PI;
            arc2[2].endAngle = 0;
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(50, 20));
            arc2[3] = null;

            

            drawer.Redraw(figures, cuts, drills);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Point2D[] pts1 = new Point2D[10];
            Point2D[] ptsh1 = new Point2D[4];
            Point2D[] ptsh1a = new Point2D[4];
            Point2D[] pts2 = new Point2D[6];
            Point2D[] ptsh2 = new Point2D[4];

            Arc[] arc1 = new Arc[4];
            Arc[] arc2 = new Arc[4];

            for (int i = 0; i < 4; i++)
            {
                arc1[i] = null;
                arc2[i] = null;
            }


           
            pts1[0] = new Point2D(5, 20);
            pts1[1] = new Point2D(10, 20);
            pts1[2] = new Point2D(10, 16);
            pts1[3] = new Point2D(21, 16);
            pts1[4] = new Point2D(21, 14);
            pts1[5] = new Point2D(10, 14);
            pts1[6] = new Point2D(10, 10);
            pts1[7] = new Point2D(20, 10);
            pts1[8] = new Point2D(20, 5);
            pts1[9] = new Point2D(5, 5);

            ptsh1[0] = new Point2D(11, 6);
            ptsh1[1] = new Point2D(18, 6);
            ptsh1[2] = new Point2D(18, 8);
            ptsh1[3] = new Point2D(11, 8);

            ptsh1a[0] = new Point2D(6, 6);
            ptsh1a[1] = new Point2D(9, 6);
            ptsh1a[2] = new Point2D(9, 8);
            ptsh1a[3] = new Point2D(6, 8);

            Figure figure1 = new Figure();
            Figure figure2 = new Figure();
            Line line;
            Arc arc;

            Node node;
            LinkedListNode<Node> lln;

            for (int i = 0; i < pts1.Length; i++)
            {
                node = new Node();
                node.pt = new Point2D(pts1[i]);
                /*if (arc1[i] != null)
                {
                    node.arc = new Arc();
                    node.arc.centre = new Point2D(arc1[i].centre);
                    node.arc.startAngle = arc1[i].startAngle;
                    node.arc.endAngle = arc1[i].endAngle;
                    node.arc.radius = arc1[i].radius;
                }*/

                lln = new LinkedListNode<Node>(node);
                figure1.shape.points.AddLast(lln);
            }

            

            Polygon hole = new Polygon();
            for (int i = 0; i < ptsh1.Length; i++)
            {
                node = new Node();
                node.pt = new Point2D(ptsh1[i]);
                /*if (arc1[i] != null)
                {
                    node.arc = new Arc();
                    node.arc.centre = new Point2D(arc1[i].centre);
                    node.arc.startAngle = arc1[i].startAngle;
                    node.arc.endAngle = arc1[i].endAngle;
                    node.arc.radius = arc1[i].radius;
                }*/

                lln = new LinkedListNode<Node>(node);
                hole.points.AddLast(lln);
            }
            figure1.holes.Add(hole);

            hole = new Polygon();
            for (int i = 0; i < ptsh1a.Length; i++)
            {
                node = new Node();
                node.pt = new Point2D(ptsh1a[i]);
                /*if (arc1[i] != null)
                {
                    node.arc = new Arc();
                    node.arc.centre = new Point2D(arc1[i].centre);
                    node.arc.startAngle = arc1[i].startAngle;
                    node.arc.endAngle = arc1[i].endAngle;
                    node.arc.radius = arc1[i].radius;
                }*/

                lln = new LinkedListNode<Node>(node);
                hole.points.AddLast(lln);
            }
            figure1.holes.Add(hole);

            pts2[0] = new Point2D(2, 25);
            pts2[1] = new Point2D(22, 25);
            pts2[2] = new Point2D(22, 3);
            pts2[3] = new Point2D(14, 3);
            pts2[4] = new Point2D(14, 20);
            pts2[5] = new Point2D(2, 20);

            ptsh2[0] = new Point2D(16, 18);
            ptsh2[1] = new Point2D(16, 7);
            ptsh2[2] = new Point2D(19, 7);
            ptsh2[3] = new Point2D(19, 18);

            figure2.holes.Add(new Polygon());

            for (int i = 0; i < pts2.Length; i++)
            {
                node = new Node();
                node.pt = new Point2D(pts2[i]);
               /* if (arc1[i] != null)
                {
                    node.arc = new Arc();
                    node.arc.centre = new Point2D(arc1[i].centre);
                    node.arc.startAngle = arc1[i].startAngle;
                    node.arc.endAngle = arc1[i].endAngle;
                    node.arc.radius = arc1[i].radius;
                }*/

                lln = new LinkedListNode<Node>(node);
                figure2.shape.points.AddLast(lln);
            }

            for (int i = 0; i < ptsh2.Length; i++)
            {
                node = new Node();
                node.pt = new Point2D(ptsh2[i]);
                /*if (arc1[i] != null)
                {
                    node.arc = new Arc();
                    node.arc.centre = new Point2D(arc1[i].centre);
                    node.arc.startAngle = arc1[i].startAngle;
                    node.arc.endAngle = arc1[i].endAngle;
                    node.arc.radius = arc1[i].radius;
                }*/

                lln = new LinkedListNode<Node>(node);
                figure2.holes[0].points.AddLast(lln);
            }

            figure1.Move(new Vector(0,0));
            figure2.Move(new Vector(0, 0));

            Merger m = new Merger(this);
            Figure mergedFigure = m.Merge(figure1, figure2);

            figures.Add(figure1);
            figures.Add(figure2);
            cuts.Add(mergedFigure);



            /*
            pts2[0] = new Point2D(12, 15);
            pts2[1] = new Point2D(14, 15);
            pts2[2] = new Point2D(14, 8);
            pts2[3] = new Point2D(12, 8);
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(0, 0));*/



            drawer.Redraw(figures, cuts, drills);
        }


    }
}
