namespace Home_Accounting
{
    partial class DebtCreationForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxLendAmount = new System.Windows.Forms.TextBox();
            this.monthCalendarPayBack = new System.Windows.Forms.MonthCalendar();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.monthCalendarLend = new System.Windows.Forms.MonthCalendar();
            this.textBoxPayBackAmount = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.labelAnnualRate = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name:";
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(85, 13);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(332, 20);
            this.textBoxName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Lend:";
            // 
            // textBoxLendAmount
            // 
            this.textBoxLendAmount.Location = new System.Drawing.Point(76, 42);
            this.textBoxLendAmount.Name = "textBoxLendAmount";
            this.textBoxLendAmount.Size = new System.Drawing.Size(100, 20);
            this.textBoxLendAmount.TabIndex = 3;
            this.textBoxLendAmount.TextChanged += new System.EventHandler(this.TextBoxLendAmount_TextChanged);
            // 
            // monthCalendarPayBack
            // 
            this.monthCalendarPayBack.Location = new System.Drawing.Point(253, 70);
            this.monthCalendarPayBack.Name = "monthCalendarPayBack";
            this.monthCalendarPayBack.TabIndex = 8;
            this.monthCalendarPayBack.DateChanged += new System.Windows.Forms.DateRangeEventHandler(this.MonthCalendarPayBack_DateChanged);
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(253, 244);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 9;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(342, 244);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // monthCalendarLend
            // 
            this.monthCalendarLend.Location = new System.Drawing.Point(21, 70);
            this.monthCalendarLend.Name = "monthCalendarLend";
            this.monthCalendarLend.TabIndex = 4;
            this.monthCalendarLend.DateChanged += new System.Windows.Forms.DateRangeEventHandler(this.MonthCalendarLend_DateChanged);
            // 
            // textBoxPayBackAmount
            // 
            this.textBoxPayBackAmount.Location = new System.Drawing.Point(317, 46);
            this.textBoxPayBackAmount.Name = "textBoxPayBackAmount";
            this.textBoxPayBackAmount.Size = new System.Drawing.Size(100, 20);
            this.textBoxPayBackAmount.TabIndex = 7;
            this.textBoxPayBackAmount.TextChanged += new System.EventHandler(this.TextBoxPayBackAmount_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(250, 49);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Pay back:";
            // 
            // labelAnnualRate
            // 
            this.labelAnnualRate.AutoSize = true;
            this.labelAnnualRate.Location = new System.Drawing.Point(201, 45);
            this.labelAnnualRate.Name = "labelAnnualRate";
            this.labelAnnualRate.Size = new System.Drawing.Size(42, 13);
            this.labelAnnualRate.TabIndex = 5;
            this.labelAnnualRate.Text = "11.11%";
            // 
            // DebtCreationForm
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(437, 285);
            this.Controls.Add(this.labelAnnualRate);
            this.Controls.Add(this.textBoxPayBackAmount);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.monthCalendarLend);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.monthCalendarPayBack);
            this.Controls.Add(this.textBoxLendAmount);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.label1);
            this.Name = "DebtCreationForm";
            this.Text = "DebtCreationForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DebtCreationForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxLendAmount;
        private System.Windows.Forms.MonthCalendar monthCalendarPayBack;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.MonthCalendar monthCalendarLend;
        private System.Windows.Forms.TextBox textBoxPayBackAmount;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label labelAnnualRate;
    }
}