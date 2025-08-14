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

        private void AddTestForm(Point2D[] pts1, Point2D[] pts2, Vector position)
        {
            Figure figure1 = new Figure();
            Figure figure2 = new Figure();
            Line line;
            Arc arc;

            Node node;
            LinkedListNode<Node> lln;

            foreach(Point2D p in pts1)
            {
                node = new Node();
                node.pt = new Point2D(p);
                lln = new LinkedListNode<Node>(node);
                figure1.points.AddLast(lln);
            }

            foreach (Point2D p in pts2)
            {
                node = new Node();
                node.pt = new Point2D(p);
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

            pts1[0] = new Point2D(10, 10);
            pts1[1] = new Point2D(15, 10);
            pts1[2] = new Point2D(15, 4);
            pts1[3] = new Point2D(10, 4);

            pts2[0] = new Point2D(12, 15);
            pts2[1] = new Point2D(14, 15);
            pts2[2] = new Point2D(14, 8);
            pts2[3] = new Point2D(12, 8);
            AddTestForm(pts1, pts2, new Vector(0, 0));

            pts2[0] = new Point2D(12, 15);
            pts2[1] = new Point2D(18, 15);
            pts2[2] = new Point2D(18, 8);
            pts2[3] = new Point2D(12, 8);
            AddTestForm(pts1, pts2, new Vector(0, 20));

            pts2[0] = new Point2D(10, 15);
            pts2[1] = new Point2D(15, 15);
            pts2[2] = new Point2D(15, 8);
            pts2[3] = new Point2D(10, 8);
            AddTestForm(pts1, pts2, new Vector(0, 40));

            pts2[0] = new Point2D(11, 15);
            pts2[1] = new Point2D(14, 15);
            pts2[2] = new Point2D(14, 10);
            pts2[3] = new Point2D(11, 10);
            AddTestForm(pts1, pts2, new Vector(0, 60));

            pts2[0] = new Point2D(11, 10);
            pts2[1] = new Point2D(15, 10);
            pts2[2] = new Point2D(16, 3);
            pts2[3] = new Point2D(11, 3);
            AddTestForm(pts1, pts2, new Vector(20, 0));

            pts2[0] = new Point2D(11, 10);
            pts2[1] = new Point2D(15, 10);
            pts2[2] = new Point2D(14, 3);
            pts2[3] = new Point2D(11, 3);
            AddTestForm(pts1, pts2, new Vector(20, 20));




            drawer.Redraw(figures, cuts, drills);
        }
    }
}
