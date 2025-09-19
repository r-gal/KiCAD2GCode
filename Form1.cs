using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static KiCad2Gcode.PcbFileParser;

namespace KiCad2Gcode
{
    public partial class Form1 : Form
    {
        MainUnit unit;
        Drawer drawer;

        Configuration config;

        String filePath = "";

        String MainText = "KiCAD2Gcode v1.2 ";

        public Form1()
        {
            InitializeComponent();


            config = new Configuration();

            drawer = new Drawer(pictureBox1, panel1);
            unit = new MainUnit(this,drawer);
            unit.SetConfig(config);

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 2;
            comboBox3.SelectedIndex = 0;

            Config2Gui();

            this.Text = MainText;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "KiCAD PCB Files |*.kicad_pcb";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                filePath = openFileDialog.FileName;

                ReloadFile();


            }

            //filePath = "manipulator.kicad_pcb";
            //textBox1.Text = pcbFileParser.Parse("test1.kicad_pcb").ToString();
            //textBox1.Text = pcbFileParser.Parse("errorPath1.kicad_pcb").ToString();
            //textBox1.Text = pcbFileParser.Parse("error12.kicad_pcb").ToString();
            //textBox1.Text = pcbFileParser.Parse("testPath3.kicad_pcb").ToString();

            //textBox1.Text = pcbFileParser.Parse("testZone5.kicad_pcb").ToString();
            //textBox1.Text = pcbFileParser.Parse("manipulator_no_islands.kicad_pcb").ToString();

            
            
        }

