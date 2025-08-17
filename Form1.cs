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
        public Form1()
        {
            InitializeComponent();
            pcbFileParser = new PcbFileParser(this);
            drawer = new Drawer(pictureBox1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = pcbFileParser.Parse("manipulator.kicad_pcb").ToString();
            //textBox1.Text = pcbFileParser.Parse("test1.kicad_pcb").ToString();
        }

        public void PrintText(string text)
        {
            richTextBox1.AppendText(text);
        }

        private void button2_Click(object sender, EventArgs e)
        {

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



            //figure.Rotate(Math.PI/4);


            figures.Add(figure);

            


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
                figure1.points.AddLast(lln);
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
                figure2.points.AddLast(lln);
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
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(15, 0));

            pts2[0] = new Point2D(11, 10);
            pts2[1] = new Point2D(15, 10);
            pts2[2] = new Point2D(16, 3);
            pts2[3] = new Point2D(11, 3);
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(15, 20));

            pts2[0] = new Point2D(11, 10);
            pts2[1] = new Point2D(15, 10);
            pts2[2] = new Point2D(14, 3);
            pts2[3] = new Point2D(11, 3);
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(15, 40));
            

            pts2[0] = new Point2D(12, 15);
            pts2[1] = new Point2D(16, 15);
            pts2[2] = new Point2D(16, 11);
            pts2[3] = new Point2D(12, 11);
            arc2[3] = new Arc();
            arc2[3].radius = 2;
            arc2[3].centre = new Point2D(14, 11);
            arc2[3].startAngle = 0;
            arc2[3].endAngle = -Math.PI;
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(30, 0));
            
            pts2[0] = new Point2D(10.5, 15);
            pts2[1] = new Point2D(14.5, 15);
            pts2[2] = new Point2D(14.5, 11);
            pts2[3] = new Point2D(10.5, 11);
            arc2[3] = new Arc();
            arc2[3].radius = 2;
            arc2[3].centre = new Point2D(12.5, 11);
            arc2[3].startAngle = 0;
            arc2[3].endAngle = -Math.PI;
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(30, 20));
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
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(30, 40));
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
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(45, 0));
            arc2[2] = null;

            /* change  form 1 to arc */

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
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(45, 20));
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
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(45, 40));
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
            AddTestForm(pts1, arc1, pts2, arc2, new Vector(60, 0));
            arc2[3] = null;

            drawer.Redraw(figures, cuts, drills);
        }
    }
}
