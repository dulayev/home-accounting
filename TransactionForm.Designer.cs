namespace Home_Accounting
{
    partial class TransactionForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TransactionForm));
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textBoxSrcAmount = new System.Windows.Forms.TextBox();
            this.labelSrc = new System.Windows.Forms.Label();
            this.labelDst = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.textBoxDstAmount = new System.Windows.Forms.TextBox();
            this.labelRate = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(128, 224);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(4);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(100, 28);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(287, 224);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(100, 28);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // textBoxSrcAmount
            // 
            this.textBoxSrcAmount.Location = new System.Drawing.Point(47, 83);
            this.textBoxSrcAmount.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxSrcAmount.Name = "textBoxSrcAmount";
            this.textBoxSrcAmount.Size = new System.Drawing.Size(132, 22);
            this.textBoxSrcAmount.TabIndex = 2;
            this.textBoxSrcAmount.TextChanged += new System.EventHandler(this.textBoxSrcAmount_TextChanged);
            // 
            // labelSrc
            // 
            this.labelSrc.Location = new System.Drawing.Point(16, 16);
            this.labelSrc.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSrc.Name = "labelSrc";
            this.labelSrc.Size = new System.Drawing.Size(207, 28);
            this.labelSrc.TabIndex = 0;
            this.labelSrc.Text = "labelSrc";
            this.labelSrc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelDst
            // 
            this.labelDst.Location = new System.Drawing.Point(269, 16);
            this.labelDst.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDst.Name = "labelDst";
            this.labelDst.Size = new System.Drawing.Size(205, 28);
            this.labelDst.TabIndex = 1;
            this.labelDst.Text = "labelDst";
            this.labelDst.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(231, 16);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(31, 28);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // textBoxDstAmount
            // 
            this.textBoxDstAmount.Location = new System.Drawing.Point(310, 83);
            this.textBoxDstAmount.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxDstAmount.Name = "textBoxDstAmount";
            this.textBoxDstAmount.Size = new System.Drawing.Size(132, 22);
            this.textBoxDstAmount.TabIndex = 4;
            this.textBoxDstAmount.TextChanged += new System.EventHandler(this.textBoxDstAmount_TextChanged);
            // 
            // labelRate
            // 
            this.labelRate.Location = new System.Drawing.Point(186, 86);
            this.labelRate.Name = "labelRate";
            this.labelRate.Size = new System.Drawing.Size(117, 23);
            this.labelRate.TabIndex = 3;
            this.labelRate.Text = "rate";
            this.labelRate.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TransactionForm
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(491, 286);
            this.Controls.Add(this.labelRate);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.labelDst);
            this.Controls.Add(this.labelSrc);
            this.Controls.Add(this.textBoxDstAmount);
            this.Controls.Add(this.textBoxSrcAmount);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "TransactionForm";
            this.Text = "TransactionForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TransactionForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TextBox textBoxSrcAmount;
        private System.Windows.Forms.Label labelSrc;
        private System.Windows.Forms.Label labelDst;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox textBoxDstAmount;
        private System.Windows.Forms.Label labelRate;
    }
}