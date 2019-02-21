using System.Windows.Forms;

namespace Home_Accounting
{
    internal class GridForm : Form
    {
        private readonly Control[] controls;
        private readonly DataGridView dataGridView1;
        private readonly FlowLayoutPanel flowLayoutPanel1;
        private System.Drawing.Size defaultSize = new System.Drawing.Size(438, 127);

        public DataGridView GridView { get { return dataGridView1; } }

        public GridForm(Control[] controls)
        {
            this.controls = controls;

            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();

            InitLayout(controls);
        }

        private void InitLayout(Control[] controls)
        {
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();

            int controlsHeight = 0;
            foreach (Control control in controls)
            {
                this.flowLayoutPanel1.Controls.Add(control);
                if (controlsHeight < control.Height)
                {
                    controlsHeight = control.Height;
                }
            }
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((
                System.Windows.Forms.AnchorStyles.Top)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(
                defaultSize.Width, controlsHeight);

            this.dataGridView1.AllowDrop = false;
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.AllowUserToResizeRows = false;

            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((
                System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.Location = new System.Drawing.Point(0, controlsHeight);
            this.dataGridView1.Size = new System.Drawing.Size(defaultSize.Width,
                defaultSize.Height - controlsHeight);
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

            this.ClientSize = defaultSize;
            this.ShowInTaskbar = false;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.dataGridView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Text = "Caption";

            this.flowLayoutPanel1.ResumeLayout(true);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
