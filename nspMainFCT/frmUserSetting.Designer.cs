namespace nspMainFCT
{
    public partial class frmUserSetting
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
            this.tbModelCode = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbRomVer = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbLoaderVer = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbMotorDriver1 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbMotorDriver2 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbRomCheckMode = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.tbRamCheckMode = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbModelCode
            // 
            this.tbModelCode.Location = new System.Drawing.Point(132, 41);
            this.tbModelCode.Name = "tbModelCode";
            this.tbModelCode.Size = new System.Drawing.Size(135, 20);
            this.tbModelCode.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Model Code";
            // 
            // tbRomVer
            // 
            this.tbRomVer.Location = new System.Drawing.Point(132, 79);
            this.tbRomVer.Name = "tbRomVer";
            this.tbRomVer.Size = new System.Drawing.Size(135, 20);
            this.tbRomVer.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 79);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Rom Version";
            // 
            // tbLoaderVer
            // 
            this.tbLoaderVer.Location = new System.Drawing.Point(132, 115);
            this.tbLoaderVer.Name = "tbLoaderVer";
            this.tbLoaderVer.Size = new System.Drawing.Size(135, 20);
            this.tbLoaderVer.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Loader Version";
            // 
            // tbMotorDriver1
            // 
            this.tbMotorDriver1.Location = new System.Drawing.Point(132, 154);
            this.tbMotorDriver1.Name = "tbMotorDriver1";
            this.tbMotorDriver1.Size = new System.Drawing.Size(135, 20);
            this.tbMotorDriver1.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 154);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Motor Driver 1";
            // 
            // tbMotorDriver2
            // 
            this.tbMotorDriver2.Location = new System.Drawing.Point(132, 196);
            this.tbMotorDriver2.Name = "tbMotorDriver2";
            this.tbMotorDriver2.Size = new System.Drawing.Size(135, 20);
            this.tbMotorDriver2.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(22, 196);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(74, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Motor Driver 2";
            // 
            // tbRomCheckMode
            // 
            this.tbRomCheckMode.Location = new System.Drawing.Point(132, 238);
            this.tbRomCheckMode.Name = "tbRomCheckMode";
            this.tbRomCheckMode.Size = new System.Drawing.Size(135, 20);
            this.tbRomCheckMode.TabIndex = 13;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(22, 238);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(93, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Rom Check Mode";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(295, 41);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(201, 58);
            this.btnOK.TabIndex = 14;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // tbRamCheckMode
            // 
            this.tbRamCheckMode.Location = new System.Drawing.Point(132, 276);
            this.tbRamCheckMode.Name = "tbRamCheckMode";
            this.tbRamCheckMode.Size = new System.Drawing.Size(135, 20);
            this.tbRamCheckMode.TabIndex = 16;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(22, 276);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(93, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Ram Check Mode";
            // 
            // frmUserSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(513, 329);
            this.Controls.Add(this.tbRamCheckMode);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.tbRomCheckMode);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbMotorDriver2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbMotorDriver1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbLoaderVer);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbRomVer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbModelCode);
            this.Controls.Add(this.label1);
            this.Name = "frmUserSetting";
            this.Text = "frmUserSetting";
            this.Load += new System.EventHandler(this.frmUserSetting_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbModelCode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbRomVer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbLoaderVer;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbMotorDriver1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbMotorDriver2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbRomCheckMode;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TextBox tbRamCheckMode;
        private System.Windows.Forms.Label label7;
    }
}