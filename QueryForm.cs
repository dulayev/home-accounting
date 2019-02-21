using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace Home_Accounting
{
    internal class QueryForm : GridForm
    {
        private OleDbCommand cmd;

        DataTable dataTable = new DataTable();

        public QueryForm(Control[] controls, OleDbCommand cmd) : base(controls)
        {
            this.cmd = cmd;
            this.Load += new System.EventHandler(this.OnLoad);
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
                base.GridView.DataSource = dataTable;
            }
        }
    }
}