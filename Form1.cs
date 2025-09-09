using System;
using System.Collections;
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
        MainUnit unit;
        Drawer drawer;




        public Form1()
        {
            InitializeComponent();

            drawer = new Drawer(pictureBox1, panel1);
            unit = new MainUnit(this,drawer);


        }

        private void button1_Click(object sender, EventArgs e)
        {
            String filePath = "manipulator.kicad_pcb";
            //textBox1.Text = pcbFileParser.Parse("test1.kicad_pcb").ToString();
            //textBox1.Text = pcbFileParser.Parse("errorPath1.kicad_pcb").ToString();
            //textBox1.Text = pcbFileParser.Parse("error12.kicad_pcb").ToString();
            //textBox1.Text = pcbFileParser.Parse("testPath3.kicad_pcb").ToString();

            //textBox1.Text = pcbFileParser.Parse("testZone5.kicad_pcb").ToString();
            //textBox1.Text = pcbFileParser.Parse("manipulator_no_islands.kicad_pcb").ToString();

            unit.LoadFile(filePath);
            
        }

        

        private void button6_Click(object sender, EventArgs e)
        {
            unit.MergePolygons();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            unit.MergePolygonsToZones();
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //Step(0);
            //RedrawAll();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            unit.MergeZones();
            
        }

        private void button13_Click(object sender, EventArgs e)
        {
            //Step(2);
            //RedrawAll();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            //Step(1);
            //RedrawAll();
        }

        public void PrintText(string text)
        {
            richTextBox1.AppendText(text);
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                int scale = int.Parse(textBox2.Text);
                unit.SetScale(scale);
            }
            catch
            {

            }            
        }

        private void button8_Click(object sender, EventArgs e)
        {           
            
        }

        private void button12_Click(object sender, EventArgs e)
        {

        }

        private void button14_Click(object sender, EventArgs e)
        {

            unit.ProceedTracesMilling();
            unit.ProceedBoardMilling();
            
        }
    }
}