        public void PrintText(string text)
        {
            richTextBox1.AppendText(text);
        }


        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int scale = int.Parse(comboBox2.Text);
                unit.SetScale(scale);
            }
            catch
            {

            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            unit.Run(comboBox1.SelectedIndex);
        }


        private void button3_Click(object sender, EventArgs e)
        {
            int freeToolNumber = config.GetFirstFreeToolNumber();
            NewDrillForm newDrillForm = new NewDrillForm(this, config,freeToolNumber);
            newDrillForm.ShowDialog();
        }

        internal void RedrawDrillList()
        {
            dataGridView1.Rows.Clear();

            foreach(DrillData drill in config.drillList)
            {
                dataGridView1.Rows.Add(drill.diameter,drill.toolNumber, drill.spindleSpeed, drill.feedRate);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if(dataGridView1.SelectedRows.Count > 0)
            {
                int toolNumber = int.Parse(dataGridView1.SelectedRows[0].Cells[1].Value.ToString());
                config.DeleteDrill(toolNumber);
                RedrawDrillList();
            }
        }

        internal void ReloadFile()
        {
            if(filePath != "")
            {
                ACTIVE_LAYER_et activeLayer;

                if(comboBox3.SelectedIndex == 0)
                {
                    activeLayer = ACTIVE_LAYER_et.TOP;
                }
                else
                {
                    activeLayer = ACTIVE_LAYER_et.BOTTOM;
                }

                bool loadResult = unit.LoadFile(filePath,activeLayer);

                if (loadResult == true)
                {
                    this.Text = MainText + filePath;
                }
            }

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            ReloadFile();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ReloadFile();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            /* open profile*/
            String filePath;
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Profile file |*.xml";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                filePath = openFileDialog.FileName;

                config.LoadFromFile(filePath);

                Config2Gui();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            /*save profile */
            String filePath;
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "Profile file |*.xml";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                filePath = saveFileDialog.FileName;

                config.SaveToFile(filePath);

            }
        }

        private void Config2Gui()
        {
            /* general */

            clearLevelTextBox.Text = config.clearLevel.ToString();
            safeLevelTextBox.Text = config.safeLevel.ToString();

            /* traces milling */
            tracesToolNumberTextBox.Text = config.traceMillToolNumber.ToString();
            tracesDiameterTextBox.Text = config.traceMillDiameter.ToString();
            tracesSpindleSpeedTextBox.Text = config.traceMillSpindleSpeed.ToString();
            tracesHFeedRateTextBox.Text = config.traceMillHFeedRate.ToString();
            tracesVFeedRateTextBox.Text = config.traceMillVFeedRate.ToString();
            tracesMillLevelTextBox.Text = config.traceMillLevel.ToString();
            millTracesCheckBox.Checked = config.traceActive;


            /* board milling */
            boardToolNumberTextBox.Text =  config.boardMillToolNumber.ToString();
            boardDiameterTextBox.Text = config.boardMillDiameter.ToString();
            boardSpindleSpeedTextBox.Text = config.boardMillSpindleSpeed.ToString();
            boardHFeedRateTextBox.Text = config.boardMillHFeedRate.ToString();
            boardVFeedRateTextBox.Text = config.boardMillVFeedRate.ToString();
            boardMillLevelTextBox.Text = config.boardMillLevel.ToString();
            boardVStepTextBox.Text = config.boardVStep.ToString();
            boardBorderCheckBox.Checked = config.boardBorderActive;
            boardHolesCheckBox.Checked = config.boardHolesActive;
            boardDrillsCheckBox.Checked = config.boardDrillsActive;

            /* drilling */
                        
            drillDrillLevelTextBox.Text = config.drillLevel.ToString();
            drillGenerateCheckBox.Checked = config.drillAcive;
            RedrawDrillList();
        }


        

        private void clearLevelTextBox_TextChanged(object sender, EventArgs e)
        {
            bool ok = false;
            try
            {
                double clearLevel = double.Parse(clearLevelTextBox.Text);
                if(clearLevel >= config.safeLevel)
                {
                    config.clearLevel = clearLevel;
                    ok = true;
                }
            }
            catch
            {

            }

            if(ok == false )
            {
                clearLevelTextBox.Text = config.clearLevel.ToString();
            }
        }

        private void safeLevelTextBox_TextChanged(object sender, EventArgs e)
        {
            bool ok = false;
            try
            {
                double safeLevel = double.Parse(safeLevelTextBox.Text);
                if (safeLevel <= config.clearLevel && safeLevel>= config.drillLevel && safeLevel >= config.traceMillLevel && safeLevel >= config.boardMillLevel)
                {
                    config.safeLevel = safeLevel;
                    ok = true;
                }
            }
            catch
            {

            }

            if (ok == false)
            {
                safeLevelTextBox.Text = config.safeLevel.ToString();
            }
        }

        private void m3DwelTextBox_TextChanged(object sender, EventArgs e)
        {
            bool ok = false;
            try
            {
                double m3dwel = double.Parse(m3DwelTextBox.Text);
                if (m3dwel >= 0)
                {
                    config.m3dwel = m3dwel;
                    ok = true;
                }
            }
            catch
            {

            }

            if (ok == false)
            {
                m3DwelTextBox.Text = config.m3dwel.ToString();
            }
        }

        private void millTracesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            config.traceActive = millTracesCheckBox.Checked;
        }

        private void tracesToolNumberTextBox_TextChanged(object sender, EventArgs e)
        {
            bool ok = false;
            try
            {
                int toolNumber = int.Parse(tracesToolNumberTextBox.Text);
                if (toolNumber > 0 && config.boardMillToolNumber != toolNumber && config.CheckIfToolNumberIsNotUsedByDrills(toolNumber))
                {
                    config.traceMillToolNumber = toolNumber;
                    ok = true;
                }
            }
            catch
            {

            }

            if (ok == false)
            {
                tracesToolNumberTextBox.Text = config.traceMillToolNumber.ToString();
            }
        }


        private void tracesDiameterTextBox_TextChanged(object sender, EventArgs e)
        {
            bool ok = false;
            try
            {
                double diameter = double.Parse(tracesDiameterTextBox.Text);
                if (diameter >0)
                {
                    config.traceMillDiameter = diameter;
                    ok = true;
                }
            }
            catch
            {

            }

            if (ok == false)
            {
                tracesDiameterTextBox.Text = config.traceMillDiameter.ToString();
            }
        }

        private void tracesSpindleSpeedTextBox_TextChanged(object sender, EventArgs e)
        {
            bool ok = false;
            try
            {
                double spindleSpeed = double.Parse(tracesSpindleSpeedTextBox.Text);
                if (spindleSpeed > 0)
                {
                    config.traceMillSpindleSpeed = spindleSpeed;
                    ok = true;
                }
            }
            catch
            {

            }

            if (ok == false)
            {
                tracesSpindleSpeedTextBox.Text = config.traceMillSpindleSpeed.ToString();
            }
        }

        private void tracesMillLevelTextBox_TextChanged(object sender, EventArgs e)
        {
            bool ok = false;
            try
            {
                double millLevel = double.Parse(tracesMillLevelTextBox.Text);
                if (millLevel < config.safeLevel)
                {
                    config.traceMillLevel = millLevel;
                    ok = true;
                }
            }
            catch
            {

            }

            if (ok == false)
            {
                tracesMillLevelTextBox.Text = config.traceMillLevel.ToString();
            }
        }

        private void tracesHFeedRateTextBox_TextChanged(object sender, EventArgs e)
        {
            bool ok = false;
            try
            {
                double feedRate = double.Parse(tracesHFeedRateTextBox.Text);
                if (feedRate > 0)
                {
                    config.traceMillHFeedRate = feedRate;
                    ok = true;
                }
            }
            catch
            {

            }

            if (ok == false)
            {
                tracesHFeedRateTextBox.Text = config.traceMillHFeedRate.ToString();
            }
        }

        private void tracesVFeedRateTextBox_TextChanged(object sender, EventArgs e)
        {
            bool ok = false;
            try
            {
                double feedRate = double.Parse(tracesVFeedRateTextBox.Text);
                if (feedRate > 0)
                {
                    config.traceMillVFeedRate = feedRate;
                    ok = true;
                }
            }
            catch
            {

            }

            if (ok == false)
            {
                tracesVFeedRateTextBox.Text = config.traceMillVFeedRate.ToString();
            }
        }

        private void boardBorderCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            config.boardBorderActive = boardBorderCheckBox.Checked;
        }

        private void boardHolesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            config.boardHolesActive = boardHolesCheckBox.Checked;
        }

        private void boardDrillsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            config.boardDrillsActive = boardDrillsCheckBox.Checked;
        }
        private void boardToolNumberTextBox_TextChanged(object sender, EventArgs e)
        {
            bool ok = false;
            try
            {
                int toolNumber = int.Parse(boardToolNumberTextBox.Text);
                if (toolNumber > 0 && config.traceMillToolNumber != toolNumber && config.CheckIfToolNumberIsNotUsedByDrills(toolNumber))
                {
                    config.boardMillToolNumber = toolNumber;
                    ok = true;
                }
            }
            catch
            {

            }

            if (ok == false)
            {
                boardToolNumberTextBox.Text = config.boardMillToolNumber.ToString();
            }
        }

        private void boardDiameterTextBox_TextChanged(object sender, EventArgs e)
        {
            bool ok = false;
            try
            {
                double diameter = double.Parse(boardDiameterTextBox.Text);
                if (diameter > 0)
                {
                    config.boardMillDiameter = diameter;
                    ok = true;
                }
            }
            catch
            {

            }

            if (ok == false)
            {
                boardDiameterTextBox.Text = config.boardMillDiameter.ToString();
            }
        }

        private void boardMillLevelTextBox_TextChanged(object sender, EventArgs e)
        {
            bool ok = false;
            try
            {
                double millLevel = double.Parse(boardMillLevelTextBox.Text);
                if (millLevel < config.safeLevel)
                {
                    config.boardMillLevel = millLevel;
                    ok = true;
                }
            }
            catch
            {

            }

            if (ok == false)
            {
                boardMillLevelTextBox.Text = config.boardMillLevel.ToString();
            }
        }

        private void boardSpindleSpeedTextBox_TextChanged(object sender, EventArgs e)
        {
            bool ok = false;
            try
            {
                double spindleSpeed = double.Parse(boardSpindleSpeedTextBox.Text);
                if (spindleSpeed > 0)
                {
                    config.boardMillSpindleSpeed = spindleSpeed;
                    ok = true;
                }
            }
            catch
            {

            }

            if (ok == false)
            {
                boardSpindleSpeedTextBox.Text = config.boardMillSpindleSpeed.ToString();
            }
        }

        private void boardHFeedRateTextBox_TextChanged(object sender, EventArgs e)
        {
            bool ok = false;
            try
            {
                double feedRate = double.Parse(boardHFeedRateTextBox.Text);
                if (feedRate > 0)
                {
                    config.boardMillHFeedRate = feedRate;
                    ok = true;
                }
            }
            catch
            {

            }

            if (ok == false)
            {
                boardHFeedRateTextBox.Text = config.boardMillHFeedRate.ToString();
            }
        }

        private void boardVFeedRateTextBox_TextChanged(object sender, EventArgs e)
        {
            bool ok = false;
            try
            {
                double feedRate = double.Parse(boardVFeedRateTextBox.Text);
                if (feedRate > 0)
                {
                    config.boardMillVFeedRate = feedRate;
                    ok = true;
                }
            }
            catch
            {

            }

            if (ok == false)
            {
                boardVFeedRateTextBox.Text = config.boardMillVFeedRate.ToString();
            }
        }

        private void boardVStepTextBox_TextChanged(object sender, EventArgs e)
        {
            bool ok = false;
            try
            {
                double step = double.Parse(boardVStepTextBox.Text);
                if (step > 0)
                {
                    config.boardVStep = step;
                    ok = true;
                }
            }
            catch
            {

            }

            if (ok == false)
            {
                boardVStepTextBox.Text = config.boardVStep.ToString();
            }
        }

        private void drillGenerateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            config.drillAcive = drillGenerateCheckBox.Checked;
        }

        private void drillDrillLevelTextBox_TextChanged(object sender, EventArgs e)
        {
            bool ok = false;
            try
            {
                double millLevel = double.Parse(drillDrillLevelTextBox.Text);
                if (millLevel < config.safeLevel)
                {
                    config.drillLevel = millLevel;
                    ok = true;
                }
            }
            catch
            {

            }

            if (ok == false)
            {
                drillDrillLevelTextBox.Text = config.drillLevel.ToString();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            unit.TestStep();
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            //unit.Run(10);
            unit.TestStep();
        }
    }
}
