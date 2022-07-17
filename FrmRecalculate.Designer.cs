
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
            this.CancelChTab = new System.Windows.Forms.TabPage();
            this.CancelProgressLbl = new System.Windows.Forms.Label();
            this.CancelProgressBar = new System.Windows.Forms.ProgressBar();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.CancelChargesQueryTxt = new System.Windows.Forms.RichTextBox();
            this.CancelChargesResultTxt = new System.Windows.Forms.RichTextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.RecalcMrTab = new System.Windows.Forms.TabPage();
            this.RecalcProgressLbl = new System.Windows.Forms.Label();
            this.RecalcProgressBar = new System.Windows.Forms.ProgressBar();
            this.IncudeEstidamaCkBx = new System.Windows.Forms.CheckBox();
            this.RecalculateBtn = new System.Windows.Forms.Button();
            this.RecalcQueryTxt = new System.Windows.Forms.RichTextBox();
            this.RecalculateResultTxt = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.UpdateMrTab = new System.Windows.Forms.TabPage();
            this.UpdateProgressBar = new System.Windows.Forms.ProgressBar();
            this.UpdateProgressLbl = new System.Windows.Forms.Label();
            this.UpdateDataResultTxt = new System.Windows.Forms.RichTextBox();
            this.UpdateQueryTxt = new System.Windows.Forms.RichTextBox();
            this.UpdateMRDataBtn = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.DBConnectionTab = new System.Windows.Forms.TabPage();
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
            this.UpdateReviewCardDate = new System.Windows.Forms.TabPage();
            this.ReviewCardProgressLbl = new System.Windows.Forms.Label();
            this.ReviewCardProgressBar = new System.Windows.Forms.ProgressBar();
            this.ReviewCardResultTxt = new System.Windows.Forms.RichTextBox();
            this.ReviewCardQueryTxt = new System.Windows.Forms.RichTextBox();
            this.ReviewCardDataBtn = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.UpdateTarrifDifference = new System.Windows.Forms.TabPage();
            this.TarrifDifferenceResultTxt = new System.Windows.Forms.RichTextBox();
            this.TarrifDifferenceProgressLbl = new System.Windows.Forms.Label();
            this.TarrifDifferenceProgressBar = new System.Windows.Forms.ProgressBar();
            this.TarrifDifferenceQueryTxt = new System.Windows.Forms.RichTextBox();
            this.TarrifDifferenceBtn = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.FixSewageCalc = new System.Windows.Forms.TabPage();
            this.FixSewageResultTxt = new System.Windows.Forms.RichTextBox();
            this.FixSewageProgressBar = new System.Windows.Forms.ProgressBar();
            this.FixSewageQueryTxt = new System.Windows.Forms.RichTextBox();
            this.FixSewageBtn = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.FixSewageProgressLbl = new System.Windows.Forms.Label();
            this.CancelChTab.SuspendLayout();
            this.RecalcMrTab.SuspendLayout();
            this.UpdateMrTab.SuspendLayout();
            this.DBConnectionTab.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.UpdateReviewCardDate.SuspendLayout();
            this.UpdateTarrifDifference.SuspendLayout();
            this.FixSewageCalc.SuspendLayout();
            this.SuspendLayout();
            // 
            // CancelChTab
            // 
            this.CancelChTab.Controls.Add(this.CancelProgressLbl);
            this.CancelChTab.Controls.Add(this.CancelProgressBar);
            this.CancelChTab.Controls.Add(this.CancelBtn);
            this.CancelChTab.Controls.Add(this.CancelChargesQueryTxt);
            this.CancelChTab.Controls.Add(this.CancelChargesResultTxt);
            this.CancelChTab.Controls.Add(this.label6);
            this.CancelChTab.Location = new System.Drawing.Point(4, 25);
            this.CancelChTab.Name = "CancelChTab";
            this.CancelChTab.Size = new System.Drawing.Size(1018, 480);
            this.CancelChTab.TabIndex = 3;
            this.CancelChTab.Text = "Cancel Charges";
            this.CancelChTab.UseVisualStyleBackColor = true;
            // 
            // CancelProgressLbl
            // 
            this.CancelProgressLbl.AutoSize = true;
            this.CancelProgressLbl.Location = new System.Drawing.Point(463, 143);
            this.CancelProgressLbl.Name = "CancelProgressLbl";
            this.CancelProgressLbl.Size = new System.Drawing.Size(0, 17);
            this.CancelProgressLbl.TabIndex = 27;
            // 
            // CancelProgressBar
            // 
            this.CancelProgressBar.Location = new System.Drawing.Point(208, 142);
            this.CancelProgressBar.Name = "CancelProgressBar";
            this.CancelProgressBar.Size = new System.Drawing.Size(242, 23);
            this.CancelProgressBar.TabIndex = 26;
            // 
            // CancelBtn
            // 
            this.CancelBtn.Location = new System.Drawing.Point(90, 140);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(93, 30);
            this.CancelBtn.TabIndex = 22;
            this.CancelBtn.Text = "Cancel charges";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // CancelChargesQueryTxt
            // 
            this.CancelChargesQueryTxt.Location = new System.Drawing.Point(28, 54);
            this.CancelChargesQueryTxt.Name = "CancelChargesQueryTxt";
            this.CancelChargesQueryTxt.Size = new System.Drawing.Size(785, 75);
            this.CancelChargesQueryTxt.TabIndex = 23;
            this.CancelChargesQueryTxt.Text = "";
            // 
            // CancelChargesResultTxt
            // 
            this.CancelChargesResultTxt.Location = new System.Drawing.Point(28, 189);
            this.CancelChargesResultTxt.Name = "CancelChargesResultTxt";
            this.CancelChargesResultTxt.ReadOnly = true;
            this.CancelChargesResultTxt.Size = new System.Drawing.Size(785, 265);
            this.CancelChargesResultTxt.TabIndex = 25;
            this.CancelChargesResultTxt.Text = "";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(25, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(586, 25);
            this.label6.TabIndex = 24;
            this.label6.Text = "Write charges query then press Cancel to begin charges cancellation";
            // 
            // RecalcMrTab
            // 
            this.RecalcMrTab.Controls.Add(this.RecalcProgressLbl);
            this.RecalcMrTab.Controls.Add(this.RecalcProgressBar);
            this.RecalcMrTab.Controls.Add(this.IncudeEstidamaCkBx);
            this.RecalcMrTab.Controls.Add(this.RecalculateBtn);
            this.RecalcMrTab.Controls.Add(this.RecalcQueryTxt);
            this.RecalcMrTab.Controls.Add(this.RecalculateResultTxt);
            this.RecalcMrTab.Controls.Add(this.label1);
            this.RecalcMrTab.Location = new System.Drawing.Point(4, 25);
            this.RecalcMrTab.Name = "RecalcMrTab";
            this.RecalcMrTab.Padding = new System.Windows.Forms.Padding(3);
            this.RecalcMrTab.Size = new System.Drawing.Size(1018, 480);
            this.RecalcMrTab.TabIndex = 1;
            this.RecalcMrTab.Text = "RecalculateMonthReadings";
            this.RecalcMrTab.UseVisualStyleBackColor = true;
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
            // RecalcQueryTxt
            // 
            this.RecalcQueryTxt.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.RecalcQueryTxt.Location = new System.Drawing.Point(24, 49);
            this.RecalcQueryTxt.Name = "RecalcQueryTxt";
            this.RecalcQueryTxt.Size = new System.Drawing.Size(785, 75);
            this.RecalcQueryTxt.TabIndex = 19;
            this.RecalcQueryTxt.Text = "";
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
            // UpdateMrTab
            // 
            this.UpdateMrTab.Controls.Add(this.UpdateProgressBar);
            this.UpdateMrTab.Controls.Add(this.UpdateProgressLbl);
            this.UpdateMrTab.Controls.Add(this.UpdateDataResultTxt);
            this.UpdateMrTab.Controls.Add(this.UpdateQueryTxt);
            this.UpdateMrTab.Controls.Add(this.UpdateMRDataBtn);
            this.UpdateMrTab.Controls.Add(this.label8);
            this.UpdateMrTab.Location = new System.Drawing.Point(4, 25);
            this.UpdateMrTab.Name = "UpdateMrTab";
            this.UpdateMrTab.Padding = new System.Windows.Forms.Padding(3);
            this.UpdateMrTab.Size = new System.Drawing.Size(1018, 480);
            this.UpdateMrTab.TabIndex = 4;
            this.UpdateMrTab.Text = "UpdateMonthReadingData";
            this.UpdateMrTab.UseVisualStyleBackColor = true;
            // 
            // UpdateProgressBar
            // 
            this.UpdateProgressBar.Location = new System.Drawing.Point(208, 149);
            this.UpdateProgressBar.Name = "UpdateProgressBar";
            this.UpdateProgressBar.Size = new System.Drawing.Size(242, 23);
            this.UpdateProgressBar.TabIndex = 27;
            // 
            // UpdateProgressLbl
            // 
            this.UpdateProgressLbl.AutoSize = true;
            this.UpdateProgressLbl.Location = new System.Drawing.Point(456, 152);
            this.UpdateProgressLbl.Name = "UpdateProgressLbl";
            this.UpdateProgressLbl.Size = new System.Drawing.Size(0, 17);
            this.UpdateProgressLbl.TabIndex = 25;
            // 
            // UpdateDataResultTxt
            // 
            this.UpdateDataResultTxt.Location = new System.Drawing.Point(25, 191);
            this.UpdateDataResultTxt.Name = "UpdateDataResultTxt";
            this.UpdateDataResultTxt.ReadOnly = true;
            this.UpdateDataResultTxt.Size = new System.Drawing.Size(785, 265);
            this.UpdateDataResultTxt.TabIndex = 24;
            this.UpdateDataResultTxt.Text = "";
            // 
            // UpdateQueryTxt
            // 
            this.UpdateQueryTxt.EnableAutoDragDrop = true;
            this.UpdateQueryTxt.Location = new System.Drawing.Point(25, 58);
            this.UpdateQueryTxt.Name = "UpdateQueryTxt";
            this.UpdateQueryTxt.Size = new System.Drawing.Size(785, 75);
            this.UpdateQueryTxt.TabIndex = 21;
            this.UpdateQueryTxt.Text = "";
            // 
            // UpdateMRDataBtn
            // 
            this.UpdateMRDataBtn.Location = new System.Drawing.Point(90, 145);
            this.UpdateMRDataBtn.Name = "UpdateMRDataBtn";
            this.UpdateMRDataBtn.Size = new System.Drawing.Size(93, 30);
            this.UpdateMRDataBtn.TabIndex = 23;
            this.UpdateMRDataBtn.Text = "Update";
            this.UpdateMRDataBtn.UseVisualStyleBackColor = true;
            this.UpdateMRDataBtn.Click += new System.EventHandler(this.UpdateMRDataBtn_Click);
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
            // DBConnectionTab
            // 
            this.DBConnectionTab.Controls.Add(this.groupBox1);
            this.DBConnectionTab.Location = new System.Drawing.Point(4, 25);
            this.DBConnectionTab.Name = "DBConnectionTab";
            this.DBConnectionTab.Padding = new System.Windows.Forms.Padding(3);
            this.DBConnectionTab.Size = new System.Drawing.Size(1018, 480);
            this.DBConnectionTab.TabIndex = 0;
            this.DBConnectionTab.Text = "DB connection";
            this.DBConnectionTab.UseVisualStyleBackColor = true;
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
            this.tabControl1.Controls.Add(this.DBConnectionTab);
            this.tabControl1.Controls.Add(this.UpdateMrTab);
            this.tabControl1.Controls.Add(this.RecalcMrTab);
            this.tabControl1.Controls.Add(this.CancelChTab);
            this.tabControl1.Controls.Add(this.UpdateReviewCardDate);
            this.tabControl1.Controls.Add(this.UpdateTarrifDifference);
            this.tabControl1.Controls.Add(this.FixSewageCalc);
            this.tabControl1.Location = new System.Drawing.Point(21, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1026, 509);
            this.tabControl1.TabIndex = 18;
            // 
            // UpdateReviewCardDate
            // 
            this.UpdateReviewCardDate.Controls.Add(this.ReviewCardProgressLbl);
            this.UpdateReviewCardDate.Controls.Add(this.ReviewCardProgressBar);
            this.UpdateReviewCardDate.Controls.Add(this.ReviewCardResultTxt);
            this.UpdateReviewCardDate.Controls.Add(this.ReviewCardQueryTxt);
            this.UpdateReviewCardDate.Controls.Add(this.ReviewCardDataBtn);
            this.UpdateReviewCardDate.Controls.Add(this.label7);
            this.UpdateReviewCardDate.Location = new System.Drawing.Point(4, 25);
            this.UpdateReviewCardDate.Name = "UpdateReviewCardDate";
            this.UpdateReviewCardDate.Size = new System.Drawing.Size(1018, 480);
            this.UpdateReviewCardDate.TabIndex = 5;
            this.UpdateReviewCardDate.Text = "UpdateReviewCardDate";
            this.UpdateReviewCardDate.UseVisualStyleBackColor = true;
            // 
            // ReviewCardProgressLbl
            // 
            this.ReviewCardProgressLbl.AutoSize = true;
            this.ReviewCardProgressLbl.Location = new System.Drawing.Point(470, 146);
            this.ReviewCardProgressLbl.Name = "ReviewCardProgressLbl";
            this.ReviewCardProgressLbl.Size = new System.Drawing.Size(0, 17);
            this.ReviewCardProgressLbl.TabIndex = 33;
            // 
            // ReviewCardProgressBar
            // 
            this.ReviewCardProgressBar.Location = new System.Drawing.Point(211, 146);
            this.ReviewCardProgressBar.Name = "ReviewCardProgressBar";
            this.ReviewCardProgressBar.Size = new System.Drawing.Size(242, 23);
            this.ReviewCardProgressBar.TabIndex = 32;
            // 
            // ReviewCardResultTxt
            // 
            this.ReviewCardResultTxt.Location = new System.Drawing.Point(28, 188);
            this.ReviewCardResultTxt.Name = "ReviewCardResultTxt";
            this.ReviewCardResultTxt.ReadOnly = true;
            this.ReviewCardResultTxt.Size = new System.Drawing.Size(785, 265);
            this.ReviewCardResultTxt.TabIndex = 31;
            this.ReviewCardResultTxt.Text = "";
            // 
            // ReviewCardQueryTxt
            // 
            this.ReviewCardQueryTxt.EnableAutoDragDrop = true;
            this.ReviewCardQueryTxt.Location = new System.Drawing.Point(28, 55);
            this.ReviewCardQueryTxt.Name = "ReviewCardQueryTxt";
            this.ReviewCardQueryTxt.Size = new System.Drawing.Size(785, 75);
            this.ReviewCardQueryTxt.TabIndex = 28;
            this.ReviewCardQueryTxt.Text = "";
            // 
            // ReviewCardDataBtn
            // 
            this.ReviewCardDataBtn.Location = new System.Drawing.Point(93, 142);
            this.ReviewCardDataBtn.Name = "ReviewCardDataBtn";
            this.ReviewCardDataBtn.Size = new System.Drawing.Size(93, 30);
            this.ReviewCardDataBtn.TabIndex = 30;
            this.ReviewCardDataBtn.Text = "Update";
            this.ReviewCardDataBtn.UseVisualStyleBackColor = true;
            this.ReviewCardDataBtn.Click += new System.EventHandler(this.ReviewCardDataBtn_Click);
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(25, 27);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(777, 25);
            this.label7.TabIndex = 29;
            this.label7.Text = "Write month reading query then press Update to begin update phase no, gucode and " +
    "activity month reading rows";
            // 
            // UpdateTarrifDifference
            // 
            this.UpdateTarrifDifference.Controls.Add(this.TarrifDifferenceResultTxt);
            this.UpdateTarrifDifference.Controls.Add(this.TarrifDifferenceProgressLbl);
            this.UpdateTarrifDifference.Controls.Add(this.TarrifDifferenceProgressBar);
            this.UpdateTarrifDifference.Controls.Add(this.TarrifDifferenceQueryTxt);
            this.UpdateTarrifDifference.Controls.Add(this.TarrifDifferenceBtn);
            this.UpdateTarrifDifference.Controls.Add(this.label9);
            this.UpdateTarrifDifference.Location = new System.Drawing.Point(4, 25);
            this.UpdateTarrifDifference.Name = "UpdateTarrifDifference";
            this.UpdateTarrifDifference.Size = new System.Drawing.Size(1018, 480);
            this.UpdateTarrifDifference.TabIndex = 6;
            this.UpdateTarrifDifference.Text = "UpdateTarrifDifference";
            this.UpdateTarrifDifference.UseVisualStyleBackColor = true;
            // 
            // TarrifDifferenceResultTxt
            // 
            this.TarrifDifferenceResultTxt.Location = new System.Drawing.Point(30, 176);
            this.TarrifDifferenceResultTxt.Name = "TarrifDifferenceResultTxt";
            this.TarrifDifferenceResultTxt.ReadOnly = true;
            this.TarrifDifferenceResultTxt.Size = new System.Drawing.Size(785, 265);
            this.TarrifDifferenceResultTxt.TabIndex = 38;
            this.TarrifDifferenceResultTxt.Text = "";
            // 
            // TarrifDifferenceProgressLbl
            // 
            this.TarrifDifferenceProgressLbl.AutoSize = true;
            this.TarrifDifferenceProgressLbl.Location = new System.Drawing.Point(478, 147);
            this.TarrifDifferenceProgressLbl.Name = "TarrifDifferenceProgressLbl";
            this.TarrifDifferenceProgressLbl.Size = new System.Drawing.Size(0, 17);
            this.TarrifDifferenceProgressLbl.TabIndex = 37;
            // 
            // TarrifDifferenceProgressBar
            // 
            this.TarrifDifferenceProgressBar.Location = new System.Drawing.Point(213, 144);
            this.TarrifDifferenceProgressBar.Name = "TarrifDifferenceProgressBar";
            this.TarrifDifferenceProgressBar.Size = new System.Drawing.Size(242, 23);
            this.TarrifDifferenceProgressBar.TabIndex = 36;
            // 
            // TarrifDifferenceQueryTxt
            // 
            this.TarrifDifferenceQueryTxt.EnableAutoDragDrop = true;
            this.TarrifDifferenceQueryTxt.Location = new System.Drawing.Point(30, 53);
            this.TarrifDifferenceQueryTxt.Name = "TarrifDifferenceQueryTxt";
            this.TarrifDifferenceQueryTxt.Size = new System.Drawing.Size(785, 75);
            this.TarrifDifferenceQueryTxt.TabIndex = 33;
            this.TarrifDifferenceQueryTxt.Text = "";
            // 
            // TarrifDifferenceBtn
            // 
            this.TarrifDifferenceBtn.Location = new System.Drawing.Point(95, 140);
            this.TarrifDifferenceBtn.Name = "TarrifDifferenceBtn";
            this.TarrifDifferenceBtn.Size = new System.Drawing.Size(93, 30);
            this.TarrifDifferenceBtn.TabIndex = 35;
            this.TarrifDifferenceBtn.Text = "Update";
            this.TarrifDifferenceBtn.UseVisualStyleBackColor = true;
            this.TarrifDifferenceBtn.Click += new System.EventHandler(this.TarrifDifferenceBtn_Click);
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(27, 25);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(777, 25);
            this.label9.TabIndex = 34;
            this.label9.Text = "Write month reading query then press Update to begin review old month readings an" +
    "d apply tarrif differenec";
            // 
            // FixSewageCalc
            // 
            this.FixSewageCalc.Controls.Add(this.FixSewageProgressLbl);
            this.FixSewageCalc.Controls.Add(this.FixSewageResultTxt);
            this.FixSewageCalc.Controls.Add(this.FixSewageProgressBar);
            this.FixSewageCalc.Controls.Add(this.FixSewageQueryTxt);
            this.FixSewageCalc.Controls.Add(this.FixSewageBtn);
            this.FixSewageCalc.Controls.Add(this.label10);
            this.FixSewageCalc.Location = new System.Drawing.Point(4, 25);
            this.FixSewageCalc.Name = "FixSewageCalc";
            this.FixSewageCalc.Size = new System.Drawing.Size(1018, 480);
            this.FixSewageCalc.TabIndex = 7;
            this.FixSewageCalc.Text = "FixSewageCalc";
            this.FixSewageCalc.UseVisualStyleBackColor = true;
            // 
            // FixSewageResultTxt
            // 
            this.FixSewageResultTxt.Location = new System.Drawing.Point(95, 183);
            this.FixSewageResultTxt.Name = "FixSewageResultTxt";
            this.FixSewageResultTxt.ReadOnly = true;
            this.FixSewageResultTxt.Size = new System.Drawing.Size(785, 265);
            this.FixSewageResultTxt.TabIndex = 43;
            this.FixSewageResultTxt.Text = "";
            // 
            // FixSewageProgressBar
            // 
            this.FixSewageProgressBar.Location = new System.Drawing.Point(278, 151);
            this.FixSewageProgressBar.Name = "FixSewageProgressBar";
            this.FixSewageProgressBar.Size = new System.Drawing.Size(242, 23);
            this.FixSewageProgressBar.TabIndex = 42;
            // 
            // FixSewageQueryTxt
            // 
            this.FixSewageQueryTxt.EnableAutoDragDrop = true;
            this.FixSewageQueryTxt.Location = new System.Drawing.Point(95, 60);
            this.FixSewageQueryTxt.Name = "FixSewageQueryTxt";
            this.FixSewageQueryTxt.Size = new System.Drawing.Size(785, 75);
            this.FixSewageQueryTxt.TabIndex = 39;
            this.FixSewageQueryTxt.Text = "";
            // 
            // FixSewageBtn
            // 
            this.FixSewageBtn.Location = new System.Drawing.Point(160, 147);
            this.FixSewageBtn.Name = "FixSewageBtn";
            this.FixSewageBtn.Size = new System.Drawing.Size(93, 30);
            this.FixSewageBtn.TabIndex = 41;
            this.FixSewageBtn.Text = "Update";
            this.FixSewageBtn.UseVisualStyleBackColor = true;
            this.FixSewageBtn.Click += new System.EventHandler(this.FixSewageBtn_Click);
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(92, 32);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(777, 25);
            this.label10.TabIndex = 40;
            this.label10.Text = "Write month reading query then press Update to begin review old month readings ta" +
    "rrif difference sewage money";
            // 
            // FixSewageProgressLbl
            // 
            this.FixSewageProgressLbl.AutoSize = true;
            this.FixSewageProgressLbl.Location = new System.Drawing.Point(527, 156);
            this.FixSewageProgressLbl.Name = "FixSewageProgressLbl";
            this.FixSewageProgressLbl.Size = new System.Drawing.Size(0, 17);
            this.FixSewageProgressLbl.TabIndex = 44;
            // 
            // FrmRecalculate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1059, 548);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FrmRecalculate";
            this.Text = "Recalcualte Month Readings";
            this.CancelChTab.ResumeLayout(false);
            this.CancelChTab.PerformLayout();
            this.RecalcMrTab.ResumeLayout(false);
            this.RecalcMrTab.PerformLayout();
            this.UpdateMrTab.ResumeLayout(false);
            this.UpdateMrTab.PerformLayout();
            this.DBConnectionTab.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.UpdateReviewCardDate.ResumeLayout(false);
            this.UpdateReviewCardDate.PerformLayout();
            this.UpdateTarrifDifference.ResumeLayout(false);
            this.UpdateTarrifDifference.PerformLayout();
            this.FixSewageCalc.ResumeLayout(false);
            this.FixSewageCalc.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage CancelChTab;
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.RichTextBox CancelChargesResultTxt;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TabPage RecalcMrTab;
        private System.Windows.Forms.Label RecalcProgressLbl;
        private System.Windows.Forms.ProgressBar RecalcProgressBar;
        private System.Windows.Forms.CheckBox IncudeEstidamaCkBx;
        private System.Windows.Forms.Button RecalculateBtn;
        private System.Windows.Forms.RichTextBox RecalculateResultTxt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage UpdateMrTab;
        private System.Windows.Forms.RichTextBox UpdateDataResultTxt;
        private System.Windows.Forms.Button UpdateMRDataBtn;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TabPage DBConnectionTab;
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
        private System.Windows.Forms.Label UpdateProgressLbl;
        public System.Windows.Forms.RichTextBox RecalcQueryTxt;
        public System.Windows.Forms.RichTextBox UpdateQueryTxt;
        public System.Windows.Forms.RichTextBox CancelChargesQueryTxt;
        private System.Windows.Forms.ProgressBar UpdateProgressBar;
        private System.Windows.Forms.Label CancelProgressLbl;
        private System.Windows.Forms.ProgressBar CancelProgressBar;
        private System.Windows.Forms.TabPage UpdateReviewCardDate;
        private System.Windows.Forms.ProgressBar ReviewCardProgressBar;
        private System.Windows.Forms.RichTextBox ReviewCardResultTxt;
        public System.Windows.Forms.RichTextBox ReviewCardQueryTxt;
        private System.Windows.Forms.Button ReviewCardDataBtn;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label ReviewCardProgressLbl;
        private System.Windows.Forms.TabPage UpdateTarrifDifference;
        private System.Windows.Forms.ProgressBar TarrifDifferenceProgressBar;
        public System.Windows.Forms.RichTextBox TarrifDifferenceQueryTxt;
        private System.Windows.Forms.Button TarrifDifferenceBtn;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label TarrifDifferenceProgressLbl;
        private System.Windows.Forms.RichTextBox TarrifDifferenceResultTxt;
        private System.Windows.Forms.TabPage FixSewageCalc;
        private System.Windows.Forms.RichTextBox FixSewageResultTxt;
        private System.Windows.Forms.ProgressBar FixSewageProgressBar;
        public System.Windows.Forms.RichTextBox FixSewageQueryTxt;
        private System.Windows.Forms.Button FixSewageBtn;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label FixSewageProgressLbl;
    }
}