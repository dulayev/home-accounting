using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace Home_Accounting
{
    internal class QueryForm : Form
    {
        private Control[] controls;
        private OleDbCommand cmd;
        private DataGridView dataGridView1;
        private FlowLayoutPanel flowLayoutPanel1;
        private System.Drawing.Size defaultSize = new System.Drawing.Size(438, 127);

        DataTable dataTable = new DataTable();

        public QueryForm(Control[] controls, OleDbCommand cmd)
        {
            this.controls = controls;

            this.cmd = cmd;

            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();

            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
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
            this.ControlBox = false;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.dataGridView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Text = "Caption";
            this.Load += new System.EventHandler(this.OnLoad);

            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Reload();
        }

        public OleDbCommand DbCommand { get { return cmd; } }

        public void Reload()
        {
            dataTable.Clear();
            if (!string.IsNullOrEmpty(cmd.CommandText))
            {
                OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);
                adapter.Fill(dataTable);
                dataGridView1.DataSource = dataTable;
            }
        }
    }
}