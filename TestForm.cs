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
    public partial class TestForm : Form
    {

        Drawer drawer;

        public TestForm()
        {
            InitializeComponent();

            drawer = new Drawer(pictureBox1, panel1);
            comboBox1.SelectedIndex = 0;
        }



        private void Redraw()
        {
            drawer.InitBitmap(500, 500);

            Point2D sPt = new Point2D((double)numericUpDown_sPtX.Value, (double)numericUpDown_sPtY.Value);
            Point2D ePt = new Point2D((double)numericUpDown_ePtX.Value, (double)numericUpDown_ePtY.Value);

            Arc arc = null;

            if(comboBox1.SelectedIndex > 0 )
            {
                arc = new Arc();

                if (comboBox1.SelectedIndex == 1 )
                {
                    arc.ccw = false;
                }
                else
                {
                    arc.ccw = true;
                }

                Vector va = ePt - sPt;

                double a = va.Length / 2;

                va.Normalize();

                Vector vh = va.GetOrtogonal(arc.ccw);



                double h = (double)numericUpDown_radius.Value;

                double r = Math.Sqrt(h * h + a * a);
                arc.radius = r;


                /*double r = (double)numericUpDown_radius.Value;
                
                arc.radius = r;




                if(a == 0)
                {
                    return;
                }

                double h2 = r * r - a * a;

                if( h2 < 0)
                {
                    return;
                }

                double h = Math.Sqrt(h2);

                */

                Point2D cPt = new Point2D(sPt);

                cPt += va * a;
                cPt += vh * h;

                arc.centre = cPt;

                arc.startAngle = Math.Atan2(sPt.y - cPt.y, sPt.x - cPt.x);
                arc.endAngle = Math.Atan2(ePt.y - cPt.y, ePt.x - cPt.x);

            }
            drawer.DrawElement(sPt, ePt, arc);
            drawer.DrawDot(sPt, 2, Color.Green);
            drawer.DrawDot(ePt, 2, Color.Red);


            /* flat cross test */

            Point2D pt = new Point2D(250, 250);
            Point2D ptTmp = new Point2D(0, 250);

            drawer.DrawElement(pt, ptTmp, null);
            drawer.DrawDot(pt, 2, Color.Black);

            LinkedList<Node> list = new LinkedList<Node>();

            Node n1 = new Node();
            n1.pt = sPt;
            list.AddLast(n1);
            Node n2 = new Node();
            n2.pt = ePt;
            n2.arc = arc;
            list.AddLast(n2);

            LinkedListNode <Node> testNode  = list.Last;

            CrossUnit crossUnit = new CrossUnit();
            CrossUnit.CROSS_TYPE_et cType = crossUnit.CheckFlatCross(pt, testNode);
            textBox1.Text = cType.ToString();

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Redraw();
        }

        private void numericUpDown_sPtY_ValueChanged(object sender, EventArgs e)
        {
            Redraw();
        }

        private void numericUpDown_ePtX_ValueChanged(object sender, EventArgs e)
        {
            Redraw();
        }

        private void numericUpDown_ePtY_ValueChanged(object sender, EventArgs e)
        {
            Redraw();
        }

        private void numericUpDown_cPtX_ValueChanged(object sender, EventArgs e)
        {
            Redraw();
        }

        private void numericUpDown_cPtY_ValueChanged(object sender, EventArgs e)
        {
            Redraw();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Redraw();
        }
    }
}
