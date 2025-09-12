namespace KiCad2Gcode
{
    partial class Form1
    {
        /// <summary>
        /// Wymagana zmienna projektanta.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Wyczyść wszystkie używane zasoby.
        /// </summary>
        /// <param name="disposing">prawda, jeżeli zarządzane zasoby powinny zostać zlikwidowane; Fałsz w przeciwnym wypadku.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kod generowany przez Projektanta formularzy systemu Windows

        /// <summary>
        /// Metoda wymagana do obsługi projektanta — nie należy modyfikować
        /// jej zawartości w edytorze kodu.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.safeLevelTextBox = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.clearLevelTextBox = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label17 = new System.Windows.Forms.Label();
            this.tracesToolNumberTextBox = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.tracesMillLevelTextBox = new System.Windows.Forms.TextBox();
            this.millTracesCheckBox = new System.Windows.Forms.CheckBox();
            this.tracesVFeedRateTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tracesDiameterTextBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.tracesHFeedRateTextBox = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.tracesSpindleSpeedTextBox = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label16 = new System.Windows.Forms.Label();
            this.boardToolNumberTextBox = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.boardVStepTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.boardVFeedRateTextBox = new System.Windows.Forms.TextBox();
            this.boardHFeedRateTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.boardMillLevelTextBox = new System.Windows.Forms.TextBox();
            this.boardSpindleSpeedTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.boardDiameterTextBox = new System.Windows.Forms.TextBox();
            this.boardDrillsCheckBox = new System.Windows.Forms.CheckBox();
            this.boardHolesCheckBox = new System.Windows.Forms.CheckBox();
            this.boardBorderCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.drillDrillLevelTextBox = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.drillGenerateCheckBox = new System.Windows.Forms.CheckBox();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.button8 = new System.Windows.Forms.Button();
            this.comboBox3 = new System.Windows.Forms.ComboBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.m3DwelTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(198, 46);
            this.button1.TabIndex = 0;
            this.button1.Text = "Open PCB File";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.HideSelection = false;
            this.richTextBox1.Location = new System.Drawing.Point(12, 948);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(640, 252);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(1673, 1005);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(428, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 20);
            this.label1.TabIndex = 13;
            this.label1.Text = "Zoom";
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Location = new System.Drawing.Point(668, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1761, 1200);
            this.panel1.TabIndex = 20;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "ALL",
            "MERGE POLYGONS",
            "JOIN ZONES",
            "MERGE ZONES",
            "TRACE PATH MILLING",
            "TRACE BOARD MILLING",
            "PROCEED HOLES",
            "GENERATE G-CODE"});
            this.comboBox1.Location = new System.Drawing.Point(12, 82);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(150, 28);
            this.comboBox1.TabIndex = 24;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(168, 72);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(150, 46);
            this.button2.TabIndex = 25;
            this.button2.Text = "RUN";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label19);
            this.groupBox1.Controls.Add(this.m3DwelTextBox);
            this.groupBox1.Controls.Add(this.safeLevelTextBox);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.clearLevelTextBox);
            this.groupBox1.Location = new System.Drawing.Point(12, 145);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(640, 119);
            this.groupBox1.TabIndex = 26;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "General";
            // 
            // safeLevelTextBox
            // 
            this.safeLevelTextBox.Location = new System.Drawing.Point(136, 78);
            this.safeLevelTextBox.Name = "safeLevelTextBox";
            this.safeLevelTextBox.Size = new System.Drawing.Size(100, 26);
            this.safeLevelTextBox.TabIndex = 31;
            this.safeLevelTextBox.Text = "1";
            this.safeLevelTextBox.TextChanged += new System.EventHandler(this.safeLevelTextBox_TextChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(14, 52);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(81, 20);
            this.label12.TabIndex = 29;
            this.label12.Text = "Clear level";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(14, 81);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(78, 20);
            this.label7.TabIndex = 30;
            this.label7.Text = "Safe level";
            // 
            // clearLevelTextBox
            // 
            this.clearLevelTextBox.Location = new System.Drawing.Point(136, 49);
            this.clearLevelTextBox.Name = "clearLevelTextBox";
            this.clearLevelTextBox.Size = new System.Drawing.Size(100, 26);
            this.clearLevelTextBox.TabIndex = 28;
            this.clearLevelTextBox.Text = "20";
            this.clearLevelTextBox.TextChanged += new System.EventHandler(this.clearLevelTextBox_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label17);
            this.groupBox2.Controls.Add(this.tracesToolNumberTextBox);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.tracesMillLevelTextBox);
            this.groupBox2.Controls.Add(this.millTracesCheckBox);
            this.groupBox2.Controls.Add(this.tracesVFeedRateTextBox);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.tracesDiameterTextBox);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.tracesHFeedRateTextBox);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.tracesSpindleSpeedTextBox);
            this.groupBox2.Location = new System.Drawing.Point(12, 270);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(640, 168);
            this.groupBox2.TabIndex = 27;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Traces milling";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(14, 68);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(97, 20);
            this.label17.TabIndex = 38;
            this.label17.Text = "Tool number";
            // 
            // tracesToolNumberTextBox
            // 
            this.tracesToolNumberTextBox.Location = new System.Drawing.Point(136, 65);
            this.tracesToolNumberTextBox.Name = "tracesToolNumberTextBox";
            this.tracesToolNumberTextBox.Size = new System.Drawing.Size(100, 26);
            this.tracesToolNumberTextBox.TabIndex = 35;
            this.tracesToolNumberTextBox.Text = "1";
            this.tracesToolNumberTextBox.TextChanged += new System.EventHandler(this.tracesToolNumberTextBox_TextChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(320, 68);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(66, 20);
            this.label15.TabIndex = 34;
            this.label15.Text = "Mill level";
            // 
            // tracesMillLevelTextBox
            // 
            this.tracesMillLevelTextBox.Location = new System.Drawing.Point(440, 65);
            this.tracesMillLevelTextBox.Name = "tracesMillLevelTextBox";
            this.tracesMillLevelTextBox.Size = new System.Drawing.Size(100, 26);
            this.tracesMillLevelTextBox.TabIndex = 34;
            this.tracesMillLevelTextBox.Text = "-0,05";
            this.tracesMillLevelTextBox.TextChanged += new System.EventHandler(this.tracesMillLevelTextBox_TextChanged);
            // 
            // millTracesCheckBox
            // 
            this.millTracesCheckBox.AutoSize = true;
            this.millTracesCheckBox.Checked = true;
            this.millTracesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.millTracesCheckBox.Location = new System.Drawing.Point(6, 25);
            this.millTracesCheckBox.Name = "millTracesCheckBox";
            this.millTracesCheckBox.Size = new System.Drawing.Size(105, 24);
            this.millTracesCheckBox.TabIndex = 17;
            this.millTracesCheckBox.Text = "Mill traces";
            this.millTracesCheckBox.UseVisualStyleBackColor = true;
            this.millTracesCheckBox.CheckedChanged += new System.EventHandler(this.millTracesCheckBox_CheckedChanged);
            // 
            // tracesVFeedRateTextBox
            // 
            this.tracesVFeedRateTextBox.Location = new System.Drawing.Point(440, 126);
            this.tracesVFeedRateTextBox.Name = "tracesVFeedRateTextBox";
            this.tracesVFeedRateTextBox.Size = new System.Drawing.Size(100, 26);
            this.tracesVFeedRateTextBox.TabIndex = 27;
            this.tracesVFeedRateTextBox.Text = "800";
            this.tracesVFeedRateTextBox.TextChanged += new System.EventHandler(this.tracesVFeedRateTextBox_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(318, 129);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(96, 20);
            this.label8.TabIndex = 26;
            this.label8.Text = "V FeedRate";
            // 
            // tracesDiameterTextBox
            // 
            this.tracesDiameterTextBox.Location = new System.Drawing.Point(136, 97);
            this.tracesDiameterTextBox.Name = "tracesDiameterTextBox";
            this.tracesDiameterTextBox.Size = new System.Drawing.Size(100, 26);
            this.tracesDiameterTextBox.TabIndex = 20;
            this.tracesDiameterTextBox.Text = "0,2";
            this.tracesDiameterTextBox.TextChanged += new System.EventHandler(this.tracesDiameterTextBox_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(318, 100);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(97, 20);
            this.label9.TabIndex = 25;
            this.label9.Text = "H FeedRate";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(14, 100);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(74, 20);
            this.label11.TabIndex = 21;
            this.label11.Text = "Diameter";
            // 
            // tracesHFeedRateTextBox
            // 
            this.tracesHFeedRateTextBox.Location = new System.Drawing.Point(440, 97);
            this.tracesHFeedRateTextBox.Name = "tracesHFeedRateTextBox";
            this.tracesHFeedRateTextBox.Size = new System.Drawing.Size(100, 26);
            this.tracesHFeedRateTextBox.TabIndex = 24;
            this.tracesHFeedRateTextBox.Text = "800";
            this.tracesHFeedRateTextBox.TextChanged += new System.EventHandler(this.tracesHFeedRateTextBox_TextChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(14, 132);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(110, 20);
            this.label10.TabIndex = 22;
            this.label10.Text = "Spindle speed";
            // 
            // tracesSpindleSpeedTextBox
            // 
            this.tracesSpindleSpeedTextBox.Location = new System.Drawing.Point(136, 126);
            this.tracesSpindleSpeedTextBox.Name = "tracesSpindleSpeedTextBox";
            this.tracesSpindleSpeedTextBox.Size = new System.Drawing.Size(100, 26);
            this.tracesSpindleSpeedTextBox.TabIndex = 23;
            this.tracesSpindleSpeedTextBox.Text = "18000";
            this.tracesSpindleSpeedTextBox.TextChanged += new System.EventHandler(this.tracesSpindleSpeedTextBox_TextChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label16);
            this.groupBox3.Controls.Add(this.boardToolNumberTextBox);
            this.groupBox3.Controls.Add(this.label14);
            this.groupBox3.Controls.Add(this.boardVStepTextBox);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.boardVFeedRateTextBox);
            this.groupBox3.Controls.Add(this.boardHFeedRateTextBox);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.boardMillLevelTextBox);
            this.groupBox3.Controls.Add(this.boardSpindleSpeedTextBox);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.boardDiameterTextBox);
            this.groupBox3.Controls.Add(this.boardDrillsCheckBox);
            this.groupBox3.Controls.Add(this.boardHolesCheckBox);
            this.groupBox3.Controls.Add(this.boardBorderCheckBox);
            this.groupBox3.Location = new System.Drawing.Point(12, 444);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(640, 212);
            this.groupBox3.TabIndex = 28;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Board milling";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(14, 104);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(97, 20);
            this.label16.TabIndex = 37;
            this.label16.Text = "Tool number";
            // 
            // boardToolNumberTextBox
            // 
            this.boardToolNumberTextBox.Location = new System.Drawing.Point(136, 101);
            this.boardToolNumberTextBox.Name = "boardToolNumberTextBox";
            this.boardToolNumberTextBox.Size = new System.Drawing.Size(100, 26);
            this.boardToolNumberTextBox.TabIndex = 36;
            this.boardToolNumberTextBox.Text = "2";
            this.boardToolNumberTextBox.TextChanged += new System.EventHandler(this.boardToolNumberTextBox_TextChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(318, 75);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(66, 20);
            this.label14.TabIndex = 33;
            this.label14.Text = "Mill level";
            // 
            // boardVStepTextBox
            // 
            this.boardVStepTextBox.Location = new System.Drawing.Point(434, 165);
            this.boardVStepTextBox.Name = "boardVStepTextBox";
            this.boardVStepTextBox.Size = new System.Drawing.Size(100, 26);
            this.boardVStepTextBox.TabIndex = 33;
            this.boardVStepTextBox.Text = "1.5";
            this.boardVStepTextBox.TextChanged += new System.EventHandler(this.boardVStepTextBox_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(320, 168);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(58, 20);
            this.label6.TabIndex = 16;
            this.label6.Text = "V Step";
            // 
            // boardVFeedRateTextBox
            // 
            this.boardVFeedRateTextBox.Location = new System.Drawing.Point(434, 133);
            this.boardVFeedRateTextBox.Name = "boardVFeedRateTextBox";
            this.boardVFeedRateTextBox.Size = new System.Drawing.Size(100, 26);
            this.boardVFeedRateTextBox.TabIndex = 15;
            this.boardVFeedRateTextBox.Text = "600";
            this.boardVFeedRateTextBox.TextChanged += new System.EventHandler(this.boardVFeedRateTextBox_TextChanged);
            // 
            // boardHFeedRateTextBox
            // 
            this.boardHFeedRateTextBox.Location = new System.Drawing.Point(434, 101);
            this.boardHFeedRateTextBox.Name = "boardHFeedRateTextBox";
            this.boardHFeedRateTextBox.Size = new System.Drawing.Size(100, 26);
            this.boardHFeedRateTextBox.TabIndex = 14;
            this.boardHFeedRateTextBox.Text = "600";
            this.boardHFeedRateTextBox.TextChanged += new System.EventHandler(this.boardHFeedRateTextBox_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(318, 136);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 20);
            this.label4.TabIndex = 13;
            this.label4.Text = "V FeedRate";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(317, 104);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(97, 20);
            this.label5.TabIndex = 12;
            this.label5.Text = "H FeedRate";
            // 
            // boardMillLevelTextBox
            // 
            this.boardMillLevelTextBox.Location = new System.Drawing.Point(434, 72);
            this.boardMillLevelTextBox.Name = "boardMillLevelTextBox";
            this.boardMillLevelTextBox.Size = new System.Drawing.Size(100, 26);
            this.boardMillLevelTextBox.TabIndex = 11;
            this.boardMillLevelTextBox.Text = "-1,6";
            this.boardMillLevelTextBox.TextChanged += new System.EventHandler(this.boardMillLevelTextBox_TextChanged);
            // 
            // boardSpindleSpeedTextBox
            // 
            this.boardSpindleSpeedTextBox.Location = new System.Drawing.Point(136, 165);
            this.boardSpindleSpeedTextBox.Name = "boardSpindleSpeedTextBox";
            this.boardSpindleSpeedTextBox.Size = new System.Drawing.Size(100, 26);
            this.boardSpindleSpeedTextBox.TabIndex = 10;
            this.boardSpindleSpeedTextBox.Text = "16000";
            this.boardSpindleSpeedTextBox.TextChanged += new System.EventHandler(this.boardSpindleSpeedTextBox_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 168);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 20);
            this.label3.TabIndex = 9;
            this.label3.Text = "Spindle speed";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 136);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 20);
            this.label2.TabIndex = 8;
            this.label2.Text = "Diameter";
            // 
            // boardDiameterTextBox
            // 
            this.boardDiameterTextBox.Location = new System.Drawing.Point(136, 133);
            this.boardDiameterTextBox.Name = "boardDiameterTextBox";
            this.boardDiameterTextBox.Size = new System.Drawing.Size(100, 26);
            this.boardDiameterTextBox.TabIndex = 7;
            this.boardDiameterTextBox.Text = "2";
            this.boardDiameterTextBox.TextChanged += new System.EventHandler(this.boardDiameterTextBox_TextChanged);
            // 
            // boardDrillsCheckBox
            // 
            this.boardDrillsCheckBox.AutoSize = true;
            this.boardDrillsCheckBox.Location = new System.Drawing.Point(294, 25);
            this.boardDrillsCheckBox.Name = "boardDrillsCheckBox";
            this.boardDrillsCheckBox.Size = new System.Drawing.Size(92, 24);
            this.boardDrillsCheckBox.TabIndex = 6;
            this.boardDrillsCheckBox.Text = "Mill drills";
            this.boardDrillsCheckBox.UseVisualStyleBackColor = true;
            this.boardDrillsCheckBox.CheckedChanged += new System.EventHandler(this.boardDrillsCheckBox_CheckedChanged);
            // 
            // boardHolesCheckBox
            // 
            this.boardHolesCheckBox.AutoSize = true;
            this.boardHolesCheckBox.Location = new System.Drawing.Point(150, 25);
            this.boardHolesCheckBox.Name = "boardHolesCheckBox";
            this.boardHolesCheckBox.Size = new System.Drawing.Size(99, 24);
            this.boardHolesCheckBox.TabIndex = 5;
            this.boardHolesCheckBox.Text = "Mill holes";
            this.boardHolesCheckBox.UseVisualStyleBackColor = true;
            this.boardHolesCheckBox.CheckedChanged += new System.EventHandler(this.boardHolesCheckBox_CheckedChanged);
            // 
            // boardBorderCheckBox
            // 
            this.boardBorderCheckBox.AutoSize = true;
            this.boardBorderCheckBox.Location = new System.Drawing.Point(6, 25);
            this.boardBorderCheckBox.Name = "boardBorderCheckBox";
            this.boardBorderCheckBox.Size = new System.Drawing.Size(107, 24);
            this.boardBorderCheckBox.TabIndex = 4;
            this.boardBorderCheckBox.Text = "Mill border";
            this.boardBorderCheckBox.UseVisualStyleBackColor = true;
            this.boardBorderCheckBox.CheckedChanged += new System.EventHandler(this.boardBorderCheckBox_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.drillDrillLevelTextBox);
            this.groupBox4.Controls.Add(this.label13);
            this.groupBox4.Controls.Add(this.drillGenerateCheckBox);
            this.groupBox4.Controls.Add(this.button4);
            this.groupBox4.Controls.Add(this.button3);
            this.groupBox4.Controls.Add(this.dataGridView1);
            this.groupBox4.Location = new System.Drawing.Point(12, 662);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(640, 280);
            this.groupBox4.TabIndex = 29;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Drilling";
            // 
            // drillDrillLevelTextBox
            // 
            this.drillDrillLevelTextBox.Location = new System.Drawing.Point(98, 78);
            this.drillDrillLevelTextBox.Name = "drillDrillLevelTextBox";
            this.drillDrillLevelTextBox.Size = new System.Drawing.Size(100, 26);
            this.drillDrillLevelTextBox.TabIndex = 32;
            this.drillDrillLevelTextBox.Text = "-1,6";
            this.drillDrillLevelTextBox.TextChanged += new System.EventHandler(this.drillDrillLevelTextBox_TextChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(14, 81);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(70, 20);
            this.label13.TabIndex = 9;
            this.label13.Text = "Drill level";
            // 
            // drillGenerateCheckBox
            // 
            this.drillGenerateCheckBox.AutoSize = true;
            this.drillGenerateCheckBox.Location = new System.Drawing.Point(12, 40);
            this.drillGenerateCheckBox.Name = "drillGenerateCheckBox";
            this.drillGenerateCheckBox.Size = new System.Drawing.Size(138, 24);
            this.drillGenerateCheckBox.TabIndex = 3;
            this.drillGenerateCheckBox.Text = "Generate drills";
            this.drillGenerateCheckBox.UseVisualStyleBackColor = true;
            this.drillGenerateCheckBox.CheckedChanged += new System.EventHandler(this.drillGenerateCheckBox_CheckedChanged);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(493, 71);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(127, 41);
            this.button4.TabIndex = 2;
            this.button4.Text = "Delete drill";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(493, 23);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(127, 41);
            this.button3.TabIndex = 1;
            this.button3.Text = "Add drill";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4});
            this.dataGridView1.Location = new System.Drawing.Point(0, 118);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidth = 50;
            this.dataGridView1.RowTemplate.Height = 28;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(628, 156);
            this.dataGridView1.TabIndex = 0;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Diameter";
            this.Column1.MinimumWidth = 8;
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Column1.Width = 85;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "Tool number";
            this.Column2.MinimumWidth = 8;
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Column2.Width = 85;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "Spindle speed";
            this.Column3.MinimumWidth = 8;
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Column3.Width = 85;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "V Feed rate ";
            this.Column4.MinimumWidth = 8;
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Column4.Width = 85;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(353, 12);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(121, 46);
            this.button5.TabIndex = 30;
            this.button5.Text = "Open Profile";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(480, 12);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(114, 46);
            this.button6.TabIndex = 31;
            this.button6.Text = "Save Profile";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // comboBox2
            // 
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Items.AddRange(new object[] {
            "2",
            "5",
            "10",
            "20",
            "30",
            "40",
            "50"});
            this.comboBox2.Location = new System.Drawing.Point(503, 72);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(129, 28);
            this.comboBox2.TabIndex = 32;
            this.comboBox2.SelectedIndexChanged += new System.EventHandler(this.comboBox2_SelectedIndexChanged);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(216, 12);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(114, 46);
            this.button8.TabIndex = 34;
            this.button8.Text = "Reload file";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // comboBox3
            // 
            this.comboBox3.FormattingEnabled = true;
            this.comboBox3.Items.AddRange(new object[] {
            "TOP",
            "BOTTOM"});
            this.comboBox3.Location = new System.Drawing.Point(503, 106);
            this.comboBox3.Name = "comboBox3";
            this.comboBox3.Size = new System.Drawing.Size(129, 28);
            this.comboBox3.TabIndex = 36;
            this.comboBox3.SelectedIndexChanged += new System.EventHandler(this.comboBox3_SelectedIndexChanged);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(428, 109);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(48, 20);
            this.label18.TabIndex = 35;
            this.label18.Text = "Layer";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(318, 52);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(70, 20);
            this.label19.TabIndex = 33;
            this.label19.Text = "M3 Dwel";
            // 
            // m3DwelTextBox
            // 
            this.m3DwelTextBox.Location = new System.Drawing.Point(440, 49);
            this.m3DwelTextBox.Name = "m3DwelTextBox";
            this.m3DwelTextBox.Size = new System.Drawing.Size(100, 26);
            this.m3DwelTextBox.TabIndex = 32;
            this.m3DwelTextBox.Text = "20";
            this.m3DwelTextBox.TextChanged += new System.EventHandler(this.m3DwelTextBox_TextChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2781, 1345);
            this.Controls.Add(this.comboBox3);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.comboBox2);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "KiCAD2Gcode";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox boardSpindleSpeedTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox boardDiameterTextBox;
        private System.Windows.Forms.CheckBox boardDrillsCheckBox;
        private System.Windows.Forms.CheckBox boardHolesCheckBox;
        private System.Windows.Forms.CheckBox boardBorderCheckBox;
        private System.Windows.Forms.CheckBox drillGenerateCheckBox;
        private System.Windows.Forms.TextBox safeLevelTextBox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox clearLevelTextBox;
        private System.Windows.Forms.CheckBox millTracesCheckBox;
        private System.Windows.Forms.TextBox tracesVFeedRateTextBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tracesDiameterTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox tracesHFeedRateTextBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox tracesSpindleSpeedTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox boardVFeedRateTextBox;
        private System.Windows.Forms.TextBox boardHFeedRateTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox boardMillLevelTextBox;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox tracesToolNumberTextBox;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox tracesMillLevelTextBox;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox boardToolNumberTextBox;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox boardVStepTextBox;
        private System.Windows.Forms.TextBox drillDrillLevelTextBox;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.ComboBox comboBox3;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox m3DwelTextBox;
    }
}

