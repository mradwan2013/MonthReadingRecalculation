﻿
namespace MonthReadingRecalculation
{
    partial class FrmRecalculate
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
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.richTextBox3 = new System.Windows.Forms.RichTextBox();
            this.richTextBox4 = new System.Windows.Forms.RichTextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.RecalcProgressLbl = new System.Windows.Forms.Label();
            this.RecalcProgressBar = new System.Windows.Forms.ProgressBar();
            this.IncudeEstidamaCkBx = new System.Windows.Forms.CheckBox();
            this.RecalculateBtn = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.RecalculateResultTxt = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.label10 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.richTextBox7 = new System.Windows.Forms.RichTextBox();
            this.richTextBox6 = new System.Windows.Forms.RichTextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage4.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.CancelBtn);
            this.tabPage4.Controls.Add(this.richTextBox3);
            this.tabPage4.Controls.Add(this.richTextBox4);
            this.tabPage4.Controls.Add(this.label6);
            this.tabPage4.Location = new System.Drawing.Point(4, 25);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(838, 480);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Cancel Charges";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // CancelBtn
            // 
            this.CancelBtn.Location = new System.Drawing.Point(359, 135);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(93, 30);
            this.CancelBtn.TabIndex = 22;
            this.CancelBtn.Text = "Cancel charges";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // richTextBox3
            // 
            this.richTextBox3.Location = new System.Drawing.Point(28, 54);
            this.richTextBox3.Name = "richTextBox3";
            this.richTextBox3.Size = new System.Drawing.Size(785, 75);
            this.richTextBox3.TabIndex = 23;
            this.richTextBox3.Text = "";
            // 
            // richTextBox4
            // 
            this.richTextBox4.Location = new System.Drawing.Point(28, 189);
            this.richTextBox4.Name = "richTextBox4";
            this.richTextBox4.ReadOnly = true;
            this.richTextBox4.Size = new System.Drawing.Size(785, 265);
            this.richTextBox4.TabIndex = 25;
            this.richTextBox4.Text = "";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(25, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(586, 25);
            this.label6.TabIndex = 24;
            this.label6.Text = "Write charges query then press Cancel to begin charges cancellation";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.RecalcProgressLbl);
            this.tabPage2.Controls.Add(this.RecalcProgressBar);
            this.tabPage2.Controls.Add(this.IncudeEstidamaCkBx);
            this.tabPage2.Controls.Add(this.RecalculateBtn);
            this.tabPage2.Controls.Add(this.richTextBox1);
            this.tabPage2.Controls.Add(this.RecalculateResultTxt);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(838, 480);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "RecalculateMonthReadings";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // RecalcProgressLbl
            // 
            this.RecalcProgressLbl.AutoSize = true;
            this.RecalcProgressLbl.Location = new System.Drawing.Point(577, 139);
            this.RecalcProgressLbl.Name = "RecalcProgressLbl";
            this.RecalcProgressLbl.Size = new System.Drawing.Size(0, 17);
            this.RecalcProgressLbl.TabIndex = 24;
            // 
            // RecalcProgressBar
            // 
            this.RecalcProgressBar.Location = new System.Drawing.Point(329, 137);
            this.RecalcProgressBar.Name = "RecalcProgressBar";
            this.RecalcProgressBar.Size = new System.Drawing.Size(242, 23);
            this.RecalcProgressBar.TabIndex = 23;
            // 
            // IncudeEstidamaCkBx
            // 
            this.IncudeEstidamaCkBx.AutoSize = true;
            this.IncudeEstidamaCkBx.Location = new System.Drawing.Point(24, 139);
            this.IncudeEstidamaCkBx.Name = "IncudeEstidamaCkBx";
            this.IncudeEstidamaCkBx.Size = new System.Drawing.Size(132, 21);
            this.IncudeEstidamaCkBx.TabIndex = 22;
            this.IncudeEstidamaCkBx.Text = "Include estidama";
            this.IncudeEstidamaCkBx.UseVisualStyleBackColor = true;
            // 
            // RecalculateBtn
            // 
            this.RecalculateBtn.Location = new System.Drawing.Point(205, 133);
            this.RecalculateBtn.Name = "RecalculateBtn";
            this.RecalculateBtn.Size = new System.Drawing.Size(93, 30);
            this.RecalculateBtn.TabIndex = 18;
            this.RecalculateBtn.Text = "Recalculate";
            this.RecalculateBtn.UseVisualStyleBackColor = true;
            this.RecalculateBtn.Click += new System.EventHandler(this.RecalculateBtn_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(24, 49);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(785, 75);
            this.richTextBox1.TabIndex = 19;
            this.richTextBox1.Text = "";
            // 
            // RecalculateResultTxt
            // 
            this.RecalculateResultTxt.Location = new System.Drawing.Point(24, 184);
            this.RecalculateResultTxt.Name = "RecalculateResultTxt";
            this.RecalculateResultTxt.ReadOnly = true;
            this.RecalculateResultTxt.Size = new System.Drawing.Size(785, 265);
            this.RecalculateResultTxt.TabIndex = 21;
            this.RecalculateResultTxt.Text = "";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(21, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(586, 25);
            this.label1.TabIndex = 20;
            this.label1.Text = "Write month reading query then press Recalculate to begin recalculation month rea" +
    "ding rows";
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.label10);
            this.tabPage5.Controls.Add(this.label7);
            this.tabPage5.Controls.Add(this.richTextBox7);
            this.tabPage5.Controls.Add(this.richTextBox6);
            this.tabPage5.Controls.Add(this.button4);
            this.tabPage5.Controls.Add(this.label8);
            this.tabPage5.Location = new System.Drawing.Point(4, 25);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(838, 480);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "UpdateMonthReadingData";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(115, 146);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(16, 17);
            this.label10.TabIndex = 26;
            this.label10.Text = "0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(44, 146);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 17);
            this.label7.TabIndex = 25;
            this.label7.Text = "counter";
            // 
            // richTextBox7
            // 
            this.richTextBox7.Location = new System.Drawing.Point(25, 191);
            this.richTextBox7.Name = "richTextBox7";
            this.richTextBox7.ReadOnly = true;
            this.richTextBox7.Size = new System.Drawing.Size(785, 265);
            this.richTextBox7.TabIndex = 24;
            this.richTextBox7.Text = "";
            // 
            // richTextBox6
            // 
            this.richTextBox6.EnableAutoDragDrop = true;
            this.richTextBox6.Location = new System.Drawing.Point(25, 58);
            this.richTextBox6.Name = "richTextBox6";
            this.richTextBox6.Size = new System.Drawing.Size(785, 75);
            this.richTextBox6.TabIndex = 21;
            this.richTextBox6.Text = "";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(305, 139);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(93, 30);
            this.button4.TabIndex = 23;
            this.button4.Text = "Update";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(22, 30);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(777, 25);
            this.label8.TabIndex = 22;
            this.label8.Text = "Write month reading query then press Update to begin update phase no, gucode and " +
    "activity month reading rows";
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(838, 480);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "DB connection";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBox4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBox3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textBox2);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Location = new System.Drawing.Point(20, 22);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(238, 173);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Database connection";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(19, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 23);
            this.label2.TabIndex = 8;
            this.label2.Text = "Server";
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(125, 134);
            this.textBox4.Name = "textBox4";
            this.textBox4.PasswordChar = '*';
            this.textBox4.Size = new System.Drawing.Size(100, 24);
            this.textBox4.TabIndex = 15;
            this.textBox4.UseSystemPasswordChar = true;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(19, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 23);
            this.label3.TabIndex = 9;
            this.label3.Text = "Database";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(125, 104);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(100, 24);
            this.textBox3.TabIndex = 14;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(19, 104);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 23);
            this.label4.TabIndex = 10;
            this.label4.Text = "UserName";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(125, 71);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(100, 24);
            this.textBox2.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(19, 137);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 23);
            this.label5.TabIndex = 11;
            this.label5.Text = "Password";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(125, 39);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 24);
            this.textBox1.TabIndex = 12;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(21, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(846, 509);
            this.tabControl1.TabIndex = 18;
            // 
            // FrmRecalculate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(886, 548);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FrmRecalculate";
            this.Text = "Recalcualte Month Readings";
            this.tabPage4.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.RichTextBox richTextBox3;
        private System.Windows.Forms.RichTextBox richTextBox4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label RecalcProgressLbl;
        private System.Windows.Forms.ProgressBar RecalcProgressBar;
        private System.Windows.Forms.CheckBox IncudeEstidamaCkBx;
        private System.Windows.Forms.Button RecalculateBtn;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.RichTextBox RecalculateResultTxt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.RichTextBox richTextBox7;
        private System.Windows.Forms.RichTextBox richTextBox6;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label7;
    }
}