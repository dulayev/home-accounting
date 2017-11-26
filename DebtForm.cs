using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;

namespace Home_Accounting
{
    public partial class DebtForm : Form
    {
        public DebtForm()
        {
            InitializeComponent();
            dataGridView1.AutoGenerateColumns = false;
        }

        DataTable debtTable = new DataTable();

        public DataRow[] SelectedRows
        {
            get
            {
                DataRow[] rows = new DataRow[dataGridView1.SelectedRows.Count];
                int i = 0;
                foreach (DataGridViewRow viewRow in dataGridView1.SelectedRows)
                {
                    DataRowView dataRowView = (DataRowView)viewRow.DataBoundItem;
                    rows[i++] = dataRowView.Row;
                }
                return rows;
            }
        }

        private void DebtForm_Load(object sender, EventArgs e)
        {
            ReloadDebts();
        }

        private void DebtForm_MouseDown(object sender, MouseEventArgs e)
        {
            DoDragDrop(this, DragDropEffects.All);
        }

        private void DebtForm_DragDrop(object sender, DragEventArgs e)
        {
            AccountForm accountForm = (AccountForm)e.Data.GetData(typeof(AccountForm));
            if (accountForm != null)
                new DebtCreationForm(accountForm, this).ShowDialog();
        }

        private void DebtForm_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(AccountForm)) != null)
                e.Effect = DragDropEffects.Move;
        }

        internal void ReloadDebts()
        {
            string sql = "SELECT Debt.ID, Debt.Name, Debt.Amount, Debt.Plan FROM Debt WHERE (((Debt.Actual) Is Null))";
            OleDbCommand cmd = new OleDbCommand(sql, DataUtil.Connection);
            OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);
            debtTable.Clear();
            adapter.Fill(debtTable);
            dataGridView1.DataSource = debtTable;
        }

        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
                DoDragDrop(this, DragDropEffects.All);
        }
    }
}