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
    public partial class NewDrillForm : Form
    {
        Form1 mainForm;
        Configuration configuration;
        public NewDrillForm(Form1 mainForm_, Configuration configuration_, int freeToolNumber)
        {
            mainForm = mainForm_;
            configuration = configuration_;
            InitializeComponent();
            textBox2.Text = freeToolNumber.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double diameter = 0;
            double feedRate = 0;
            double spindleSpeed = 0;
            int toolNumber = 0;

            bool dataOk = true;

            try
            {
                diameter = Double.Parse(textBox1.Text);
            }
            catch
            {
                dataOk = false;
            }

            try
            {
                toolNumber = int.Parse(textBox2.Text);

                dataOk = configuration.CheckIfToolNumberIsFree(toolNumber);
            }
            catch
            {
                dataOk = false;
            }

            try
            {
                spindleSpeed = Double.Parse(textBox3.Text);
            }
            catch
            {
                dataOk = false;
            }

            try
            {
                feedRate = Double.Parse(textBox4.Text);
            }
            catch
            {
                dataOk = false;
            }

            if(dataOk)
            {
                DrillData drill = new DrillData();
                drill.toolNumber = toolNumber;
                drill.diameter = diameter;
                drill.feedRate = feedRate;
                drill.spindleSpeed = spindleSpeed;

                configuration.AddDrill(drill);

                mainForm.RedrawDrillList();
                this.Close();
            }


        }
    }
}
