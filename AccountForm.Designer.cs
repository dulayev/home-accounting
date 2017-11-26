namespace Home_Accounting
{
    partial class AccountForm
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
            this.labelBalance = new System.Windows.Forms.Label();
            this.buttonCheck = new System.Windows.Forms.Button();
            this.labelCheck = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // labelBalance
            // 
            this.labelBalance.AllowDrop = true;
            this.labelBalance.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelBalance.ForeColor = System.Drawing.Color.Red;
            this.labelBalance.Location = new System.Drawing.Point(176, 0);
            this.labelBalance.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelBalance.Name = "labelBalance";
            this.labelBalance.Size = new System.Drawing.Size(219, 42);
            this.labelBalance.TabIndex = 0;
            this.labelBalance.Text = "100";
            this.labelBalance.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelBalance.DragDrop += new System.Windows.Forms.DragEventHandler(this.AccountForm_DragDrop);
            this.labelBalance.DragOver += new System.Windows.Forms.DragEventHandler(this.AccountForm_DragOver);
            this.labelBalance.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AccountForm_MouseDown);
            // 
            // buttonCheck
            // 
            this.buttonCheck.Location = new System.Drawing.Point(247, 46);
            this.buttonCheck.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonCheck.Name = "buttonCheck";
            this.buttonCheck.Size = new System.Drawing.Size(80, 28);
            this.buttonCheck.TabIndex = 1;
            this.buttonCheck.Text = "Сверить";
            this.buttonCheck.UseVisualStyleBackColor = true;
            this.buttonCheck.Click += new System.EventHandler(this.buttonCheck_Click);
            // 
            // labelCheck
            // 
            this.labelCheck.Location = new System.Drawing.Point(181, 78);
            this.labelCheck.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelCheck.Name = "labelCheck";
            this.labelCheck.Size = new System.Drawing.Size(213, 28);
            this.labelCheck.TabIndex = 2;
            this.labelCheck.Text = "label1";
            this.labelCheck.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(177, 107);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.DragDrop += new System.Windows.Forms.DragEventHandler(this.AccountForm_DragDrop);
            this.pictureBox1.DragOver += new System.Windows.Forms.DragEventHandler(this.AccountForm_DragOver);
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AccountForm_MouseDown);
            // 
            // AccountForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.Pink;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(395, 107);
            this.ControlBox = false;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.labelCheck);
            this.Controls.Add(this.buttonCheck);
            this.Controls.Add(this.labelBalance);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "AccountForm";
            this.Text = "AccountForm";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.AccountForm_DragDrop);
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.AccountForm_DragOver);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AccountForm_MouseDown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelBalance;
        private System.Windows.Forms.Button buttonCheck;
        private System.Windows.Forms.Label labelCheck;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}