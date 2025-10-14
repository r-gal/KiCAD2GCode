namespace KiCad2Gcode
{
    partial class TestForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.numericUpDown_sPtX = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDown_sPtY = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_ePtY = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDown_ePtX = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDown_radius = new System.Windows.Forms.NumericUpDown();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_sPtX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_sPtY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_ePtY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_ePtX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_radius)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(43, 35);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(113, 47);
            this.button1.TabIndex = 6;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // numericUpDown_sPtX
            // 
            this.numericUpDown_sPtX.DecimalPlaces = 1;
            this.numericUpDown_sPtX.Location = new System.Drawing.Point(160, 136);
            this.numericUpDown_sPtX.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericUpDown_sPtX.Name = "numericUpDown_sPtX";
            this.numericUpDown_sPtX.Size = new System.Drawing.Size(169, 26);
            this.numericUpDown_sPtX.TabIndex = 7;
            this.numericUpDown_sPtX.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.label1.Location = new System.Drawing.Point(57, 138);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 20);
            this.label1.TabIndex = 8;
            this.label1.Text = "Start point";
            // 
            // numericUpDown_sPtY
            // 
            this.numericUpDown_sPtY.DecimalPlaces = 1;
            this.numericUpDown_sPtY.Location = new System.Drawing.Point(335, 136);
            this.numericUpDown_sPtY.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericUpDown_sPtY.Name = "numericUpDown_sPtY";
            this.numericUpDown_sPtY.Size = new System.Drawing.Size(169, 26);
            this.numericUpDown_sPtY.TabIndex = 9;
            this.numericUpDown_sPtY.ValueChanged += new System.EventHandler(this.numericUpDown_sPtY_ValueChanged);
            // 
            // numericUpDown_ePtY
            // 
            this.numericUpDown_ePtY.DecimalPlaces = 1;
            this.numericUpDown_ePtY.Location = new System.Drawing.Point(335, 178);
            this.numericUpDown_ePtY.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericUpDown_ePtY.Name = "numericUpDown_ePtY";
            this.numericUpDown_ePtY.Size = new System.Drawing.Size(169, 26);
            this.numericUpDown_ePtY.TabIndex = 12;
            this.numericUpDown_ePtY.ValueChanged += new System.EventHandler(this.numericUpDown_ePtY_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(57, 180);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 20);
            this.label2.TabIndex = 11;
            this.label2.Text = "End point";
            // 
            // numericUpDown_ePtX
            // 
            this.numericUpDown_ePtX.DecimalPlaces = 1;
            this.numericUpDown_ePtX.Location = new System.Drawing.Point(160, 178);
            this.numericUpDown_ePtX.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericUpDown_ePtX.Name = "numericUpDown_ePtX";
            this.numericUpDown_ePtX.Size = new System.Drawing.Size(169, 26);
            this.numericUpDown_ePtX.TabIndex = 10;
            this.numericUpDown_ePtX.ValueChanged += new System.EventHandler(this.numericUpDown_ePtX_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(57, 225);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 20);
            this.label3.TabIndex = 14;
            this.label3.Text = "Radius";
            // 
            // numericUpDown_radius
            // 
            this.numericUpDown_radius.DecimalPlaces = 1;
            this.numericUpDown_radius.Location = new System.Drawing.Point(160, 223);
            this.numericUpDown_radius.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericUpDown_radius.Minimum = new decimal(new int[] {
            500,
            0,
            0,
            -2147483648});
            this.numericUpDown_radius.Name = "numericUpDown_radius";
            this.numericUpDown_radius.Size = new System.Drawing.Size(169, 26);
            this.numericUpDown_radius.TabIndex = 13;
            this.numericUpDown_radius.ValueChanged += new System.EventHandler(this.numericUpDown_cPtX_ValueChanged);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "LINE",
            "ARC_CW",
            "ARC_CCW"});
            this.comboBox1.Location = new System.Drawing.Point(61, 274);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 28);
            this.comboBox1.TabIndex = 16;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Location = new System.Drawing.Point(547, 26);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1761, 1200);
            this.panel1.TabIndex = 21;
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
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(160, 322);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(310, 26);
            this.textBox1.TabIndex = 22;
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2350, 1295);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numericUpDown_radius);
            this.Controls.Add(this.numericUpDown_ePtY);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numericUpDown_ePtX);
            this.Controls.Add(this.numericUpDown_sPtY);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDown_sPtX);
            this.Controls.Add(this.button1);
            this.Name = "TestForm";
            this.Text = "TestForm";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_sPtX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_sPtY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_ePtY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_ePtX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_radius)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.NumericUpDown numericUpDown_sPtX;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDown_sPtY;
        private System.Windows.Forms.NumericUpDown numericUpDown_ePtY;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDown_ePtX;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDown_radius;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox textBox1;
    }
}