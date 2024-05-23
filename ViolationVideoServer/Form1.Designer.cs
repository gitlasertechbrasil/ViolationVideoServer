namespace ViolationVideoServer
{
    partial class Form1
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
            this.btnStart = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tbxCameraIP = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbxAPIPort = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.nupframerate = new System.Windows.Forms.NumericUpDown();
            this.nupQuality = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.pctVideo = new System.Windows.Forms.PictureBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.nupBefore = new System.Windows.Forms.NumericUpDown();
            this.nupAfter = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nupframerate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nupQuality)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pctVideo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nupBefore)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nupAfter)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 17);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 275);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(576, 163);
            this.textBox1.TabIndex = 1;
            // 
            // tbxCameraIP
            // 
            this.tbxCameraIP.Location = new System.Drawing.Point(99, 54);
            this.tbxCameraIP.Name = "tbxCameraIP";
            this.tbxCameraIP.Size = new System.Drawing.Size(141, 20);
            this.tbxCameraIP.TabIndex = 2;
            this.tbxCameraIP.Text = "192.168.50.172";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Camera IP:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "API Port:";
            // 
            // tbxAPIPort
            // 
            this.tbxAPIPort.Location = new System.Drawing.Point(99, 87);
            this.tbxAPIPort.Name = "tbxAPIPort";
            this.tbxAPIPort.Size = new System.Drawing.Size(141, 20);
            this.tbxAPIPort.TabIndex = 5;
            this.tbxAPIPort.Text = "51000";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 127);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Frame rate:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(157, 127);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(27, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "FPS";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(18, 165);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Quality:";
            // 
            // nupframerate
            // 
            this.nupframerate.Location = new System.Drawing.Point(99, 125);
            this.nupframerate.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.nupframerate.Name = "nupframerate";
            this.nupframerate.Size = new System.Drawing.Size(47, 20);
            this.nupframerate.TabIndex = 10;
            this.nupframerate.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // nupQuality
            // 
            this.nupQuality.Location = new System.Drawing.Point(99, 163);
            this.nupQuality.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.nupQuality.Name = "nupQuality";
            this.nupQuality.Size = new System.Drawing.Size(47, 20);
            this.nupQuality.TabIndex = 11;
            this.nupQuality.Value = new decimal(new int[] {
            70,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(157, 165);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(15, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "%";
            // 
            // pctVideo
            // 
            this.pctVideo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pctVideo.Location = new System.Drawing.Point(265, 18);
            this.pctVideo.Name = "pctVideo";
            this.pctVideo.Size = new System.Drawing.Size(320, 240);
            this.pctVideo.TabIndex = 13;
            this.pctVideo.TabStop = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(19, 237);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(55, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "TimeAfter:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(18, 204);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(64, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "TimeBefore:";
            // 
            // nupBefore
            // 
            this.nupBefore.Location = new System.Drawing.Point(99, 200);
            this.nupBefore.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nupBefore.Name = "nupBefore";
            this.nupBefore.Size = new System.Drawing.Size(47, 20);
            this.nupBefore.TabIndex = 16;
            this.nupBefore.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // nupAfter
            // 
            this.nupAfter.Location = new System.Drawing.Point(99, 232);
            this.nupAfter.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nupAfter.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nupAfter.Name = "nupAfter";
            this.nupAfter.Size = new System.Drawing.Size(47, 20);
            this.nupAfter.TabIndex = 17;
            this.nupAfter.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(156, 204);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(47, 13);
            this.label9.TabIndex = 18;
            this.label9.Text = "seconds";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(157, 237);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(47, 13);
            this.label10.TabIndex = 19;
            this.label10.Text = "seconds";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(605, 450);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.nupAfter);
            this.Controls.Add(this.nupBefore);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.pctVideo);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.nupQuality);
            this.Controls.Add(this.nupframerate);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbxAPIPort);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbxCameraIP);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnStart);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nupframerate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nupQuality)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pctVideo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nupBefore)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nupAfter)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox tbxCameraIP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbxAPIPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown nupframerate;
        private System.Windows.Forms.NumericUpDown nupQuality;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.PictureBox pctVideo;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown nupBefore;
        private System.Windows.Forms.NumericUpDown nupAfter;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
    }
}

